using CWAEmu.OFUCU.Flash;
using CWAEmu.OFUCU.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        private bool hasAnimatedMatrix = false;
        private bool hasAnimatedColor = false;
        private bool hasAnimatedColorMult = false;
        private bool hasAnimatedColorAdd = false;

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
            // make sure the initial values are what we want (if we are animating frame one right now, the addKeyFrame call will handle that)
            if (!hasAnimatedColor) {
                addKeyframe(hasm, 1, frameRate, 0, false);
                addKeyframe(hasa, 1, frameRate, 0, false);
            }

            hasAnimatedColor = true;

            bool animateMult = ct.hasMult || animatedMultLast;
            addKeyframe(hasm, frame, frameRate, animateMult ? 1 : 0, false);
            if (ct.hasMult) {
                Color col = ct.mult;
                if (!hasAnimatedColorMult && frame != 1) {
                    addKeyframe(mr, 1, frameRate, col.r);
                    addKeyframe(mg, 1, frameRate, col.g);
                    addKeyframe(mb, 1, frameRate, col.b);
                    addKeyframe(ma, 1, frameRate, col.a);
                }

                hasAnimatedColorMult = true;
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
            addKeyframe(hasa, frame, frameRate, animateAdd ? 1 : 0, false);
            if (ct.hasAdd) {
                Color col = ct.add;
                if (!hasAnimatedColorAdd && frame != 1) {
                    addKeyframe(ar, 1, frameRate, col.r);
                    addKeyframe(ag, 1, frameRate, col.g);
                    addKeyframe(ab, 1, frameRate, col.b);
                    addKeyframe(aa, 1, frameRate, col.a);
                }

                hasAnimatedColorAdd = true;
                animatedAddLast = true;
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
        }

        public void animateEnable(int frame, float frameRate, bool enable) {
            addKeyframe(enabled, frame, frameRate, enable ? 1 : 0, false);
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
            int lastFrameIdx = (int) Math.Round(last.time * frameRate) + 1;

            // check for the gap that we need to fil
            if (lastFrameIdx != frame - 1) {
                var lastTime = (frame - 2) / frameRate;
                last = new(lastTime, last.value);
                kfs.Add(last);
            }

            if (interp) {
                var lastVal = last.value;
                var interpolate = (value - lastVal) / (time - last.time);
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

                        Debug.Log($"Adjusting offset on zrot for path {path} by {offset}");
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

    public abstract class AnimatedThing {
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
