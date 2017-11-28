using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class MoveClip : Manipulator
	{
		private static readonly float kDragBufferInPixels = 3f;

		private Rect m_PreviewRect;

		private Vector2 m_PreviewOffset = Vector2.get_zero();

		private Vector2 m_MouseDownPosition = Vector2.get_zero();

		private bool m_HasValidDropTarget;

		private TimelineTrackGUI m_DropTarget;

		private readonly FrameSnap m_FrameSnap = new FrameSnap();

		private bool m_IsDragDriver;

		private bool m_IsVerticalDrag;

		private Vector2 m_DragPixelOffset;

		private bool m_IsDragDriverActive;

		private bool m_UndoSet;

		private readonly ItemSelectionIndicator m_SelectionIndicator = new ItemSelectionIndicator();

		private bool m_IsCaptured;

		private bool m_CaptureOnNextMouseDrag;

		private MagnetEngine m_MagnetEngine;

		public override void Init(IControl parent)
		{
			parent.MouseDown += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				this.m_UndoSet = false;
				this.m_IsCaptured = (this.m_CaptureOnNextMouseDrag = false);
				bool result;
				if (evt.get_modifiers() == 4 || evt.get_button() == 2 || evt.get_button() == 1)
				{
					result = base.IgnoreEvent();
				}
				else if (!SelectionManager.IsMouseHoveringOnItem())
				{
					result = base.IgnoreEvent();
				}
				else
				{
					TimelineClipGUI timelineClipGUI = (TimelineClipGUI)target;
					if (!SelectionManager.Contains(timelineClipGUI.clip))
					{
						result = base.IgnoreEvent();
					}
					else
					{
						this.m_FrameSnap.Reset();
						this.m_CaptureOnNextMouseDrag = true;
						this.m_MouseDownPosition = evt.get_mousePosition();
						this.m_HasValidDropTarget = false;
						result = base.ConsumeEvent();
					}
				}
				return result;
			};
			parent.MouseUp += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				bool result;
				if (!this.m_IsCaptured)
				{
					result = base.IgnoreEvent();
				}
				else
				{
					this.m_MagnetEngine = null;
					if (this.m_IsVerticalDrag)
					{
						state.captured.Clear();
						this.m_IsVerticalDrag = false;
						state.isDragging = false;
						if (this.m_HasValidDropTarget && this.m_DropTarget != null)
						{
							TimelineClipGUI timelineClipGUI = (TimelineClipGUI)target;
							if (TrackExtensions.MoveClipToTrack(timelineClipGUI.clip, this.m_DropTarget.track))
							{
								timelineClipGUI.clip.start = (double)state.PixelToTime(this.m_PreviewRect.get_x());
								timelineClipGUI.parentTrackGUI.SortClipsByStartTime();
								this.m_DropTarget.SortClipsByStartTime();
								state.Refresh();
							}
						}
					}
					state.Evaluate();
					state.captured.Remove(target as IControl);
					this.m_IsCaptured = false;
					this.m_IsDragDriver = false;
					result = base.ConsumeEvent();
				}
				return result;
			};
			parent.DragExited += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				this.m_IsVerticalDrag = false;
				return base.IgnoreEvent();
			};
			parent.MouseDrag += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				if (!this.m_IsCaptured && this.m_CaptureOnNextMouseDrag)
				{
					state.captured.Add(target as IControl);
					this.m_IsCaptured = true;
					this.m_CaptureOnNextMouseDrag = false;
					TimelineClipGUI timelineClipGUI = (TimelineClipGUI)target;
					this.m_IsDragDriver = MoveItemUtilities.IsDriver(timelineClipGUI, evt, state);
					if (this.m_IsDragDriver)
					{
						this.m_DragPixelOffset = Vector2.get_zero();
						this.m_IsDragDriverActive = false;
						if (!SelectionManager.IsMultiSelect() && state.edgeSnaps)
						{
							this.m_MagnetEngine = new MagnetEngine(timelineClipGUI, new MoveClipAttractionHandler(), state);
						}
					}
				}
				bool result;
				if (!this.m_IsCaptured)
				{
					result = base.IgnoreEvent();
				}
				else
				{
					TimelineClipGUI timelineClipGUI2 = (TimelineClipGUI)target;
					if (SelectionManager.Count() == 1)
					{
						TimelineClipGUI timelineClipGUI3 = SelectionManager.SelectedClipGUI().FirstOrDefault<TimelineClipGUI>();
						if (timelineClipGUI3 != null)
						{
							TimelineTrackGUI dropTargetAt = MoveClip.GetDropTargetAt(state, evt.get_mousePosition());
							if (dropTargetAt != null)
							{
								bool flag = dropTargetAt != timelineClipGUI3.parentTrackGUI;
								if (flag && !this.m_IsVerticalDrag)
								{
									state.isDragging = true;
									this.m_HasValidDropTarget = false;
									this.m_DropTarget = null;
									this.m_MouseDownPosition.x = evt.get_mousePosition().x;
									this.m_PreviewOffset = this.m_MouseDownPosition - timelineClipGUI2.bounds.get_position();
								}
								else if (this.m_IsVerticalDrag && !flag)
								{
									state.isDragging = false;
									this.m_HasValidDropTarget = false;
									this.m_DropTarget = null;
									this.m_DragPixelOffset = evt.get_mousePosition() - this.m_MouseDownPosition - evt.get_delta();
								}
								this.m_IsVerticalDrag = flag;
							}
						}
					}
					if (this.m_IsVerticalDrag)
					{
						this.m_PreviewRect = new Rect(evt.get_mousePosition().x, evt.get_mousePosition().y, timelineClipGUI2.bounds.get_width(), timelineClipGUI2.bounds.get_height());
						this.m_PreviewRect.set_position(this.m_PreviewRect.get_position() - this.m_PreviewOffset);
						this.UpdateDragTarget(timelineClipGUI2, evt.get_mousePosition(), state);
						result = base.ConsumeEvent();
					}
					else
					{
						if (this.m_IsDragDriver)
						{
							this.m_DragPixelOffset += evt.get_delta();
							this.m_IsDragDriverActive |= (Math.Abs(this.m_DragPixelOffset.x) > MoveClip.kDragBufferInPixels);
							if (this.m_IsDragDriverActive)
							{
								float delta = this.m_DragPixelOffset.x / state.timeAreaScale.x;
								this.m_DragPixelOffset = Vector3.get_zero();
								this.SetUndo();
								if (SelectionManager.IsMultiSelect())
								{
									double currentValue = (from x in SelectionManager.SelectedItems<TimelineClip>()
									select x.start).DefaultIfEmpty(timelineClipGUI2.clip.start).Min();
									this.m_FrameSnap.ApplyOffset(currentValue, delta, state);
									foreach (TimelineClip current in from x in SelectionManager.SelectedItems<TimelineClip>()
									orderby x.start
									select x)
									{
										if (current.start + this.m_FrameSnap.lastOffsetApplied < 0.0)
										{
											break;
										}
										current.start += this.m_FrameSnap.lastOffsetApplied;
									}
								}
								else
								{
									timelineClipGUI2.clip.start = this.m_FrameSnap.ApplyOffset(timelineClipGUI2.clip.start, delta, state);
								}
								if (this.m_MagnetEngine != null)
								{
									this.m_MagnetEngine.Snap(evt.get_delta().x);
								}
								timelineClipGUI2.InvalidateEditor();
								timelineClipGUI2.parentTrackGUI.SortClipsByStartTime();
								state.Evaluate();
							}
						}
						result = base.ConsumeEvent();
					}
				}
				return result;
			};
			parent.Overlay += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				if (this.m_MagnetEngine != null)
				{
					this.m_MagnetEngine.OnGUI();
				}
				bool result;
				if (this.m_IsVerticalDrag)
				{
					TimelineClipGUI timelineClipGUI = (TimelineClipGUI)target;
					Color color = (!this.m_HasValidDropTarget) ? DirectorStyles.Instance.customSkin.colorInvalidDropTarget : DirectorStyles.Instance.customSkin.colorValidDropTarget;
					timelineClipGUI.DrawDragPreview(this.m_PreviewRect, color);
					result = base.ConsumeEvent();
				}
				else if (this.m_IsDragDriver)
				{
					IEnumerable<TimelineClip> enumerable = SelectionManager.SelectedItems<TimelineClip>();
					double num = 1.7976931348623157E+308;
					double num2 = -1.7976931348623157E+308;
					foreach (TimelineClip current in enumerable)
					{
						if (current.start < num)
						{
							num = current.start;
						}
						if (current.end > num2)
						{
							num2 = current.end;
						}
					}
					this.m_SelectionIndicator.Draw(num, num2, this.m_MagnetEngine);
					result = base.ConsumeEvent();
				}
				else
				{
					result = base.IgnoreEvent();
				}
				return result;
			};
		}

		private void SetUndo()
		{
			if (!this.m_UndoSet)
			{
				IEnumerable<TrackAsset> enumerable = (from x in SelectionManager.SelectedItems<TimelineClip>()
				select x.parentTrack).Distinct<TrackAsset>();
				foreach (TrackAsset current in enumerable)
				{
					TimelineUndo.PushUndo(current, "Drag Clip");
				}
				this.m_UndoSet = true;
			}
		}

		private void UpdateDragTarget(TimelineClipGUI uiClip, Vector2 point, TimelineWindow.TimelineState state)
		{
			List<IBounds> elementsAtPosition = Manipulator.GetElementsAtPosition(state.quadTree, point);
			TimelineTrackGUI timelineTrackGUI = elementsAtPosition.OfType<TimelineTrackGUI>().FirstOrDefault((TimelineTrackGUI t) => MoveClip.ValidateClipDrag(t.track, uiClip.clip));
			this.m_HasValidDropTarget = (timelineTrackGUI != null && timelineTrackGUI.track.IsCompatibleWithItem(uiClip.clip));
			if (this.m_HasValidDropTarget)
			{
				AnimationTrack animationTrack = timelineTrackGUI.track as AnimationTrack;
				float start = state.PixelToTime(this.m_PreviewRect.get_x());
				float end = state.PixelToTime(this.m_PreviewRect.get_xMax());
				bool hasValidDropTarget;
				if (animationTrack != null && animationTrack.CanConvertToClipMode())
				{
					hasValidDropTarget = ((animationTrack.animClip.get_startTime() < start || animationTrack.animClip.get_stopTime() > end) && (start < animationTrack.animClip.get_startTime() || end > animationTrack.animClip.get_stopTime()));
				}
				else
				{
					float num = end - start;
					start = Math.Max(start, 0f);
					end = start + num;
					hasValidDropTarget = (!timelineTrackGUI.track.clips.Any((TimelineClip x) => x.start >= (double)start && x.end <= (double)end) && !timelineTrackGUI.track.clips.Any((TimelineClip x) => (double)start >= x.start && (double)end <= x.end));
				}
				this.m_HasValidDropTarget = hasValidDropTarget;
			}
			this.m_DropTarget = ((!this.m_HasValidDropTarget) ? null : timelineTrackGUI);
		}

		private static TimelineTrackGUI GetDropTargetAt(TimelineWindow.TimelineState state, Vector2 point)
		{
			List<IBounds> elementsAtPosition = Manipulator.GetElementsAtPosition(state.quadTree, point);
			return elementsAtPosition.OfType<TimelineTrackGUI>().FirstOrDefault<TimelineTrackGUI>();
		}

		private static bool ValidateClipDrag(TrackAsset target, TimelineClip clip)
		{
			TrackAsset parentTrack = clip.parentTrack;
			return !target.locked && target.mediaType == parentTrack.mediaType;
		}
	}
}
