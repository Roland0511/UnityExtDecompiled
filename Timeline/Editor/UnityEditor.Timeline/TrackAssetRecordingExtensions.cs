using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal static class TrackAssetRecordingExtensions
	{
		private static readonly Dictionary<TrackAsset, AnimationClip> s_ActiveClips = new Dictionary<TrackAsset, AnimationClip>();

		internal static void OnRecordingArmed(this TrackAsset track, PlayableDirector director)
		{
			if (!(track == null))
			{
				AnimationClip animationClip = track.FindRecordingAnimationClipAtTime(director.get_time());
				if (!(animationClip == null))
				{
					TrackAssetRecordingExtensions.s_ActiveClips[track] = animationClip;
					track.SetShowInlineCurves(true);
				}
			}
		}

		internal static void OnRecordingTimeChanged(this TrackAsset track, PlayableDirector director)
		{
			if (!(track == null))
			{
				AnimationClip animationClip = track.FindRecordingAnimationClipAtTime(director.get_time());
				AnimationClip activeRecordingAnimationClip = track.GetActiveRecordingAnimationClip();
				if (activeRecordingAnimationClip != animationClip)
				{
					TrackAssetRecordingExtensions.s_ActiveClips[track] = animationClip;
				}
			}
		}

		internal static void OnRecordingUnarmed(this TrackAsset track, PlayableDirector director)
		{
			TrackAssetRecordingExtensions.s_ActiveClips.Remove(track);
		}

		internal static AnimationClip GetActiveRecordingAnimationClip(this TrackAsset track)
		{
			AnimationClip result = null;
			TrackAssetRecordingExtensions.s_ActiveClips.TryGetValue(track, out result);
			return result;
		}

		internal static bool IsRecordingToClip(this TrackAsset track, TimelineClip clip)
		{
			bool result;
			if (track == null || clip == null)
			{
				result = false;
			}
			else
			{
				AnimationClip activeRecordingAnimationClip = track.GetActiveRecordingAnimationClip();
				if (activeRecordingAnimationClip == null)
				{
					result = false;
				}
				else if (activeRecordingAnimationClip == clip.curves)
				{
					result = true;
				}
				else
				{
					AnimationPlayableAsset animationPlayableAsset = clip.asset as AnimationPlayableAsset;
					result = (animationPlayableAsset != null && activeRecordingAnimationClip == animationPlayableAsset.clip);
				}
			}
			return result;
		}

		internal static TimelineClip FindRecordingClipAtTime(this TrackAsset track, double time)
		{
			TimelineClip result;
			if (track == null)
			{
				result = null;
			}
			else
			{
				bool flag = track as AnimationTrack != null;
				TimelineClip timelineClip;
				if (flag)
				{
					timelineClip = (from x in track.clips
					where x.recordable && x.start < time + TimeUtility.kTimeEpsilon
					orderby x.start
					select x).LastOrDefault<TimelineClip>();
				}
				else
				{
					timelineClip = (from x in track.clips
					where x.start < time + TimeUtility.kTimeEpsilon && x.HasAnyAnimatableParameters()
					orderby x.start
					select x).LastOrDefault<TimelineClip>();
				}
				result = timelineClip;
			}
			return result;
		}

		internal static AnimationClip FindRecordingAnimationClipAtTime(this TrackAsset trackAsset, double time)
		{
			AnimationClip result;
			if (trackAsset == null)
			{
				result = null;
			}
			else
			{
				AnimationTrack animationTrack = trackAsset as AnimationTrack;
				if (animationTrack != null && !animationTrack.inClipMode)
				{
					result = animationTrack.animClip;
				}
				else
				{
					TimelineClip timelineClip = trackAsset.FindRecordingClipAtTime(time);
					if (timelineClip != null)
					{
						AnimationPlayableAsset animationPlayableAsset = timelineClip.asset as AnimationPlayableAsset;
						if (animationPlayableAsset != null)
						{
							result = animationPlayableAsset.clip;
						}
						else
						{
							AnimatedParameterExtensions.CreateCurvesIfRequired(timelineClip, null);
							result = timelineClip.curves;
						}
					}
					else
					{
						result = null;
					}
				}
			}
			return result;
		}

		internal static void ClearRecordingState()
		{
			TrackAssetRecordingExtensions.s_ActiveClips.Clear();
		}
	}
}
