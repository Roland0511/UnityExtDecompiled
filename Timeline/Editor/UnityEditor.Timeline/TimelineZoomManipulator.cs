using System;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class TimelineZoomManipulator : Manipulator
	{
		private static float s_LastMouseDownPosition = 0f;

		private static float s_LastMouseDownTime = -1f;

		private float m_LastMouseMoveX = -1f;

		private float m_FocalTime;

		private const float kTime0Pad = 10f;

		private const float kMaxZoomFactor = 50f;

		internal static void DoZoom(float zoomFactor, TimelineWindow.TimelineState state)
		{
			TimelineZoomManipulator.DoZoom(zoomFactor, state, TimelineZoomManipulator.s_LastMouseDownTime, TimelineZoomManipulator.s_LastMouseDownPosition);
		}

		internal static void DoZoom(float zoomFactor, TimelineWindow.TimelineState state, float focalTime, float pixel)
		{
			zoomFactor = Mathf.Clamp(zoomFactor, -50f, 50f);
			float num = Mathf.Max(0.01f, 1f + zoomFactor * 0.01f);
			Vector2 timeAreaTranslation = state.timeAreaTranslation;
			Vector2 timeAreaScale = state.timeAreaScale;
			timeAreaScale.x = state.timeAreaScale.x * num;
			timeAreaTranslation.x = pixel - focalTime * timeAreaScale.x - state.timeAreaRect.get_x();
			if (Mathf.Abs(timeAreaTranslation.x - 10f) < 1.401298E-45f)
			{
				timeAreaTranslation.x = 10f;
			}
			state.SetTimeAreaTransform(timeAreaTranslation, timeAreaScale);
		}

		public override void Init(IControl parent)
		{
			parent.MouseDown += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				TimelineZoomManipulator.s_LastMouseDownPosition = evt.get_mousePosition().x;
				TimelineZoomManipulator.s_LastMouseDownTime = state.PixelToTime(TimelineZoomManipulator.s_LastMouseDownPosition);
				return base.IgnoreEvent();
			};
			parent.MouseWheel += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				bool result;
				if (evt.get_delta().y == 0f)
				{
					result = base.IgnoreEvent();
				}
				else
				{
					if (this.m_LastMouseMoveX < 0f || Mathf.Abs(this.m_LastMouseMoveX - evt.get_mousePosition().x) > 1f)
					{
						this.m_LastMouseMoveX = evt.get_mousePosition().x;
						this.m_FocalTime = state.PixelToTime(this.m_LastMouseMoveX);
					}
					float num = Event.get_current().get_delta().x + Event.get_current().get_delta().y;
					num = -num;
					TimelineZoomManipulator.DoZoom(num, state, this.m_FocalTime, evt.get_mousePosition().x);
					result = base.ConsumeEvent();
				}
				return result;
			};
			parent.MouseDrag += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				bool result;
				if (evt.get_modifiers() == 4 && evt.get_button() == 1)
				{
					float num = Event.get_current().get_delta().x + Event.get_current().get_delta().y;
					num = -num;
					TimelineZoomManipulator.DoZoom(num, state, TimelineZoomManipulator.s_LastMouseDownTime, TimelineZoomManipulator.s_LastMouseDownPosition);
					result = base.ConsumeEvent();
				}
				else
				{
					result = base.IgnoreEvent();
				}
				return result;
			};
		}
	}
}
