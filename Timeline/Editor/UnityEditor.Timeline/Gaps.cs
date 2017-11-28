using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal static class Gaps
	{
		public static void Insert(TimelineAsset asset, double at, double amount, float tolerance)
		{
			IEnumerable<TimelineClip> enumerable = from x in asset.flattenedTracks.SelectMany((TrackAsset x) => x.clips)
			where x.start - at >= (double)(-(double)tolerance)
			select x;
			foreach (TrackAsset current in (from x in enumerable
			select x.parentTrack).Distinct<TrackAsset>())
			{
				TimelineUndo.PushUndo(current, "Insert Time");
			}
			foreach (TimelineClip current2 in enumerable)
			{
				current2.start += amount;
			}
		}
	}
}
