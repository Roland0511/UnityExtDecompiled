using System;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal static class TrackModifier
	{
		public static bool DeleteTrack(TimelineAsset timeline, TrackAsset track)
		{
			return timeline.DeleteTrack(track);
		}
	}
}
