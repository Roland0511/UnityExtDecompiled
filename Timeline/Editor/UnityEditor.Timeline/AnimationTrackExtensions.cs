using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal static class AnimationTrackExtensions
	{
		public static void ConvertToClipMode(this AnimationTrack track)
		{
			if (track.CanConvertToClipMode())
			{
				TimelineUndo.PushUndo(track, "Convert To Clip");
				if (!track.animClip.get_empty())
				{
					TimelineUndo.PushUndo(track.animClip, "Convert To Clip");
					float num = AnimationClipCurveCache.Instance.GetCurveInfo(track.animClip).keyTimes.FirstOrDefault<float>();
					track.animClip.ShiftBySeconds(-num);
					TimelineClip timelineClip = track.CreateClipFromAsset(track.animClip);
					TimelineCreateUtilities.SaveAssetIntoObject(timelineClip.asset, track);
					timelineClip.start = (double)num;
					timelineClip.preExtrapolationMode = track.openClipPreExtrapolation;
					timelineClip.postExtrapolationMode = track.openClipPostExtrapolation;
					timelineClip.recordable = true;
					if (Math.Abs(timelineClip.duration) < 1.4012984643248171E-45)
					{
						timelineClip.duration = 1.0;
					}
					AnimationPlayableAsset animationPlayableAsset = timelineClip.asset as AnimationPlayableAsset;
					if (animationPlayableAsset)
					{
						animationPlayableAsset.position = track.openClipOffsetPosition;
						animationPlayableAsset.rotation = track.openClipOffsetRotation;
						track.openClipOffsetPosition = Vector3.get_zero();
						track.openClipOffsetRotation = Quaternion.get_identity();
					}
					track.CalculateExtrapolationTimes();
				}
				track.animClip = null;
				EditorUtility.SetDirty(track);
			}
		}

		public static void ConvertFromClipMode(this AnimationTrack track, TimelineAsset timeline)
		{
			if (track.CanConvertFromClipMode())
			{
				TimelineUndo.PushUndo(track, "Convert From Clip");
				TimelineClip timelineClip = track.clips[0];
				float time = (float)timelineClip.start;
				track.openClipTimeOffset = 0.0;
				track.openClipPreExtrapolation = timelineClip.preExtrapolationMode;
				track.openClipPostExtrapolation = timelineClip.postExtrapolationMode;
				AnimationPlayableAsset animationPlayableAsset = timelineClip.asset as AnimationPlayableAsset;
				if (animationPlayableAsset)
				{
					track.openClipOffsetPosition = animationPlayableAsset.position;
					track.openClipOffsetRotation = animationPlayableAsset.rotation;
				}
				AnimationClip animationClip = timelineClip.animationClip;
				float num = (float)timelineClip.timeScale;
				if (!Mathf.Approximately(num, 1f))
				{
					if (!Mathf.Approximately(num, 0f))
					{
						num = 1f / num;
					}
					animationClip.ScaleTime(num);
				}
				TimelineUndo.PushUndo(animationClip, "Convert From Clip");
				animationClip.ShiftBySeconds(time);
				Object asset = timelineClip.asset;
				timelineClip.asset = null;
				ClipModifier.Delete(timeline, timelineClip);
				TimelineUndo.PushDestroyUndo(null, track, asset, "Convert From Clip");
				track.animClip = animationClip;
				EditorUtility.SetDirty(track);
			}
		}

		public static bool CanConvertToClipMode(this AnimationTrack track)
		{
			return !(track == null) && !track.inClipMode && track.animClip != null && !track.animClip.get_empty();
		}

		public static bool CanConvertFromClipMode(this AnimationTrack track)
		{
			bool result;
			if (track == null || !track.inClipMode || track.clips.Length != 1 || track.clips[0].start < 0.0 || !track.clips[0].recordable)
			{
				result = false;
			}
			else
			{
				AnimationPlayableAsset animationPlayableAsset = track.clips[0].asset as AnimationPlayableAsset;
				result = (!(animationPlayableAsset == null) && TimelineHelpers.HaveSameContainerAsset(track, animationPlayableAsset.clip));
			}
			return result;
		}

		internal static bool ShouldShowInfiniteClipEditor(this AnimationTrack track)
		{
			return track != null && !track.inClipMode && track.animClip != null;
		}
	}
}
