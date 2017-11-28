using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class SequencerContextMenu
	{
		public static void Show(TrackDrawer drawer, TrackAsset track, Vector2 mousePosition)
		{
			GenericMenu genericMenu = new GenericMenu();
			TimelineAction.AddToMenu(genericMenu, TimelineWindow.instance.state);
			genericMenu.AddSeparator("");
			TrackAction.AddToMenu(genericMenu, TimelineWindow.instance.state);
			GroupTrack groupTrack = track as GroupTrack;
			if (groupTrack == null)
			{
				if (drawer != null)
				{
					genericMenu.AddSeparator("");
					drawer.OnBuildTrackContextMenu(genericMenu, track, TimelineWindow.instance.state);
				}
			}
			else
			{
				genericMenu.AddSeparator("");
				TimelineGroupGUI.AddMenuItems(genericMenu, groupTrack);
			}
			genericMenu.ShowAsContext();
		}

		public static void Show(TrackDrawer drawer, Vector2 mousePosition)
		{
			GenericMenu genericMenu = new GenericMenu();
			TimelineAction.AddToMenu(genericMenu, TimelineWindow.instance.state);
			ItemAction<TimelineClip>.AddToMenu(genericMenu, TimelineWindow.instance.state);
			ItemAction<TimelineMarker>.AddToMenu(genericMenu, TimelineWindow.instance.state);
			if (drawer != null)
			{
				TimelineClip[] array = SelectionManager.SelectedItems<TimelineClip>().ToArray<TimelineClip>();
				if (array.Length > 0)
				{
					genericMenu.AddSeparator("");
					drawer.OnBuildClipContextMenu(genericMenu, array, TimelineWindow.instance.state);
				}
			}
			genericMenu.ShowAsContext();
		}
	}
}
