using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[CustomTrackDrawer(typeof(PlayableTrack))]
	internal class PlayableTrackDrawer : TrackDrawer
	{
		public override Color trackColor
		{
			get
			{
				return Color.get_white();
			}
		}

		public override GUIContent GetIcon()
		{
			return EditorGUIUtility.IconContent("cs Script Icon");
		}

		public override void OnBuildTrackContextMenu(GenericMenu menu, TrackAsset trackAsset, ITimelineState state)
		{
			Type[] allStandalonePlayableAssets = TimelineHelpers.GetAllStandalonePlayableAssets();
			Type[] array = allStandalonePlayableAssets;
			for (int i = 0; i < array.Length; i++)
			{
				Type type = array[i];
				if (!type.IsDefined(typeof(HideInMenuAttribute), true) && !type.IsDefined(typeof(IgnoreOnPlayableTrackAttribute), true))
				{
					string displayName = TrackDrawer.GetDisplayName(type);
					GUIContent gUIContent = new GUIContent("Add Clip/" + displayName);
					menu.AddItem(gUIContent, false, delegate(object userData)
					{
						TimelineHelpers.CreateClipOnTrack(userData as Type, trackAsset, state);
					}, type);
				}
			}
		}
	}
}
