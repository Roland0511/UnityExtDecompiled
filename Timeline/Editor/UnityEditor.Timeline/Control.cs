using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class Control : IControl
	{
		internal enum MouseDownState
		{
			None,
			SingleClick,
			DoubleClick
		}

		private List<Manipulator> m_Manipulators = new List<Manipulator>();

		private List<Control> m_Children = new List<Control>();

		private Control m_ParentControl;

		private Control.MouseDownState m_MouseDownState = Control.MouseDownState.None;

		private float m_MouseDownTime;

		private static List<CursorInfo> m_Cursors = new List<CursorInfo>();

		public event TimelineUIEvent MouseDown
		{
			add
			{
				TimelineUIEvent timelineUIEvent = this.MouseDown;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.MouseDown, (TimelineUIEvent)Delegate.Combine(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
			remove
			{
				TimelineUIEvent timelineUIEvent = this.MouseDown;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.MouseDown, (TimelineUIEvent)Delegate.Remove(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
		}

		public event TimelineUIEvent MouseDrag
		{
			add
			{
				TimelineUIEvent timelineUIEvent = this.MouseDrag;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.MouseDrag, (TimelineUIEvent)Delegate.Combine(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
			remove
			{
				TimelineUIEvent timelineUIEvent = this.MouseDrag;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.MouseDrag, (TimelineUIEvent)Delegate.Remove(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
		}

		public event TimelineUIEvent MouseWheel
		{
			add
			{
				TimelineUIEvent timelineUIEvent = this.MouseWheel;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.MouseWheel, (TimelineUIEvent)Delegate.Combine(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
			remove
			{
				TimelineUIEvent timelineUIEvent = this.MouseWheel;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.MouseWheel, (TimelineUIEvent)Delegate.Remove(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
		}

		public event TimelineUIEvent MouseUp
		{
			add
			{
				TimelineUIEvent timelineUIEvent = this.MouseUp;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.MouseUp, (TimelineUIEvent)Delegate.Combine(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
			remove
			{
				TimelineUIEvent timelineUIEvent = this.MouseUp;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.MouseUp, (TimelineUIEvent)Delegate.Remove(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
		}

		public event TimelineUIEvent DoubleClick
		{
			add
			{
				TimelineUIEvent timelineUIEvent = this.DoubleClick;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.DoubleClick, (TimelineUIEvent)Delegate.Combine(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
			remove
			{
				TimelineUIEvent timelineUIEvent = this.DoubleClick;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.DoubleClick, (TimelineUIEvent)Delegate.Remove(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
		}

		public event TimelineUIEvent KeyDown
		{
			add
			{
				TimelineUIEvent timelineUIEvent = this.KeyDown;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.KeyDown, (TimelineUIEvent)Delegate.Combine(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
			remove
			{
				TimelineUIEvent timelineUIEvent = this.KeyDown;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.KeyDown, (TimelineUIEvent)Delegate.Remove(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
		}

		public event TimelineUIEvent KeyUp
		{
			add
			{
				TimelineUIEvent timelineUIEvent = this.KeyUp;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.KeyUp, (TimelineUIEvent)Delegate.Combine(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
			remove
			{
				TimelineUIEvent timelineUIEvent = this.KeyUp;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.KeyUp, (TimelineUIEvent)Delegate.Remove(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
		}

		public event TimelineUIEvent DragPerform
		{
			add
			{
				TimelineUIEvent timelineUIEvent = this.DragPerform;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.DragPerform, (TimelineUIEvent)Delegate.Combine(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
			remove
			{
				TimelineUIEvent timelineUIEvent = this.DragPerform;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.DragPerform, (TimelineUIEvent)Delegate.Remove(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
		}

		public event TimelineUIEvent DragExited
		{
			add
			{
				TimelineUIEvent timelineUIEvent = this.DragExited;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.DragExited, (TimelineUIEvent)Delegate.Combine(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
			remove
			{
				TimelineUIEvent timelineUIEvent = this.DragExited;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.DragExited, (TimelineUIEvent)Delegate.Remove(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
		}

		public event TimelineUIEvent DragUpdated
		{
			add
			{
				TimelineUIEvent timelineUIEvent = this.DragUpdated;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.DragUpdated, (TimelineUIEvent)Delegate.Combine(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
			remove
			{
				TimelineUIEvent timelineUIEvent = this.DragUpdated;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.DragUpdated, (TimelineUIEvent)Delegate.Remove(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
		}

		public event TimelineUIEvent Overlay
		{
			add
			{
				TimelineUIEvent timelineUIEvent = this.Overlay;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.Overlay, (TimelineUIEvent)Delegate.Combine(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
			remove
			{
				TimelineUIEvent timelineUIEvent = this.Overlay;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.Overlay, (TimelineUIEvent)Delegate.Remove(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
		}

		public event TimelineUIEvent ContextClick
		{
			add
			{
				TimelineUIEvent timelineUIEvent = this.ContextClick;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.ContextClick, (TimelineUIEvent)Delegate.Combine(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
			remove
			{
				TimelineUIEvent timelineUIEvent = this.ContextClick;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.ContextClick, (TimelineUIEvent)Delegate.Remove(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
		}

		public event TimelineUIEvent ValidateCommand
		{
			add
			{
				TimelineUIEvent timelineUIEvent = this.ValidateCommand;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.ValidateCommand, (TimelineUIEvent)Delegate.Combine(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
			remove
			{
				TimelineUIEvent timelineUIEvent = this.ValidateCommand;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.ValidateCommand, (TimelineUIEvent)Delegate.Remove(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
		}

		public event TimelineUIEvent ExecuteCommand
		{
			add
			{
				TimelineUIEvent timelineUIEvent = this.ExecuteCommand;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.ExecuteCommand, (TimelineUIEvent)Delegate.Combine(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
			remove
			{
				TimelineUIEvent timelineUIEvent = this.ExecuteCommand;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.ExecuteCommand, (TimelineUIEvent)Delegate.Remove(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
		}

		internal static float doubleClickSpeed
		{
			get
			{
				return 0.2f;
			}
		}

		public Control parentControl
		{
			get
			{
				return this.m_ParentControl;
			}
			set
			{
				this.m_ParentControl = value;
			}
		}

		public virtual Rect bounds
		{
			get
			{
				return default(Rect);
			}
		}

		public List<Control> children
		{
			get
			{
				return this.m_Children;
			}
		}

		public Control()
		{
			this.MouseDown += new TimelineUIEvent(this.NoOp);
			this.MouseDrag += new TimelineUIEvent(this.NoOp);
			this.MouseUp += new TimelineUIEvent(this.NoOp);
			this.DoubleClick += new TimelineUIEvent(this.NoOp);
			this.KeyDown += new TimelineUIEvent(this.NoOp);
			this.KeyUp += new TimelineUIEvent(this.NoOp);
			this.DragPerform += new TimelineUIEvent(this.NoOp);
			this.DragExited += new TimelineUIEvent(this.NoOp);
			this.DragUpdated += new TimelineUIEvent(this.NoOp);
			this.MouseWheel += new TimelineUIEvent(this.NoOp);
			this.ContextClick += new TimelineUIEvent(this.NoOp);
			this.ValidateCommand += new TimelineUIEvent(this.NoOp);
			this.ExecuteCommand += new TimelineUIEvent(this.NoOp);
		}

		public virtual bool IsMouseOver(Vector2 mousePosition)
		{
			return this.bounds.Contains(mousePosition);
		}

		public static void AddCursor(CursorInfo ci)
		{
			for (int num = 0; num != Control.m_Cursors.Count; num++)
			{
				if (Control.m_Cursors[num].ID == ci.ID)
				{
					return;
				}
			}
			Control.m_Cursors.Add(ci);
		}

		public static void RemoveCursor(CursorInfo ci)
		{
			int num = -1;
			for (int num2 = 0; num2 != Control.m_Cursors.Count; num2++)
			{
				if (Control.m_Cursors[num2].ID == ci.ID)
				{
					num = num2;
					break;
				}
			}
			if (num > -1)
			{
				Control.m_Cursors.RemoveAt(num);
			}
		}

		public static void DrawCursors()
		{
			Event current = Event.get_current();
			Rect rect = new Rect(current.get_mousePosition().x - 100f, current.get_mousePosition().y - 100f, 200f, 200f);
			foreach (CursorInfo current2 in Control.m_Cursors)
			{
				EditorGUIUtility.AddCursorRect(rect, current2.cursor);
			}
		}

		public void AddChild(Control child)
		{
			this.m_Children.Add(child);
		}

		public virtual bool OnEvent(Event evt, TimelineWindow.TimelineState state, bool isCaptureSession)
		{
			bool flag = false;
			if (!isCaptureSession)
			{
				foreach (Control current in this.m_Children)
				{
					if (!flag && current.bounds.Contains(evt.get_mousePosition()))
					{
						flag = current.OnEvent(evt, state, false);
					}
				}
			}
			bool result;
			if (flag)
			{
				result = true;
			}
			else if (evt == null)
			{
				result = false;
			}
			else
			{
				switch (evt.get_type())
				{
				case 0:
				{
					float num = Time.get_realtimeSinceStartup() - this.m_MouseDownTime;
					if (this.m_MouseDownState == Control.MouseDownState.None)
					{
						this.m_MouseDownState = Control.MouseDownState.SingleClick;
					}
					else if (this.m_MouseDownState == Control.MouseDownState.SingleClick)
					{
						this.m_MouseDownState = ((num >= Control.doubleClickSpeed) ? Control.MouseDownState.SingleClick : Control.MouseDownState.DoubleClick);
					}
					else if (this.m_MouseDownState == Control.MouseDownState.DoubleClick)
					{
						this.m_MouseDownState = Control.MouseDownState.SingleClick;
					}
					this.m_MouseDownTime = Time.get_realtimeSinceStartup();
					if (this.m_MouseDownState == Control.MouseDownState.SingleClick)
					{
						flag = Control.InvokeEvents(this.MouseDown, this, evt, state);
					}
					if (this.m_MouseDownState == Control.MouseDownState.DoubleClick)
					{
						flag = Control.InvokeEvents(this.DoubleClick, this, evt, state);
					}
					break;
				}
				case 1:
					flag = this.MouseUp(this, evt, state);
					break;
				case 3:
					flag = this.MouseDrag(this, evt, state);
					break;
				case 4:
					flag = this.KeyDown(this, evt, state);
					break;
				case 5:
					flag = this.KeyUp(this, evt, state);
					break;
				case 6:
					flag = this.MouseWheel(this, evt, state);
					break;
				case 9:
					flag = this.DragUpdated(this, evt, state);
					break;
				case 10:
					flag = this.DragPerform(this, evt, state);
					break;
				case 13:
					flag = this.ValidateCommand(this, evt, state);
					break;
				case 14:
					flag = this.ExecuteCommand(this, evt, state);
					break;
				case 15:
					flag = this.DragExited(this, evt, state);
					break;
				case 16:
					flag = this.ContextClick(this, evt, state);
					break;
				}
				if (flag)
				{
					evt.Use();
				}
				result = flag;
			}
			return result;
		}

		public virtual void DrawOverlays(Event evt, TimelineWindow.TimelineState state)
		{
			if (this.Overlay != null)
			{
				this.Overlay(this, evt, state);
			}
		}

		public void ClearManipulators()
		{
			this.m_Manipulators.Clear();
		}

		public void AddManipulator(Manipulator m)
		{
			m.Init(this);
			this.m_Manipulators.Add(m);
		}

		private bool NoOp(object target, Event e, TimelineWindow.TimelineState state)
		{
			return false;
		}

		public static bool InvokeEvents(TimelineUIEvent eventList, object target, Event evt, TimelineWindow.TimelineState state)
		{
			bool flag = false;
			bool result;
			if (eventList == null)
			{
				result = false;
			}
			else
			{
				Delegate[] invocationList = eventList.GetInvocationList();
				Delegate[] array = invocationList;
				for (int i = 0; i < array.Length; i++)
				{
					Delegate @delegate = array[i];
					if (@delegate != null)
					{
						flag = (bool)@delegate.DynamicInvoke(new object[]
						{
							target,
							evt,
							state
						});
						if (flag)
						{
							break;
						}
					}
				}
				result = flag;
			}
			return result;
		}

		public static bool IsMouseContainsInControls(IEnumerable<IControl> controls)
		{
			return controls.Any((IControl c) => c.IsMouseOver(Event.get_current().get_mousePosition()));
		}
	}
}
