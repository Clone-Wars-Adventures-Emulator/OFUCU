using CWAEmu.OFUCU.Flash;
using CWAEmu.OFUCU.Flash.Tags;
using CWAEmu.OFUCU.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VectorGraphics;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace CWAEmu.OFUCU {
    public class OFUCUSWF : MonoBehaviour {
        [SerializeField]
        private RectTransform vfswfhT;
        [SerializeField]
        private RectTransform dictonaryT;

        [SerializeField]
        private SWFFile file;
        [SerializeField]
        private string unityRoot;
        
        [SerializeField]
        private HashSet<int> dependencies = new();

        public Dictionary<int, OFUCUSprite> neoSprites = new();
        public HashSet<int> svgIds = new();

        public static void placeNewSWFFile(SWFFile file, string unityRoot) {
            if (!Directory.Exists(unityRoot)) {
                Debug.LogError($"Input/Output {unityRoot} does not exist.");
                return;
            }

            GameObject go = new($"SWF Root: {file.Name}");
            OFUCUSWF swf = go.AddComponent<OFUCUSWF>();
            swf.unityRoot = unityRoot;
            swf.file = file;
            swf.init();
        }

        private void init() {
            var files = Directory.EnumerateFiles($"{unityRoot}/shapes", "*.svg", SearchOption.TopDirectoryOnly);
            foreach (var filePath in files) {
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                if (int.TryParse(fileName, out var id)) {
                    svgIds.Add(id);
                }
            }

            Canvas canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2 | AdditionalCanvasShaderChannels.TexCoord3;

            CanvasScaler cScaler = gameObject.AddComponent<CanvasScaler>();
            cScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            cScaler.referenceResolution = new Vector2(file.FrameSize.Width, file.FrameSize.Height);
            cScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            cScaler.matchWidthOrHeight = 1.0f;
            cScaler.referencePixelsPerUnit = 100.0f;

            GraphicRaycaster raycaster = gameObject.AddComponent<GraphicRaycaster>();
            raycaster.ignoreReversedGraphics = true;
            raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
            // TODO: blocking mask
            // raycaster.blockingMask = 

            GameObject vfswfh = new($"VirtualFlashSWFHolder", typeof(RectTransform));
            vfswfhT = vfswfh.transform as RectTransform;

            vfswfhT.SetParent(canvas.transform, false);
            vfswfhT.sizeDelta = new Vector2(file.FrameSize.Width, file.FrameSize.Height);

            GameObject dictonaryRoot = new($"Dictonary", typeof(RectTransform));
            dictonaryT = dictonaryRoot.transform as RectTransform;
            dictonaryT.SetParent(canvas.transform, false);

            // find shape SVGs from specified output foler
            // create dictionary of all sprites and svg shapes
            // sprites need custom inspector that allows them to be made into prefabs (with further changes to the sprite by code modifying the prefab), and placed manually or animated
            
            foreach (var pair in file.Sprites) {
                var name = $"Sprite.{pair.Value.CharacterId}";
                var prefabDir = $"{unityRoot}/prefabs";

                GameObject go;
                if (File.Exists($"{prefabDir}/{name}.prefab")) {
                    GameObject pgo = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabDir}/{name}.prefab");
                    go = (GameObject)PrefabUtility.InstantiatePrefab(pgo);
                } else {
                    go = new(name, typeof(OFUCUSprite));
                }

                RectTransform rt = go.transform as RectTransform;
                rt.SetParent(dictonaryT, false);
                var dict = go.GetComponent<OFUCUSprite>();
                dict.init(this, pair.Value, prefabDir);
                neoSprites.Add(pair.Key, dict);
            }

            // calculate dependencies
            foreach (var f in file.Frames) {
                foreach (var t in f.Tags) {
                    if (t is PlaceObject2 po2 && po2.HasCharacter) {
                        dependencies.Add(po2.CharacterId);
                    }
                }
            }
        }

        public void placeFrames(RectTransform root, List<Frame> frames, HashSet<int> dependencies = null) {
            if (dependencies != null) {
                foreach (int i in dependencies) {
                    if (neoSprites.TryGetValue(i, out var sprite) && !sprite.Filled) {
                        sprite.place(true);
                    }
                }
            }

            DisplayList dl = new(frames);

            foreach (var df in dl.frames) {
                string name = df.label ?? $"Frame {df.frameIndex}";

                // create frame object
                GameObject frameGo = new(name, typeof(RectTransform));
                RectTransform frameRt = (RectTransform)frameGo.transform;
                frameRt.SetParent(root, false);

                RectTransform maskTrans = null;
                int maskDepth = -1;
                int curDepth = 0;

                foreach (var obj in df.states.Values.OrderBy(o => o.depth)) {
                    curDepth = obj.depth;
                    if (maskDepth > curDepth && maskTrans != null) {
                        maskTrans = null;
                    }

                    // create object (can be extracted)
                    var (go, aoo, ro) = createObjectReference(frameRt, obj);

                    // handle all the funnies
                    RectTransform goRt = (RectTransform)go.transform;
                    goRt.SetParent(maskTrans ?? frameRt, false);

                    // handle matrix (can be extracted?? (prob not, considering anim needs to do its own thing))
                    var (translate, scale, rotz) = obj.matrix.getTransformation();
                    goRt.anchoredPosition = translate;
                    goRt.localScale = scale.ToVector3(1);
                    goRt.rotation = Quaternion.Euler(0, 0, rotz);

                    if (obj.hasClipDepth) {
                        maskDepth = obj.clipDepth;
                        maskTrans = goRt;
                        var mask = go.AddComponent<Mask>();
                        mask.showMaskGraphic = false;
                    }

                    if (obj.hasColor) {
                        var col = obj.color;
                        if (col.hasMult) {
                            ro.setMultColor(col.mult);
                        }
                        if (col.hasAdd) {
                            ro.setAddColor(col.add);
                        }
                    }

                    if (obj.hasName) {
                        // TODO: decide if this is an override or just an additional
                        go.name = obj.name;
                    }

                    if (obj.hasBlendMode) {
                        aoo.setBlendMode(obj.blendMode);
                    }

                    if (obj.depth < maskDepth) {
                        go.AddComponent<RuntimeAnchor>();
                    }
                }
            }
        }

        public void animateFrames(RectTransform root, List<Frame> frames) {
            AnimateFramesWindow.root = root;
            AnimateFramesWindow.frames = frames;
            AnimateFramesWindow.onPress = onAnimateButton;
            EditorWindow.GetWindow<AnimateFramesWindow>($"Animate {root.name}");
        }

        private void onAnimateButton(RectTransform root, List<Frame> frames, bool labelsAsClips, List<int> clipIndexes) {
            if (!root.gameObject.TryGetComponent<Animator>(out var anim)) {
                anim = root.gameObject.AddComponent<Animator>();
            }

            var animDir = $"{unityRoot}/animations";
            if (!Directory.Exists(animDir)) {
                Directory.CreateDirectory(animDir);
            }

            var controllerPath = $"{animDir}/{root.name}.controller";
            if (AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath) != null) {
                AssetDatabase.DeleteAsset(controllerPath);
            }

            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            anim.runtimeAnimatorController = controller;

            var clipDefs = new List<(int start, int end, string name)>();
            if (labelsAsClips) {
                int start = 1;
                string name = null;

                foreach (Frame f in frames) {
                    foreach (var t in f.Tags) {
                        if (t is FrameLabel fl) {
                            if (f.FrameIndex != 1) {
                                clipDefs.Add((start, f.FrameIndex - 1, name));
                                start = f.FrameIndex;
                            }
                            name = fl.Label;
                            break;
                        }
                    }
                }

                // if name is null, give it a name (means that there likely were not any frame labels)
                name ??= $"Clip.{start}-{frames.Count}";
                clipDefs.Add((start, frames.Count, name));
            } else {
                clipIndexes ??= new();
                clipIndexes.Add(frames.Count);

                int start = 1;
                foreach (int i in clipIndexes) {
                    string name = $"Clip.{start}-{i}";
                    clipDefs.Add((start, i, name));
                    start = i + 1;
                }
            }


            DisplayList dl = new(frames);
            AnimatedThingList<AnimatedFrameObject> objs = new();
            Dictionary<int, (int start, int end, RectTransform rt)> masks = new();
            // for each frame
            foreach (var f in dl.frames) {
                // check objects removed and set them
                foreach (var depth in f.objectsRemoved) {
                    // find the object we are specifically referring to at that depth
                    if (objs.tryGetObject(depth, f.frameIndex, out var afo)) {
                        afo.end = f.frameIndex;
                    }

                    if (masks.ContainsKey(depth)) {
                        masks.Remove(depth);
                    }
                }

                // check objects added and spawn them
                foreach (var depth in f.objectsAdded) {
                    var (go, _, _) = createObjectReference(root, f.states[depth]);
                    go.AddComponent<AnimatedRuntimeObject>();

                    go.name = $"{go.name}.{depth}";

                    go.SetActive(false);
                    string objPath = go.name;

                    // TODO: does this work the way i epxect?
                    if (f.states.TryGetValue(depth, out var o) && o.hasClipDepth) {
                        masks.Add(depth, (depth, o.clipDepth, go.transform as RectTransform));
                        var mask = go.AddComponent<Mask>();
                        mask.showMaskGraphic = false;
                    }

                    foreach (var trip in masks.Values) {
                        if (trip.start < depth && trip.end >= depth) {
                            go.transform.SetParent(trip.rt, false);
                            go.AddComponent<RuntimeAnchor>();
                            objPath = $"{trip.rt.name}/{objPath}";
                        }
                    }

                    // TODO: initial state?

                    var afo = new AnimatedFrameObject() {
                        start = f.frameIndex,
                        go = go,
                        path = objPath,
                    };

                    objs.addAtDepth(depth, afo);
                }
            }

            List<AnimationClip> clips = new();
            foreach (var clipDef in clipDefs) {
                var clip = animateImpl(dl, objs, clipDef.start, clipDef.end, $"{root.name}.{clipDef.name}");
                clips.Add(clip);
            }

            var rootSM = controller.layers[0].stateMachine;
            foreach (var clip in clips) {
                var state = rootSM.AddState(clip.name.Replace('.', ' '));
                state.motion = clip;
            }

            try {
                AssetDatabase.StartAssetEditing();

                foreach (var clip in clips) {
                    var name = $"{animDir}/{clip.name}.anim";
                    var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(name);
                    if (existing != null) {
                        AssetDatabase.DeleteAsset(name);
                    }
                    AssetDatabase.CreateAsset(clip, name);
                }
            } catch (Exception e) {
                Debug.LogException(e);
            } finally {
                AssetDatabase.StopAssetEditing();
            }
        }

        private AnimationClip animateImpl(DisplayList dl, AnimatedThingList<AnimatedFrameObject> objs, int start, int end, string clipname = "default") {
            AnimationClip ac = new() {
                name = clipname,
                frameRate = file.FrameRate
            };

            AnimatedThingList<AnimationData> animData = new();
            animData.initFromOther(objs);

            foreach (var ad in animData) {
                ad.animateEnable(1, file.FrameRate, false);
            }

            for (int i = start; i <= end; i++) {
                DisplayFrame f = dl.frames[i - 1];

                foreach (var remove in f.objectsRemoved) {
                    if (animData.tryGetObject(remove, i - 1, out var anim)) {
                        anim.animateEnable(i - start + 1, file.FrameRate, false);
                    }
                }

                foreach (var add in f.objectsAdded) {
                    if (animData.tryGetObject(add, i, out var anim)) {
                        anim.animateEnable(i - start + 1, file.FrameRate, true);
                    }
                }

                foreach (var change in f.changes.Values) {
                    AnimatedFrameObject afo = objs.getObject(change.depth, i);
                    if (afo == null) {
                        Debug.LogWarning($"There is a change at frame {i}@{change.depth} that does not have an AFO.");
                        continue;
                    }

                    AnimationData ad = animData.getObject(change.depth, i);
                    if (ad == null) {
                        Debug.LogWarning($"There is a change at frame {i}@{change.depth} that does not have AnimData.");
                        continue;
                    }

                    if (change.hasMatrixChange) {
                        ad.animateMatrix(i - start + 1, file.FrameRate, change.matrix);
                    }

                    if (change.hasColor) {
                        ad.animateColor(i - start + 1, file.FrameRate, change.color);
                    }
                }
            }

            foreach (AnimationData anim in animData) {
                anim.applyToAnim(ac);
            }

            return ac;
        }

        public int allSpritesFilled(HashSet<int> spriteIds) {
            foreach (int id in spriteIds) {
                if (svgIds.Contains(id)) {
                    continue;
                }

                if (!neoSprites.TryGetValue(id, out var sprite) || !sprite.Filled || !sprite.HasPrefab) {
                    return id;
                }
            }

            return 0;
        }

        private (GameObject go, AbstractOFUCUObject aoo, RuntimeObject ro) createObjectReference(RectTransform parent, DisplayObject obj) {
            GameObject go = null;
            AbstractOFUCUObject aoo = null;
            RuntimeObject ro = null;
            if (neoSprites.TryGetValue(obj.charId, out var sprite)) {
                go = sprite.getCopy();
                go.transform.SetParent(parent, false);
                aoo = go.GetComponent<AbstractOFUCUObject>();
                Debug.Log($"Found {obj.charId} as sprite");
            }

            if (aoo == null) {
                string svg = $"{unityRoot}/shapes/{obj.charId}.svg";
                Debug.Log($"Looking for {obj.charId} as shape at {svg}");
                GameObject prefabGo = AssetDatabase.LoadAssetAtPath<GameObject>(svg);
                if (prefabGo == null) {
                    Debug.LogError("Failed to find svg file");
                }

                go = (GameObject)PrefabUtility.InstantiatePrefab(prefabGo, parent);
                aoo = go.AddComponent<OFUCUShape>();
                // Dont duplicate materials for now, if extra materials are needed, do it manually
                // TODO: no duplication breaks (as expected) when multiple instances of the same shape are being used at the same time and one is being animated
                // manual feels like a lot of extra work to get this to work right, and not something i want to do
                // Do i do this type of stuff as a check box on the animations only?
                // that would make sense for animation cases but what if in the place case there are 2 of the same shape being used that have 2 different adds or mults? what then?
                // or even better, one of them is additive blend and the other is regular blend? how does that get de-conflicted?
                // If i do do some auto de-duplication tactics, i will essentially need to save the path of the material and the depth as its name so it can be uniqely identified

                // var sg = go.GetComponent<SVGImage>();
                // sg.material = new Material(sg.material);
            }

            // TODO: handle text and other? objects
            if (aoo == null) {
                Debug.Log($"Not placing {obj.charId}, not shape or sprite");
            } else {
                ro = go.GetComponent<RuntimeObject>();
            }

            return (go, aoo, ro);
        }

        public void placeSwf() {
            // check if dependencies are filled, if not, dont do this
            var dep = allSpritesFilled(dependencies);
            if (dep != 0) {
                Debug.LogError($"Not placing swf, sprite {dep} is not filled.");
                return;
            }

            placeFrames(vfswfhT, file.Frames);
        }

        public void animSwf() {
            // check if dependencies are filled, if not, dont do this
            var dep = allSpritesFilled(dependencies);
            if (dep != 0) {
                Debug.LogError($"Not animating swf, sprite {dep} is not filled.");
                return;
            }

            animateFrames(vfswfhT, file.Frames);
        }

        private class AnimatedThingList<T> : IEnumerable<T> where T : AnimatedThing {
            private readonly Dictionary<int, List<T>> objs = new();

            public void initFromOther<V>(AnimatedThingList<V> other) where V : AnimatedThing {
                foreach (var pair in other.objs) {
                    var list = new List<T>();
                    objs.Add(pair.Key, list);

                    foreach (var v in pair.Value) {
                        var t = (T)Activator.CreateInstance(typeof(T));
                        t.Start = v.Start;
                        t.End = v.End;
                        t.Path = v.Path;
                        list.Add(t);
                    }
                }
            }

            public void addAtDepth(int depth, T obj) {
                if (!objs.TryGetValue(depth, out var dObjs)) {
                    dObjs = new();
                    objs.Add(depth, dObjs);
                }
                dObjs.Add(obj);
            }

            public T getObject(int depth, int frame) {
                if (!objs.TryGetValue(depth, out var afoList)) {
                    return null;
                }

                foreach (var check in afoList) {
                    if (check.isDesiredObj(frame)) {
                        return check;
                    }
                }

                return null;
            }

            public bool tryGetObject(int depth, int frame, out T obj) {
                obj = getObject(depth, frame);
                return obj != null;
            }

            public IEnumerator<T> GetEnumerator() {
                return objs.SelectMany(pair => pair.Value).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }
        }

        private class AnimatedFrameObject : AnimatedThing {
            public override int Start {
                get => start;
                set => start = value;
            }
            public int start;
            public override int End {
                get => end;
                set => end = value;
            }
            public int end = int.MaxValue;
            public override string Path {
                get => path;
                set => path = value;
            }
            public string path;
            public GameObject go;
        }

        private class AnimationData : AnimatedThing {
            public override int Start {
                get => start;
                set => start = value;
            }
            public int start;
            public override int End {
                get => end;
                set => end = value;
            }
            public int end;
            public override string Path {
                get => path;
                set => path = value;
            }
            public string path;

            private bool hasAnimatedColor = false;
            private bool hasAnimatedColorMult = false;
            private bool hasAnimatedColorAdd = false;

            private List<Keyframe> enabled = new();
            private List<Keyframe> xpos = new();
            private List<Keyframe> ypos = new();
            private List<Keyframe> xscale = new();
            private List<Keyframe> yscale = new();
            private List<Keyframe> zrot = new();
            private List<Keyframe> hasm = new();
            private List<Keyframe> hasa = new();
            private List<Keyframe> mr = new();
            private List<Keyframe> mg = new();
            private List<Keyframe> mb = new();
            private List<Keyframe> ma = new();
            private List<Keyframe> ar = new();
            private List<Keyframe> ag = new();
            private List<Keyframe> ab = new();
            private List<Keyframe> aa = new();

            public void animateMatrix(int frame, float frameRate, Matrix2x3 matrix) {
                var (t, s, r) = matrix.getTransformation();
                addKeyframe(xpos, frame, frameRate, t.x);
                addKeyframe(ypos, frame, frameRate, t.y);
                addKeyframe(xscale, frame, frameRate, s.x);
                addKeyframe(yscale, frame, frameRate, s.y);
                addKeyframe(zrot, frame, frameRate, r);
            }

            public void animateColor(int frame, float frameRate, ColorTransform ct) {
                // make sure the initial values are what we want (if we are animating frame one right now, the addKeyFrame call will handle that)
                if (!hasAnimatedColor) {
                    addKeyframe(hasm, 1, frameRate, 0, false);
                    addKeyframe(hasa, 1, frameRate, 0, false);
                }

                hasAnimatedColor = true;

                addKeyframe(hasm, frame, frameRate, ct.hasMult ? 1 : 0, false);
                if (ct.hasMult) {
                    if (!hasAnimatedColorMult) {
                        addKeyframe(mr, 1, frameRate, 1);
                        addKeyframe(mg, 1, frameRate, 1);
                        addKeyframe(mb, 1, frameRate, 1);
                        addKeyframe(ma, 1, frameRate, 1);
                    }

                    hasAnimatedColorMult = true;
                    Color col = ct.mult;
                    addKeyframe(mr, frame, frameRate, col.r);
                    addKeyframe(mg, frame, frameRate, col.g);
                    addKeyframe(mb, frame, frameRate, col.b);
                    addKeyframe(ma, frame, frameRate, col.a);
                }
                addKeyframe(hasa, frame, frameRate, ct.hasAdd ? 1 : 0, false);
                if (ct.hasAdd) {
                    if (!hasAnimatedColorAdd) {
                        addKeyframe(ar, 1, frameRate, 0);
                        addKeyframe(ag, 1, frameRate, 0);
                        addKeyframe(ab, 1, frameRate, 0);
                        addKeyframe(aa, 1, frameRate, 1);
                    }

                    hasAnimatedColorAdd = true;
                    Color col = ct.add;
                    addKeyframe(ar, frame, frameRate, col.r);
                    addKeyframe(ag, frame, frameRate, col.g);
                    addKeyframe(ab, frame, frameRate, col.b);
                    addKeyframe(aa, frame, frameRate, col.a);
                }
            }

            public void animateEnable(int frame, float frameRate, bool enable) {
                addKeyframe(enabled, frame, frameRate, enable ? 1 : 0, false);
            }

            private void addKeyframe(List<Keyframe> kfs, int frame, float frameRate, float value, bool interp = true) {
                // find previous frame index
                //   if none, add
                //   if found, check how many frames ago
                //     if last frame, add interpolate
                //     if not last frame, duplicate that to be last frame, add this frame interpolate

                var time = (frame - 1) / frameRate;
                if (kfs.Count == 0) {
                    Keyframe kf = new(time, value);
                    kfs.Add(kf);
                    return;
                }

                // if setting something on frame one
                if (frame == 1) {
                    if (kfs.Count > 1) {
                        Debug.LogError("There should not be more than one frame on the first frame");
                        return;
                    }

                    var kf = kfs[0];
                    kf.value = value;
                    kfs[0] = kf;
                    return;
                }

                // TODO: check if there is already a KF for this frame? (would this be needed now that i fixed the timing?)

                var last = kfs[^1];
                int lastFrameIdx = (int)Math.Round(last.time * frameRate) + 1;

                // check for the gap that we need to fil
                if (lastFrameIdx != frame - 1) {
                    var lastTime = (frame - 2) / frameRate;
                    last = new(lastTime, last.value);
                    kfs.Add(last);
                }

                if (interp) {
                    var lastVal = last.value;
                    var interpolate = (value -  lastVal) / (time - last.time);
                    last = new Keyframe(last.time, last.value, last.inTangent, interpolate);
                    var thisF = new Keyframe(time, value, interpolate, 0f);
                    kfs[^1] = last;
                    kfs.Add(thisF);
                } else {
                    kfs.Add(new Keyframe(time, value));
                }
            }

            public void applyToAnim(AnimationClip ac) {
                if (enabled.Count != 0) {
                    ac.SetCurve(path, typeof(GameObject), "m_IsActive", new AnimationCurve(enabled.ToArray()));
                }

                // Matrix Props
                if (xpos.Count != 0) {
                    ac.SetCurve(path, typeof(RectTransform), "m_AnchoredPosition.x", new AnimationCurve(xpos.ToArray()));
                }
                if (ypos.Count != 0) {
                    ac.SetCurve(path, typeof(RectTransform), "m_AnchoredPosition.y", new AnimationCurve(ypos.ToArray()));
                }
                if (xscale.Count != 0) {
                    ac.SetCurve(path, typeof(RectTransform), "localScale.x", new AnimationCurve(xscale.ToArray()));
                }
                if (yscale.Count != 0) {
                    ac.SetCurve(path, typeof(RectTransform), "localScale.y", new AnimationCurve(yscale.ToArray()));
                }
                if (zrot.Count != 0) {
                    ac.SetCurve(path, typeof(AnimatedRuntimeObject), "zRot", new AnimationCurve(zrot.ToArray()));
                }

                // color props
                if (hasAnimatedColor) {
                    if (hasm.Count != 0) {
                        ac.SetCurve(path, typeof(AnimatedRuntimeObject), "hasMult", new AnimationCurve(hasm.ToArray()));
                    }
                    if (mr.Count != 0) {
                        ac.SetCurve(path, typeof(AnimatedRuntimeObject), "multColor.r", new AnimationCurve(mr.ToArray()));
                    }
                    if (mg.Count != 0) {
                        ac.SetCurve(path, typeof(AnimatedRuntimeObject), "multColor.g", new AnimationCurve(mg.ToArray()));
                    }
                    if (mb.Count != 0) {
                        ac.SetCurve(path, typeof(AnimatedRuntimeObject), "multColor.b", new AnimationCurve(mb.ToArray()));
                    }
                    if (ma.Count != 0) {
                        ac.SetCurve(path, typeof(AnimatedRuntimeObject), "multColor.a", new AnimationCurve(ma.ToArray()));
                    }
                    
                    if (hasa.Count != 0) {
                        ac.SetCurve(path, typeof(AnimatedRuntimeObject), "hasAdd", new AnimationCurve(hasa.ToArray()));
                    }
                    if (ar.Count != 0) {
                        ac.SetCurve(path, typeof(AnimatedRuntimeObject), "addColor.r", new AnimationCurve(ar.ToArray()));
                    }
                    if (ag.Count != 0) {
                        ac.SetCurve(path, typeof(AnimatedRuntimeObject), "addColor.g", new AnimationCurve(ag.ToArray()));
                    }
                    if (ab.Count != 0) {
                        ac.SetCurve(path, typeof(AnimatedRuntimeObject), "addColor.b", new AnimationCurve(ab.ToArray()));
                    }
                    if (aa.Count != 0) {
                        ac.SetCurve(path, typeof(AnimatedRuntimeObject), "addColor.a", new AnimationCurve(aa.ToArray()));
                    }
                }
            }
        }

        private abstract class AnimatedThing {
            public abstract int Start { get; set; }
            public abstract int End { get; set; }
            public abstract string Path { get; set; }

            public bool isDesiredObj(int frameIndex) {
                if (frameIndex >= End) {
                    return false;
                }

                return frameIndex >= Start;
            }
        }
    }
}
