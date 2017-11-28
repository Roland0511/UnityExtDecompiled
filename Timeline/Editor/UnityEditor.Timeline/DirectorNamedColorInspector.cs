using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[CustomEditor(typeof(DirectorNamedColor))]
	public class DirectorNamedColorInspector : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			if (GUILayout.Button("ToTextAsset", new GUILayoutOption[0]))
			{
				DirectorStyles.Instance.ExportSkinToFile();
			}
			if (GUILayout.Button("Reload From File", new GUILayoutOption[0]))
			{
				DirectorStyles.Instance.ReloadSkin();
				Selection.set_activeObject(DirectorStyles.Instance.customSkin);
			}
		}
	}
}
