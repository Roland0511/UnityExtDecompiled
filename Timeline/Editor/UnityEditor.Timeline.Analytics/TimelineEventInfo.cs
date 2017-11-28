using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEditor.Timeline.Analytics
{
	[Serializable]
	internal class TimelineEventInfo
	{
		public int num_timelines;

		public int min_duration;

		public int max_duration;

		public int min_num_tracks;

		public int max_num_tracks;

		public double recorded_percent;

		public List<TrackInfo> track_info = new List<TrackInfo>();

		public string most_popular_user_track = string.Empty;

		public TimelineEventInfo(TimelineSceneInfo sceneInfo)
		{
			this.num_timelines = sceneInfo.uniqueDirectors.Count;
			this.min_duration = sceneInfo.minDuration;
			this.max_duration = sceneInfo.maxDuration;
			this.min_num_tracks = sceneInfo.minNumTracks;
			this.max_num_tracks = sceneInfo.maxNumTracks;
			this.recorded_percent = Math.Round(100.0 * (double)sceneInfo.numRecorded / (double)sceneInfo.numTracks, 1);
			foreach (KeyValuePair<string, int> current in from x in sceneInfo.trackCount
			where x.Value > 0
			select x)
			{
				this.track_info.Add(new TrackInfo
				{
					name = current.Key,
					percent = Math.Round(100.0 * (double)current.Value / (double)sceneInfo.numTracks, 1)
				});
			}
			if (sceneInfo.userTrackTypesCount.Any<KeyValuePair<string, int>>())
			{
				this.most_popular_user_track = sceneInfo.userTrackTypesCount.First((KeyValuePair<string, int> x) => x.Value == sceneInfo.userTrackTypesCount.Values.Max()).Key;
			}
		}

		public static bool IsUserType(Type t)
		{
			string @namespace = t.Namespace;
			return string.IsNullOrEmpty(@namespace) || !@namespace.StartsWith("UnityEngine.Timeline");
		}
	}
}
