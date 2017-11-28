using System;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class TrackVerticalResize : Manipulator
	{
		private bool m_Captured;

		private bool m_UndoAdded;

		private float m_CapturedHeight;

		private float m_CaptureMouseYPos;

		public override void Init(IControl parent)
		{
			this.m_Captured = false;
			this.m_UndoAdded = false;
			this.m_CapturedHeight = 0f;
			this.m_CaptureMouseYPos = 0f;
			parent.MouseDown += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				TimelineTrackGUI timelineTrackGUI = target as TimelineTrackGUI;
				bool result;
				if (timelineTrackGUI == null)
				{
					result = base.IgnoreEvent();
				}
				else
				{
					Rect rect = RectUtils.Encompass(timelineTrackGUI.headerBounds, timelineTrackGUI.boundingRect);
					rect.set_y(rect.get_yMax() - 5f);
					if (rect.Contains(evt.get_mousePosition()))
					{
						this.m_Captured = true;
						this.m_CapturedHeight = TimelineWindowViewPrefs.GetInlineCurveHeight(timelineTrackGUI.track);
						this.m_CaptureMouseYPos = GUIUtility.GUIToScreenPoint(Event.get_current().get_mousePosition()).y;
						state.captured.Add((IControl)target);
						this.m_UndoAdded = false;
						result = base.ConsumeEvent();
					}
					else
					{
						result = base.IgnoreEvent();
					}
				}
				return result;
			};
			parent.MouseDrag += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				bool result;
				if (!this.m_Captured)
				{
					result = base.IgnoreEvent();
				}
				else
				{
					TimelineTrackGUI timelineTrackGUI = target as TimelineTrackGUI;
					if (timelineTrackGUI == null)
					{
						result = base.IgnoreEvent();
					}
					else
					{
						if (!this.m_UndoAdded)
						{
							TimelineUndo.PushUndo(timelineTrackGUI.track, "Set Track Height");
							this.m_UndoAdded = true;
						}
						float num = this.m_CapturedHeight + (GUIUtility.GUIToScreenPoint(Event.get_current().get_mousePosition()).y - this.m_CaptureMouseYPos);
						TimelineWindowViewPrefs.SetInlineCurveHeight(timelineTrackGUI.track, Mathf.Max(num, 60f));
						state.GetWindow().treeView.CalculateRowRects();
						result = base.ConsumeEvent();
					}
				}
				return result;
			};
			parent.MouseUp += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				bool result;
				if (!this.m_Captured)
				{
					result = base.IgnoreEvent();
				}
				else
				{
					state.captured.Remove(target as IControl);
					this.m_Captured = false;
					result = base.ConsumeEvent();
				}
				return result;
			};
		}
	}
}
