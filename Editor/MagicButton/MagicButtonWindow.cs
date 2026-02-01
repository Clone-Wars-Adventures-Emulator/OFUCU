using System;
using UnityEditor;
using UnityEngine;

namespace CWAEmu.OFUCU.MagicButton {
    public class MagicButtonWindow : EditorWindow {
        private Vector2 scrollPos = new();
        private SwfAnalysis analysis;
        private Func<OFUCUSWF> swfCreate;


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
                GUILayout.Space(5);
                return;
            }

            scrollPos = GUILayout.BeginScrollView(scrollPos);

            GUILayout.Space(5);

            GUILayout.Label($"{analysis.swfName}");

            guiFor(analysis.swfData);

            foreach (var analyzed in analysis.spriteData.Values) {
                guiFor(analyzed);
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            bool applyAnalysis = GUILayout.Button("Apply");
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.EndScrollView();

            if (applyAnalysis) {
                // TODO: apply the analysis after it was modified
                // for now log it

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
