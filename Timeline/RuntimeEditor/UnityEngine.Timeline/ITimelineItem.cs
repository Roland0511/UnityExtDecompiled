using System;

namespace UnityEngine.Timeline
{
	internal interface ITimelineItem
	{
		TrackAsset parentTrack
		{
			get;
		}

		double start
		{
			get;
		}

		int Hash();
	}
}
