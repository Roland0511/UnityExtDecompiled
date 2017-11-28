using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Timeline;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor
{
	internal abstract class TimelineTrackBaseGUI : TreeViewItem, IControl, IBounds
	{
		protected List<Control> m_ChildrenControls = new List<Control>();

		protected bool m_IsRoot = false;

		private readonly TimelineTreeViewGUI m_TreeViewGUI;

		private readonly TrackDrawer m_Drawer;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cache0;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cache1;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cache2;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cache3;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cache4;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cache5;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cache6;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cache7;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cache8;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cache9;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cacheA;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cacheB;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cacheC;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cacheD;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cacheE;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cacheF;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cache10;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cache11;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cache12;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cache13;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cache14;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cache15;

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

		public bool isExpanded
		{
			get;
			set;
		}

		public bool isDropTarget
		{
			get;
			set;
		}

		public TrackAsset track
		{
			get;
			set;
		}

		public TreeViewController treeView
		{
			[CompilerGenerated]
			get
			{
				return this.<treeView>k__BackingField;
			}
		}

		public TimelineWindow TimelineWindow
		{
			get
			{
				TimelineWindow result;
				if (this.m_TreeViewGUI == null)
				{
					result = null;
				}
				else
				{
					result = this.m_TreeViewGUI.TimelineWindow;
				}
				return result;
			}
		}

		public bool isRoot
		{
			get
			{
				return this.m_IsRoot;
			}
		}

		public bool selectable
		{
			get
			{
				return true;
			}
		}

		public object selectableObject
		{
			get
			{
				return this.track;
			}
		}

		public TrackDrawer drawer
		{
			get
			{
				return this.m_Drawer;
			}
		}

		public TimelineTrackBaseGUI parentTrackGUI
		{
			get
			{
				return this.get_parent() as TimelineTrackBaseGUI;
			}
		}

		public bool locked
		{
			get
			{
				return this.track.locked;
			}
			set
			{
				if (this.track.locked != value)
				{
					this.OnLockedChanged(value);
					this.track.locked = value;
					TimelineUndo.PushUndo(this.track, "Lock Track");
					TimelineWindow.instance.state.Refresh(true);
				}
			}
		}

		public bool muted
		{
			get
			{
				return this.track.muted;
			}
			set
			{
				this.track.muted = value;
			}
		}

		public abstract Rect boundingRect
		{
			get;
		}

		public abstract Rect headerBounds
		{
			get;
		}

		public abstract bool expandable
		{
			get;
		}

		protected TimelineTrackBaseGUI(int id, int depth, TreeViewItem parent, string displayName, TrackAsset trackAsset, TreeViewController tv, TimelineTreeViewGUI tvgui) : base(id, depth, parent, displayName)
		{
			this.m_Drawer = TrackDrawer.CreateInstance(trackAsset);
			this.m_Drawer.track = trackAsset;
			this.m_Drawer.sequencerState = tvgui.TimelineWindow.state;
			this.m_Drawer.ConfigureUITrack(this);
			this.isExpanded = false;
			this.isDropTarget = false;
			this.track = trackAsset;
			this.<treeView>k__BackingField = tv;
			this.m_TreeViewGUI = tvgui;
			if (TimelineTrackBaseGUI.<>f__mg$cache0 == null)
			{
				TimelineTrackBaseGUI.<>f__mg$cache0 = new TimelineUIEvent(TimelineTrackBaseGUI.NoOp);
			}
			this.MouseDown += TimelineTrackBaseGUI.<>f__mg$cache0;
			if (TimelineTrackBaseGUI.<>f__mg$cache1 == null)
			{
				TimelineTrackBaseGUI.<>f__mg$cache1 = new TimelineUIEvent(TimelineTrackBaseGUI.NoOp);
			}
			this.MouseDrag += TimelineTrackBaseGUI.<>f__mg$cache1;
			if (TimelineTrackBaseGUI.<>f__mg$cache2 == null)
			{
				TimelineTrackBaseGUI.<>f__mg$cache2 = new TimelineUIEvent(TimelineTrackBaseGUI.NoOp);
			}
			this.MouseUp += TimelineTrackBaseGUI.<>f__mg$cache2;
			if (TimelineTrackBaseGUI.<>f__mg$cache3 == null)
			{
				TimelineTrackBaseGUI.<>f__mg$cache3 = new TimelineUIEvent(TimelineTrackBaseGUI.NoOp);
			}
			this.DoubleClick += TimelineTrackBaseGUI.<>f__mg$cache3;
			if (TimelineTrackBaseGUI.<>f__mg$cache4 == null)
			{
				TimelineTrackBaseGUI.<>f__mg$cache4 = new TimelineUIEvent(TimelineTrackBaseGUI.NoOp);
			}
			this.KeyDown += TimelineTrackBaseGUI.<>f__mg$cache4;
			if (TimelineTrackBaseGUI.<>f__mg$cache5 == null)
			{
				TimelineTrackBaseGUI.<>f__mg$cache5 = new TimelineUIEvent(TimelineTrackBaseGUI.NoOp);
			}
			this.KeyUp += TimelineTrackBaseGUI.<>f__mg$cache5;
			if (TimelineTrackBaseGUI.<>f__mg$cache6 == null)
			{
				TimelineTrackBaseGUI.<>f__mg$cache6 = new TimelineUIEvent(TimelineTrackBaseGUI.NoOp);
			}
			this.DragPerform += TimelineTrackBaseGUI.<>f__mg$cache6;
			if (TimelineTrackBaseGUI.<>f__mg$cache7 == null)
			{
				TimelineTrackBaseGUI.<>f__mg$cache7 = new TimelineUIEvent(TimelineTrackBaseGUI.NoOp);
			}
			this.DragExited += TimelineTrackBaseGUI.<>f__mg$cache7;
			if (TimelineTrackBaseGUI.<>f__mg$cache8 == null)
			{
				TimelineTrackBaseGUI.<>f__mg$cache8 = new TimelineUIEvent(TimelineTrackBaseGUI.NoOp);
			}
			this.DragUpdated += TimelineTrackBaseGUI.<>f__mg$cache8;
			if (TimelineTrackBaseGUI.<>f__mg$cache9 == null)
			{
				TimelineTrackBaseGUI.<>f__mg$cache9 = new TimelineUIEvent(TimelineTrackBaseGUI.NoOp);
			}
			this.MouseWheel += TimelineTrackBaseGUI.<>f__mg$cache9;
			if (TimelineTrackBaseGUI.<>f__mg$cacheA == null)
			{
				TimelineTrackBaseGUI.<>f__mg$cacheA = new TimelineUIEvent(TimelineTrackBaseGUI.NoOp);
			}
			this.ContextClick += TimelineTrackBaseGUI.<>f__mg$cacheA;
		}

		public virtual float GetVerticalSpacingBetweenTracks()
		{
			return 3f;
		}

		protected abstract void OnLockedChanged(bool value);

		protected abstract bool DetectProblems(TimelineWindow.TimelineState state);

		public abstract bool IsMouseOver(Vector2 mousePosition);

		public abstract float GetHeight(TimelineWindow.TimelineState state);

		public abstract void SetHeight(float height);

		public abstract void Draw(Rect headerRect, Rect trackRect, TimelineWindow.TimelineState state, float identWidth);

		public abstract bool CanBeSelected(Vector2 mousePosition);

		public abstract void OnGraphRebuilt();

		public static TimelineTrackBaseGUI FindGUITrack(TrackAsset track)
		{
			List<TimelineTrackBaseGUI> allTracks = TimelineWindow.instance.allTracks;
			return allTracks.Find((TimelineTrackBaseGUI x) => x.track == track);
		}

		public virtual void Delete(ITimelineState state)
		{
			DeleteTracks.Do(state.timeline, this.track);
		}

		public void DrawOverlays(Event evt, TimelineWindow.TimelineState state)
		{
			if (this.Overlay != null)
			{
				this.Overlay(this, evt, state);
			}
		}

		protected static bool NoOp(object target, Event e, TimelineWindow.TimelineState state)
		{
			return false;
		}

		private bool HandleChildrenControls(Event evt, TimelineWindow.TimelineState state, bool isCaptureSession)
		{
			bool result;
			for (int num = 0; num != this.m_ChildrenControls.Count; num++)
			{
				if (this.m_ChildrenControls[num].OnEvent(evt, state, isCaptureSession))
				{
					result = true;
					return result;
				}
			}
			result = false;
			return result;
		}

		public virtual bool OnEvent(Event evt, TimelineWindow.TimelineState state, bool isCaptureSession)
		{
			bool flag = false;
			switch (evt.get_type())
			{
			case 0:
				if (evt.get_clickCount() < 2)
				{
					flag = Control.InvokeEvents(this.MouseDown, this, evt, state);
				}
				else
				{
					flag = Control.InvokeEvents(this.DoubleClick, this, evt, state);
				}
				break;
			case 1:
				flag = Control.InvokeEvents(this.MouseUp, this, evt, state);
				break;
			case 3:
				flag = Control.InvokeEvents(this.MouseDrag, this, evt, state);
				break;
			case 4:
				flag = Control.InvokeEvents(this.KeyDown, this, evt, state);
				break;
			case 5:
				flag = Control.InvokeEvents(this.KeyUp, this, evt, state);
				break;
			case 6:
				flag = Control.InvokeEvents(this.MouseWheel, this, evt, state);
				break;
			case 9:
				flag = Control.InvokeEvents(this.DragUpdated, this, evt, state);
				break;
			case 10:
				flag = Control.InvokeEvents(this.DragPerform, this, evt, state);
				break;
			case 13:
				flag = Control.InvokeEvents(this.ValidateCommand, this, evt, state);
				break;
			case 14:
				flag = Control.InvokeEvents(this.ExecuteCommand, this, evt, state);
				break;
			case 15:
				flag = Control.InvokeEvents(this.DragExited, this, evt, state);
				break;
			case 16:
				flag = Control.InvokeEvents(this.ContextClick, this, evt, state);
				break;
			}
			if (!flag)
			{
				flag = this.HandleChildrenControls(evt, state, isCaptureSession);
			}
			if (flag)
			{
				evt.Use();
			}
			return flag;
		}

		public void ClearManipulators()
		{
			this.MouseDown = null;
			this.MouseDrag = null;
			this.MouseWheel = null;
			this.MouseUp = null;
			this.DoubleClick = null;
			this.KeyDown = null;
			this.KeyUp = null;
			this.DragPerform = null;
			this.DragExited = null;
			this.DragUpdated = null;
			this.Overlay = null;
			this.ContextClick = null;
			if (TimelineTrackBaseGUI.<>f__mg$cacheB == null)
			{
				TimelineTrackBaseGUI.<>f__mg$cacheB = new TimelineUIEvent(TimelineTrackBaseGUI.NoOp);
			}
			this.MouseDown += TimelineTrackBaseGUI.<>f__mg$cacheB;
			if (TimelineTrackBaseGUI.<>f__mg$cacheC == null)
			{
				TimelineTrackBaseGUI.<>f__mg$cacheC = new TimelineUIEvent(TimelineTrackBaseGUI.NoOp);
			}
			this.MouseDrag += TimelineTrackBaseGUI.<>f__mg$cacheC;
			if (TimelineTrackBaseGUI.<>f__mg$cacheD == null)
			{
				TimelineTrackBaseGUI.<>f__mg$cacheD = new TimelineUIEvent(TimelineTrackBaseGUI.NoOp);
			}
			this.MouseUp += TimelineTrackBaseGUI.<>f__mg$cacheD;
			if (TimelineTrackBaseGUI.<>f__mg$cacheE == null)
			{
				TimelineTrackBaseGUI.<>f__mg$cacheE = new TimelineUIEvent(TimelineTrackBaseGUI.NoOp);
			}
			this.DoubleClick += TimelineTrackBaseGUI.<>f__mg$cacheE;
			if (TimelineTrackBaseGUI.<>f__mg$cacheF == null)
			{
				TimelineTrackBaseGUI.<>f__mg$cacheF = new TimelineUIEvent(TimelineTrackBaseGUI.NoOp);
			}
			this.KeyDown += TimelineTrackBaseGUI.<>f__mg$cacheF;
			if (TimelineTrackBaseGUI.<>f__mg$cache10 == null)
			{
				TimelineTrackBaseGUI.<>f__mg$cache10 = new TimelineUIEvent(TimelineTrackBaseGUI.NoOp);
			}
			this.KeyUp += TimelineTrackBaseGUI.<>f__mg$cache10;
			if (TimelineTrackBaseGUI.<>f__mg$cache11 == null)
			{
				TimelineTrackBaseGUI.<>f__mg$cache11 = new TimelineUIEvent(TimelineTrackBaseGUI.NoOp);
			}
			this.DragPerform += TimelineTrackBaseGUI.<>f__mg$cache11;
			if (TimelineTrackBaseGUI.<>f__mg$cache12 == null)
			{
				TimelineTrackBaseGUI.<>f__mg$cache12 = new TimelineUIEvent(TimelineTrackBaseGUI.NoOp);
			}
			this.DragExited += TimelineTrackBaseGUI.<>f__mg$cache12;
			if (TimelineTrackBaseGUI.<>f__mg$cache13 == null)
			{
				TimelineTrackBaseGUI.<>f__mg$cache13 = new TimelineUIEvent(TimelineTrackBaseGUI.NoOp);
			}
			this.DragUpdated += TimelineTrackBaseGUI.<>f__mg$cache13;
			if (TimelineTrackBaseGUI.<>f__mg$cache14 == null)
			{
				TimelineTrackBaseGUI.<>f__mg$cache14 = new TimelineUIEvent(TimelineTrackBaseGUI.NoOp);
			}
			this.MouseWheel += TimelineTrackBaseGUI.<>f__mg$cache14;
			if (TimelineTrackBaseGUI.<>f__mg$cache15 == null)
			{
				TimelineTrackBaseGUI.<>f__mg$cache15 = new TimelineUIEvent(TimelineTrackBaseGUI.NoOp);
			}
			this.ContextClick += TimelineTrackBaseGUI.<>f__mg$cache15;
		}

		public void AddManipulator(Manipulator m)
		{
			m.Init(this);
		}

		protected float GetChildrenHeight(TreeViewItem item)
		{
			float num = 0f;
			TimelineTrackBaseGUI timelineTrackBaseGUI = item as TimelineTrackBaseGUI;
			bool flag = timelineTrackBaseGUI != null && timelineTrackBaseGUI.track.GetCollapsed();
			if (item.get_children() != null && !flag)
			{
				IList<TreeViewItem> rows = this.treeView.get_data().GetRows();
				for (int num2 = 0; num2 != item.get_children().Count; num2++)
				{
					TreeViewItem treeViewItem = item.get_children()[num2];
					if (this.treeView.get_data().IsRevealed(treeViewItem.get_id()))
					{
						int num3 = rows.IndexOf(treeViewItem);
						if (num3 >= 0)
						{
							num += this.m_TreeViewGUI.GetRowRect(num3).get_height();
							TimelineGroupGUI timelineGroupGUI = treeViewItem as TimelineGroupGUI;
							if (timelineGroupGUI != null)
							{
								if (timelineGroupGUI.track != null)
								{
									TrackAsset trackAsset = timelineGroupGUI.track.parent as TrackAsset;
									if (trackAsset != null)
									{
										num += 3f;
									}
								}
							}
						}
					}
					num += this.GetChildrenHeight(treeViewItem);
				}
			}
			return num;
		}

		public void DisplayTrackMenu(TimelineWindow.TimelineState state)
		{
			SequencerContextMenu.Show(this.drawer, this.track, Event.get_current().get_mousePosition());
		}
	}
}
