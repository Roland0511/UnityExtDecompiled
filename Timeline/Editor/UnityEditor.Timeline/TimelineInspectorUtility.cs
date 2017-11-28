using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal static class TimelineInspectorUtility
	{
		private static class Styles
		{
			public static readonly GUIContent SecondsPrefix = EditorGUIUtility.TextContent("s|Seconds");

			public static readonly GUIContent FramesPrefix = EditorGUIUtility.TextContent("f|Frames");
		}

		public static void TimeField(SerializedProperty property, GUIContent label, bool readOnly, double frameRate, double minValue, double maxValue)
		{
			Rect controlRect = EditorGUILayout.GetControlRect(new GUILayoutOption[0]);
			TimelineInspectorUtility.TimeField(controlRect, property, label, readOnly, frameRate, minValue, maxValue);
		}

		public static void TimeField(Rect rect, SerializedProperty property, GUIContent label, bool readOnly, double frameRate, double minValue, double maxValue)
		{
			GUIContent gUIContent = EditorGUI.BeginProperty(rect, label, property);
			rect = EditorGUI.PrefixLabel(rect, gUIContent);
			int indentLevel = EditorGUI.get_indentLevel();
			float labelWidth = EditorGUIUtility.get_labelWidth();
			EditorGUI.set_indentLevel(0);
			EditorGUIUtility.set_labelWidth(13f);
			EditorGUI.set_showMixedValue(property.get_hasMultipleDifferentValues());
			Rect rect2 = new Rect(rect.get_xMin(), rect.get_yMin(), rect.get_width() / 2f - 2f, rect.get_height());
			Rect rect3 = new Rect(rect.get_xMin() + rect.get_width() / 2f, rect.get_yMin(), rect.get_width() / 2f, rect.get_height());
			if (readOnly)
			{
				EditorGUI.FloatField(rect2, TimelineInspectorUtility.Styles.SecondsPrefix, (float)property.get_doubleValue(), EditorStyles.get_label());
			}
			else
			{
				EditorGUI.BeginChangeCheck();
				EditorGUI.PropertyField(rect2, property, TimelineInspectorUtility.Styles.SecondsPrefix);
				if (EditorGUI.EndChangeCheck())
				{
					property.set_doubleValue(Math.Min(maxValue, Math.Max(minValue, property.get_doubleValue())));
				}
			}
			if (frameRate > TimeUtility.kTimeEpsilon)
			{
				EditorGUI.set_showMixedValue(property.get_hasMultipleDifferentValues());
				EditorGUI.BeginChangeCheck();
				double num = property.get_doubleValue();
				int num2 = TimeUtility.ToFrames(num, frameRate);
				double num3 = TimeUtility.ToExactFrames(num, frameRate);
				bool flag = TimeUtility.OnFrameBoundary(num, frameRate);
				if (readOnly)
				{
					if (flag)
					{
						EditorGUI.IntField(rect3, TimelineInspectorUtility.Styles.FramesPrefix, num2, EditorStyles.get_label());
					}
					else
					{
						EditorGUI.DoubleField(rect3, TimelineInspectorUtility.Styles.FramesPrefix, num3, EditorStyles.get_label());
					}
				}
				else if (flag)
				{
					int frames = EditorGUI.IntField(rect3, TimelineInspectorUtility.Styles.FramesPrefix, num2);
					num = Math.Max(0.0, TimeUtility.FromFrames(frames, frameRate));
				}
				else
				{
					double d = EditorGUI.DoubleField(rect3, TimelineInspectorUtility.Styles.FramesPrefix, num3);
					num = Math.Max(0.0, TimeUtility.FromFrames((int)Math.Floor(d), frameRate));
				}
				if (EditorGUI.EndChangeCheck())
				{
					property.set_doubleValue(Math.Min(maxValue, Math.Max(-maxValue, num)));
				}
			}
			EditorGUI.set_indentLevel(indentLevel);
			EditorGUIUtility.set_labelWidth(labelWidth);
			EditorGUI.EndProperty();
		}

		public static double TimeField(GUIContent label, double time, bool readOnly, bool showMixed, double frameRate, double minValue, double maxValue)
		{
			Rect rect = EditorGUILayout.GetControlRect(new GUILayoutOption[0]);
			rect = EditorGUI.PrefixLabel(rect, label);
			int indentLevel = EditorGUI.get_indentLevel();
			float labelWidth = EditorGUIUtility.get_labelWidth();
			EditorGUI.set_indentLevel(0);
			EditorGUIUtility.set_labelWidth(13f);
			bool showMixedValue = EditorGUI.get_showMixedValue();
			EditorGUI.set_showMixedValue(showMixed);
			Rect rect2 = new Rect(rect.get_xMin(), rect.get_yMin(), rect.get_width() / 2f - 2f, rect.get_height());
			Rect rect3 = new Rect(rect.get_xMin() + rect.get_width() / 2f, rect.get_yMin(), rect.get_width() / 2f, rect.get_height());
			if (readOnly)
			{
				EditorGUI.FloatField(rect2, TimelineInspectorUtility.Styles.SecondsPrefix, (float)time, EditorStyles.get_label());
			}
			else
			{
				time = EditorGUI.DoubleField(rect2, TimelineInspectorUtility.Styles.SecondsPrefix, time);
			}
			if (frameRate > TimeUtility.kTimeEpsilon)
			{
				int num = TimeUtility.ToFrames(time, frameRate);
				double num2 = TimeUtility.ToExactFrames(time, frameRate);
				bool flag = TimeUtility.OnFrameBoundary(time, frameRate);
				if (readOnly)
				{
					if (flag)
					{
						EditorGUI.IntField(rect3, TimelineInspectorUtility.Styles.FramesPrefix, num, EditorStyles.get_label());
					}
					else
					{
						EditorGUI.FloatField(rect3, TimelineInspectorUtility.Styles.FramesPrefix, (float)num2, EditorStyles.get_label());
					}
				}
				else
				{
					EditorGUI.BeginChangeCheck();
					double num3;
					if (flag)
					{
						int frames = EditorGUI.IntField(rect3, TimelineInspectorUtility.Styles.FramesPrefix, num);
						num3 = Math.Max(0.0, TimeUtility.FromFrames(frames, frameRate));
					}
					else
					{
						double d = EditorGUI.DoubleField(rect3, TimelineInspectorUtility.Styles.FramesPrefix, num2);
						num3 = Math.Max(0.0, TimeUtility.FromFrames((int)Math.Floor(d), frameRate));
					}
					if (EditorGUI.EndChangeCheck())
					{
						time = num3;
					}
				}
			}
			EditorGUI.set_showMixedValue(showMixedValue);
			EditorGUI.set_indentLevel(indentLevel);
			EditorGUIUtility.set_labelWidth(labelWidth);
			return Math.Min(maxValue, Math.Max(minValue, time));
		}

		public static Editor GetInspectorForObjects(Object[] objects)
		{
			Editor result;
			try
			{
				if (!objects.Any<Object>())
				{
					result = null;
					return result;
				}
				Editor editor = null;
				PlayableDirector currentDirector = TimelineWindow.instance.state.currentDirector;
				Editor.CreateCachedEditorWithContext(objects, currentDirector, null, ref editor);
				result = editor;
				return result;
			}
			catch (Exception)
			{
			}
			result = null;
			return result;
		}
	}
}
