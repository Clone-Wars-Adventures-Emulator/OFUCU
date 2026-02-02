using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CWAEmu.OFUCU.MagicButton {
    public class MagicButtonWindow : EditorWindow {
        private Vector2 scrollPos = new();
        private SwfAnalysis analysis;
        private Func<OFUCUSWF> swfCreate;
        private OFUCUSWF swf;
        private (int level, int idx) manualWaitingOnIdx;
        private int[][] sortedIds;

        [MenuItem("OFUCU/Flash Magic Button %#&M")]
        public static void showWindow() {
            GetWindow<MagicButtonWindow>("Magic Button");
        }

        public MagicButtonWindow setCurrent(SwfAnalysis analysis, Func<OFUCUSWF> swfCreator) {
            this.analysis = analysis;
            swfCreate = swfCreator;
            return this;
        }

        private void OnGUI() {
            if (analysis == null) {
                GUILayout.Space(5);
                GUILayout.Label("No Analysis data, close this window or reuse the magic button");
                return;
            }

            scrollPos = GUILayout.BeginScrollView(scrollPos);

            GUILayout.Space(5);

            GUILayout.Label($"{analysis.swfName}");

            guiFor(analysis.swfData);

            foreach (var analyzed in analysis.spriteData.Values) {
                if (!analyzed.hasBeenPlaced) {
                    guiFor(analyzed);
                }
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            bool applyAnalysis = GUILayout.Button(manualWaitingOnIdx.idx != 0 ? $"Resume After {sortedIds[manualWaitingOnIdx.level][manualWaitingOnIdx.idx]}" : "Apply");
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.EndScrollView();

            if (applyAnalysis) {
                HashSet<string> violators = new();
                if (analysis.swfData.userSelectedType == EnumAnalyzedType.Unknown) {
                    violators.Add(analysis.swfName);
                }
                foreach (var af in analysis.spriteData.Values) {
                    if (af.userSelectedType == EnumAnalyzedType.Unknown) {
                        violators.Add(af.label);
                    }
                }
                if (violators.Count != 0) {
                    Debug.LogError($"User selection of Unknown is not allowed. {violators.Count} offenders: {string.Join(',', violators)}");
                    return;
                }

                // if the sprite hasnt been created yet, we need to create it
                if (swf == null) {
                    try {
                        // which requires that we start asset editing
                        AssetDatabase.StartAssetEditing();
                        swf = swfCreate();

                        // so we make sure the texts have been prefabbed
                        foreach (var text in swf.texts.Values) {
                            if (!text.HasPrefab) {
                                text.saveAsPrefab();
                            }
                        }

                        // and so we make sure the non sprite buttons have been prefabbed
                        foreach (var button in swf.buttons.Values) {
                            if (!button.HasPrefab) {
                                button.saveAsPrefab();
                            }
                        }

                        // then once thats done, we need to construct the dependency level dictionary to determine which sprites can be batched together
                        Dictionary<int, int> spriteIdToDepLevel = new();
                        Dictionary<int, List<int>> depLevelToSpriteId = new();
                        int heighestLevel = 0;

                        // swf contents are automatically stored in a dependents come first approach, so if we iterate though the sorted sprite char ids,
                        // we can correctly build a level based heirarchy to figure out what we can batch with what
                        int[] sortedCharacterSpriteIds = analysis.spriteData.Keys.ToArray();
                        Array.Sort(sortedCharacterSpriteIds);
                        foreach (var spriteId in sortedCharacterSpriteIds) {
                            if (!swf.sprites.TryGetValue(spriteId, out var sprite)) {
                                continue;
                            }
                            if (sprite.HasPrefab) {
                                analysis.spriteData[spriteId].hasBeenPlaced = true;
                            }

                            if (!spriteIdToDepLevel.TryGetValue(spriteId, out var myLevel)) {
                                myLevel = 0;
                            }

                            foreach (var depCharId in analysis.spriteData[spriteId].directDependencies) {
                                if (!analysis.spriteData.ContainsKey(depCharId)) {
                                    // dependency isnt a sprite, skip
                                    continue;
                                }

                                var theirLevel = spriteIdToDepLevel[depCharId];
                                if (theirLevel >= myLevel) {
                                    myLevel = theirLevel + 1;
                                }
                            }

                            spriteIdToDepLevel[spriteId] = myLevel;

                            if (!depLevelToSpriteId.TryGetValue(myLevel, out var spritesAtMyLevel)) {
                                spritesAtMyLevel = new();
                                depLevelToSpriteId[myLevel] = spritesAtMyLevel;
                            }
                            spritesAtMyLevel.Add(spriteId);

                            heighestLevel = Math.Max(heighestLevel, myLevel);
                        }

                        sortedIds = new int[heighestLevel + 1][];
                        for (int lev = 0; lev < sortedIds.Length; lev++) {
                            sortedIds[lev] = depLevelToSpriteId[lev].ToArray();
                            Array.Sort(sortedIds[lev]);
                        }
                    } catch (Exception ex) {
                        Debug.LogError($"Exception thrown when initializing magic button actions");
                        Debug.LogException(ex);
                        return;
                    } finally {
                        AssetDatabase.StopAssetEditing();
                    }
                }

                int level = manualWaitingOnIdx.level != 0 ? manualWaitingOnIdx.level : 0;
                int idx = manualWaitingOnIdx.idx != 0 ? manualWaitingOnIdx.idx : 0;
                manualWaitingOnIdx = (0, 0);
                bool hitManual = false;
                // for level = (above), while level is in bounds and we didnt hid a manual, do loop. after, increase level and reset idx
                for ( ; level < sortedIds.Length && !hitManual; level++, idx = 0) {
                    Debug.Log($"Handling level {level}");
                    try {
                        // each dependency level needs its own start and stop asset editing. If we dont, something at level 1 wont be able to get the prefab for a sprite at level 0
                        AssetDatabase.StartAssetEditing();
                        for ( ; idx < sortedIds[level].Length && !hitManual; idx++) {
                            var charId = sortedIds[level][idx];
                            Debug.Log($"handling char {charId} @ {level} {idx}");

                            if (!swf.sprites.TryGetValue(charId, out var sprite) || !analysis.spriteData.TryGetValue(charId, out var analyzed)) {
                                Debug.LogError($"Cannot find placed sprite for analyzed sprite {charId}, did something go wrong?");
                                return;
                            }
                            analyzed.hasBeenPlaced = true;

                            try {
                                switch (analyzed.userSelectedType) {
                                    case EnumAnalyzedType.Manual:
                                        manualWaitingOnIdx = (level, idx);
                                        hitManual = true;
                                        EditorUtility.DisplayDialog($"Magic Button {swf.name}",
                                            $"The manual option was selection for the sprite {charId}, please manually select one of the fill options on the sprite editor, then" +
                                            $"come back to the Magic Button Window and click Resume After",
                                            "ok");
                                        break;
                                    case EnumAnalyzedType.Place:
                                        sprite.place();
                                        break;
                                    case EnumAnalyzedType.PlacedButton:
                                        sprite.place(onlyLabled: true);
                                        break;
                                    case EnumAnalyzedType.PlaceExcludeEmpty:
                                        sprite.place(dropEmpty: true);
                                        break;
                                    case EnumAnalyzedType.Animated:
                                        sprite.automationAnimate(analyzed.userParams.labelsAsClips, analyzed.userParams.manualClipIndicies, analyzed.userParams.looping,
                                            analyzed.userParams.playOnAwke, analyzed.userParams.includeEmpty);
                                        sprite.uniquifyMaterials();
                                        break;
                                    default:
                                        Debug.LogError($"Unknown user selection {analyzed.userSelectedType} for sprite {charId}, skipping.");
                                        break;
                                }

                                if (!hitManual) {
                                    sprite.saveAsPrefab();
                                }
                            } catch (Exception e) {
                                Debug.LogError($"Failed to preform action on {analyzed.label}");
                                Debug.LogException(e);
                                // TODO: what does this failure look like?
                            }
                        }
                    } catch (Exception ex) {
                        Debug.LogError($"Exception thrown during application of magic button");
                        Debug.LogException(ex);
                        return;
                    } finally {
                        AssetDatabase.StopAssetEditing();
                    }
                }

                if (!hitManual) {
                    try {
                        swf.destroyCreatedDictionary();
                        switch (analysis.swfData.userSelectedType) {
                            case EnumAnalyzedType.Manual:
                                manualWaitingOnIdx = (level, idx);
                                hitManual = true;
                                EditorUtility.DisplayDialog($"Magic Button {swf.name}",
                                    $"The manual option was selection for the swf, please manually select one of the fill options on the swf editor, then come back to the Magic" +
                                    $"Button Window and click Resume After",
                                    "ok");
                                break;
                            case EnumAnalyzedType.Place:
                                swf.placeSwf();
                                break;
                            case EnumAnalyzedType.PlacedButton:
                                swf.placeSwf(onlyLabled: true);
                                break;
                            case EnumAnalyzedType.PlaceExcludeEmpty:
                                swf.placeSwf(dropEmpty: true);
                                break;
                            case EnumAnalyzedType.Animated:
                                swf.automationAnimSwf(analysis.swfData.userParams.labelsAsClips, analysis.swfData.userParams.manualClipIndicies, analysis.swfData.userParams.looping,
                                    analysis.swfData.userParams.playOnAwke, analysis.swfData.userParams.includeEmpty);
                                break;
                            default:
                                Debug.LogError($"Unknown user selection {analysis.swfData.userSelectedType} for swf, skipping.");
                                break;
                        }
                    } catch (Exception e) {
                        Debug.LogError($"Failed to preform action on the swf, recommend preforming manually");
                        Debug.LogException(e);
                        return;
                    }

                    if (!hitManual) {
                        swf.saveAsPrefab();
                    }
                }
            }
        }

        private void guiFor(AnalyzedFrames analyzed) {
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            GUILayout.Label(analyzed.label);
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            GUILayout.Label($"Type: {analyzed.analyzedType}", GUILayout.Width(150));
            GUILayout.Space(5);
            analyzed.userSelectedType = (EnumAnalyzedType) EditorGUILayout.EnumPopup(analyzed.userSelectedType);
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            if (analyzed.userSelectedType == EnumAnalyzedType.Animated) {
                GUILayout.BeginHorizontal();
                GUILayout.Space(5);
                analyzed.userParams.looping = GUILayout.Toggle(analyzed.userParams.looping, "Looping");
                if (analyzed.userParams.looping) {
                    analyzed.userParams.playOnAwke = true;
                }
                GUILayout.Space(5);
                analyzed.userParams.playOnAwke = GUILayout.Toggle(analyzed.userParams.playOnAwke, "Play on Awake");
                GUILayout.Space(5);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Space(5);
                analyzed.userParams.includeEmpty = GUILayout.Toggle(analyzed.userParams.includeEmpty, "Include Empty");
                GUILayout.Space(5);
                analyzed.userParams.labelsAsClips = GUILayout.Toggle(analyzed.userParams.labelsAsClips, "Frame Labels as Clips");
                GUILayout.Space(5);
                GUILayout.EndHorizontal();

                if (!analyzed.userParams.labelsAsClips) {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(5);
                    GUILayout.Label("Comma Seperated Clip Seperation Indices (1 based)");
                    GUILayout.Space(5);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(5);
                    analyzed.commaSeperatedIndicies = GUILayout.TextField(analyzed.commaSeperatedIndicies);
                    GUILayout.Space(5);
                    GUILayout.EndHorizontal();
                }
            }
        }
    }
}
