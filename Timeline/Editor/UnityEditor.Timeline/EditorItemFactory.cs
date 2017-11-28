using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class EditorItemFactory
	{
		private static Dictionary<ITimelineItem, IEditorItem> m_EditorCache = new Dictionary<ITimelineItem, IEditorItem>();

		private static U GetEditorItem<T, U>(T item) where T : ITimelineItem where U : EditorItem<T>
		{
			if (item == null)
			{
				throw new ArgumentException("parameter cannot be null");
			}
			U result;
			if (EditorItemFactory.m_EditorCache.ContainsKey(item))
			{
				result = (EditorItemFactory.m_EditorCache[item] as U);
			}
			else
			{
				TimelineAsset timelineAsset = item.parentTrack.timelineAsset;
				U u = ScriptableObject.CreateInstance<U>();
				u.set_hideFlags(u.get_hideFlags() | 5);
				u.lastHash = -1;
				u.timelineName = ((!(timelineAsset != null)) ? string.Empty : timelineAsset.get_name());
				u.SetItem(item);
				EditorItemFactory.m_EditorCache.Add(item, u);
				result = u;
			}
			return result;
		}

		public static EditorClip GetEditorClip(TimelineClip timelineClip)
		{
			return EditorItemFactory.GetEditorItem<TimelineClip, EditorClip>(timelineClip);
		}

		public static EditorMarker GetEditorMarker(TimelineMarker timelineMarker)
		{
			return EditorItemFactory.GetEditorItem<TimelineMarker, EditorMarker>(timelineMarker);
		}
	}
}
