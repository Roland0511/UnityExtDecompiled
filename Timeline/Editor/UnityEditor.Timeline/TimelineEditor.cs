using System;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	public static class TimelineEditor
	{
		public static PlayableDirector playableDirector
		{
			get
			{
				PlayableDirector result;
				if (TimelineWindow.instance == null)
				{
					result = null;
				}
				else if (TimelineWindow.instance.state == null)
				{
					result = null;
				}
				else
				{
					result = TimelineWindow.instance.state.currentDirector;
				}
				return result;
			}
		}

		public static TimelineAsset timelineAsset
		{
			get
			{
				TimelineAsset result;
				if (TimelineWindow.instance == null)
				{
					result = null;
				}
				else
				{
					result = TimelineWindow.instance.timeline;
				}
				return result;
			}
		}
	}
}
