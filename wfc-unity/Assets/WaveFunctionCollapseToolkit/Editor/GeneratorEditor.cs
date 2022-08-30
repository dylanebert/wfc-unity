using System;
using UnityEditor;
using UnityEngine;
using Wave;

namespace WaveEditor {
    [CustomEditor(typeof(Generator))]
    public class GeneratorEditor : Editor {
        Generator generator => target as Generator;

        WaveGridEditor[] waveGridEditors;

        public override void OnInspectorGUI() {
            CheckWaveGridEditors();
            SerializedProperty grids = serializedObject.FindProperty("grids");
            for(int i = 0; i < waveGridEditors.Length; i++) {
                SerializedProperty grid = grids.GetArrayElementAtIndex(i);
                EditorGUILayout.LabelField(waveGridEditors[i].target.name, EditorStyles.boldLabel);
                waveGridEditors[i].OnInspectorGUI(grid);
                EditorGUILayout.Space();
            }

            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Add Graph")) {
                Array.Resize(ref generator.grids, generator.grids.Length + 1);
                generator.grids[generator.grids.Length - 1] = new WaveGrid();
            }
            if(waveGridEditors.Length > 0) {
                if(GUILayout.Button("Remove Graph")) {
                    waveGridEditors[generator.grids.Length - 1].target.Clear();
                    Array.Resize(ref generator.grids, generator.grids.Length - 1);
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("loggingLevel"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("drawGizmos"));

            generator.delay = EditorGUILayout.Slider("Delay", generator.delay, 0f, 1f);

            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Generate")) {
                for(int i = 0; i < waveGridEditors.Length; i++)
                    waveGridEditors[i].target.Generate();
            }
            if(GUILayout.Button("Clear")) {
                for(int i = 0; i < waveGridEditors.Length; i++)
                    waveGridEditors[i].target.Clear();
            }
            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        void CheckWaveGridEditors() {
            if(waveGridEditors == null || waveGridEditors.Length != generator.grids.Length) {
                waveGridEditors = new WaveGridEditor[generator.grids.Length];

                for(int i = 0; i < waveGridEditors.Length; i++)
                    waveGridEditors[i] = new WaveGridEditor(generator.grids[i]);
            }
        }
    }
}