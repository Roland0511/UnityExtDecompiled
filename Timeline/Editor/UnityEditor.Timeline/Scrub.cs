using System;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class Scrub : Manipulator
	{
		private Action<double, bool> m_OnDrag;

		public Scrub(Action<double, bool> onDrag)
		{
			this.m_OnDrag = onDrag;
		}

		public override void Init(IControl parent)
		{
			bool isCaptured = false;
			parent.MouseDown += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				bool result;
				if (evt.get_button() != 0)
				{
					result = this.IgnoreEvent();
				}
				else
				{
					state.captured.Remove(target as IControl);
					state.captured.Add(target as IControl);
					isCaptured = true;
					result = this.ConsumeEvent();
				}
				return result;
			};
			parent.MouseUp += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				bool result;
				if (evt.get_button() != 0)
				{
					result = this.IgnoreEvent();
				}
				else
				{
					state.captured.Clear();
					isCaptured = false;
					result = this.ConsumeEvent();
				}
				return result;
			};
			parent.MouseDrag += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				bool result;
				if (evt.get_button() != 0)
				{
					result = this.IgnoreEvent();
				}
				else if (!isCaptured)
				{
					result = this.IgnoreEvent();
				}
				else
				{
					if (this.m_OnDrag != null)
					{
						this.m_OnDrag(state.GetSnappedTimeAtMousePosition(evt.get_mousePosition()), false);
					}
					result = this.ConsumeEvent();
				}
				return result;
			};
		}
	}
}
