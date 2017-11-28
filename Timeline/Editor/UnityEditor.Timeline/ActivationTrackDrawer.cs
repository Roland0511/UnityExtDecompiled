using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[CustomTrackDrawer(typeof(ActivationTrack))]
	internal class ActivationTrackDrawer : TrackDrawer
	{
		internal static class Styles
		{
			public static readonly GUIContent MenuText = EditorGUIUtility.TextContent("Add Activation Clip");

			public static readonly GUIContent ClipText = EditorGUIUtility.TextContent("Active");
		}

		private static GUIContent s_IconContent;

		public override Color trackColor
		{
			get
			{
				return DirectorStyles.Instance.customSkin.colorActivation;
			}
		}

		public override GUIContent GetIcon()
		{
			if (ActivationTrackDrawer.s_IconContent == null)
			{
				ActivationTrackDrawer.s_IconContent = new GUIContent(DirectorStyles.Instance.activation.get_normal().get_background());
			}
			return ActivationTrackDrawer.s_IconContent;
		}

		public override void OnBuildTrackContextMenu(GenericMenu menu, TrackAsset track, ITimelineState state)
		{
			if (track is ActivationTrack)
			{
				menu.AddItem(ActivationTrackDrawer.Styles.MenuText, false, delegate(object userData)
				{
					TimelineClip timelineClip = TimelineHelpers.CreateClipOnTrack(userData as Type, track, state);
					timelineClip.displayName = ActivationTrackDrawer.Styles.ClipText.get_text();
				}, typeof(ActivationPlayableAsset));
			}
		}
	}
}
