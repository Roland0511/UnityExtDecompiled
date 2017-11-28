using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal static class ClipExtensions
	{
		public static double FindClipInsertionTime(TimelineClip clip, TrackAsset track)
		{
			return ClipExtensions.FindClipInsertionTime(clip, track.clips);
		}

		private static double FindClipInsertionTime(TimelineClip clip, IEnumerable<TimelineClip> clips)
		{
			List<TimelineClip> list = (from x in clips
			where x.parentTrack == clip.parentTrack
			where x.start >= clip.start
			orderby x.start
			select x).ToList<TimelineClip>();
			double end;
			if (list.Count == 0)
			{
				end = clip.end;
			}
			else
			{
				int num = list.Count - 1;
				TimelineClip timelineClip = list.Last<TimelineClip>();
				if (num == 0)
				{
					end = timelineClip.end;
				}
				else
				{
					for (int num2 = 0; num2 != list.Count; num2++)
					{
						if (num2 == num)
						{
							end = timelineClip.end;
							return end;
						}
						if (list[num2 + 1].start - list[num2].end >= clip.duration)
						{
							end = list[num2].end;
							return end;
						}
					}
					end = timelineClip.end;
				}
			}
			return end;
		}

		public static TimelineClip Duplicate(this TimelineClip clip, PlayableDirector director)
		{
			TrackAsset parentTrack = clip.parentTrack;
			TimelineAsset timelineAsset = parentTrack.timelineAsset;
			TimelineClip result;
			if (parentTrack == null || timelineAsset == null)
			{
				result = null;
			}
			else
			{
				double num = ClipExtensions.FindClipInsertionTime(clip, parentTrack.clips);
				if (double.IsInfinity(num))
				{
					result = null;
				}
				else
				{
					TimelineUndo.PushUndo(parentTrack, "Clone Clip");
					TimelineClip timelineClip = TimelineHelpers.Clone(clip, director);
					timelineClip.start = num;
					clip.parentTrack.AddClip(timelineClip);
					clip.parentTrack.SortClips();
					TrackExtensions.ComputeBlendsFromOverlaps(clip.parentTrack.clips);
					result = timelineClip;
				}
			}
			return result;
		}

		public static TimelineClip DuplicateAtTime(this TimelineClip clip, TrackAsset track, double time, PlayableDirector director)
		{
			TimelineUndo.PushUndo(track, "Clone Clip");
			TimelineClip timelineClip = TimelineHelpers.Clone(clip, director);
			timelineClip.start = time;
			timelineClip.parentTrack = track;
			track.AddClip(timelineClip);
			track.SortClips();
			TrackExtensions.ComputeBlendsFromOverlaps(track.clips);
			return timelineClip;
		}
	}
}
