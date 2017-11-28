using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class MoveEvent : Manipulator
	{
		private static readonly float kDragBufferInPixels = 3f;

		private readonly FrameSnap m_FrameSnap = new FrameSnap();

		private bool m_IsDragDriver;

		private Vector2 m_DragPixelOffset;

		private bool m_IsDragDriverActive;

		private bool m_UndoSet;

		private bool m_HasMoved;

		private readonly ItemSelectionIndicator m_SelectionIndicator = new ItemSelectionIndicator();

		public override void Init(IControl parent)
		{
			bool isCaptured = false;
			MagnetEngine magnetEngine = null;
			parent.MouseDown += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				this.m_UndoSet = false;
				bool result;
				if (evt.get_modifiers() == 4 || evt.get_button() == 2 || evt.get_button() == 1)
				{
					result = this.IgnoreEvent();
				}
				else if (!SelectionManager.IsMouseHoveringOnItem())
				{
					result = this.IgnoreEvent();
				}
				else
				{
					TimelineMarkerGUI timelineMarkerGUI = target as TimelineMarkerGUI;
					if (!SelectionManager.Contains(timelineMarkerGUI.timelineMarker))
					{
						result = this.IgnoreEvent();
					}
					else
					{
						this.m_IsDragDriver = MoveItemUtilities.IsDriver(timelineMarkerGUI, evt, state);
						this.m_FrameSnap.Reset();
						state.captured.Add(target as IControl);
						isCaptured = true;
						if (this.m_IsDragDriver)
						{
							this.m_DragPixelOffset = Vector2.get_zero();
							this.m_IsDragDriverActive = false;
							if (SelectionManager.Count() <= 1 && state.edgeSnaps)
							{
								magnetEngine = new MagnetEngine(timelineMarkerGUI, new MoveEventAttractionHandler(), state);
							}
						}
						result = this.ConsumeEvent();
					}
				}
				return result;
			};
			parent.MouseUp += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				bool result;
				if (!isCaptured)
				{
					result = this.IgnoreEvent();
				}
				else
				{
					magnetEngine = null;
					if (this.m_HasMoved)
					{
						state.rebuildGraph = true;
						this.m_HasMoved = false;
					}
					state.Evaluate();
					state.captured.Remove(target as IControl);
					isCaptured = false;
					this.m_IsDragDriver = false;
					result = this.ConsumeEvent();
				}
				return result;
			};
			parent.DragExited += ((object target, Event evt, TimelineWindow.TimelineState state) => this.IgnoreEvent());
			parent.MouseDrag += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				bool result;
				if (!isCaptured)
				{
					result = this.IgnoreEvent();
				}
				else
				{
					TimelineMarkerGUI timelineMarkerGUI = (TimelineMarkerGUI)target;
					if (this.m_IsDragDriver)
					{
						this.m_DragPixelOffset += evt.get_delta();
						this.m_IsDragDriverActive |= (Math.Abs(this.m_DragPixelOffset.x) > MoveEvent.kDragBufferInPixels);
						if (this.m_IsDragDriverActive)
						{
							float delta = this.m_DragPixelOffset.x / state.timeAreaScale.x;
							this.m_DragPixelOffset = Vector3.get_zero();
							this.SetUndo(state);
							if (SelectionManager.Count() > 1)
							{
								double currentValue = (from x in SelectionManager.SelectedItems<TimelineMarker>()
								select x.time).DefaultIfEmpty(timelineMarkerGUI.timelineMarker.time).Min();
								this.m_FrameSnap.ApplyOffset(currentValue, delta, state);
								foreach (TimelineMarker current in from x in SelectionManager.SelectedItems<TimelineMarker>()
								orderby x.time
								select x)
								{
									if (current.time + this.m_FrameSnap.lastOffsetApplied < 0.0)
									{
										break;
									}
									current.time += this.m_FrameSnap.lastOffsetApplied;
								}
							}
							else
							{
								double num = this.m_FrameSnap.ApplyOffset(timelineMarkerGUI.timelineMarker.time, delta, state);
								if (num < 0.0)
								{
									num = 0.0;
								}
								timelineMarkerGUI.timelineMarker.time = num;
							}
							if (magnetEngine != null)
							{
								magnetEngine.Snap(evt.get_delta().x);
							}
							this.m_HasMoved = true;
							state.Evaluate();
						}
					}
					result = this.ConsumeEvent();
				}
				return result;
			};
			parent.Overlay += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				if (magnetEngine != null)
				{
					magnetEngine.OnGUI();
				}
				bool result;
				if (this.m_IsDragDriver)
				{
					IEnumerable<TimelineMarker> enumerable = SelectionManager.SelectedItems<TimelineMarker>();
					double num = 1.7976931348623157E+308;
					double num2 = -1.7976931348623157E+308;
					foreach (TimelineMarker current in enumerable)
					{
						if (current.time < num)
						{
							num = current.time;
						}
						if (current.time > num2)
						{
							num2 = current.time;
						}
					}
					this.m_SelectionIndicator.Draw(num, num2, magnetEngine);
					result = this.ConsumeEvent();
				}
				else
				{
					result = this.IgnoreEvent();
				}
				return result;
			};
		}

		private void SetUndo(TimelineWindow.TimelineState state)
		{
			if (!this.m_UndoSet)
			{
				IEnumerable<TrackAsset> enumerable = (from x in SelectionManager.SelectedItems<TimelineMarker>()
				select x.parentTrack).Distinct<TrackAsset>();
				foreach (TrackAsset current in enumerable)
				{
					TimelineUndo.PushUndo(current, "event.drag");
				}
				this.m_UndoSet = true;
			}
		}
	}
}
