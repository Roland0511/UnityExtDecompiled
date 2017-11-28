using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal static class TrackExtensions
	{
		public static readonly double kMinOverlapTime = TimeUtility.kTimeEpsilon * 1000.0;

		public static AnimationClip GetOrCreateClip(this TrackAsset track)
		{
			bool flag = false;
			AnimationTrack animationTrack = track as AnimationTrack;
			if (animationTrack != null)
			{
				flag = animationTrack.inClipMode;
			}
			if (track.animClip == null && !flag)
			{
				track.animClip = new AnimationClip();
				track.animClip.set_name(AnimationTrackRecorder.GetUniqueRecordedClipName(track, AnimationTrackRecorder.kRecordClipDefaultName));
				Undo.RegisterCreatedObjectUndo(track.animClip, "Create Track");
				AnimationUtility.SetGenerateMotionCurves(track.animClip, true);
				TimelineHelpers.SaveAnimClipIntoObject(track.animClip, track);
			}
			return track.animClip;
		}

		public static TimelineClip CreateClip(this TrackAsset track, double time)
		{
			object[] customAttributes = track.GetType().GetCustomAttributes(typeof(TrackClipTypeAttribute), true);
			TimelineClip result;
			if (customAttributes.Length == 0)
			{
				result = null;
			}
			else if (TimelineWindow.instance.state == null)
			{
				result = null;
			}
			else if (customAttributes.Length == 1)
			{
				TrackClipTypeAttribute trackClipTypeAttribute = (TrackClipTypeAttribute)customAttributes[0];
				TimelineClip timelineClip = TimelineHelpers.CreateClipOnTrack(trackClipTypeAttribute.inspectedType, track, TimelineWindow.instance.state);
				timelineClip.start = time;
				result = timelineClip;
			}
			else
			{
				result = null;
			}
			return result;
		}

		private static bool Overlaps(TimelineClip blendOut, TimelineClip blendIn)
		{
			bool result;
			if (blendIn == blendOut)
			{
				result = false;
			}
			else if (Math.Abs(blendIn.start - blendOut.start) < TimeUtility.kTimeEpsilon)
			{
				result = (blendIn.duration > blendOut.duration);
			}
			else
			{
				result = (blendIn.start >= blendOut.start && blendIn.start < blendOut.end);
			}
			return result;
		}

		public static void ComputeBlendsFromOverlaps(this TrackAsset asset)
		{
			TrackExtensions.ComputeBlendsFromOverlaps(asset.clips);
		}

		internal static void ComputeBlendsFromOverlaps(TimelineClip[] clips)
		{
			for (int i = 0; i < clips.Length; i++)
			{
				TimelineClip timelineClip = clips[i];
				timelineClip.blendInDuration = -1.0;
				timelineClip.blendOutDuration = -1.0;
			}
			for (int j = 0; j < clips.Length; j++)
			{
				TimelineClip blendIn2 = clips[j];
				TimelineClip blendIn = blendIn2;
				TimelineClip timelineClip2 = (from c in clips
				where TrackExtensions.Overlaps(c, blendIn)
				orderby c.start
				select c).FirstOrDefault<TimelineClip>();
				if (timelineClip2 != null)
				{
					TrackExtensions.UpdateClipIntersection(timelineClip2, blendIn);
				}
			}
		}

		internal static void UpdateClipIntersection(TimelineClip blendOutClip, TimelineClip blendInClip)
		{
			if (blendOutClip.SupportsBlending() && blendInClip.SupportsBlending())
			{
				double num = Math.Max(0.0, blendOutClip.start + blendOutClip.duration - blendInClip.start);
				num = ((num > TrackExtensions.kMinOverlapTime) ? num : 0.0);
				blendOutClip.blendOutDuration = num;
				blendInClip.blendInDuration = num;
				TimelineClip.BlendCurveMode blendInCurveMode = blendInClip.blendInCurveMode;
				TimelineClip.BlendCurveMode blendOutCurveMode = blendOutClip.blendOutCurveMode;
				if (blendInCurveMode == TimelineClip.BlendCurveMode.Manual && blendOutCurveMode == TimelineClip.BlendCurveMode.Auto)
				{
					blendOutClip.mixOutCurve = CurveEditUtility.CreateMatchingCurve(blendInClip.mixInCurve);
				}
				else if (blendInCurveMode == TimelineClip.BlendCurveMode.Auto && blendOutCurveMode == TimelineClip.BlendCurveMode.Manual)
				{
					blendInClip.mixInCurve = CurveEditUtility.CreateMatchingCurve(blendOutClip.mixOutCurve);
				}
				else if (blendInCurveMode == TimelineClip.BlendCurveMode.Auto && blendOutCurveMode == TimelineClip.BlendCurveMode.Auto)
				{
					blendInClip.mixInCurve = null;
					blendOutClip.mixOutCurve = null;
				}
			}
		}

		internal static void UpdateClipIntersectionNoOverlap(TimelineClip blendOutClip, TimelineClip blendInClip)
		{
			blendInClip.start = blendOutClip.end;
		}

		internal static bool MoveClipToTrack(TimelineClip clip, TrackAsset track)
		{
			bool result;
			if (clip == null || track == null || clip.parentTrack == track)
			{
				result = false;
			}
			else
			{
				TimelineUndo.PushUndo(clip.parentTrack, "Move Clip");
				TimelineUndo.PushUndo(track, "Move Clip");
				AnimationTrack animationTrack = track as AnimationTrack;
				if (animationTrack != null)
				{
					animationTrack.ConvertToClipMode();
				}
				TrackAsset parentTrack = clip.parentTrack;
				clip.parentTrack = track;
				parentTrack.CalculateExtrapolationTimes();
				result = true;
			}
			return result;
		}

		internal static void RecursiveSubtrackClone(TrackAsset source, TrackAsset duplicate, PlayableDirector director)
		{
			List<TrackAsset> subTracks = source.subTracks;
			foreach (TrackAsset current in subTracks)
			{
				TrackAsset trackAsset = TimelineHelpers.Clone(duplicate, current, director);
				duplicate.AddChild(trackAsset);
				TrackExtensions.RecursiveSubtrackClone(current, trackAsset, director);
				Undo.RegisterCreatedObjectUndo(trackAsset, "Duplicate");
				TimelineCreateUtilities.SaveAssetIntoObject(trackAsset, source);
			}
		}

		internal static bool Duplicate(this TrackAsset track, PlayableDirector director, TimelineAsset destinationTimeline = null)
		{
			bool result;
			if (track == null)
			{
				result = false;
			}
			else
			{
				if (destinationTimeline == track.timelineAsset)
				{
					destinationTimeline = null;
				}
				TimelineAsset timelineAsset = track.parent as TimelineAsset;
				TrackAsset trackAsset = track.parent as TrackAsset;
				if (timelineAsset == null && trackAsset == null)
				{
					Debug.LogWarning("Cannot duplicate track because it is not parented to known type");
					result = false;
				}
				else
				{
					PlayableAsset playableAsset = destinationTimeline ?? track.parent;
					TrackAsset trackAsset2 = TimelineHelpers.Clone(playableAsset, track, director);
					TrackExtensions.RecursiveSubtrackClone(track, trackAsset2, director);
					Undo.RegisterCreatedObjectUndo(trackAsset2, "Duplicate");
					TimelineCreateUtilities.SaveAssetIntoObject(trackAsset2, playableAsset);
					TimelineUndo.PushUndo(playableAsset, "Duplicate");
					if (destinationTimeline != null)
					{
						destinationTimeline.AddTrackInternal(trackAsset2);
					}
					else if (timelineAsset != null)
					{
						TrackExtensions.ReparentTracks(new List<TrackAsset>
						{
							trackAsset2
						}, timelineAsset, track, false);
					}
					else
					{
						trackAsset.AddChildAfter(trackAsset2, track);
					}
					result = true;
				}
			}
			return result;
		}

		internal static bool ReparentTracks(List<TrackAsset> tracksToMove, PlayableAsset targetParent, TrackAsset insertMarker, bool insertBefore)
		{
			TrackAsset trackAsset = targetParent as TrackAsset;
			TimelineAsset timelineAsset = targetParent as TimelineAsset;
			bool result;
			if (tracksToMove == null || tracksToMove.Count == 0 || (trackAsset == null && timelineAsset == null))
			{
				result = false;
			}
			else
			{
				List<TrackAsset> list = (from x in tracksToMove
				where x.parent != targetParent
				select x).ToList<TrackAsset>();
				if (insertMarker == null && !list.Any<TrackAsset>())
				{
					result = false;
				}
				else
				{
					List<PlayableAsset> list2 = (from x in list
					select x.parent into x
					where x != null
					select x).Distinct<PlayableAsset>().ToList<PlayableAsset>();
					TimelineUndo.PushUndo(targetParent, "Reparent");
					foreach (PlayableAsset current in list2)
					{
						TimelineUndo.PushUndo(current, "Reparent");
					}
					foreach (TrackAsset current2 in list)
					{
						TimelineUndo.PushUndo(current2, "Reparent");
					}
					foreach (TrackAsset current3 in list)
					{
						if (current3.parent != targetParent)
						{
							TrackAsset trackAsset2 = current3.parent as TrackAsset;
							TimelineAsset timelineAsset2 = current3.parent as TimelineAsset;
							if (timelineAsset2 != null)
							{
								timelineAsset2.RemoveTrack(current3);
							}
							else if (trackAsset2 != null)
							{
								trackAsset2.RemoveSubTrack(current3);
							}
							if (trackAsset != null)
							{
								trackAsset.AddChild(current3);
								trackAsset.SetCollapsed(false);
							}
							else
							{
								timelineAsset.AddTrackInternal(current3);
							}
						}
					}
					if (insertMarker != null)
					{
						List<TrackAsset> allTracks = (!(trackAsset != null)) ? timelineAsset.tracks : trackAsset.subTracks;
						TimelineUtility.ReorderTracks(allTracks, tracksToMove, insertMarker, insertBefore);
						if (insertMarker.timelineAsset != null)
						{
							insertMarker.timelineAsset.Invalidate();
						}
					}
					result = true;
				}
			}
			return result;
		}

		internal static Type GetCustomPlayableType(this TrackAsset track)
		{
			Type result;
			if (track == null)
			{
				result = null;
			}
			else
			{
				TrackClipTypeAttribute trackClipTypeAttribute = track.GetType().GetCustomAttributes(typeof(TrackClipTypeAttribute), true).OfType<TrackClipTypeAttribute>().FirstOrDefault((TrackClipTypeAttribute t) => typeof(IPlayableAsset).IsAssignableFrom(t.inspectedType) && typeof(ScriptableObject).IsAssignableFrom(t.inspectedType));
				if (trackClipTypeAttribute != null)
				{
					result = trackClipTypeAttribute.inspectedType;
				}
				else
				{
					result = null;
				}
			}
			return result;
		}

		internal static bool GetCollapsed(this TrackAsset track)
		{
			return TimelineWindowViewPrefs.IsTrackCollapsed(track);
		}

		internal static void SetCollapsed(this TrackAsset track, bool collapsed)
		{
			TimelineWindowViewPrefs.SetTrackCollapsed(track, collapsed);
		}

		internal static bool GetShowInlineCurves(this TrackAsset track)
		{
			return TimelineWindowViewPrefs.GetShowInlineCurves(track);
		}

		internal static void SetShowInlineCurves(this TrackAsset track, bool inlineOn)
		{
			TimelineWindowViewPrefs.SetShowInlineCurves(track, inlineOn);
		}
	}
}
