using CWAEmu.OFUCU.Data;
using CWAEmu.OFUCU.Flash;
using CWAEmu.OFUCU.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CWAEmu.OFUCU {
    public class AnimatedThingList<T> : IEnumerable<T> where T : AnimatedThing {
        private readonly Dictionary<int, List<T>> objs = new();

        public void initFromOther<V>(AnimatedThingList<V> other) where V : AnimatedThing {
            foreach (var pair in other.objs) {
                var list = new List<T>();
                objs.Add(pair.Key, list);

                foreach (var v in pair.Value) {
                    var t = (T) Activator.CreateInstance(typeof(T));
                    t.Start = v.Start;
                    t.End = v.End;
                    t.Path = v.Path;
                    t.Masked = v.Masked;
                    t.allPaths.Clear();
                    t.allPaths.AddRange(v.allPaths);
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

    public class AnimatedFrameObject : AnimatedThing {
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
        public override bool Masked {
            get => masked;
            set => masked = value;
        }
        public bool masked;
        public GameObject go;
    }

    public class AnimationData : AnimatedThing {
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
        public override bool Masked {
            get => masked;
            set => masked = value;
        }
        public bool masked;

        private bool hasAnimatedMatrix = false;
        private bool hasInitializedColor = false;
        private bool hasInitializedColorMult = false;
        private bool hasInitializedColorAdd = false;

        private bool animatedMultLast = false;
        private bool animatedAddLast = false;

        private readonly List<Keyframe> enabled = new();
        private readonly List<Keyframe> xpos = new();
        private readonly List<Keyframe> ypos = new();
        private readonly List<Keyframe> xscale = new();
        private readonly List<Keyframe> yscale = new();
        private readonly List<Keyframe> zrot = new();
        private readonly List<Keyframe> hasm = new();
        private readonly List<Keyframe> hasa = new();
        private readonly List<Keyframe> mr = new();
        private readonly List<Keyframe> mg = new();
        private readonly List<Keyframe> mb = new();
        private readonly List<Keyframe> ma = new();
        private readonly List<Keyframe> ar = new();
        private readonly List<Keyframe> ag = new();
        private readonly List<Keyframe> ab = new();
        private readonly List<Keyframe> aa = new();

        public void animateMatrix(int frame, float frameRate, Matrix2x3 matrix) {
            var (t, s, r) = matrix.getTransformation();
            // ensure we have some data for the first frame
            if (!hasAnimatedMatrix && frame != 1) {
                addKeyframe(xpos, 1, frameRate, t.x);
                addKeyframe(ypos, 1, frameRate, t.y);
                addKeyframe(xscale, 1, frameRate, s.x);
                addKeyframe(yscale, 1, frameRate, s.y);
                addKeyframe(zrot, 1, frameRate, r);
            }

            hasAnimatedMatrix = true;
            addKeyframe(xpos, frame, frameRate, t.x);
            addKeyframe(ypos, frame, frameRate, t.y);
            addKeyframe(xscale, frame, frameRate, s.x);
            addKeyframe(yscale, frame, frameRate, s.y);
            addKeyframe(zrot, frame, frameRate, r);
        }

        public void animateColor(int frame, float frameRate, ColorTransform ct) {
            bool animateMult = ct.hasMult || animatedMultLast;
            addKeyframe(hasm, frame, frameRate, animateMult ? 1 : 0, true);
            if (ct.hasMult) {
                Color col = ct.mult;

                // if this isnt frame one and we havent initialized this type yet, initialize frame one with defaults
                // for multiply though we want to initialize to the closest default value so the anims are smooth and dont flicker
                if (frame != 1 && !hasInitializedColorMult) {
                    addKeyframe(mr, 1, frameRate, col.r >= 0.5f ? 1 : 0);
                    addKeyframe(mg, 1, frameRate, col.g >= 0.5f ? 1 : 0);
                    addKeyframe(mb, 1, frameRate, col.b >= 0.5f ? 1 : 0);
                    addKeyframe(ma, 1, frameRate, col.a >= 0.5f ? 1 : 0);
                    hasInitializedColorMult = true;
                } else if (frame == 1) {
                    hasInitializedColorMult = true;
                }

                animatedMultLast = true;

                addKeyframe(mr, frame, frameRate, col.r);
                addKeyframe(mg, frame, frameRate, col.g);
                addKeyframe(mb, frame, frameRate, col.b);
                addKeyframe(ma, frame, frameRate, col.a);
            } else {
                if (animatedMultLast) {
                    addKeyframe(mr, frame, frameRate, 1);
                    addKeyframe(mg, frame, frameRate, 1);
                    addKeyframe(mb, frame, frameRate, 1);
                    addKeyframe(ma, frame, frameRate, 1);
                }
                animatedMultLast = false;
            }

            bool animateAdd = ct.hasAdd || animatedAddLast;
            addKeyframe(hasa, frame, frameRate, animateAdd ? 1 : 0, true);
            if (ct.hasAdd) {
                // if this isnt frame one and we havent initialized this type yet, initialize frame one with defaults
                if (frame != 1 && !hasInitializedColorAdd) {
                    addKeyframe(ar, 1, frameRate, 0);
                    addKeyframe(ag, 1, frameRate, 0);
                    addKeyframe(ab, 1, frameRate, 0);
                    addKeyframe(aa, 1, frameRate, 0);
                    hasInitializedColorAdd = true;
                } else if (frame == 1) {
                    hasInitializedColorAdd = true;
                }

                animatedAddLast = true;

                Color col = ct.add;
                addKeyframe(ar, frame, frameRate, col.r);
                addKeyframe(ag, frame, frameRate, col.g);
                addKeyframe(ab, frame, frameRate, col.b);
                addKeyframe(aa, frame, frameRate, col.a);
            } else {
                if (animatedAddLast) {
                    addKeyframe(ar, frame, frameRate, 0);
                    addKeyframe(ag, frame, frameRate, 0);
                    addKeyframe(ab, frame, frameRate, 0);
                    addKeyframe(aa, frame, frameRate, 0);
                }
                animatedAddLast = false;
            }

            // we need to do this down here at the end so that we ensure this is always the case
            if (!hasInitializedColor) {
                addKeyframe(hasm, 1, frameRate, 1, true);
                addKeyframe(hasm, 2, frameRate, 0, true);

                addKeyframe(hasa, 1, frameRate, 1, true);
                addKeyframe(hasa, 2, frameRate, 0, true);
            }

            hasInitializedColor = true;
        }

        public void animateEnable(int frame, float frameRate, bool enable) {
            addKeyframe(enabled, frame, frameRate, enable ? 1 : 0, true);
        }

        public void addEnableOnFinal(int end, float frameRate) {
            var endTime = (end - 1) / frameRate;
            bool lastEnabled = false;
            foreach (var frame in enabled) {
                if (Math.Abs(endTime - frame.time) < double.Epsilon) {
                    // found a key frame at the end point, not atting
                    return;
                }

                lastEnabled = frame.value == 1;
            }

            animateEnable(end, frameRate, lastEnabled);
        }

        private void addKeyframe(List<Keyframe> kfs, int frame, float frameRate, float value, bool hold = false) {
            // if no kfs, add
            // if kfs
            //   if there is already an entry for this frame
            //     override its values and the interpolation as needed
            //   else
            //      find insert location
            //      do interpolation if needed

            var time = (frame - 1) / frameRate;
            if (kfs.Count == 0) {
                Keyframe kf = new(time, value);
                kfs.Add(kf);
                return;
            }

            // check for overrides, and copy the value if we are overriding
            for (int i = 0; i < kfs.Count; i++) {
                if (kfs[i].time == time) {
                    var kf = kfs[i];
                    kf.value = value;

                    kfs[i] = interpolate(kf, kfs, i, hold);

                    return;
                }
            }

            // check for inserting new in between different frames
            for (int i = 1; i < kfs.Count; i++) {
                if (kfs[i - 1].time < time && time < kfs[i].time) {
                    checkNeedHoldKeyframe(kfs, i - i, time, frameRate, hold);

                    var kf = interpolate(new(time, value), kfs, i, hold);

                    kfs.Insert(i, kf);

                    return;
                }
            }

            // if we are down here, there is no previous frame time where we fit in, we need logic here
            checkNeedHoldKeyframe(kfs, kfs.Count - 1, time, frameRate, hold);

            // add this frame
            Keyframe nkf = new(time, value);
            kfs.Add(nkf);
            kfs[^1] = interpolate(nkf, kfs, kfs.Count - 1, hold);
        }

        private void checkNeedHoldKeyframe(List<Keyframe> kfs, int lastIdx, float time, float frameRate, bool hold) {
            if (lastIdx < 0) {
                return;
            }

            var last = kfs[^1];
            int lastFrame = (int) Math.Round(last.time * frameRate) + 1;
            int curFrame = (int) Math.Round(time * frameRate) + 1;

            // if the last frame isnt actually the last frame (current frame - 1), we need a new hold keyframe
            if (lastFrame != curFrame - 1) {
                var newTime = time - (1.0f / frameRate);
                Keyframe kf = new(newTime, last.value);
                kfs.Add(kf);
                kfs[^1] = interpolate(kf, kfs, kfs.Count - 1, hold);
            }
        }

        private Keyframe interpolate(Keyframe kf, List<Keyframe> kfs, int idx, bool hold) {
            // check for previous frame
            if (idx > 0) {
                var last = kfs[idx - 1];
                if (hold) {
                    // for hold, use infinity
                    last.outTangent = float.PositiveInfinity;
                    kf.inTangent = float.PositiveInfinity;
                } else {
                    // calculate the tangent
                    float iVal = (kf.value - last.value) / (kf.time - last.time);
                    last.outTangent = iVal;
                    kf.inTangent = iVal;
                }

                kfs[idx - 1] = last;
            }

            // check for next frame
            if (idx + 1 < kfs.Count) {
                var next = kfs[idx + 1];
                if (hold) {
                    // for hold, use infinity
                    kf.outTangent = float.PositiveInfinity;
                    next.inTangent = float.PositiveInfinity;
                } else {
                    // calculate the tangent
                    float ival = (kf.value - next.value) / (kf.time - next.time);
                    kf.outTangent = ival;
                    next.inTangent = ival;
                }

                kfs[idx + 1] = next;
            }

            return kf;
        }

        private void addObjectKeyFrame(List<ObjectReferenceKeyframe> kfs, int frame, float frameRate, UnityEngine.Object value) {
            // this is like add Key frame, but we dont do any interpolation and we work on ObjectReferenceKeyFrames

            var time = (frame - 1) / frameRate;
            if (kfs.Count == 0) {
                var kf = new ObjectReferenceKeyframe {
                    time = time,
                    value = value
                };
                kfs.Add(kf);
                return;
            }

            // check for overrides, copy the value if we are overriding
            for (int i = 0; i < kfs.Count; i++) {
                if (kfs[i].time == time) {
                    var kf = kfs[i];
                    kf.value = value;
                    kfs[i] = kf;
                    return;
                }
            }

            // check for inserting new in between different frames
            for (int i = 1; i < kfs.Count; i++) {
                if (kfs[i - 1].time < time && time < kfs[i].time) {
                    kfs.Insert(i, new ObjectReferenceKeyframe {
                        time = time,
                        value = value
                    });
                    return;
                }
            }

            kfs.Add(new ObjectReferenceKeyframe {
                time = time,
                value = value
            });
        }

        public void applyToAnim(AnimationClip ac) {
            if (enabled.Count != 0) {
                applyFloatCurve(ac, typeof(GameObject), "m_IsActive", enabled);
            }

            // Matrix Props
            if (xpos.Count != 0) {
                if (masked) {
                    applyFloatCurve(ac, typeof(AnchoredAnimatedRuntimeObject), "position.x", xpos);
                } else {
                    applyFloatCurve(ac, typeof(RectTransform), "m_AnchoredPosition.x", xpos);
                }
            }
            if (ypos.Count != 0) {
                if (masked) {
                    applyFloatCurve(ac, typeof(AnchoredAnimatedRuntimeObject), "position.y", ypos);
                } else {
                    applyFloatCurve(ac, typeof(RectTransform), "m_AnchoredPosition.y", ypos);
                }
            }
            if (xscale.Count != 0) {
                if (masked) {
                    applyFloatCurve(ac, typeof(AnchoredAnimatedRuntimeObject), "scale.x", xscale);
                } else {
                    applyFloatCurve(ac, typeof(RectTransform), "localScale.x", xscale);
                }
            }
            if (yscale.Count != 0) {
                if (masked) {
                    applyFloatCurve(ac, typeof(AnchoredAnimatedRuntimeObject), "scale.y", yscale);
                } else {
                    applyFloatCurve(ac, typeof(RectTransform), "localScale.y", yscale);
                }
            }
            if (zrot.Count != 0) {
                // Run a sanity check on the zRot values to make sure there are no extreme jumps that cause weird interpolations
                float offset = 0;
                float lastVal = zrot[0].value;
                for (int i = 1; i < zrot.Count; i++) {
                    float val = zrot[i].value;

                    if (Mathf.Abs(lastVal - val) > 330) {
                        if (lastVal < 0) {
                            offset -= 360;
                        } else {
                            offset += 360;
                        }

                        if (Settings.Instance.EnhancedLogging) {
                            Debug.Log($"Adjusting offset on zrot for path {path} by {offset}");
                        }
                    }

                    var lastKf = zrot[i - 1];
                    var kf = zrot[i];
                    kf.value += offset;

                    var interpolate = (kf.value - lastKf.value) / (kf.time - lastKf.time);
                    lastKf = new Keyframe(lastKf.time, lastKf.value, lastKf.inTangent, interpolate);
                    kf = new Keyframe(kf.time, kf.value, interpolate, kf.outTangent);
                    zrot[i - 1] = lastKf;
                    zrot[i] = kf;

                    lastVal = val;
                }

                applyFloatCurve(ac, typeof(AnimatedRuntimeObject), "zRot", zrot);
            }

            // color props
            if (hasInitializedColor) {
                if (hasm.Count != 0) {
                    applyFloatCurve(ac, typeof(AnimatedRuntimeObject), "hasMult", hasm);
                }
                if (mr.Count != 0) {
                    applyFloatCurve(ac, typeof(AnimatedRuntimeObject), "multColor.r", mr);
                }
                if (mg.Count != 0) {
                    applyFloatCurve(ac, typeof(AnimatedRuntimeObject), "multColor.g", mg);
                }
                if (mb.Count != 0) {
                    applyFloatCurve(ac, typeof(AnimatedRuntimeObject), "multColor.b", mb);
                }
                if (ma.Count != 0) {
                    applyFloatCurve(ac, typeof(AnimatedRuntimeObject), "multColor.a", ma);
                }

                if (hasa.Count != 0) {
                    applyFloatCurve(ac, typeof(AnimatedRuntimeObject), "hasAdd", hasa);
                }
                if (ar.Count != 0) {
                    applyFloatCurve(ac, typeof(AnimatedRuntimeObject), "addColor.r", ar);
                }
                if (ag.Count != 0) {
                    applyFloatCurve(ac, typeof(AnimatedRuntimeObject), "addColor.g", ag);
                }
                if (ab.Count != 0) {
                    applyFloatCurve(ac, typeof(AnimatedRuntimeObject), "addColor.b", ab);
                }
                if (aa.Count != 0) {
                    applyFloatCurve(ac, typeof(AnimatedRuntimeObject), "addColor.a", aa);
                }
            }
        }

        private void applyFloatCurve(AnimationClip ac, Type targetType, string propPath, List<Keyframe> kfs) {
            var arr = kfs.ToArray();

            foreach (var path in allPaths) {
                ac.SetCurve(path, targetType, propPath, new AnimationCurve(arr));
            }
        }

        private void applyObjectCurve(AnimationClip ac, Type targetType, string propPath, List<ObjectReferenceKeyframe> kfs) {
            var arr = kfs.ToArray();

            foreach (var path in allPaths) {
                var binding = EditorCurveBinding.PPtrCurve(path, typeof(AnimatedRuntimeObject), propPath);
                AnimationUtility.SetObjectReferenceCurve(ac, binding, arr);
            }
        }

    }

    public abstract class AnimatedThing {
        public abstract int Start { get; set; }
        public abstract int End { get; set; }
        public abstract string Path { get; set; }
        public abstract bool Masked { get; set; }
        public readonly List<string> allPaths = new();

        public bool isDesiredObj(int frameIndex) {
            return Start <= frameIndex && frameIndex < End;
        }
    }
}
