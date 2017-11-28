using System;
using System.ComponentModel;
using System.Linq;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[DisplayName("Add Track Sub-Group")]
	internal class AddTrackSubGroup : TrackAction
	{
		public override MenuActionDisplayState GetDisplayState(TimelineWindow.TimelineState state, TrackAsset[] tracks)
		{
			MenuActionDisplayState result;
			if ((from x in tracks
			where x is GroupTrack
			select x).ToArray<TrackAsset>().Length != tracks.Length)
			{
				result = MenuActionDisplayState.Hidden;
			}
			else
			{
				result = MenuActionDisplayState.Visible;
			}
			return result;
		}

		public override bool Execute(TimelineWindow.TimelineState state, TrackAsset[] tracks)
		{
			for (int i = 0; i < tracks.Length; i++)
			{
				TrackAsset trackAsset = tracks[i];
				state.timeline.CreateTrack<GroupTrack>(trackAsset, "Track Sub-Group");
				TimelineTrackBaseGUI timelineTrackBaseGUI = TimelineTrackBaseGUI.FindGUITrack(trackAsset);
				if (timelineTrackBaseGUI != null)
				{
					TimelineWindow.instance.treeView.data.SetExpanded(timelineTrackBaseGUI, true);
				}
			}
			state.Refresh();
			return true;
		}
	}
}
