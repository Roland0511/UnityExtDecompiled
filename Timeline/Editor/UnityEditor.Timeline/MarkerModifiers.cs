using System;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class MarkerModifiers
	{
		private const double k_EventInsertionOffset = 0.5;

		public static TimelineMarker Duplicate(TimelineMarker theMarker, PlayableDirector directorComponent)
		{
			return MarkerModifiers.DuplicateAtTime(theMarker, directorComponent, MarkerModifiers.FindEventInsertionTime(theMarker));
		}

		public static double FindEventInsertionTime(TimelineMarker theMarker)
		{
			return theMarker.time + 0.5;
		}

		public static TimelineMarker DuplicateAtTime(TimelineMarker theMarker, TrackAsset track, PlayableDirector directorComponent, double newTime)
		{
			TrackAsset trackAsset = theMarker.parentTrack;
			if (track != null)
			{
				trackAsset = track;
			}
			ITimelineMarkerContainer timelineMarkerContainer = trackAsset as ITimelineMarkerContainer;
			TimelineMarker result;
			if (timelineMarkerContainer == null || trackAsset.timelineAsset == null)
			{
				result = null;
			}
			else if (double.IsInfinity(newTime))
			{
				result = null;
			}
			else
			{
				result = timelineMarkerContainer.CreateMarker(theMarker.key, newTime);
			}
			return result;
		}

		public static TimelineMarker DuplicateAtTime(TimelineMarker theMarker, PlayableDirector directorComponent, double newTime)
		{
			return MarkerModifiers.DuplicateAtTime(theMarker, null, directorComponent, newTime);
		}

		public static bool DuplicateEvents(TimelineMarker[] markers, PlayableDirector directorComponent)
		{
			for (int i = 0; i < markers.Length; i++)
			{
				TimelineMarker theMarker = markers[i];
				MarkerModifiers.Duplicate(theMarker, directorComponent);
			}
			return true;
		}
	}
}
