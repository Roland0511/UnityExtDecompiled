using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[CustomEditor(typeof(ActivationTrack))]
	internal class ActivationTrackInspector : TrackAssetInspector
	{
		private static class Styles
		{
			public static readonly GUIContent PostPlaybackStateText = EditorGUIUtility.TextContent("Post-playback state");
		}

		private SerializedProperty m_PostPlaybackProperty;

		public override void OnInspectorGUI()
		{
			base.get_serializedObject().Update();
			EditorGUI.BeginChangeCheck();
			if (this.m_PostPlaybackProperty != null)
			{
				EditorGUILayout.PropertyField(this.m_PostPlaybackProperty, ActivationTrackInspector.Styles.PostPlaybackStateText, new GUILayoutOption[0]);
			}
			if (EditorGUI.EndChangeCheck())
			{
				base.get_serializedObject().ApplyModifiedProperties();
				ActivationTrack activationTrack = base.get_target() as ActivationTrack;
				if (activationTrack != null)
				{
					activationTrack.UpdateTrackMode();
				}
			}
			base.OnInspectorGUI();
		}

		public override void OnEnable()
		{
			base.OnEnable();
			this.m_PostPlaybackProperty = base.get_serializedObject().FindProperty("m_PostPlaybackState");
		}
	}
}
