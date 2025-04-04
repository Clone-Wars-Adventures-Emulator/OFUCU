using CWAEmu.OFUCU.Data;
using CWAEmu.OFUCU.Flash;
using CWAEmu.OFUCU.Flash.Tags;
using CWAEmu.OFUCU.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.UI;

namespace CWAEmu.OFUCU {
    public class OFUCUSWF : MonoBehaviour {
        [SerializeField]
        private AnimationClip emptyClip;

        [SerializeField]
        private RectTransform vfswfhT;
        [SerializeField]
        private RectTransform dictonaryT;

        [SerializeField]
        private SWFFile file;
        [SerializeField]
        private string unityRoot;
        private bool placeDict = true;

        [SerializeField]
        private HashSet<int> dependencies = new();
        [SerializeField]
        private string prefabDir;
        [SerializeField]
        private string matDir;

        public Dictionary<int, OFUCUSprite> sprites = new();
        public Dictionary<int, OFUCUText> texts = new();
        public Dictionary<int, Font> fontMap = new();
        public Dictionary<int, OFUCUButton2> buttons = new();
        public HashSet<int> svgIds = new();

        public static void placeNewSWFFile(SWFFile file, string unityRoot, bool placeDict, Dictionary<int, Font> fontMap) {
            if (!Directory.Exists(unityRoot)) {
                Debug.LogError($"Input/Output root directory '{unityRoot}' does not exist.");
                return;
            }

            GameObject go = new($"SWFRoot-{file.Name}");
            OFUCUSWF swf = go.AddComponent<OFUCUSWF>();
            swf.unityRoot = unityRoot;
            swf.file = file;
            swf.fontMap = fontMap;
            swf.placeDict = placeDict;
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

            // calculate dependencies
            foreach (var f in file.Frames) {
                foreach (var t in f.Tags) {
                    if (t is PlaceObject2 po2 && po2.HasCharacter) {
                        dependencies.Add(po2.CharacterId);
                    }
                }
            }

            prefabDir = $"{unityRoot}/prefabs";
            matDir = $"{unityRoot}/materials";

            if (!placeDict) {
                return;
            }

            GameObject dictonaryRoot = new($"Dictonary", typeof(RectTransform));
            dictonaryT = dictonaryRoot.transform as RectTransform;
            dictonaryT.SetParent(canvas.transform, false);
            if (!Directory.Exists(prefabDir)) {
                Directory.CreateDirectory(prefabDir);
            }

            if (!Directory.Exists(matDir)) {
                Directory.CreateDirectory(matDir);
            }

            foreach (var pair in file.Sprites) {
                var name = $"Sprite.{pair.Value.CharacterId}";

                GameObject go;
                if (File.Exists($"{prefabDir}/{name}.prefab")) {
                    GameObject pgo = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabDir}/{name}.prefab");
                    go = (GameObject) PrefabUtility.InstantiatePrefab(pgo);
                } else {
                    go = new(name, typeof(OFUCUSprite));
                }

                RectTransform rt = go.transform as RectTransform;
                rt.SetParent(dictonaryT, false);
                var sprite = go.GetComponent<OFUCUSprite>();
                sprite.init(this, pair.Value, prefabDir, matDir);
                sprites.Add(pair.Key, sprite);
            }

            foreach (var pair in file.EditTexts) {
                var name = $"EditText.{pair.Value.CharacterId}";

                GameObject go;
                if (File.Exists($"{prefabDir}/{name}.prefab")) {
                    GameObject pgo = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabDir}/{name}.prefab");
                    go = (GameObject) PrefabUtility.InstantiatePrefab(pgo);
                } else {
                    go = new(name, typeof(OFUCUText));
                }

                RectTransform rt = go.transform as RectTransform;
                rt.SetParent(dictonaryT, false);
                var text = go.GetComponent<OFUCUText>();
                text.init(this, pair.Value, prefabDir, matDir);
                texts.Add(pair.Key, text);
            }

            foreach (var pair in file.Texts) {
                var name = $"Text.{pair.Value.CharacterId}";

                GameObject go;
                if (File.Exists($"{prefabDir}/{name}.prefab")) {
                    GameObject pgo = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabDir}/{name}.prefab");
                    go = (GameObject) PrefabUtility.InstantiatePrefab(pgo);
                } else {
                    go = new(name, typeof(OFUCUText));
                }

