using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal static class AnimationOffsetMenu
	{
		public static GUIContent MatchPreviousMenuItem = EditorGUIUtility.TextContent("Match Offsets To Previous Clip");

		public static GUIContent MatchNextMenuItem = EditorGUIUtility.TextContent("Match Offsets To Next Clip");

		public static string MatchFieldsPrefix = "Match Offsets Fields/";

		public static GUIContent ResetOffsetMenuItem = EditorGUIUtility.TextContent("Reset Offsets");

		private static bool EnforcePreviewMode(ITimelineState state)
		{
			state.previewMode = true;
			bool result;
			if (!state.previewMode)
			{
				Debug.LogError("Match clips cannot be completed because preview mode cannot be enabed");
				result = false;
			}
			else
			{
				result = true;
			}
			return result;
		}

		internal static void MatchClipsToPrevious(ITimelineState state, TimelineClip[] clips)
		{
			if (AnimationOffsetMenu.EnforcePreviewMode(state))
			{
				clips = (from x in clips
				orderby x.start
				select x).ToArray<TimelineClip>();
				TimelineClip[] array = clips;
				for (int i = 0; i < array.Length; i++)
				{
					TimelineClip timelineClip = array[i];
					TimelineUndo.PushUndo(timelineClip.asset, "Match Clip");
					GameObject sceneGameObject = TimelineUtility.GetSceneGameObject(state.currentDirector, timelineClip.parentTrack);
					TimelineAnimationUtilities.MatchPrevious(timelineClip, sceneGameObject.get_transform(), state.currentDirector);
				}
			}
		}

		internal static void MatchClipsToNext(ITimelineState state, TimelineClip[] clips)
		{
			if (AnimationOffsetMenu.EnforcePreviewMode(state))
			{
				clips = (from x in clips
				orderby x.start descending
				select x).ToArray<TimelineClip>();
				TimelineClip[] array = clips;
				for (int i = 0; i < array.Length; i++)
				{
					TimelineClip timelineClip = array[i];
					TimelineUndo.PushUndo(timelineClip.asset, "Match Clip");
					GameObject sceneGameObject = TimelineUtility.GetSceneGameObject(state.currentDirector, timelineClip.parentTrack);
					TimelineAnimationUtilities.MatchNext(timelineClip, sceneGameObject.get_transform(), state.currentDirector);
				}
			}
		}

		private static void ResetClipOffsets(ITimelineState state, TimelineClip[] clips)
		{
			for (int i = 0; i < clips.Length; i++)
			{
				TimelineClip timelineClip = clips[i];
				if (timelineClip.asset is AnimationPlayableAsset)
				{
					AnimationPlayableAsset animationPlayableAsset = (AnimationPlayableAsset)timelineClip.asset;
					animationPlayableAsset.ResetOffsets();
				}
			}
			state.rebuildGraph = true;
		}

		public static void OnClipMenu(ITimelineState state, TimelineClip[] clips, GenericMenu menu)
		{
			if (!(state.currentDirector == null))
			{
				TimelineClip[] array = (from c in clips
				where c.asset as AnimationPlayableAsset != null && c.parentTrack.clips.Any((TimelineClip x) => x.start < c.start)
				select c).ToArray<TimelineClip>();
				TimelineClip[] array2 = (from c in clips
				where c.asset as AnimationPlayableAsset != null && c.parentTrack.clips.Any((TimelineClip x) => x.start > c.start)
				select c).ToArray<TimelineClip>();
				if (array.Any<TimelineClip>() || array2.Any<TimelineClip>())
				{
					if (array.Any<TimelineClip>())
					{
						menu.AddItem(AnimationOffsetMenu.MatchPreviousMenuItem, false, delegate(object x)
						{
							AnimationOffsetMenu.MatchClipsToPrevious(state, (TimelineClip[])x);
						}, array);
					}
					if (array2.Any<TimelineClip>())
					{
						menu.AddItem(AnimationOffsetMenu.MatchNextMenuItem, false, delegate(object x)
						{
							AnimationOffsetMenu.MatchClipsToNext(state, (TimelineClip[])x);
						}, array2);
					}
					menu.AddItem(AnimationOffsetMenu.ResetOffsetMenuItem, false, delegate
					{
						AnimationOffsetMenu.ResetClipOffsets(state, clips);
					});
				}
			}
		}
	}
}
