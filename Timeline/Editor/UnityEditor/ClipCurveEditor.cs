using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Timeline;
using UnityEngine;

namespace UnityEditor
{
	internal class ClipCurveEditor
	{
		private class FrameFormatCurveEditorState : ICurveEditorState
		{
			public TimeArea.TimeFormat timeFormat
			{
				get
				{
					return 2;
				}
			}
		}

		private class UnformattedCurveEditorState : ICurveEditorState
		{
			public TimeArea.TimeFormat timeFormat
			{
				get
				{
					return 0;
				}
			}
		}

		private readonly CurveEditor m_CurveEditor;

		private static readonly CurveEditorSettings s_CurveEditorSettings = new CurveEditorSettings();

		private static readonly float s_GridLabelWidth = 40f;

		private readonly BindingSelector m_BindingHierarchy;

		private Vector2 m_ScrollPosition = Vector2.get_zero();

		private readonly CurveDataSource m_DataSource;

		private float m_LastFrameRate = 30f;

		private int m_LastClipVersion = -1;

		private int m_LastCurveCount = -1;

		public CurveDataSource dataSource
		{
			get
			{
				return this.m_DataSource;
			}
		}

		public ClipCurveEditor(CurveDataSource dataSource, TimelineWindow parentWindow)
		{
			this.m_DataSource = dataSource;
			this.m_CurveEditor = new CurveEditor(new Rect(0f, 0f, 1000f, 100f), new CurveWrapper[0], false);
			ClipCurveEditor.s_CurveEditorSettings.set_hSlider(false);
			ClipCurveEditor.s_CurveEditorSettings.set_vSlider(false);
			ClipCurveEditor.s_CurveEditorSettings.set_hRangeLocked(false);
			ClipCurveEditor.s_CurveEditorSettings.set_vRangeLocked(false);
			ClipCurveEditor.s_CurveEditorSettings.set_scaleWithWindow(true);
			ClipCurveEditor.s_CurveEditorSettings.set_hRangeMin(0f);
			ClipCurveEditor.s_CurveEditorSettings.showAxisLabels = true;
			ClipCurveEditor.s_CurveEditorSettings.allowDeleteLastKeyInCurve = true;
			ClipCurveEditor.s_CurveEditorSettings.rectangleToolFlags = 0;
			CurveEditorSettings arg_F9_0 = ClipCurveEditor.s_CurveEditorSettings;
			TickStyle tickStyle = new TickStyle();
			tickStyle.tickColor.set_color(DirectorStyles.Instance.customSkin.colorInlineCurveVerticalLines);
			tickStyle.distLabel = 20;
			tickStyle.stubs = true;
			arg_F9_0.set_vTickStyle(tickStyle);
			CurveEditorSettings arg_135_0 = ClipCurveEditor.s_CurveEditorSettings;
			tickStyle = new TickStyle();
			tickStyle.tickColor.set_color(new Color(0f, 0f, 0f, 0f));
			tickStyle.distLabel = 0;
			arg_135_0.set_hTickStyle(tickStyle);
			this.m_CurveEditor.set_settings(ClipCurveEditor.s_CurveEditorSettings);
			this.m_CurveEditor.set_shownArea(new Rect(1f, 1f, 1f, 1f));
			this.m_CurveEditor.set_ignoreScrollWheelUntilClicked(true);
			this.m_CurveEditor.curvesUpdated = new CurveEditor.CallbackFunction(this.OnCurvesUpdated);
			this.m_BindingHierarchy = new BindingSelector(parentWindow, this.m_CurveEditor);
		}

		public void SelectAllKeys()
		{
			this.m_CurveEditor.SelectAll();
		}

		public void FrameClip()
		{
			this.m_CurveEditor.InvalidateBounds();
			this.m_CurveEditor.FrameClip(false, true);
		}

		public bool HasSelection()
		{
			return this.m_CurveEditor.get_hasSelection();
		}

		public Vector2 GetSelectionRange()
		{
			Bounds selectionBounds = this.m_CurveEditor.get_selectionBounds();
			return new Vector2(selectionBounds.get_min().x, selectionBounds.get_max().x);
		}

		private void OnCurvesUpdated()
		{
			if (this.m_DataSource != null)
			{
				if (this.m_CurveEditor != null)
				{
					if (this.m_CurveEditor.get_animationCurves().Length != 0)
					{
						List<CurveWrapper> list = (from c in this.m_CurveEditor.get_animationCurves()
						where c.get_changed()
						select c).ToList<CurveWrapper>();
						if (list.Count != 0)
						{
							AnimationClip animationClip = this.m_DataSource.animationClip;
							Undo.RegisterCompleteObjectUndo(animationClip, "Edit Clip Curve");
							foreach (CurveWrapper current in list)
							{
								AnimationUtility.SetEditorCurve(animationClip, current.binding, current.get_curve());
								current.set_changed(false);
							}
						}
					}
				}
			}
		}

		public void DrawHeader(Rect headerRect)
		{
			this.m_BindingHierarchy.InitIfNeeded(headerRect, this.m_DataSource);
			try
			{
				GUILayout.BeginArea(headerRect);
				this.m_ScrollPosition = GUILayout.BeginScrollView(this.m_ScrollPosition, GUIStyle.get_none(), GUI.get_skin().get_verticalScrollbar(), new GUILayoutOption[0]);
				this.m_BindingHierarchy.OnGUI(new Rect(0f, 0f, headerRect.get_width(), headerRect.get_height()));
				GUILayout.EndScrollView();
				GUILayout.EndArea();
			}
			catch (Exception ex)
			{
				Debug.Log(ex.Message);
			}
		}

