using System;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class EditorMarker : EditorItem<TimelineMarker>
	{
		public TimelineMarker theMarker
		{
			get
			{
				return this.m_Item;
			}
			set
			{
				this.m_Item = value;
			}
		}
	}
}