                RectTransform rt = go.transform as RectTransform;
                rt.SetParent(dictonaryT, false);
                var text = go.GetComponent<OFUCUText>();
                text.init(file, this, pair.Value, prefabDir, matDir);
                texts.Add(pair.Key, text);
            }

            foreach (var pair in file.Button2s) {
                var name = $"Button2.{pair.Value.CharacterId}";

                GameObject go;
                if (File.Exists($"{prefabDir}/{name}.prefab")) {
                    GameObject pgo = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabDir}/{name}.prefab");
                    go = (GameObject) PrefabUtility.InstantiatePrefab(pgo);
                } else {
                    go = new(name, typeof(OFUCUButton2));
                }

                RectTransform rt = go.transform as RectTransform;
                rt.SetParent(dictonaryT, false);
                var btn = go.GetComponent<OFUCUButton2>();
                btn.init(this, pair.Value, prefabDir, matDir);
                buttons.Add(pair.Key, btn);
            }
        }

        public void placeFrames(RectTransform root, List<Frame> frames, HashSet<int> dependencies = null, bool anchorTopLeft = false, bool missingIsError = true) {
            if (dependencies != null) {
                foreach (int i in dependencies) {
                    if (sprites.TryGetValue(i, out var sprite) && !sprite.Filled) {
                        sprite.place(forceDeps: true);
                    }
                }
            }

            DisplayList dl = new(frames);

            foreach (var df in dl.frames) {
                string name = df.label ?? $"Frame {df.frameIndex}";

                // create frame object (only when there is more than one frame)
                RectTransform frameRt = root;
                if (dl.frames.Length > 1) {
                    GameObject frameGo = new(name, typeof(RectTransform));
                    frameRt = (RectTransform) frameGo.transform;
                    frameRt.SetParent(root, false);
                }

                RectTransform maskTrans = null;
                int maskDepth = -1;
                int curDepth = 0;

                foreach (var obj in df.states.Values.OrderBy(o => o.depth)) {
                    curDepth = obj.depth;
                    if (maskDepth > curDepth && maskTrans != null) {
                        maskTrans = null;
                    }

                    // create object (can be extracted)
                    var (go, aoo, ro) = createObjectReference(frameRt, obj.charId, missingIsError);

                    if (go == null) {
                        Debug.LogWarning($"Skipping missing dependency {obj.charId} of {root.name}");
                        continue;
                    }

                    ro.initReferences();

                    // handle all the funnies
                    RectTransform goRt = (RectTransform) go.transform;
                    goRt.SetParent(maskTrans ?? frameRt, false);

                    if (anchorTopLeft) {
                        goRt.anchorMax = new(0, 1);
                        goRt.anchorMin = new(0, 1);
                    }

                    // handle matrix (can be extracted?? (prob not, considering anim needs to do its own thing))
                    var (translate, scale, rotz) = obj.matrix.getTransformation();
                    goRt.anchoredPosition = translate;
                    goRt.localScale = scale.ToVector3(1);
                    goRt.rotation = Quaternion.Euler(0, 0, rotz);

                    // TODO: future fix: support sprites as masks
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
                        go.name = obj.name;
                    }

                    if (obj.hasBlendMode) {
                        aoo.setBlendMode(obj.blendMode, $"{unityRoot}/materials", goRt.parent.name);
                    }

                    if (obj.depth < maskDepth) {
                        // TODO: handle this being marked Obsolete
                        go.AddComponent<RuntimeAnchor>();
                    }
                }
            }

            // delete all RuntimeRoots in the children of this
            var rrs = root.GetComponentsInChildren<RuntimeRoot>(true);
            foreach (var r in rrs) {
                DestroyImmediate(r);
            }

            var rr = root.gameObject.AddComponent<RuntimeRoot>();
            rr.canvasScalar = transform as RectTransform;
        }

        public void animateFrames(RectTransform root, List<Frame> frames) {
            void animateDelegate(bool labelsAsClips, List<int> indicies, bool animationsLoop, bool playOnAwake, bool includeEmptyTrail) {
                onAnimateButton(root, frames, labelsAsClips, indicies, animationsLoop, playOnAwake, includeEmptyTrail);
            }
            AnimateFramesWindow.onPress = animateDelegate;
            EditorWindow.GetWindow<AnimateFramesWindow>($"Animate {root.name}");
        }

        private void onAnimateButton(RectTransform root, List<Frame> frames, bool labelsAsClips, List<int> clipIndexes, bool animationsLoop, bool playOnAwake, bool includeEmptyTrail) {
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

            // object creation vars
            DisplayList dl = new(frames);
            AnimatedThingList<AnimatedFrameObject> objs = new();
            Dictionary<int, (int start, int end, RectTransform rt, string path)> masks = new();

            // depth fixing vars
            Dictionary<int, List<GameObject>> sortableObjectByDepth = new();

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
                    var objDesc = f.states[depth];
                    var (go, aoo, _) = createObjectReference(root, objDesc.charId);
                    var aro = go.AddComponent<AnimatedRuntimeObject>();
                    var goRt = go.transform as RectTransform;

                    if (objDesc.hasName) {
                        go.name = objDesc.name;
                    }

                    // deconflict the names and add some "debug" info to it as well
                    go.name = $"{go.name}.{depth}.{f.frameIndex}";

                    go.SetActive(false);
                    string objPath = go.name;

                    // if this object is a mask
                    if (f.states.TryGetValue(depth, out var o) && o.hasClipDepth) {
                        string path = go.name;
                        GameObject target = go;
                        if (aoo is OFUCUSprite) {
                            if (go.transform.childCount != 1) {
                                Debug.LogError($"Cannot process mask for {objDesc.name}.{objDesc.depth} on frame {f.frameIndex} (its a sprite with more than one child), will not animate");
                                return;
                            }

                            var child = go.transform.GetChild(0);
                            target = child.gameObject;
                            path = $"{path}/{child.name}";
                        }

                        masks.Add(depth, (depth, o.clipDepth, target.transform as RectTransform, path));
                        var mask = target.AddComponent<Mask>();
                        mask.showMaskGraphic = false;

                        // loop through my depth to my clip depth to see if i am masking anything that already exists
                        // this will exclude things removed this frame thankfully, as thats handled by the above removed loop removing the object
                        for (int curDepth = o.depth + 1; curDepth <= o.clipDepth; curDepth++) {
                            if (objs.tryGetObject(curDepth, f.frameIndex, out var maskedAFO)) {
                                var lastFrameIndexZeroBased = f.frameIndex - 2;
                                if (lastFrameIndexZeroBased < 0) {
                                    Debug.LogWarning(
                                        $"Something Fishy is going on... adding a mask of an object we already have an AFO for on last frame, but last frame index is {f.frameIndex}." +
                                        $"There might be an error following this warning.");
                                }

                                var lastFrame = dl.frames[lastFrameIndexZeroBased];
                                var maskedObjDescLastFrame = lastFrame.states[curDepth];

                                // when ever a new mask is encountered, duplicate the masked object and add its path to a list of paths
                                // only duplicate the object in the hirearchy, dont make a new AFO for it, as it doesnt actually have different data
                                maskedAFO.allPaths.Add($"{path}/{maskedAFO.go.name}");
                                var (maskedGoClone, maskedAoo, _) = createObjectReference(goRt, maskedObjDescLastFrame.charId);

                                // make sure we set the right property data
                                maskedGoClone.name = maskedAFO.go.name;
                                if (maskedObjDescLastFrame.hasBlendMode) {
                                    maskedAoo.setBlendMode(maskedObjDescLastFrame.blendMode, $"{unityRoot}/materials", maskedGoClone.transform.parent.name);
                                }

                                // check to see if they have an aaro, this sets up the aaro and the reference properly
                                if (!maskedGoClone.TryGetComponent<AnchoredAnimatedRuntimeObject>(out var _)) {
                                    // if not, try to get the aro, remove it, then add an aaro
                                    if (maskedGoClone.TryGetComponent<AnimatedRuntimeObject>(out var maskedAro)) {
                                        DestroyImmediate(maskedAro);
                                    }
                                    var aaro = maskedGoClone.AddComponent<AnchoredAnimatedRuntimeObject>();
                                    aaro.anchorReference = maskedGoClone.transform.parent as RectTransform;
                                }
                            }
                        }
                    }

                    // check to see if this object is getting masked
                    int maskedByCount = 0;
                    foreach (var (start, end, rt, path) in masks.Values) {
                        if (start < depth && depth <= end) {
                            // set the initial positions because they need to be saved
                            var (translate, scale, rotz) = objDesc.matrix.getTransformation();
                            goRt.anchoredPosition = translate;
                            goRt.localScale = scale.ToVector3(1);
                            goRt.rotation = Quaternion.Euler(0, 0, rotz);
                            go.transform.SetParent(rt, true);
                            objPath = $"{path}/{objPath}";

                            // Handle Anchoring
                            DestroyImmediate(aro);
                            aro = go.AddComponent<AnchoredAnimatedRuntimeObject>();
                            var aaro = (AnchoredAnimatedRuntimeObject) aro;
                            aaro.anchorReference = root;

                            maskedByCount++;
                        }
                    }

                    // we arent being masked, we can be sorted (TODO: sort masked children or something)
                    if (maskedByCount == 0) {
                        if (!sortableObjectByDepth.TryGetValue(depth, out var list)) {
                            list = new();
                            sortableObjectByDepth.Add(depth, list);
                        }
                        list.Add(go);
                    }

                    if (objDesc.hasBlendMode) {
                        aoo.setBlendMode(objDesc.blendMode, $"{unityRoot}/materials", go.transform.parent.name);
                    }

                    var afo = new AnimatedFrameObject() {
                        start = f.frameIndex,
                        go = go,
                        path = objPath,
                        masked = maskedByCount != 0,
                    };

                    afo.allPaths.Add(objPath);

                    objs.addAtDepth(depth, afo);
                }
            }

            // fix depth sorting that got borked as a result of the way we load stuff
            var sortedDepths = sortableObjectByDepth.Keys.ToArray();
            Array.Sort(sortedDepths);
            int siblingIndex = 0;

            foreach (var depth in sortedDepths) {
                var toSort = sortableObjectByDepth[depth];
                foreach (var go in toSort) {
                    go.transform.SetSiblingIndex(siblingIndex++);
                }
            }

            // delete all RuntimeRoots in the children of this
            var rrs = root.GetComponentsInChildren<RuntimeRoot>(true);
            foreach (var r in rrs) {
                DestroyImmediate(r);
            }

            var rr = root.gameObject.AddComponent<RuntimeRoot>();
            rr.canvasScalar = transform as RectTransform;

            List<AnimationClip> clips = new();
            foreach (var clipDef in clipDefs) {
                var clip = animateImpl(dl, objs, clipDef.start, clipDef.end, includeEmptyTrail, $"{root.name}.{clipDef.name}");
                clips.Add(clip);
                if (animationsLoop) {
                    var settings = AnimationUtility.GetAnimationClipSettings(clip);
                    settings.loopTime = true;
                    AnimationUtility.SetAnimationClipSettings(clip, settings);
                }
            }

            var rootSM = controller.layers[0].stateMachine;
            if (!animationsLoop && !playOnAwake) {
                rootSM.AddState("Empty loop").motion = emptyClip;
            }

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

        private AnimationClip animateImpl(DisplayList dl, AnimatedThingList<AnimatedFrameObject> objs, int start, int end, bool includeEmpty, string clipname = "default") {
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
                int clipFrameIndex = i - start + 1;

                foreach (var remove in f.objectsRemoved) {
                    if (animData.tryGetObject(remove, i - 1, out var anim)) {
                        anim.animateEnable(clipFrameIndex, file.FrameRate, false);
                    }
                }

                foreach (var add in f.objectsAdded) {
                    if (!animData.tryGetObject(add, i, out var anim)) {
                        Debug.LogError($"There is an object being added at frame {i}@{add} that does not have AnimData.");
                        continue;
                    }

                    anim.animateEnable(clipFrameIndex, file.FrameRate, true);

                    var state = f.states[add];

                    if (state.hasMatrixChange) {
                        anim.animateMatrix(clipFrameIndex, file.FrameRate, state.matrix);
                    }

                    if (state.hasColor) {
                        anim.animateColor(clipFrameIndex, file.FrameRate, state.color);
                    }
                }

                foreach (var change in f.changes.Values) {
                    AnimationData ad = animData.getObject(change.depth, i);
                    if (ad == null) {
                        Debug.LogWarning($"There is a change at frame {i}@{change.depth} that does not have AnimData.");
                        continue;
                    }

                    if (change.hasMatrixChange) {
                        ad.animateMatrix(clipFrameIndex, file.FrameRate, change.matrix);
                    }

                    if (change.hasColor) {
                        ad.animateColor(clipFrameIndex, file.FrameRate, change.color);
                    }
                }
            }

            // in the first object we animate, inject a key frame at the end of the "gap" of empty frames
            if (includeEmpty) {
                var itor = animData.GetEnumerator();
                itor.MoveNext();
                var anim = itor.Current;

                anim.addEnableOnFinal(end, file.FrameRate);
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

                if (texts.ContainsKey(id)) {
                    continue;
                }

                if (sprites.TryGetValue(id, out var sprite) && (sprite.Filled || sprite.HasPrefab)) {
                    continue;
                }

                // the dictionary may not be loaded, so check the files for prefabs
                if (File.Exists($"{prefabDir}/Sprite.{id}.prefab")) {
                    continue;
                }

                if (File.Exists($"{prefabDir}/EditText.{id}.prefab")) {
                    continue;
                }

                return id;
            }

            return 0;
        }

        public (GameObject go, AbstractOFUCUObject aoo, RuntimeObject ro) createObjectReference(RectTransform parent, int charId, bool missingIsError = true) {
            GameObject go = null;
            AbstractOFUCUObject aoo = null;
            RuntimeObject ro = null;
            if (sprites.TryGetValue(charId, out var sprite)) {
                go = sprite.getCopy();
                go.transform.SetParent(parent, false);
                aoo = go.GetComponent<AbstractOFUCUObject>();
                if (Settings.Instance.EnhancedLogging) {
                    Debug.Log($"Found {charId} as sprite in dictionary");
                }
            }

            if (aoo == null && File.Exists($"{prefabDir}/Sprite.{charId}.prefab")) {
                GameObject pgo = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabDir}/Sprite.{charId}.prefab");
                go = (GameObject) PrefabUtility.InstantiatePrefab(pgo);
                go.transform.SetParent(parent, false);
                aoo = go.GetComponent<AbstractOFUCUObject>();
                if (Settings.Instance.EnhancedLogging) {
                    Debug.Log($"Found {charId} as sprite prefab");
                }
            }

            if (aoo == null && texts.TryGetValue(charId, out var text)) {
                go = text.getCopy();
                go.transform.SetParent(parent, false);
                aoo = go.GetComponent<AbstractOFUCUObject>();
                if (Settings.Instance.EnhancedLogging) {
                    Debug.Log($"Found {charId} as text in dictionary");
                }
            }

            if (aoo == null && File.Exists($"{prefabDir}/EditText.{charId}.prefab")) {
                GameObject pgo = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabDir}/EditText.{charId}.prefab");
                go = (GameObject) PrefabUtility.InstantiatePrefab(pgo);
                go.transform.SetParent(parent, false);
                aoo = go.GetComponent<AbstractOFUCUObject>();
                if (Settings.Instance.EnhancedLogging) {
                    Debug.Log($"Found {charId} as text prefab");
                }
            }

            if (aoo == null && buttons.TryGetValue(charId, out var button)) {
                go = button.getCopy();
                go.transform.SetParent(parent, false);
                aoo = go.GetComponent<AbstractOFUCUObject>();
                if (Settings.Instance.EnhancedLogging) {
                    Debug.Log($"Found {charId} as button in dictionary");
                }
            }

            if (aoo == null && File.Exists($"{prefabDir}/Button2.{charId}.prefab")) {
                GameObject pgo = AssetDatabase.LoadAssetAtPath<GameObject>($"{prefabDir}/Button2.{charId}.prefab");
                go = (GameObject) PrefabUtility.InstantiatePrefab(pgo);
                go.transform.SetParent(parent, false);
                aoo = go.GetComponent<AbstractOFUCUObject>();
                if (Settings.Instance.EnhancedLogging) {
                    Debug.Log($"Found {charId} as button prefab");
                }
            }

            if (aoo == null) {
                string svg = $"{unityRoot}/shapes/{charId}.svg";
                if (Settings.Instance.EnhancedLogging) {
                    Debug.Log($"Looking for {charId} as shape at {svg}");
                }
                GameObject prefabGo = AssetDatabase.LoadAssetAtPath<GameObject>(svg);
                if (prefabGo == null) {
                    if (missingIsError) {
                        Debug.LogError($"Failed to find svg file {charId}");
                    }
                } else {
                    go = (GameObject) PrefabUtility.InstantiatePrefab(prefabGo, parent);
                    aoo = go.AddComponent<OFUCUShape>();
                }
            }

            if (aoo == null) {
                Debug.LogWarning($"Not placing {charId}, not shape or sprite");
            } else {
                ro = go.GetComponent<RuntimeObject>();
                go.name = go.name.Replace("(Clone)", "");
            }

            return (go, aoo, ro);
        }

        public void placeSwf(bool ignoreMissing = false) {
            // check if dependencies are filled, if not, dont do this
            var dep = allSpritesFilled(dependencies);
            if (dep != 0 && !ignoreMissing) {
                Debug.LogError($"Not placing swf, sprite {dep} is not filled.");
                return;
            }

            placeFrames(vfswfhT, file.Frames, anchorTopLeft: true);
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

        public void saveAsPrefab() {
            // TODO: the implementations of these methods across all instances are wrong. They should check for the asset and use that as the condition
            // This would prevent needing to reload all the time....
            string path = $"{unityRoot}/{name}.prefab";
            if (File.Exists(path)) {
                // TODO: do i override it?
                Debug.LogWarning($"{name} already has a prefab at path {path}, modify that directly");
                return;
            }

            PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, path, InteractionMode.AutomatedAction);
        }
    }

    [Serializable]
    public class FontMapping {
        public int fontId;
        public Font font;
    }
}
