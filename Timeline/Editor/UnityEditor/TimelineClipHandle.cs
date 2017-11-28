using System;
using UnityEditor.Timeline;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor
{
	internal class TimelineClipHandle : Control, IBounds
	{
		public enum DragDirection
		{
			Left,
			Right
		}

		private Rect m_Rect;

		private TimelineClipGUI m_Clip;

		private TimelineClipHandle.DragDirection m_Direction;

		private readonly float k_HandleWidth = 10f;

		public override Rect bounds
		{
			get
			{
				return this.m_Rect;
			}
		}

		public Rect boundingRect
		{
			get
			{
				return this.bounds;
			}
		}

		public TimelineClipHandle.DragDirection direction
		{
			get
			{
				return this.m_Direction;
			}
		}

		public TimelineClipGUI clip
		{
			get
			{
				return this.m_Clip;
			}
		}

		public TimelineClipHandle(TimelineClipGUI theClip, TimelineClipHandle.DragDirection direction, DragClipHandle clipHandleManipulator)
		{
			this.m_Direction = direction;
			this.m_Clip = theClip;
			base.AddManipulator(clipHandleManipulator);
		}

		public void Draw(Rect clientRect)
		{
			Rect rect = clientRect;
			rect.set_width(this.k_HandleWidth);
			if (this.m_Direction == TimelineClipHandle.DragDirection.Left)
			{
				rect.set_width(this.k_HandleWidth);
				rect.set_x(rect.get_x() - 1f);
			}
			if (this.m_Direction == TimelineClipHandle.DragDirection.Right)
			{
				rect.set_x(clientRect.get_xMax() - this.k_HandleWidth);
				rect.set_x(rect.get_x() + 1f);
			}
			EditorGUIUtility.AddCursorRect(rect, 19);
			this.m_Rect = rect;
		}
	}
}
