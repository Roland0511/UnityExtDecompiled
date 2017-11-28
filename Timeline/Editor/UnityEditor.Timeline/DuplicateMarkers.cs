using System;
using System.Linq;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[HideInMenu]
	internal class DuplicateMarkers : ItemAction<TimelineMarker>
	{
		public override bool Execute(TimelineWindow.TimelineState state, TimelineMarker[] markers)
		{
			PlayableDirector directorComponent = (state == null) ? null : state.currentDirector;
			bool result;
			if (!markers.Any<TimelineMarker>())
			{
				result = false;
			}
			else
			{
				SelectionManager.Clear();
				for (int i = 0; i < markers.Length; i++)
				{
					TimelineMarker theMarker = markers[i];
					TimelineMarker timelineMarker = MarkerModifiers.Duplicate(theMarker, directorComponent);
					if (timelineMarker != null && state != null)
					{
						SelectionManager.Add(timelineMarker);
					}
				}
				if (state != null)
				{
					state.Refresh();
				}
				result = true;
			}
			return result;
		}
	}
}
