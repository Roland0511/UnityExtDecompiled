using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline.inspectors
{
	[CustomPropertyDrawer(typeof(TimeFieldAttribute), true)]
	internal class TimeFieldDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			if (property.get_propertyType() != 2)
			{
				GUILayout.Label("TimeFields only work on floating point types", new GUILayoutOption[0]);
			}
			else
			{
				double frameRate = (TimelineWindow.instance.state == null) ? 0.0 : ((double)TimelineWindow.instance.state.frameRate);
				TimelineInspectorUtility.TimeField(position, property, label, false, frameRate, 4.94065645841247E-324, TimelineClip.kMaxTimeValue);
			}
		}
	}
}
