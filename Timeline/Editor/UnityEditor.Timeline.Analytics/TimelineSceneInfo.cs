using System;
using System.Collections.Generic;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline.Analytics
{
	internal class TimelineSceneInfo
	{
		public Dictionary<string, int> trackCount = new Dictionary<string, int>
		{
			{
				"ActivationTrack",
				0
			},
			{
				"AnimationTrack",
				0
			},
			{
				"AudioTrack",
				0
			},
			{
				"ControlTrack",
				0
			},
			{
				"PlayableTrack",
				0
			},
			{
				"UserType",
				0
			},
			{
				"Other",
				0
			}
		};

		public Dictionary<string, int> userTrackTypesCount = new Dictionary<string, int>();

		public HashSet<TimelineAsset> uniqueDirectors = new HashSet<TimelineAsset>();

		public int numTracks = 0;

		public int minDuration = 2147483647;

		public int maxDuration = -2147483648;

		public int minNumTracks = 2147483647;

		public int maxNumTracks = -2147483648;

		public int numRecorded = 0;
	}
}
