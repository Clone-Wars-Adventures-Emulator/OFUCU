using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CWAEmu.OFUCU {
    public class AnimateFramesWindow : EditorWindow {
        public static Action<bool, List<int>, bool, bool, bool> onPress;

        [SerializeField]
        private List<int> indices;
        private bool labelsAsSeps;
        [SerializeField]
        private bool animsLoop;
        [SerializeField]
        private bool playOnAwake;
        [SerializeField]
        private bool animEmpty;
        private SerializedObject so;
        private bool debounced;

        private void OnEnable() {
            debounced = false;
            var p = position;
            p.width = 600 / EditorGUIUtility.pixelsPerPoint;
            p.height = 600 / EditorGUIUtility.pixelsPerPoint;
            position = p;

            so = new(this);
        }

        private void OnGUI() {
            GUILayout.BeginArea(new Rect(0, 0, Screen.width / EditorGUIUtility.pixelsPerPoint, Screen.height / EditorGUIUtility.pixelsPerPoint));

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            animsLoop = GUILayout.Toggle(animsLoop, "Animations loop");
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            playOnAwake = GUILayout.Toggle(playOnAwake, "Play First Animation on Awake");
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            animEmpty = GUILayout.Toggle(animEmpty, "Include Empty Trailing Frames");
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            labelsAsSeps = GUILayout.Toggle(labelsAsSeps, "Frame Labels as Clips");
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            if (!labelsAsSeps) {
                GUILayout.BeginHorizontal();
                GUILayout.Space(5);
                EditorGUILayout.PropertyField(so.FindProperty("indices"), new GUIContent("Clip Seperation Indices (1 based)"));
                if (so.hasModifiedProperties) {
                    so.ApplyModifiedPropertiesWithoutUndo();
                }
                GUILayout.Space(5);
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.Space(5);
            if (GUILayout.Button("Animate")) {
                debounce();
            }
            GUILayout.Space(5);
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private void debounce() {
            if (debounced) {
                return;
            }
            debounced = true;

            bool l = labelsAsSeps;
            labelsAsSeps = false;

            Close();

            onPress.Invoke(l, indices, animsLoop, playOnAwake, animEmpty);
        }
    }
}
