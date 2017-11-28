using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[CanEditMultipleObjects, CustomEditor(typeof(TimelineAsset))]
	internal class TimelineAssetInspector : Editor
	{
		private static class Styles
		{
			public static readonly GUIContent FrameRate = EditorGUIUtility.TextContent("Frame Rate|The frame rate at which this sequence updates");

			public static readonly GUIContent DurationMode = EditorGUIUtility.TextContent("Duration Mode|Specified how the duration of the sequence is calculated");

			public static readonly GUIContent Duration = EditorGUIUtility.TextContent("Duration|The length of the sequence");

			public static readonly GUIContent HeaderTitleMultiselection = EditorGUIUtility.TextContent("Timeline Assets");
		}

		private SerializedProperty m_FrameRateProperty;

		private SerializedProperty m_DurationModeProperty;

		private SerializedProperty m_FixedDurationProperty;

		private void InitializeProperties()
		{
			this.m_FrameRateProperty = base.get_serializedObject().FindProperty("m_EditorSettings").FindPropertyRelative("fps");
			this.m_DurationModeProperty = base.get_serializedObject().FindProperty("m_DurationMode");
			this.m_FixedDurationProperty = base.get_serializedObject().FindProperty("m_FixedDuration");
		}

		public void OnEnable()
		{
			this.InitializeProperties();
		}

		protected override void OnHeaderGUI()
		{
			string text;
			if (base.get_targets().Length == 1)
			{
				text = base.get_target().get_name();
			}
			else
			{
				text = base.get_targets().Length.ToString() + " " + TimelineAssetInspector.Styles.HeaderTitleMultiselection.get_text();
			}
			Editor.DrawHeaderGUI(this, text, 0f);
		}

		public override void OnInspectorGUI()
		{
			base.get_serializedObject().Update();
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(this.m_FrameRateProperty, TimelineAssetInspector.Styles.FrameRate, new GUILayoutOption[]
			{
				GUILayout.MinWidth(140f)
			});
			double num = Math.Max(this.m_FrameRateProperty.get_doubleValue(), TimeUtility.kFrameRateEpsilon);
			this.m_FrameRateProperty.set_doubleValue(num);
			double frameRate = num;
			EditorGUILayout.PropertyField(this.m_DurationModeProperty, TimelineAssetInspector.Styles.DurationMode, new GUILayoutOption[]
			{
				GUILayout.MinWidth(140f)
			});
			TimelineAsset.DurationMode enumValueIndex = (TimelineAsset.DurationMode)this.m_DurationModeProperty.get_enumValueIndex();
			if (enumValueIndex == TimelineAsset.DurationMode.FixedLength)
			{
				TimelineInspectorUtility.TimeField(this.m_FixedDurationProperty, TimelineAssetInspector.Styles.Duration, false, frameRate, 4.94065645841247E-324, TimelineClip.kMaxTimeValue * 2.0);
			}
			else
			{
				bool showMixed = base.get_targets().Length > 1;
				TimelineInspectorUtility.TimeField(TimelineAssetInspector.Styles.Duration, ((TimelineAsset)base.get_target()).get_duration(), true, showMixed, frameRate, -1.7976931348623157E+308, 1.7976931348623157E+308);
			}
			bool flag = EditorGUI.EndChangeCheck();
			base.get_serializedObject().ApplyModifiedProperties();
			if (flag)
			{
				TimelineWindow.RepaintIfEditingTimelineAsset((TimelineAsset)base.get_target());
			}
		}
	}
}
