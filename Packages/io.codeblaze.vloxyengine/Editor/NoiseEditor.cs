using System;
using CodeBlaze.Vloxy.Engine.Noise;
using CodeBlaze.Vloxy.Engine.Settings;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace CodeBlaze.Editor {

    [CustomEditor(typeof(NoiseSettings))]
    public class NoiseEditor : UnityEditor.Editor {

        private NoiseProfile NoiseProfile;
        private Texture2D Image;

        private int PreviewScale = 10;

        private void OnEnable() {
            Image = new Texture2D(256, 256);
            
            UpdatePreview();
        }
        
        private void UpdatePreview() {
            var settings = (NoiseSettings) target;

            NoiseProfile = new NoiseProfile(new NoiseProfile.Settings {
                Height = settings.Height,
                WaterLevel = settings.WaterLevel,
                Seed = settings.Seed,
                Scale = settings.Scale,
                Lacunarity = settings.Lacunarity,
                Persistance = settings.Persistance,
                Octaves = settings.Octaves
            });

            for (var x = 0; x < Image.width; x++) {
                for (var y = 0; y < Image.height; y++) {
                    var noise = NoiseProfile.GetNoise(new int3(x, 0, y) * PreviewScale);

                    var height = ((float) noise.Height + settings.Height / 2) / settings.Height;
                    
                    Image.SetPixel(x, y, new Color(height, height, height));
                }
            }

            Image.Apply();
        }

        public override void OnInspectorGUI() {
            EditorGUILayout.LabelField("Vloxy Noise Editor");
            
            EditorGUI.BeginChangeCheck();
            
            DrawDefaultInspector();
            
            EditorGUILayout.Separator();
            EditorGUILayout.LabelField("Preview");
            
            PreviewScale = Mathf.RoundToInt(EditorGUILayout.Slider("Scale", PreviewScale, 1f, 100f));
            
            if (EditorGUI.EndChangeCheck()) UpdatePreview();
            
            EditorGUILayout.Separator();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(Image);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

    }

}