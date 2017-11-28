using System;

namespace UnityEditor.Timeline
{
	internal class MoveEventAttractionHandler : IAttractionHandler
	{
		public void OnAttractedEdge(IAttractable attractable, AttractedEdge edge, double time, double duration)
		{
			TimelineMarkerGUI timelineMarkerGUI = attractable as TimelineMarkerGUI;
			if (timelineMarkerGUI != null)
			{
				timelineMarkerGUI.timelineMarker.time = time;
			}
		}
	}
}
