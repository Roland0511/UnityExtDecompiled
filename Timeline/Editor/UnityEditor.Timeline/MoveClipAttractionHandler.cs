using System;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class MoveClipAttractionHandler : IAttractionHandler
	{
		public void OnAttractedEdge(IAttractable attractable, AttractedEdge edge, double time, double duration)
		{
			TimelineClipGUI timelineClipGUI = attractable as TimelineClipGUI;
			if (timelineClipGUI != null)
			{
				TimelineClip clip = timelineClipGUI.clip;
				if (edge == AttractedEdge.Left || edge == AttractedEdge.None)
				{
					clip.start = time;
				}
				else
				{
					clip.start = time - clip.duration;
				}
			}
		}
	}
}
