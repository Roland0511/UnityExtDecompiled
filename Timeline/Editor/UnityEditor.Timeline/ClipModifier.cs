using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class ClipModifier
	{
		public static bool Delete(TimelineAsset timeline, TimelineClip clip)
		{
			return timeline.DeleteClip(clip);
		}

		public static bool Delete(TimelineAsset timeline, TimelineClip[] clips)
		{
			for (int i = 0; i < clips.Length; i++)
			{
				TimelineClip clip = clips[i];
				ClipModifier.Delete(timeline, clip);
			}
			return true;
		}

		public static bool Tile(TimelineClip[] clips)
		{
			bool result;
			if (clips.Length < 2)
			{
				result = false;
			}
			else
			{
				var enumerable = from x in clips
				group x by x.parentTrack into track
				select new
				{
					Key = track.Key,
					Items = from c in track
					orderby c.start
					select c
				};
				foreach (var current in enumerable)
				{
					TimelineUndo.PushUndo(current.Key, "Tile");
				}
				foreach (var current2 in enumerable)
				{
					double num = current2.Items.First<TimelineClip>().start;
					foreach (TimelineClip current3 in current2.Items)
					{
						current3.start = num;
						num += current3.duration;
					}
				}
				result = true;
			}
			return result;
		}

		public static bool TrimStart(double trimTime, TimelineClip[] clips)
		{
			bool flag = false;
			for (int i = 0; i < clips.Length; i++)
			{
				TimelineClip clip = clips[i];
				flag |= ClipModifier.TrimStart(trimTime, clip);
			}
			return flag;
		}

		public static bool TrimStart(double trimTime, TimelineClip clip)
		{
			bool result;
			if (clip.asset == null)
			{
				result = false;
			}
			else if (clip.start > trimTime)
			{
				result = false;
			}
			else if (clip.start + clip.duration < trimTime)
			{
				result = false;
			}
			else
			{
				TimelineUndo.PushUndo(clip.parentTrack, "Trim Clip Start");
				clip.clipIn = (trimTime - clip.start) * clip.timeScale;
				clip.duration -= trimTime - clip.start;
				clip.start = trimTime;
				result = true;
			}
			return result;
		}

		public static bool TrimEnd(double trimTime, TimelineClip[] clips)
		{
			bool flag = false;
			for (int i = 0; i < clips.Length; i++)
			{
				TimelineClip clip = clips[i];
				flag |= ClipModifier.TrimEnd(trimTime, clip);
			}
			return flag;
		}

		public static bool TrimEnd(double trimTime, TimelineClip clip)
		{
			bool result;
			if (clip.asset == null)
			{
				result = false;
			}
			else if (clip.start > trimTime)
			{
				result = false;
			}
			else if (clip.start + clip.duration < trimTime)
			{
				result = false;
			}
			else
			{
				TimelineUndo.PushUndo(clip.parentTrack, "Trim Clip End");
				clip.duration = trimTime - clip.start;
				result = true;
			}
			return result;
		}

		public static bool MatchDuration(TimelineClip[] clips)
		{
			double duration = clips[0].duration;
			for (int i = 1; i < clips.Length; i++)
			{
				TimelineUndo.PushUndo(clips[i].parentTrack, "Match Clip Duration");
				clips[i].duration = duration;
			}
			return true;
		}

		public static bool Split(PlayableDirector directorComponent, double splitTime, TimelineClip[] clips)
		{
			bool result = false;
			for (int i = 0; i < clips.Length; i++)
			{
				TimelineClip timelineClip = clips[i];
				if (timelineClip.start <= splitTime)
				{
					if (timelineClip.start + timelineClip.duration >= splitTime)
					{
						TimelineUndo.PushUndo(timelineClip.parentTrack, "Split Clip");
						double duration = timelineClip.duration;
						timelineClip.duration = splitTime - timelineClip.start;
						TimelineClip timelineClip2 = TimelineHelpers.Clone(timelineClip, directorComponent);
						timelineClip2.start = splitTime;
						timelineClip2.clipIn = timelineClip.duration * timelineClip.timeScale + timelineClip.clipIn;
						timelineClip2.duration = duration - timelineClip.duration;
						timelineClip.parentTrack.AddClip(timelineClip2);
						result = true;
					}
				}
			}
			return result;
		}

		public static bool ResetEditing(TimelineClip[] clips)
		{
			bool flag = false;
			for (int i = 0; i < clips.Length; i++)
			{
				TimelineClip clip = clips[i];
				flag |= ClipModifier.ResetEditing(clip);
			}
			return flag;
		}

		public static bool ResetEditing(TimelineClip clip)
		{
			bool result;
			if (clip.asset == null)
			{
				result = false;
			}
			else
			{
				TimelineUndo.PushUndo(clip.parentTrack, "Reset Clip Editing");
				if (clip.clipAssetDuration < 1.7976931348623157E+308)
				{
					clip.duration = clip.clipAssetDuration / clip.timeScale;
				}
				clip.start -= clip.clipIn / clip.timeScale;
				clip.clipIn = 0.0;
				if (clip.start < 0.0)
				{
					clip.start = 0.0;
				}
				result = true;
			}
			return result;
		}

		public static bool CompleteLastLoop(TimelineClip[] clips)
		{
			for (int i = 0; i < clips.Length; i++)
			{
				TimelineClip timelineClip = clips[i];
				if (TimelineHelpers.HasUsableAssetDuration(timelineClip))
				{
					double[] loopTimes = TimelineHelpers.GetLoopTimes(timelineClip);
					double loopDuration = TimelineHelpers.GetLoopDuration(timelineClip);
					TimelineUndo.PushUndo(timelineClip.parentTrack, "Complete Clip Last Loop");
					timelineClip.duration = timelineClip.start + loopTimes.LastOrDefault<double>() + loopDuration;
				}
			}
			return true;
		}

		public static bool CompleteLastLoop(TimelineClip clip)
		{
			TimelineClip[] clips = new TimelineClip[]
			{
				clip
			};
			return ClipModifier.CompleteLastLoop(clips);
		}

		public static bool TrimLastLoop(TimelineClip[] clips)
		{
			for (int i = 0; i < clips.Length; i++)
			{
				TimelineClip timelineClip = clips[i];
				if (TimelineHelpers.HasUsableAssetDuration(timelineClip))
				{
					double[] loopTimes = TimelineHelpers.GetLoopTimes(timelineClip);
					double loopDuration = TimelineHelpers.GetLoopDuration(timelineClip);
					double num = timelineClip.duration - loopTimes.FirstOrDefault<double>();
					if (loopDuration > 0.0)
					{
						num = (timelineClip.duration - loopTimes.FirstOrDefault<double>()) / loopDuration;
					}
					int num2 = Mathf.FloorToInt((float)num);
					if (num2 > 0)
					{
						TimelineUndo.PushUndo(timelineClip.parentTrack, "Trim Clip Last Loop");
						timelineClip.duration = loopTimes.FirstOrDefault<double>() + (double)num2 * loopDuration;
					}
				}
			}
			return true;
		}

		public static bool TrimLastLoop(TimelineClip clip)
		{
			TimelineClip[] clips = new TimelineClip[]
			{
				clip
			};
			return ClipModifier.TrimLastLoop(clips);
		}

		public static bool DoubleSpeed(TimelineClip[] clips)
		{
			for (int i = 0; i < clips.Length; i++)
			{
				TimelineClip timelineClip = clips[i];
				if (timelineClip.SupportsSpeedMultiplier())
				{
					TimelineUndo.PushUndo(timelineClip.parentTrack, "Double Clip Speed");
					timelineClip.timeScale *= 2.0;
					timelineClip.duration *= 0.5;
				}
			}
			return true;
		}

		public static bool HalfSpeed(TimelineClip[] clips)
		{
			for (int i = 0; i < clips.Length; i++)
			{
				TimelineClip timelineClip = clips[i];
				if (timelineClip.SupportsSpeedMultiplier())
				{
					TimelineUndo.PushUndo(timelineClip.parentTrack, "Half Clip Speed");
					timelineClip.timeScale *= 0.5;
					timelineClip.duration *= 2.0;
				}
			}
			return true;
		}

		public static bool ResetSpeed(TimelineClip[] clips)
		{
			for (int i = 0; i < clips.Length; i++)
			{
				TimelineClip timelineClip = clips[i];
				if (timelineClip.timeScale != 1.0)
				{
					TimelineUndo.PushUndo(timelineClip.parentTrack, "Reset Clip Speed");
					timelineClip.duration *= timelineClip.timeScale;
					timelineClip.timeScale = 1.0;
				}
			}
			return true;
		}

		public static TimelineClip DuplicateClip(PlayableDirector directorComponent, TimelineClip clip)
		{
			return clip.Duplicate(directorComponent);
		}

		public static bool DuplicateClips(PlayableDirector directorComponent, TimelineClip[] clips)
		{
			for (int i = 0; i < clips.Length; i++)
			{
				TimelineClip clip = clips[i];
				ClipModifier.DuplicateClip(directorComponent, clip);
			}
			return true;
		}
	}
}
