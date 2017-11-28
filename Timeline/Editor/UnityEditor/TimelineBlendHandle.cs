using System;
using UnityEditor.Timeline;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor
{
	internal class TimelineBlendHandle : Control, IBounds
	{
		public enum DragDirection
		{
			Left,
			Right
		}

		private Rect m_Rect;

		private DirectorStyles m_Styles;

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

		public TimelineBlendHandle.DragDirection direction
		{
			get;
			private set;
		}

		public TimelineClipGUI clip
		{
			get;
			private set;
		}

		public TimelineBlendHandle(TimelineClipGUI theClip, TimelineBlendHandle.DragDirection direction)
		{
			this.direction = direction;
			this.clip = theClip;
			this.m_Styles = DirectorStyles.Instance;
		}

		public void Draw(Rect clientRect, TimelineWindow.TimelineState state)
		{
			Rect rect = clientRect;
			if (this.direction == TimelineBlendHandle.DragDirection.Left)
			{
				rect.set_width(this.m_Styles.handLeft.get_fixedWidth());
				rect.set_x(rect.get_x() - this.m_Styles.handLeft.get_fixedWidth());
				rect.set_y(rect.get_y() + rect.get_height() - this.m_Styles.handLeft.get_fixedHeight());
				rect.set_height(this.m_Styles.handLeft.get_fixedHeight());
			}
			if (this.direction == TimelineBlendHandle.DragDirection.Right)
			{
				rect.set_x(state.TimeToTimeAreaPixel((double)this.clip.blendingStopsAt));
				rect.set_y(rect.get_y() + 1f);
				rect.set_width(this.m_Styles.handLeft.get_fixedWidth());
				rect.set_height(this.m_Styles.handLeft.get_fixedHeight());
			}
			EditorGUIUtility.AddCursorRect(rect, 19);
			this.m_Rect = rect;
			GUI.Box(rect, GUIContent.none, (this.direction != TimelineBlendHandle.DragDirection.Left) ? this.m_Styles.handRight : this.m_Styles.handLeft);
		}
	}
}
