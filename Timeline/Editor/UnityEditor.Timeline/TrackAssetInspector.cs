using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[CanEditMultipleObjects, CustomEditor(typeof(TrackAsset), true)]
	internal class TrackAssetInspector : Editor
	{
		private SerializedProperty m_Name;

		protected TimelineWindow timelineWindow
		{
			get
			{
				return TimelineWindow.instance;
			}
		}

		public override void OnInspectorGUI()
		{
		}

		internal override void OnHeaderTitleGUI(Rect titleRect, string header)
		{
			this.m_Name = base.get_serializedObject().FindProperty("m_Name");
			base.get_serializedObject().Update();
			Rect rect = titleRect;
			rect.set_height(16f);
			EditorGUI.BeginChangeCheck();
			EditorGUI.set_showMixedValue(this.m_Name.get_hasMultipleDifferentValues());
			TimelineWindow instance = TimelineWindow.instance;
			bool flag = instance == null || instance.state == null || instance.state.currentDirector == null;
			EditorGUI.BeginDisabledGroup(flag);
			EditorGUI.BeginChangeCheck();
			string text = EditorGUI.DelayedTextField(rect, this.m_Name.get_stringValue(), EditorStyles.get_textField());
			EditorGUI.set_showMixedValue(false);
			if (EditorGUI.EndChangeCheck() && !string.IsNullOrEmpty(text))
			{
				for (int i = 0; i < base.get_targets().Count<Object>(); i++)
				{
					ObjectNames.SetNameSmart(base.get_targets()[i], text);
				}
				if (instance != null)
				{
					instance.Repaint();
				}
			}
			EditorGUI.EndDisabledGroup();
			base.get_serializedObject().ApplyModifiedProperties();
		}

		internal override void OnHeaderIconGUI(Rect iconRect)
		{
			if (!(TimelineWindow.instance == null))
			{
				TimelineTrackBaseGUI timelineTrackBaseGUI = TimelineWindow.instance.allTracks.Find((TimelineTrackBaseGUI uiTrack) => uiTrack.track == base.get_target() as TrackAsset);
				if (timelineTrackBaseGUI != null)
				{
					GUI.Label(iconRect, timelineTrackBaseGUI.drawer.GetIcon());
				}
			}
		}

		internal override void DrawHeaderHelpAndSettingsGUI(Rect r)
		{
			Vector2 vector = EditorStyles.get_iconButton().CalcSize(EditorGUI.GUIContents.get_helpIcon());
			Object target = base.get_target();
			EditorGUI.HelpIconButton(new Rect(r.get_xMax() - vector.x, r.get_y() + 5f, vector.x, vector.y), target);
		}

		public virtual void OnEnable()
		{
		}

		public virtual void OnDestroy()
		{
		}
	}
}
