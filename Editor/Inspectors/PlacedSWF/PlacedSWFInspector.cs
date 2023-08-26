using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace CWAEmu.OFUCU {
    [CustomEditor(typeof(PlacedSWFFile))]
    public class PlacedSWFInspector : Editor {
        public VisualTreeAsset Inspector;

        private PlacedSWFFile entry;

        private void OnEnable() {
            entry = (PlacedSWFFile)target;
        }

        public override VisualElement CreateInspectorGUI() {
            VisualElement root = new();

            Inspector.CloneTree(root);

            var saveBtnEle = root.Q("save");
            if (saveBtnEle is Button saveBtn) {
                saveBtn.clicked += () => {
                    AssetPathModal.ShowModal(entry.File.Name, saveAllImaages);
                };
            }

            var fillBtnEle = root.Q("fill");
            if (fillBtnEle is Button fillBtn) {
                fillBtn.clicked += () => {
                    entry.runOnAllOfType(entry => entry.fillShape(), DictonaryEntry.EnumDictonaryCharacterType.Shape);
                };
            }

            var flattenBtnEle = root.Q("flat");
            if (flattenBtnEle is Button flattenBtn) {
                flattenBtn.clicked += () => {
                    entry.runOnAllOfType(entry => entry.flattenShape(), DictonaryEntry.EnumDictonaryCharacterType.Shape);
                };
            }

            var placeBtnEle = root.Q("place");
            if (placeBtnEle is Button placeBtn) {
                placeBtn.clicked += () => {
                    Debug.Log("C1");
                    entry.placeSWFFrames();
                };
            }

            var animBtnEle = root.Q("anim");
            if (animBtnEle is Button animBtn) {
                animBtn.clicked += () => {
                    Debug.Log("C2");
                    entry.animateSWFFrames();
                };
            }

            var placeAllBtnEle = root.Q("place-all");
            if (placeAllBtnEle is Button placeAllBtn) {
                placeAllBtn.clicked += () => {
                    Debug.Log("C3");
                    entry.runOnAllOfType(entry => entry.placeFrames(), DictonaryEntry.EnumDictonaryCharacterType.Sprite);
                };
            }

            var animAllBtnEle = root.Q("anim-all");
            if (animAllBtnEle is Button animAllBtn) {
                animAllBtn.clicked += () => {
                    Debug.Log("C4");
                    entry.runOnAllOfType(entry => entry.animateFrames(), DictonaryEntry.EnumDictonaryCharacterType.Sprite);
                };
            }

            return root;
        }

        private void saveAllImaages(string path) {
            entry.runOnAllOfType(entry => entry.saveImageToAsset(path), DictonaryEntry.EnumDictonaryCharacterType.Image);
        }
    }
}
