using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[CustomTrackDrawer(typeof(AnimationTrack))]
	internal class AnimationTrackDrawer : TrackDrawer
	{
		private static class Styles
		{
			public static readonly GUIContent s_AnimationTrackIcon = EditorGUIUtility.IconContent("AnimationClip Icon");

			public static readonly GUIContent s_AnimationButtonOnTooltip = EditorGUIUtility.TextContent("|Avatar Mask enabled\nClick to disable");

			public static readonly GUIContent s_AnimationButtonOffTooltip = EditorGUIUtility.TextContent("|Avatar Mask disabled\nClick to enable");
		}

		public static GUIContent s_MissingIcon;

		public override Color trackColor
		{
			get
			{
				return DirectorStyles.Instance.customSkin.colorAnimation;
			}
		}

		public override Color GetTrackBackgroundColor(TrackAsset trackAsset)
		{
			return DirectorStyles.Instance.customSkin.colorTrackBackground;
		}

		public override Color GetClipBaseColor(TimelineClip clip)
		{
			Color result;
			if (clip.recordable)
			{
				result = DirectorStyles.Instance.customSkin.colorAnimationRecorded;
			}
			else
			{
				result = DirectorStyles.Instance.customSkin.colorAnimation;
			}
			return result;
		}

		public override GUIContent GetIcon()
		{
			return AnimationTrackDrawer.Styles.s_AnimationTrackIcon;
		}

		public override void OnBuildTrackContextMenu(GenericMenu menu, TrackAsset track, ITimelineState state)
		{
			base.OnBuildTrackContextMenu(menu, track, state);
			bool flag = false;
			AnimationTrack animTrack = track as AnimationTrack;
			if (animTrack != null)
			{
				if (animTrack.CanConvertFromClipMode() || animTrack.CanConvertToClipMode())
				{
					bool flag2 = animTrack.CanConvertFromClipMode();
					bool flag3 = animTrack.CanConvertToClipMode();
					if (flag2)
					{
						menu.AddItem(EditorGUIUtility.TextContent("Convert To Infinite Clip"), false, delegate(object parentTrack)
						{
							animTrack.ConvertFromClipMode(state.timeline);
						}, track);
						flag = true;
					}
					if (flag3)
					{
						menu.AddItem(EditorGUIUtility.TextContent("Convert To Clip Track"), false, delegate(object parentTrack)
						{
							animTrack.ConvertToClipMode();
							state.Refresh();
						}, track);
					}
				}
			}
			if (!track.isSubTrack)
			{
				if (flag)
				{
					menu.AddSeparator("");
				}
				menu.AddItem(EditorGUIUtility.TextContent("Add Override Track"), false, delegate(object parentTrack)
				{
					AnimationTrackDrawer.AddSubTrack(state, typeof(AnimationTrack), "Override " + track.subTracks.Count.ToString(), track);
				}, track);
			}
		}

		private static void AddSubTrack(ITimelineState state, Type trackOfType, string trackName, TrackAsset track)
		{
			TrackAsset childAsset = state.timeline.CreateTrack(trackOfType, track, trackName);
			TimelineCreateUtilities.SaveAssetIntoObject(childAsset, track);
			track.SetCollapsed(false);
			state.Refresh();
		}

		public override void DrawClip(TrackDrawer.ClipDrawData drawData)
		{
			TimelineClip clip = drawData.clip;
			if (clip.animationClip == null)
			{
				base.DrawClip(drawData);
			}
			else
			{
				bool flag = false;
				if (clip.asset != null)
				{
					AnimationPlayableAsset animationPlayableAsset = clip.asset as AnimationPlayableAsset;
					if (animationPlayableAsset != null)
					{
						flag = (animationPlayableAsset.clip == null);
					}
				}
				if (flag)
				{
					if (AnimationTrackDrawer.s_MissingIcon == null)
					{
						Texture2D errorIcon = EditorGUIUtility.get_errorIcon();
						AnimationTrackDrawer.s_MissingIcon = new GUIContent(errorIcon, "This clip has no animation assigned");
					}
					base.DrawClipErrorIcon(drawData, AnimationTrackDrawer.s_MissingIcon);
				}
				base.DrawClip(drawData);
			}
		}

		public override void OnBuildClipContextMenu(GenericMenu menu, TimelineClip[] clips, ITimelineState state)
		{
			AnimationOffsetMenu.OnClipMenu(state, clips, menu);
		}

		public override bool DrawTrackHeaderButton(Rect rect, TrackAsset track, ITimelineState state)
		{
			AnimationTrack animationTrack = track as AnimationTrack;
			bool flag = animationTrack != null && animationTrack.avatarMask != null;
			if (flag)
			{
				GUIStyle gUIStyle = (!animationTrack.applyAvatarMask) ? DirectorStyles.Instance.avatarMaskOff : DirectorStyles.Instance.avatarMaskOn;
				GUIContent gUIContent = (!animationTrack.applyAvatarMask) ? AnimationTrackDrawer.Styles.s_AnimationButtonOffTooltip : AnimationTrackDrawer.Styles.s_AnimationButtonOnTooltip;
				if (GUI.Button(rect, gUIContent, gUIStyle))
				{
					animationTrack.applyAvatarMask = !animationTrack.applyAvatarMask;
					if (state != null)
					{
						state.rebuildGraph = true;
					}
				}
			}
			return flag;
		}
	}
}
