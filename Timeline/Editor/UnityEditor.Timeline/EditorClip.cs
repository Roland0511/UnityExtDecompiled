using System;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class EditorClip : EditorItem<TimelineClip>
	{
		public TimelineClip clip
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
