using System;
using System.ComponentModel;
using System.Linq;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[DisplayName("Copy"), Shortcut("Copy")]
	internal class CopyAction : TimelineAction
	{
		public override bool Execute(TimelineWindow.TimelineState state)
		{
			Clipboard.Clear();
			TimelineClip[] array = SelectionManager.SelectedItems<TimelineClip>().ToArray<TimelineClip>();
			if (array.Length > 0)
			{
				CopyClipsToClipboard.Do(state, array);
			}
			TimelineMarker[] array2 = SelectionManager.SelectedItems<TimelineMarker>().ToArray<TimelineMarker>();
			if (array2.Length > 0)
			{
				CopyMarkersToClipboard.Do(state, array2);
			}
			TrackAsset[] array3 = SelectionManager.SelectedTracks().ToArray<TrackAsset>();
			if (array3.Length > 0)
			{
				CopyTracksToClipboard.Do(state, array3);
			}
			return true;
		}
	}
}
