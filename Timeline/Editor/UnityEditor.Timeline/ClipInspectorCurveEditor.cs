using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class ClipInspectorCurveEditor
	{
		private CurveEditor m_CurveEditor;

		private AnimationCurve[] m_Curves;

		private CurveWrapper[] m_CurveWrappers;

		private const float k_HeaderHeight = 30f;

		private const float k_PresetHeight = 30f;

		private Action<AnimationCurve, EditorCurveBinding> m_CurveUpdatedCallback;

		private GUIContent m_TextContent = new GUIContent();

		private GUIStyle m_LabelStyle;

		private GUIStyle m_LegendStyle;

		public static readonly double kDisableTrackTime = double.NaN;

		private double m_trackTime = ClipInspectorCurveEditor.kDisableTrackTime;

		private static char[] s_LabelMarkers = new char[]
		{
			'_'
		};

		public double trackTime
		{
			get
			{
				return this.m_trackTime;
			}
			set
			{
				this.m_trackTime = value;
			}
		}

		public string headerString
		{
			get;
			set;
		}

		public ClipInspectorCurveEditor()
		{
			CurveEditorSettings curveEditorSettings = new CurveEditorSettings();
			curveEditorSettings.allowDeleteLastKeyInCurve = false;
			curveEditorSettings.allowDraggingCurvesAndRegions = true;
			curveEditorSettings.hTickLabelOffset = 0.1f;
			curveEditorSettings.showAxisLabels = true;
			curveEditorSettings.useFocusColors = false;
			curveEditorSettings.wrapColor = new EditorGUIUtility.SkinnedColor(Color.get_black());
			curveEditorSettings.set_hSlider(false);
			curveEditorSettings.set_hRangeMin(0f);
			curveEditorSettings.set_vRangeMin(0f);
			curveEditorSettings.set_vRangeMax(1f);
			curveEditorSettings.set_hRangeMax(1f);
			curveEditorSettings.set_vSlider(false);
			curveEditorSettings.set_hRangeLocked(false);
			curveEditorSettings.set_vRangeLocked(false);
			curveEditorSettings.set_hTickStyle(new TickStyle
			{
				tickColor = new EditorGUIUtility.SkinnedColor(new Color(0f, 0f, 0f, 0.2f)),
				distLabel = 30,
				stubs = false,
				centerLabel = true
			});
			curveEditorSettings.set_vTickStyle(new TickStyle
			{
				tickColor = new EditorGUIUtility.SkinnedColor(new Color(1f, 0f, 0f, 0.2f)),
				distLabel = 20,
				stubs = false,
				centerLabel = true
			});
			CurveEditorSettings settings = curveEditorSettings;
			CurveEditor curveEditor = new CurveEditor(new Rect(0f, 0f, 1000f, 100f), new CurveWrapper[0], true);
			curveEditor.set_settings(settings);
			curveEditor.set_ignoreScrollWheelUntilClicked(true);
			this.m_CurveEditor = curveEditor;
		}

		internal bool InitStyles()
		{
			bool result;
			if (EditorStyles.s_Current == null)
			{
				result = false;
			}
			else
			{
				if (this.m_LabelStyle == null)
				{
					this.m_LabelStyle = new GUIStyle(EditorStyles.get_whiteLargeLabel());
					this.m_LegendStyle = new GUIStyle(EditorStyles.get_miniBoldLabel());
					this.m_LabelStyle.set_alignment(4);
					this.m_LegendStyle.set_alignment(4);
				}
				result = true;
			}
			return result;
		}

		internal void OnGUI(Rect clientRect, CurvePresetLibrary presets)
		{
			if (this.InitStyles())
			{
				if (this.m_Curves != null && this.m_Curves.Length != 0)
				{
					Rect rect = new Rect(clientRect.get_x(), clientRect.get_y(), clientRect.get_width(), 30f);
					Rect rect2 = new Rect(clientRect.get_x(), clientRect.get_y() + rect.get_height(), clientRect.get_width(), clientRect.get_height() - 30f - 30f);
					Rect rect3 = new Rect(clientRect.get_x() + 30f, clientRect.get_y() + rect2.get_height() + 30f, clientRect.get_width() - 30f, 30f);
					GUI.Box(rect, this.headerString, this.m_LabelStyle);
					this.m_CurveEditor.set_rect(rect2);
					this.m_CurveEditor.set_shownAreaInsideMargins(new Rect(0f, 0f, 1f, 1f));
					this.m_CurveEditor.set_animationCurves(this.m_CurveWrappers);
					this.UpdateSelectionColors();
					this.DrawTrackHead(rect2);
					EditorGUI.BeginChangeCheck();
					this.m_CurveEditor.OnGUI();
					this.DrawPresets(rect3, presets);
					bool flag = EditorGUI.EndChangeCheck();
					if (presets == null)
					{
						this.DrawLegend(rect3);
					}
					if (flag)
					{
						this.ProcessUpdates();
					}
				}
			}
		}

		private void DrawPresets(Rect position, PresetLibrary curveLibrary)
		{
			if (!(curveLibrary == null) && curveLibrary.Count() != 0)
			{
				int num = curveLibrary.Count();
				int num2 = Mathf.Min(num, 9);
				float num3 = (float)num2 * 30f + (float)(num2 - 1) * 10f;
				float num4 = (position.get_width() - num3) * 0.5f;
				float num5 = (position.get_height() - 15f) * 0.5f;
				float num6 = 3f;
				if (num4 > 0f)
				{
					num6 = num4;
				}
				GUI.BeginGroup(position);
				for (int i = 0; i < num2; i++)
				{
					if (i > 0)
					{
						num6 += 10f;
					}
					Rect rect = new Rect(num6, num5, 30f, 15f);
					this.m_TextContent.set_tooltip(curveLibrary.GetName(i));
					if (GUI.Button(rect, this.m_TextContent, GUIStyle.get_none()))
					{
						IEnumerable<CurveWrapper> enumerable = this.m_CurveWrappers;
						if (this.m_CurveWrappers.Length > 1)
						{
							enumerable = from x in this.m_CurveWrappers
							where x.selected == 1
							select x;
						}
						foreach (CurveWrapper current in enumerable)
						{
							AnimationCurve animationCurve = (AnimationCurve)curveLibrary.GetPreset(i);
							current.get_curve().set_keys((Keyframe[])animationCurve.get_keys().Clone());
							current.set_changed(true);
						}
					}
					if (Event.get_current().get_type() == 7)
					{
						curveLibrary.Draw(rect, i);
					}
					num6 += 30f;
				}
				GUI.EndGroup();
			}
		}

		private void DrawTrackHead(Rect clientRect)
		{
			if (TimelineWindow.styles != null)
			{
				if (!double.IsNaN(this.m_trackTime))
				{
					float num = this.m_CurveEditor.TimeToPixel((float)this.m_trackTime, clientRect);
					num = Mathf.Clamp(num, clientRect.get_xMin(), clientRect.get_xMax());
					Vector2 vector = new Vector2(num, clientRect.get_yMin());
					Vector2 vector2 = new Vector2(num, clientRect.get_yMax());
					Graphics.DrawLine(vector, vector2, DirectorStyles.Instance.customSkin.colorPlayhead);
				}
			}
		}

		private void DrawLegend(Rect r)
		{
			if (this.m_CurveWrappers != null && this.m_CurveWrappers.Length != 0)
			{
				Color color = GUI.get_color();
				float num = r.get_width() / (float)this.m_CurveWrappers.Length;
				for (int i = 0; i < this.m_CurveWrappers.Length; i++)
				{
					CurveWrapper curveWrapper = this.m_CurveWrappers[i];
					if (curveWrapper != null)
					{
						Rect rect = new Rect(r.get_x() + (float)i * num, r.get_y(), num, r.get_height());
						Color color2 = curveWrapper.color;
						color2.a = 1f;
						GUI.set_color(color2);
						string text = ClipInspectorCurveEditor.LabelName(curveWrapper.binding.propertyName);
						EditorGUI.LabelField(rect, text, this.m_LegendStyle);
					}
				}
				GUI.set_color(color);
			}
		}

		private static string LabelName(string propertyName)
		{
			propertyName = AnimationWindowUtility.GetPropertyDisplayName(propertyName);
			int num = propertyName.LastIndexOfAny(ClipInspectorCurveEditor.s_LabelMarkers);
			if (num >= 0)
			{
				propertyName = propertyName.Substring(num);
			}
			return propertyName;
		}

		public void SetCurves(AnimationCurve[] curves, EditorCurveBinding[] bindings)
		{
			this.m_Curves = curves;
			if (this.m_Curves != null && this.m_Curves.Length > 0)
			{
				this.m_CurveWrappers = new CurveWrapper[this.m_Curves.Length];
				for (int i = 0; i < this.m_Curves.Length; i++)
				{
					CurveWrapper curveWrapper = new CurveWrapper();
					curveWrapper.set_renderer(new NormalCurveRenderer(this.m_Curves[i]));
					curveWrapper.readOnly = false;
					curveWrapper.color = EditorGUI.kCurveColor;
					curveWrapper.id = curves[i].GetHashCode();
					curveWrapper.hidden = false;
					curveWrapper.regionId = -1;
					CurveWrapper curveWrapper2 = curveWrapper;
					curveWrapper2.get_renderer().SetWrap(1, 1);
					curveWrapper2.get_renderer().SetCustomRange(0f, 1f);
					if (bindings != null)
					{
						curveWrapper2.binding = bindings[i];
						curveWrapper2.color = CurveUtility.GetPropertyColor(bindings[i].propertyName);
						curveWrapper2.id = bindings[i].GetHashCode();
					}
					this.m_CurveWrappers[i] = curveWrapper2;
				}
				this.UpdateSelectionColors();
				this.m_CurveEditor.set_animationCurves(this.m_CurveWrappers);
			}
		}

		internal void SetUpdateCurveCallback(Action<AnimationCurve, EditorCurveBinding> callback)
		{
			this.m_CurveUpdatedCallback = callback;
		}

		private void ProcessUpdates()
		{
			CurveWrapper[] curveWrappers = this.m_CurveWrappers;
			for (int i = 0; i < curveWrappers.Length; i++)
			{
				CurveWrapper curveWrapper = curveWrappers[i];
				if (curveWrapper.get_changed())
				{
					curveWrapper.set_changed(false);
					if (this.m_CurveUpdatedCallback != null)
					{
						this.m_CurveUpdatedCallback(curveWrapper.get_curve(), curveWrapper.binding);
					}
				}
			}
		}

		public void SetSelected(AnimationCurve curve)
		{
			this.m_CurveEditor.SelectNone();
			for (int i = 0; i < this.m_Curves.Length; i++)
			{
				if (curve == this.m_Curves[i])
				{
					this.m_CurveWrappers[i].selected = 1;
					this.m_CurveEditor.AddSelection(new CurveSelection(this.m_CurveWrappers[i].id, 0));
				}
			}
			this.UpdateSelectionColors();
		}

		private void UpdateSelectionColors()
		{
			if (this.m_CurveWrappers != null)
			{
				CurveWrapper[] curveWrappers = this.m_CurveWrappers;
				for (int i = 0; i < curveWrappers.Length; i++)
				{
					CurveWrapper curveWrapper = curveWrappers[i];
					Color color = curveWrapper.color;
					if (curveWrapper.readOnly)
					{
						color.a = 0.75f;
					}
					else if (curveWrapper.selected != null)
					{
						color.a = 1f;
					}
					else
					{
						color.a = 0.5f;
					}
					curveWrapper.color = color;
				}
			}
		}

		public static void CurveField(GUIContent title, SerializedProperty property, Action<SerializedProperty> onClick)
		{
			Rect controlRect = EditorGUILayout.GetControlRect(new GUILayoutOption[]
			{
				GUILayout.MinWidth(20f)
			});
			EditorGUI.BeginProperty(controlRect, title, property);
			ClipInspectorCurveEditor.DrawCurve(controlRect, property, onClick, EditorGUI.kCurveColor, EditorGUI.kCurveBGColor);
			EditorGUI.EndProperty();
		}

		private static Rect DrawCurve(Rect controlRect, SerializedProperty property, Action<SerializedProperty> onClick, Color fgColor, Color bgColor)
		{
			if (GUI.Button(controlRect, GUIContent.none))
			{
				if (onClick != null)
				{
					onClick(property);
				}
			}
			EditorGUIUtility.DrawCurveSwatch(controlRect, null, property, fgColor, bgColor);
			return controlRect;
		}
	}
}
