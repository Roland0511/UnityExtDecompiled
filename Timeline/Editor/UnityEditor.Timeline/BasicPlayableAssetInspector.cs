using System;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class BasicPlayableAssetInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			base.get_serializedObject().Update();
			SerializedProperty iterator = base.get_serializedObject().GetIterator();
			bool flag = true;
			while (iterator.NextVisible(flag))
			{
				flag = false;
				if (!("m_Script" == iterator.get_propertyPath()))
				{
					EditorGUILayout.PropertyField(iterator, true, new GUILayoutOption[0]);
				}
			}
			base.get_serializedObject().ApplyModifiedProperties();
			EditorGUI.EndChangeCheck();
		}
	}
}