		private void UpdateCurveEditorIfNeeded(TimelineWindow.TimelineState state)
		{
			if (Event.get_current().get_type() == 8 && this.m_DataSource != null && this.m_BindingHierarchy != null && !(this.m_DataSource.animationClip == null))
			{
				AnimationClipCurveInfo curveInfo = AnimationClipCurveCache.Instance.GetCurveInfo(this.m_DataSource.animationClip);
				int version = curveInfo.version;
				if (version != this.m_LastClipVersion)
				{
					if (this.m_LastCurveCount != curveInfo.curves.Length)
					{
						this.m_BindingHierarchy.RefreshTree();
						this.m_LastCurveCount = curveInfo.curves.Length;
					}
					else
					{
						this.m_BindingHierarchy.RefreshCurves();
					}
					if (this.m_LastClipVersion == -1)
					{
						this.FrameClip();
					}
					this.m_LastClipVersion = version;
				}
				if (state.timeInFrames)
				{
					this.m_CurveEditor.state = new ClipCurveEditor.FrameFormatCurveEditorState();
				}
				else
				{
					this.m_CurveEditor.state = new ClipCurveEditor.UnformattedCurveEditorState();
				}
				this.m_CurveEditor.invSnap = state.frameRate;
			}
		}

		public void DrawCurveEditor(Rect animEditorRect, TimelineWindow.TimelineState state, Vector2 activeRange, bool loop, bool selected)
		{
			this.UpdateCurveEditorIfNeeded(state);
			ZoomableArea arg_29_0 = this.m_CurveEditor;
			float num = this.CalculateTopMargin(animEditorRect.get_height());
			this.m_CurveEditor.set_bottommargin(num);
			arg_29_0.set_topmargin(num);
			float num2 = state.TimeToPixel((double)this.m_DataSource.start) - animEditorRect.get_xMin();
			this.m_CurveEditor.set_rightmargin(0f);
			this.m_CurveEditor.set_leftmargin(num2);
			this.m_CurveEditor.set_rect(new Rect(0f, 0f, animEditorRect.get_width(), animEditorRect.get_height()));
			this.m_CurveEditor.SetShownHRangeInsideMargins(0f, (state.PixelToTime(animEditorRect.get_xMax()) - this.m_DataSource.start) * this.m_DataSource.timeScale);
			if (this.m_LastFrameRate != state.frameRate)
			{
				this.m_CurveEditor.get_hTicks().SetTickModulosForFrameRate(state.frameRate);
				this.m_LastFrameRate = state.frameRate;
			}
			CurveWrapper[] animationCurves = this.m_CurveEditor.get_animationCurves();
			for (int i = 0; i < animationCurves.Length; i++)
			{
				CurveWrapper curveWrapper = animationCurves[i];
				curveWrapper.get_renderer().SetWrap(0, (!loop) ? 0 : 2);
			}
			this.m_CurveEditor.BeginViewGUI();
			Color color = GUI.get_color();
			GUI.set_color(Color.get_white());
			GUI.BeginGroup(animEditorRect);
			Graphics.DrawLine(new Vector2(num2, 0f), new Vector2(num2, animEditorRect.get_height()), new Color(1f, 1f, 1f, 0.5f));
			float num3 = activeRange.x - animEditorRect.get_x();
			float num4 = activeRange.y - activeRange.x;
			if (selected)
			{
				Rect rect = new Rect(num3, 0f, num4, animEditorRect.get_height());
				ClipCurveEditor.DrawOutline(rect, 2f);
			}
			EditorGUI.BeginChangeCheck();
			Event current = Event.get_current();
			if (current.get_type() == 8 || current.get_type() == 7 || selected)
			{
				this.m_CurveEditor.CurveGUI();
			}
			this.m_CurveEditor.EndViewGUI();
			if (EditorGUI.EndChangeCheck())
			{
				this.OnCurvesUpdated();
			}
			Color colorInlineCurveOutOfRangeOverlay = DirectorStyles.Instance.customSkin.colorInlineCurveOutOfRangeOverlay;
			Rect rect2 = new Rect(num2, 0f, num3 - num2, animEditorRect.get_height());
			EditorGUI.DrawRect(rect2, colorInlineCurveOutOfRangeOverlay);
			Rect rect3 = new Rect(num3 + num4, 0f, animEditorRect.get_width() - num3 - num4, animEditorRect.get_height());
			EditorGUI.DrawRect(rect3, colorInlineCurveOutOfRangeOverlay);
			GUI.set_color(color);
			GUI.EndGroup();
			Rect rect4 = animEditorRect;
			rect4.set_width(ClipCurveEditor.s_GridLabelWidth);
			float num5 = num2 - ClipCurveEditor.s_GridLabelWidth;
			if (num5 > 0f)
			{
				rect4.set_x(animEditorRect.get_x() + num5);
			}
			GUI.BeginGroup(rect4);
			this.m_CurveEditor.GridGUI();
			GUI.EndGroup();
		}

		private float CalculateTopMargin(float height)
		{
			return Mathf.Clamp(0.15f * height, 10f, 40f);
		}

		private static void DrawOutline(Rect rect, float tickness = 2f)
		{
			EditorGUI.DrawRect(new Rect(rect.get_xMin(), rect.get_yMin(), rect.get_width(), tickness), Color.get_white());
			EditorGUI.DrawRect(new Rect(rect.get_xMin(), rect.get_yMax() - tickness, rect.get_width(), tickness), Color.get_white());
			EditorGUI.DrawRect(new Rect(rect.get_xMin(), rect.get_yMin(), tickness, rect.get_height()), Color.get_white());
			EditorGUI.DrawRect(new Rect(rect.get_xMax() - tickness, rect.get_yMin(), tickness, rect.get_height()), Color.get_white());
		}
	}
}
