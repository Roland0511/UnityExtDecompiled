using System;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[HideInMenu]
	internal class DeleteMarkers : ItemAction<TimelineMarker>
	{
		public override bool Execute(TimelineWindow.TimelineState state, TimelineMarker[] markers)
		{
			state.Stop();
			for (int i = 0; i < markers.Length; i++)
			{
				TimelineMarker timelineMarker = markers[i];
				ITimelineMarkerContainer timelineMarkerContainer = timelineMarker.parentTrack as ITimelineMarkerContainer;
				if (timelineMarkerContainer != null)
				{
					timelineMarkerContainer.RemoveMarker(timelineMarker);
				}
			}
			SelectionManager.Clear();
			state.Refresh(true);
			return true;
		}
	}
}
