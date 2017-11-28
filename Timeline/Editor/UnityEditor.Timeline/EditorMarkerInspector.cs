using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[CanEditMultipleObjects, CustomEditor(typeof(EditorMarker))]
	internal class EditorMarkerInspector : Editor
	{
		private GUIContent m_Label = EditorGUIUtility.TextContent("Marker");

		private SerializedProperty m_ItemProperty;

		private SerializedProperty m_KeyProperty;

		private SerializedProperty m_TimeProperty;

		private bool m_HasDrawer = false;

		internal override string targetTitle
		{
			get
			{
				return this.m_KeyProperty.get_stringValue();
			}
		}

		public void OnEnable()
		{
			this.m_ItemProperty = base.get_serializedObject().FindProperty("m_Item");
			this.m_KeyProperty = base.get_serializedObject().FindProperty("m_Item.m_Key");
			this.m_TimeProperty = base.get_serializedObject().FindProperty("m_Item.m_Time");
			this.m_HasDrawer = (ScriptAttributeUtility.GetDrawerTypeForType(typeof(TimelineMarker)) != null);
		}

		public override void OnInspectorGUI()
		{
			base.get_serializedObject().Update();
			EditorGUI.BeginChangeCheck();
			if (this.m_HasDrawer)
			{
				EditorGUILayout.PropertyField(this.m_ItemProperty, this.m_Label, new GUILayoutOption[0]);
			}
			else
			{
				EditorGUILayout.PropertyField(this.m_KeyProperty, new GUILayoutOption[0]);
				EditorGUILayout.PropertyField(this.m_TimeProperty, new GUILayoutOption[0]);
			}
			if (EditorGUI.EndChangeCheck())
			{
				base.get_serializedObject().ApplyModifiedProperties();
				TimelineWindow instance = TimelineWindow.instance;
				if (instance != null)
				{
					instance.Repaint();
					if (instance.state != null)
					{
						instance.state.rebuildGraph = true;
					}
				}
			}
		}

		internal override void DrawHeaderHelpAndSettingsGUI(Rect r)
		{
		}

		internal override void OnHeaderIconGUI(Rect iconRect)
		{
			Texture2D background = DirectorStyles.Instance.eventTrakIcon.get_normal().get_background();
			Rect rect = new Rect(iconRect.get_x(), iconRect.get_y(), Mathf.Min((float)background.get_width(), iconRect.get_width()), Mathf.Min((float)background.get_height(), iconRect.get_height()));
			GUI.DrawTexture(rect, background);
		}
	}
}
