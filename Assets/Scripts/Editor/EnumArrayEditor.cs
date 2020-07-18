using System;
using UnityEditor;
using UnityEngine;

namespace Editor {
	[CustomPropertyDrawer(typeof(EnumArrayAttribute), true)]
	public class EnumArrayEditor: PropertyDrawer {
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
			EnumArrayAttribute enumArrayAttribute = attribute as EnumArrayAttribute;
			string[] names = Enum.GetNames(enumArrayAttribute.EnumType);

			SerializedProperty valuesProperty = property.FindPropertyRelative("Values");

			while (valuesProperty.arraySize != names.Length) {
				valuesProperty.InsertArrayElementAtIndex(valuesProperty.arraySize);
				SerializedProperty prop = valuesProperty.GetArrayElementAtIndex(valuesProperty.arraySize - 1);
				prop.floatValue = enumArrayAttribute.Exception == (valuesProperty.arraySize-1) ? enumArrayAttribute.ExceptionValue : enumArrayAttribute.DefaultValue;
				prop.serializedObject.ApplyModifiedProperties();
			}

			//position.y = 55;
			position.height = EditorGUIUtility.singleLineHeight;
			EditorGUI.LabelField(position, enumArrayAttribute.EnumLabel, EditorStyles.boldLabel);
			
			EditorGUI.BeginChangeCheck();
			for (int i = 0; i < valuesProperty.arraySize; i++) {
				string name = names[i];
				position.x = 35;
				position.y += position.height;
				position.width = Screen.width / 2f;
				EditorGUI.PrefixLabel(position, new GUIContent(name));
				
				position.x += position.width;
				SerializedProperty floatAtIndex = valuesProperty.GetArrayElementAtIndex(i);
				floatAtIndex.floatValue = EditorGUI.FloatField(position, floatAtIndex.floatValue);
			}

			if (EditorGUI.EndChangeCheck()) {
				property.serializedObject.ApplyModifiedProperties();
			}
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			EnumArrayAttribute enumArrayAttribute = attribute as EnumArrayAttribute;
			string[] names = Enum.GetNames(enumArrayAttribute.EnumType);
			return names.Length * (EditorGUIUtility.singleLineHeight + 8);
		}
	}
}