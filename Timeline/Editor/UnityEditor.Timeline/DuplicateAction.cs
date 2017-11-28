using System;
using System.ComponentModel;
using System.Linq;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[DisplayName("Duplicate"), Shortcut("Duplicate")]
	internal class DuplicateAction : TimelineAction
	{
		public override bool Execute(TimelineWindow.TimelineState state)
		{
			ITimelineItem[] array = (from x in SelectionManager.SelectedItemGUI()
			select x.selectableObject as ITimelineItem).ToArray<ITimelineItem>();
			if (array.Length > 0)
			{
				ItemActionInvoker.InvokeByName<TimelineClip>("DuplicateClips", state, array);
				ItemActionInvoker.InvokeByName<TimelineMarker>("DuplicateMarkers", state, array);
			}
			TrackAsset[] array2 = SelectionManager.SelectedTracks().ToArray<TrackAsset>();
			if (array2.Length > 0)
			{
				TrackAction.InvokeByName("DuplicateTracks", state, array2);
			}
			return true;
		}
	}
}
