using CWAEmu.OFUCU.Flash;
using CWAEmu.OFUCU.Flash.Tags;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace CWAEmu.OFUCU {
    public class OFUCUSWF : MonoBehaviour {
        private RectTransform vfswfhT;
        private RectTransform dictonaryT;

        private SWFFile file;
        private string svgRoot;
        
        private readonly HashSet<int> dependencies = new();

        public readonly Dictionary<int, OFUCUSprite> neoSprites = new();
        public readonly HashSet<int> svgIds = new();

        public static void placeNewSWFFile(SWFFile file, string svgRoot) {
            if (!Directory.Exists(svgRoot)) {
                Debug.LogError($"SvgRoot {svgRoot} does not exist.");
                return;
            }

            GameObject go = new($"SWF Root: {file.Name}");
            OFUCUSWF swf = go.AddComponent<OFUCUSWF>();
            swf.svgRoot = svgRoot;
            swf.file = file;
            swf.init();

            // Debug stuff, removable
            Debug.Log($"Generated {swf.dictonaryT.childCount} objects as a child of {swf.dictonaryT.name}");
            int dt = swf.dictonaryT.childCount;
            int total = 0;
            for (int i = 0; i < dt; i++) {
                var t = swf.dictonaryT.GetChild(i);
                Debug.Log($"{t.name} generated {t.childCount} direct child objects and {t.FullChildCount()} total child objects");
                total += t.FullChildCount();
            }
            Debug.Log($"Total children of dictionary objects: {total}");
        }

        private void init() {
            var files = Directory.EnumerateFiles(svgRoot, "*.svg", SearchOption.TopDirectoryOnly);
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
                GameObject go = new($"{file.Name}.Sprite.{pair.Value.CharacterId}", typeof(OFUCUSprite), typeof(RectTransform));
                RectTransform rt = go.transform as RectTransform;
                rt.SetParent(dictonaryT, false);
                var dict = go.GetComponent<OFUCUSprite>();
                dict.init(this, pair.Value);
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
                    var (go, aoo) = createObjectReference(frameRt, obj);

                    // handle all the funnies
                    RectTransform goRt = (RectTransform)go.transform;
                    goRt.SetParent(maskTrans ?? frameRt, false);

                    // handle matrix (can be extracted?? (prob not, considering anim needs to do its own thing))
                    var transform = obj.matrix.getTransformation();
                    goRt.anchoredPosition = transform.translate;
                    goRt.localScale = transform.scale.ToVector3(1);
                    goRt.rotation = Quaternion.Euler(0, 0, transform.rotz);

                    if (obj.hasClipDepth) {
                        maskDepth = obj.clipDepth;
                        maskTrans = goRt;
                        var mask = go.AddComponent<Mask>();
                        mask.showMaskGraphic = false;
                    }

                    if (obj.hasColor) {
                        var col = obj.color;
                        if (col.hasMult) {
                            aoo.setMultColor(col.mult);
                        }
                        if (col.hasAdd) {
                            aoo.setAddColor(col.add);
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
                        go.AddComponent<OFUCUAnchor>();
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

        private void onAnimateButton(RectTransform root, List<Frame> frames, string outputDir, bool labelsAsClips, List<int> clipIndexes) {
            Animator anim = root.gameObject.AddComponent<Animator>();
            var controller = AnimatorController.CreateAnimatorControllerAtPath($"{outputDir}/{root.name}.controller");
            anim.runtimeAnimatorController = controller;

            if (labelsAsClips) {
                clipIndexes = new();
                foreach (Frame f in frames) {
                    foreach (var t in f.Tags) {
                        if (t is FrameLabel && f.FrameIndex != 1) {
                            clipIndexes.Add(f.FrameIndex);
                            break;
                        }
                    }
                }
            }

            clipIndexes ??= new();
            clipIndexes.Add(frames.Count);

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
                    var (go, _) = createObjectReference(root, f.states[depth]);
                    go.AddComponent<AnimatedOFUCUObject>();

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
                            go.AddComponent<OFUCUAnchor>();
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
            int start = 1;
            // loop each index and create the appropriate clips
            foreach (int i in clipIndexes) {
                string clipName = $"Clip {start}-{i}";

                // check if the start frame has a label for the name
                if (start - 1 < frames.Count) {
                    Frame f = frames[start - 1];
                    foreach (var t in f.Tags) {
                        if (t is FrameLabel fl) {
                            clipName = fl.Label;
                        }
                    }
                }

                var clip = animateImpl(dl, objs, start, i, clipName);
                clips.Add(clip);
                // start next clip at the next frame
                start = i + 1;
            }

            var rootSM = controller.layers[0].stateMachine;
            foreach (var clip in clips) {
                var state = rootSM.AddState(clip.name);
                state.motion = clip;
            }

            try {
                AssetDatabase.StartAssetEditing();

                foreach (var clip in clips) {
                    AssetDatabase.CreateAsset(clip, $"{outputDir}/{clip.name}.clip");
                }
            } catch (Exception e) {
                Debug.LogException(e);
            } finally {
                AssetDatabase.StopAssetEditing();
            }
        }

        private AnimationClip animateImpl(DisplayList dl, AnimatedThingList<AnimatedFrameObject> objs, int start, int end, string clipname = "default") {
            AnimationClip ac = new();
            ac.name = clipname;
            ac.frameRate = file.FrameRate;

            AnimatedThingList<AnimationData> animData = new();
            animData.initFromOther(objs);

            for (int i = start + 1; i <= end; i++) {
                DisplayFrame f = dl.frames[i - 1];

                foreach (var remove in f.objectsRemoved) {
                    if (animData.tryGetObject(remove, i - 1, out var anim)) {
                        anim.animateEnable(i, file.FrameRate, false);
                    }
                }

                foreach (var add in f.objectsAdded) {
                    if (animData.tryGetObject(add, i, out var anim)) {
                        anim.animateEnable(i, file.FrameRate, true);
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
                        ad.animateMatrix(i, file.FrameRate, change.matrix);
                    }

                    if (change.hasColor) {
                        ad.animateColor(i, file.FrameRate, change.color);
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

                if (!neoSprites.TryGetValue(id, out var sprite) || !sprite.Filled) {
                    return id;
                }
            }

            return 0;
        }

        private (GameObject go, AbstractOFUCUObject aoo) createObjectReference(RectTransform parent, DisplayObject obj) {
            GameObject go = null;
            AbstractOFUCUObject aoo = null;
            if (neoSprites.TryGetValue(obj.charId, out var sprite)) {
                go = sprite.getCopy();
                aoo = go.GetComponent<AbstractOFUCUObject>();
                Debug.Log($"Found {obj.charId} as sprite");
            }

            if (aoo == null) {
                string svg = $"{svgRoot}/{obj.charId}.svg";
                Debug.Log($"Looking for {obj.charId} as shape at {svg}");
                GameObject prefabGo = AssetDatabase.LoadAssetAtPath<GameObject>(svg);
                if (prefabGo == null) {
                    Debug.LogError("Failed to find svg file");
                }

                go = (GameObject)PrefabUtility.InstantiatePrefab(prefabGo, parent);
                aoo = go.AddComponent<OFUCUShape>();
            }

            // TODO: handle text and other? objects
            if (aoo == null) {
                Debug.Log($"Not placing {obj.charId}, not shape or sprite");
            }

            return (go, aoo);
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
                        var t = default(T);
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
                // TODO: has it been more than one frame since last KF? if yes, copy that KF first so we dont fuck up state then add our self in
            }

            public void animateColor(int frame, float frameRate, ColorTransform ct) {
                // TODO: 
            }

            public void animateEnable(int frame, float frameRate, bool enable) {
                // TODO: 
            }

            private void addKeyframe(List<Keyframe> kfs, int frame, float frameRate, float value) {

            }

            // some resulable function code that will do the prev frame check and apply things

            public void applyToAnim(AnimationClip ac) {
                // TODO: apply interpolation curve rules to all KFs
            }
        }

        private abstract class AnimatedThing {
            public abstract int Start { get; set; }
            public abstract int End { get; set; }
            public abstract string Path { get; set; }

            public bool isDesiredObj(int frameIndex) {
                if (frameIndex > End) {
                    return false;
                }

                if (frameIndex < Start) {
                    return true;
                }

                return false;
            }
        }
    }
}
