using CWAEmu.OFUCU.Flash;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CWAEmu.OFUCU.MagicButton {
    public enum EnumAnalyzedType {
        Unknown,
        Manual,
        Place,
        PlacedButton,
        PlaceExcludeEmpty,
        Animated,
    }

    public class AnimationParams {
        public bool looping;
        public bool playOnAwke;
        public bool includeEmpty;
        public bool labelsAsClips;
        public List<int> manualClipIndicies;
    }

    public class SpriteDepTree {
        public int characterId;
        public readonly Dictionary<int, SpriteDepTree> children = new();
    }

    public class AnalyzedFrames {
        public EnumAnalyzedType analyzedType;
        public EnumAnalyzedType userSelectedType;
        public AnimationParams defaultParams;
        public AnimationParams userParams;
        public SpriteDepTree dependencies;

        public int frameCount;
        public string label;
        public string commaSeperatedIndicies;
    }

    public class SwfAnalysis {
        public string swfName;
        public readonly Dictionary<int, SpriteDepTree> depTree = new();
        public readonly Dictionary<int, SpriteDepTree> depFlat = new();
        public readonly Dictionary<int, AnalyzedFrames> spriteData = new();

        public AnalyzedFrames swfData;

        public static SwfAnalysis of(SWFFile swf, string unityRoot) {
            var analysis = new SwfAnalysis {
                swfName = swf.Name,
            };

            var keys = swf.Sprites.Keys.ToArray();
            Array.Sort(keys);
            foreach (var key in keys) {
                // check if this sprite already has a prefab, if so, skip the analysis
                if (File.Exists($"{unityRoot}/prefabs/Sprite.{key}.prefab")) {
                    continue;
                }

                analysis.analyze(swf.Sprites[key].Frames, swf, out var data);
                if (data.analyzedType == EnumAnalyzedType.Unknown) {
                    continue;
                }
                analysis.spriteData[key] = data;
                data.label = $"Sprite: {key}";
            }

            // oh thats so goofy, you can out into a ref.field. Common C# W?
            analysis.analyze(swf.Frames, swf, out analysis.swfData);
            analysis.swfData.label = $"Swf: {swf.Name}";

            return analysis;
        }

        private void analyze(List<Frame> frames, SWFFile swf, out AnalyzedFrames analysis) {
            var deps = new SpriteDepTree();
            analysis = new AnalyzedFrames {
                defaultParams = new(),
                userParams = new(),
                frameCount = frames.Count,
            };

            HashSet<int> foundDeps = new();

            bool hasUp = false;
            bool hasOver = false;
            bool hasDown = false;
            int labeledFrames = 0;

            foreach (var frame in frames) {
                var dispFrame = frame.asDisplayFrame();
                foreach (var charId in dispFrame.objectsAdded) {
                    foundDeps.Add(charId);
                }

                if (frame.Label != null) {
                    labeledFrames++;

                    switch (frame.Label.ToLower()) {
                        case "up":
                            hasUp = true;
                            break;
                        case "over":
                            hasOver = true;
                            break;
                        case "down":
                            hasDown = true;
                            break;
                    }
                }
            }

            // build dependency tree
            foreach (var dep in foundDeps) {
                // if there is an entry in the dep tree, remove it (cause it is no longer a root (this needs inverted)
                if (depTree.TryGetValue(dep, out var d)) {
                    depTree.Remove(dep);
                } else if (!depFlat.TryGetValue(dep, out d)) {
                    d = new SpriteDepTree() {
                        characterId = dep
                    };
                }
                deps.children.Add(dep, d);
            }
            analysis.dependencies = deps;

            // analyze the sprite to see what type it is
            // 0 frames is not allowed
            if (frames.Count == 0) {
                analysis.analyzedType = analysis.userSelectedType = EnumAnalyzedType.Unknown;
                return;
            }

            // if only one frame, its a place
            if (frames.Count == 1) {
                analysis.analyzedType = analysis.userSelectedType = EnumAnalyzedType.Place;
                return;
            }

            // if multiple frames and there is a frame labled up, over, and down, its likely a button
            if (hasUp && hasOver && hasDown) {
                analysis.analyzedType = analysis.userSelectedType = EnumAnalyzedType.PlacedButton;
                return;
            }

            // if here, its animated
            analysis.analyzedType = analysis.userSelectedType = EnumAnalyzedType.Animated;

            if (labeledFrames > 0) {
                analysis.defaultParams.labelsAsClips = true;
                analysis.userParams.labelsAsClips = true;
            }

            // TODO: try to parse the DoActions and look for any calls to goToAndStop / Stop???? idk, seems like more effort than its worth
            // even though this is the magic button

            return;
        }
    }
}
