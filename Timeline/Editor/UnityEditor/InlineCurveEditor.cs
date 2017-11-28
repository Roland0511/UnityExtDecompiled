using System;
using System.Linq;
using UnityEditor.Timeline;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor
{
	internal class InlineCurveEditor : IBounds
	{
		private Rect m_TrackRect;

		private Rect m_HeaderRect;

		private readonly TimelineTrackGUI m_TrackGUI;

		private TimelineClipGUI m_LastSelectedClipGUI;

		private static readonly float s_FrameAllMarginFactor = 0.1f;

		private static readonly float s_FrameSelectedMarginFactor = 0.2f;

		Rect IBounds.boundingRect
		{
			get
			{
				return this.m_TrackRect;
			}
		}

		public Rect resizeRect
		{
			get;
			set;
		}

		public InlineCurveEditor(TimelineTrackGUI trackGUI)
		{
			this.m_TrackGUI = trackGUI;
		}

		public bool OnEvent(Event evt, TimelineWindow.TimelineState state, bool isCaptureSession)
		{
			bool result;
			if (evt.get_type() == null || evt.get_type() == 16)
			{
				result = this.MouseOver(state);
			}
			else
			{
				IClipCurveEditorOwner clipCurveEditorOwner = this.GetClipCurveEditorOwner();
				if (clipCurveEditorOwner == null)
				{
					result = false;
				}
				else if (!clipCurveEditorOwner.inlineCurvesSelected)
				{
					result = false;
				}
				else
				{
					ClipCurveEditor clipCurveEditor = clipCurveEditorOwner.clipCurveEditor;
					if (clipCurveEditor == null)
					{
						result = false;
					}
					else if (evt.get_commandName() == "FrameSelected")
					{
						this.FrameSelected(state, clipCurveEditor);
						result = true;
					}
					else
					{
						if (evt.get_type() == 4 && evt.get_modifiers() == null)
						{
							if (evt.get_character() == 'a')
							{
								this.FrameAll(state, clipCurveEditor);
								result = true;
								return result;
							}
						}
						result = false;
					}
				}
			}
			return result;
		}

		private void FrameAll(TimelineWindow.TimelineState state, ClipCurveEditor clipCurveEditor)
		{
			CurveDataSource dataSource = clipCurveEditor.dataSource;
			float start = dataSource.start;
			float duration = dataSource.animationClip.get_length() / dataSource.timeScale;
			InlineCurveEditor.Frame(state, start, duration, InlineCurveEditor.s_FrameAllMarginFactor);
		}

		private void FrameSelected(TimelineWindow.TimelineState state, ClipCurveEditor clipCurveEditor)
		{
			if (!clipCurveEditor.HasSelection())
			{
				this.FrameAll(state, clipCurveEditor);
			}
			else
			{
				Vector2 selectionRange = clipCurveEditor.GetSelectionRange();
				if (selectionRange.x != selectionRange.y)
				{
					CurveDataSource dataSource = clipCurveEditor.dataSource;
					float start = dataSource.start + selectionRange.x / dataSource.timeScale;
					float duration = (selectionRange.y - selectionRange.x) / dataSource.timeScale;
					InlineCurveEditor.Frame(state, start, duration, InlineCurveEditor.s_FrameSelectedMarginFactor);
				}
			}
		}

		private static void Frame(TimelineWindow.TimelineState state, float start, float duration, float marginFactor)
		{
			float num = duration * marginFactor;
			state.SetTimeAreaShownRange(Mathf.Max(start - num, -10f), start + duration + num);
			state.Evaluate();
		}

		private bool MouseOver(TimelineWindow.TimelineState state)
		{
			bool result;
			if (InlineCurveEditor.MouseOverHeaderArea(this.m_HeaderRect, this.m_TrackRect))
			{
				result = true;
			}
			else
			{
				ClipCurveEditor clipCurveEditor = this.GetClipCurveEditorOwner().clipCurveEditor;
				if (clipCurveEditor == null)
				{
					result = false;
				}
				else
				{
					Rect backgroundRect = clipCurveEditor.dataSource.GetBackgroundRect(state);
					result = InlineCurveEditor.MouseOverTrackArea(backgroundRect, this.m_TrackRect);
				}
			}
			return result;
		}

		private IClipCurveEditorOwner GetClipCurveEditorOwner()
		{
			return (this.m_LastSelectedClipGUI == null) ? this.m_TrackGUI : this.m_LastSelectedClipGUI;
		}

		private static bool MouseOverTrackArea(Rect curveRect, Rect trackRect)
		{
			curveRect.set_y(trackRect.get_y());
			curveRect.set_height(trackRect.get_height());
			curveRect.set_xMin(Mathf.Max(curveRect.get_xMin(), trackRect.get_xMin()));
			curveRect.set_xMax(trackRect.get_xMax());
			return curveRect.Contains(Event.get_current().get_mousePosition());
		}

		private static bool MouseOverHeaderArea(Rect headerRect, Rect trackRect)
		{
			headerRect.set_y(trackRect.get_y());
			headerRect.set_height(trackRect.get_height());
			return headerRect.Contains(Event.get_current().get_mousePosition());
		}

		private static void DrawCurveEditor(IClipCurveEditorOwner clipCurveEditorOwner, TimelineWindow.TimelineState state, Rect headerRect, Rect trackRect, Vector2 activeRange, bool locked)
		{
			ClipCurveEditor clipCurveEditor = clipCurveEditorOwner.clipCurveEditor;
			CurveDataSource dataSource = clipCurveEditor.dataSource;
			Rect backgroundRect = dataSource.GetBackgroundRect(state);
			bool flag = false;
			if (Event.get_current().get_type() == null)
			{
				flag = (InlineCurveEditor.MouseOverTrackArea(backgroundRect, trackRect) || InlineCurveEditor.MouseOverHeaderArea(headerRect, trackRect));
			}
			clipCurveEditorOwner.clipCurveEditor.DrawHeader(headerRect);
			bool selected = !locked && (clipCurveEditorOwner.inlineCurvesSelected || flag);
			using (new EditorGUI.DisabledScope(locked))
			{
				using (new GUIViewportScope(trackRect))
				{
					Rect animEditorRect = backgroundRect;
					animEditorRect.set_y(trackRect.get_y());
					animEditorRect.set_height(trackRect.get_height());
					animEditorRect.set_xMin(Mathf.Max(animEditorRect.get_xMin(), trackRect.get_xMin()));
					animEditorRect.set_xMax(trackRect.get_xMax());
					if (activeRange == Vector2.get_zero())
					{
						activeRange = new Vector2(animEditorRect.get_xMin(), animEditorRect.get_xMax());
					}
					clipCurveEditor.DrawCurveEditor(animEditorRect, state, activeRange, clipCurveEditorOwner.supportsLooping, selected);
				}
			}
			if (flag)
			{
				clipCurveEditorOwner.inlineCurvesSelected = true;
			}
		}

		public void Draw(Rect headerRect, Rect trackRect, TimelineWindow.TimelineState state, float identWidth)
		{
			this.m_TrackRect = trackRect;
			this.m_TrackRect.set_height(this.m_TrackRect.get_height() - 5f);
			if (Event.get_current().get_type() == 7)
			{
				state.quadTree.Insert(this);
			}
			headerRect.set_x(headerRect.get_x() + identWidth);
			headerRect.set_width(headerRect.get_width() - identWidth);
			headerRect.set_x(headerRect.get_x() - DirectorStyles.kBaseIndent);
			headerRect.set_width(headerRect.get_width() + DirectorStyles.kBaseIndent);
			headerRect.set_x(headerRect.get_x() + 4f);
			headerRect.set_width(headerRect.get_width() - 4f);
			this.m_HeaderRect = headerRect;
			EditorGUI.DrawRect(this.m_HeaderRect, DirectorStyles.Instance.customSkin.colorAnimEditorBinding);
			AnimationTrack animationTrack = this.m_TrackGUI.track as AnimationTrack;
			if (animationTrack != null && !animationTrack.inClipMode)
			{
				this.DrawCurveEditorForInfiniteClip(this.m_HeaderRect, this.m_TrackRect, state);
			}
			else
			{
				this.DrawCurveEditorsForClipsOnTrack(this.m_HeaderRect, this.m_TrackRect, state);
			}
			if (Event.get_current().get_type() == 7)
			{
				GUIStyle gUIStyle = new GUIStyle("RL DragHandle");
				gUIStyle.Draw(this.resizeRect, GUIContent.none, false, false, false, false);
			}
			this.m_TrackGUI.DrawLockState(trackRect, state);
			Rect rect = new Rect(headerRect.get_xMax() + 4f, headerRect.get_yMax() - 5f, trackRect.get_width() - 4f, 5f);
			Color color = Handles.get_color();
			Handles.set_color(Color.get_black());
			Handles.DrawAAPolyLine(1f, new Vector3[]
			{
				new Vector3(rect.get_x(), rect.get_yMax(), 0f),
				new Vector3(rect.get_xMax(), rect.get_yMax(), 0f)
			});
			Handles.set_color(color);
			EditorGUIUtility.AddCursorRect(rect, 18);
			this.resizeRect = rect;
		}

		private void DrawCurveEditorForInfiniteClip(Rect headerRect, Rect trackRect, TimelineWindow.TimelineState state)
		{
			if (this.m_TrackGUI.clipCurveEditor != null)
			{
				InlineCurveEditor.DrawCurveEditor(this.m_TrackGUI, state, headerRect, trackRect, Vector2.get_zero(), this.m_TrackGUI.locked);
			}
		}

		private void DrawCurveEditorsForClipsOnTrack(Rect headerRect, Rect trackRect, TimelineWindow.TimelineState state)
		{
			if (this.m_TrackGUI.clips.Count != 0)
			{
				if (Event.get_current().get_type() == 8)
				{
					TimelineClipGUI timelineClipGUI = SelectionManager.SelectedClipGUI().FirstOrDefault((TimelineClipGUI x) => x.parentTrackGUI == this.m_TrackGUI);
					if (timelineClipGUI != null && timelineClipGUI != this.m_LastSelectedClipGUI)
					{
						this.m_LastSelectedClipGUI = timelineClipGUI;
					}
					if (this.m_LastSelectedClipGUI == null)
					{
						this.m_LastSelectedClipGUI = this.m_TrackGUI.clips[0];
					}
				}
				if (this.m_LastSelectedClipGUI != null && this.m_LastSelectedClipGUI.clipCurveEditor != null)
				{
					Rect rect = this.m_LastSelectedClipGUI.rect;
					InlineCurveEditor.DrawCurveEditor(this.m_LastSelectedClipGUI, state, headerRect, trackRect, new Vector2(rect.get_xMin(), rect.get_xMax()), this.m_TrackGUI.locked);
				}
			}
		}
	}
}
