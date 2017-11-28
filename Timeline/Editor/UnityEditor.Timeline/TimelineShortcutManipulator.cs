using System;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class TimelineShortcutManipulator : Manipulator
	{
		public override void Init(IControl parent)
		{
			parent.KeyDown += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				bool result;
				if (state.IsCurrentEditingASequencerTextField())
				{
					result = base.IgnoreEvent();
				}
				else
				{
					result = TimelineAction.HandleShortcut(state, evt);
				}
				return result;
			};
			parent.ValidateCommand += ((object target, Event evt, TimelineWindow.TimelineState state) => evt.get_commandName() == "Copy" || evt.get_commandName() == "Paste" || evt.get_commandName() == "Duplicate" || evt.get_commandName() == "SelectAll");
			parent.ExecuteCommand += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				bool result;
				if (evt.get_commandName() == "Copy")
				{
					TimelineAction.InvokeByName("CopyAction", state);
					result = base.ConsumeEvent();
				}
				else if (evt.get_commandName() == "Paste")
				{
					TimelineAction.InvokeByName("PasteAction", state);
					result = base.ConsumeEvent();
				}
				else if (evt.get_commandName() == "Duplicate")
				{
					TimelineAction.InvokeByName("DuplicateAction", state);
					result = base.ConsumeEvent();
				}
				else if (evt.get_commandName() == "SelectAll")
				{
					TimelineAction.InvokeByName("SelectAllAction", state);
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
