using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class TimelinePanManipulator : Manipulator
	{
		private readonly CursorInfo m_Cursor = new CursorInfo();

		private bool m_Active;

		private static bool IsOverAnimEditor(Event evt, TimelineWindow.TimelineState state)
		{
			List<IBounds> elementsAtPosition = Manipulator.GetElementsAtPosition(state.quadTree, evt.get_mousePosition());
			return elementsAtPosition.Any((IBounds x) => x is InlineCurveEditor);
		}

		public override void Init(IControl parent)
		{
			parent.MouseDown += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				bool result;
				if ((evt.get_button() == 2 && evt.get_modifiers() == null) || (evt.get_button() == 0 && evt.get_modifiers() == 4))
				{
					if (TimelinePanManipulator.IsOverAnimEditor(evt, state))
					{
						result = base.IgnoreEvent();
					}
					else
					{
						this.m_Cursor.cursor = 13;
						Control.AddCursor(this.m_Cursor);
						this.m_Active = true;
						result = base.ConsumeEvent();
					}
				}
				else
				{
					result = base.IgnoreEvent();
				}
				return result;
			};
			parent.MouseUp += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				if (this.m_Active)
				{
					Control.RemoveCursor(this.m_Cursor);
					state.editorWindow.Repaint();
				}
				return base.IgnoreEvent();
			};
			parent.MouseDrag += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				bool result;
				if (!this.m_Active)
				{
					result = base.IgnoreEvent();
				}
				else
				{
					Rect treeviewBounds = TimelineWindow.instance.treeviewBounds;
					treeviewBounds.set_xMax(TimelineWindow.instance.get_position().get_xMax());
					treeviewBounds.set_yMax(TimelineWindow.instance.get_position().get_yMax());
					if ((evt.get_button() == 2 && evt.get_modifiers() == null) || (evt.get_button() == 0 && evt.get_modifiers() == 4))
					{
						if (state.GetWindow() != null && state.GetWindow().treeView != null)
						{
							Vector2 scrollPosition = state.GetWindow().treeView.scrollPosition;
							scrollPosition.y -= evt.get_delta().y;
							state.GetWindow().treeView.scrollPosition = scrollPosition;
							state.OffsetTimeArea((int)evt.get_delta().x);
							result = base.ConsumeEvent();
							return result;
						}
					}
					result = base.IgnoreEvent();
				}
				return result;
			};
		}
	}
}
