using System;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor
{
	internal abstract class TimelineItemGUI : Control, IBounds
	{
		protected readonly DirectorStyles m_Styles;

		private Rect m_Rect;

		protected Rect m_ClippedRect;

		protected Rect m_UnclippedRect;

		protected readonly TimelineTrackGUI m_ParentTrack;

		protected int m_ZOrder;

		protected IEditorItem m_EditorItem;

		internal static Dictionary<ITimelineItem, TimelineItemGUI> s_ItemToItemGUI = new Dictionary<ITimelineItem, TimelineItemGUI>();

		public int zOrder
		{
			get
			{
				return this.m_ZOrder;
			}
			set
			{
				this.m_ZOrder = value;
			}
		}

		public Rect rect
		{
			get
			{
				return this.m_Rect;
			}
			set
			{
				this.m_Rect = value;
				if (this.m_Rect.get_width() < 0f)
				{
					this.m_Rect.set_width(1f);
				}
			}
		}

		public override Rect bounds
		{
			get
			{
				return this.rect;
			}
		}

		public Rect boundingRect
		{
			get
			{
				return this.rect;
			}
		}

		public Rect clippedRect
		{
			get
			{
				return this.m_ClippedRect;
			}
			set
			{
				this.m_ClippedRect = value;
			}
		}

		public Rect UnClippedRect
		{
			get
			{
				return this.m_UnclippedRect;
			}
		}

		public TimelineTrackGUI parentTrackGUI
		{
			get
			{
				return this.m_ParentTrack;
			}
		}

		public object selectableObject
		{
			get
			{
				return this.m_EditorItem.item;
			}
		}

		public bool selectable
		{
			get
			{
				return this.m_EditorItem.item != null && !this.m_EditorItem.locked;
			}
		}

		public bool visible
		{
			get;
			set;
		}

		public ITimelineItem item
		{
			get
			{
				return this.m_EditorItem.item;
			}
		}

		protected TimelineItemGUI(TimelineTrackGUI parent)
		{
			this.m_ParentTrack = parent;
			this.m_Styles = DirectorStyles.Instance;
		}

		internal void InvalidateEditor()
		{
			EditorUtility.SetDirty(this.m_EditorItem as Object);
		}
	}
}
