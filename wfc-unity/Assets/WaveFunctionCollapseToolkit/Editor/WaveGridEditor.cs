using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Wave;

namespace WaveEditor {
    public class WaveGridEditor {
        public WaveGrid target { get; private set; }

        public WaveGridEditor(WaveGrid target) {
            this.target = target;
        }

        public void OnInspectorGUI(SerializedProperty property) {
            // Name
            target.name = EditorGUILayout.TextField("Name", target.name);

            // Height, width
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Width");
            target.width = EditorGUILayout.IntField(target.width);
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Height");
            target.height = EditorGUILayout.IntField(target.height);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            // Module size
            target.moduleSize = EditorGUILayout.FloatField("Module Size", target.moduleSize);

            // Offset
            EditorGUILayout.PropertyField(property.FindPropertyRelative("offset"));

            // Modules
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(property.FindPropertyRelative("modules"));
        }
    }
}