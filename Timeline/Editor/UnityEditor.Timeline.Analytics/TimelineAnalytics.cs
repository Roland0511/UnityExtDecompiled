using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline.Analytics
{
	internal static class TimelineAnalytics
	{
		private class TimelineAnalyticsPreProcess : IPreprocessBuild, IOrderedCallback
		{
			public int callbackOrder
			{
				get
				{
					return 0;
				}
			}

			public void OnPreprocessBuild(BuildTarget target, string path)
			{
				TimelineAnalytics._timelineSceneInfo = new TimelineSceneInfo();
			}
		}

		private class TimelineAnalyticsProcess : IProcessScene, IOrderedCallback
		{
			public int callbackOrder
			{
				get
				{
					return 0;
				}
			}

			public void OnProcessScene(Scene scene)
			{
				IEnumerable<TimelineAsset> enumerable = (from pd in Object.FindObjectsOfType<PlayableDirector>()
				select pd.get_playableAsset()).OfType<TimelineAsset>().Distinct<TimelineAsset>();
				foreach (TimelineAsset current in enumerable)
				{
					if (TimelineAnalytics._timelineSceneInfo.uniqueDirectors.Add(current))
					{
						TimelineAnalytics._timelineSceneInfo.numTracks += current.flattenedTracks.Count<TrackAsset>();
						TimelineAnalytics._timelineSceneInfo.minDuration = Math.Min(TimelineAnalytics._timelineSceneInfo.minDuration, (int)(current.get_duration() * 1000.0));
						TimelineAnalytics._timelineSceneInfo.maxDuration = Math.Max(TimelineAnalytics._timelineSceneInfo.maxDuration, (int)(current.get_duration() * 1000.0));
						TimelineAnalytics._timelineSceneInfo.minNumTracks = Math.Min(TimelineAnalytics._timelineSceneInfo.minNumTracks, current.flattenedTracks.Count<TrackAsset>());
						TimelineAnalytics._timelineSceneInfo.maxNumTracks = Math.Max(TimelineAnalytics._timelineSceneInfo.maxNumTracks, current.flattenedTracks.Count<TrackAsset>());
						foreach (TrackAsset current2 in current.flattenedTracks)
						{
							string name = current2.GetType().Name;
							if (TimelineAnalytics._timelineSceneInfo.trackCount.ContainsKey(name))
							{
								Dictionary<string, int> dictionary;
								string key;
								(dictionary = TimelineAnalytics._timelineSceneInfo.trackCount)[key = name] = dictionary[key] + 1;
							}
							else if (TimelineEventInfo.IsUserType(current2.GetType()))
							{
								Dictionary<string, int> dictionary;
								(dictionary = TimelineAnalytics._timelineSceneInfo.trackCount)["UserType"] = dictionary["UserType"] + 1;
								if (TimelineAnalytics._timelineSceneInfo.userTrackTypesCount.ContainsKey(name))
								{
									string key2;
									(dictionary = TimelineAnalytics._timelineSceneInfo.userTrackTypesCount)[key2 = name] = dictionary[key2] + 1;
								}
								else
								{
									TimelineAnalytics._timelineSceneInfo.userTrackTypesCount[name] = 1;
								}
							}
							else
							{
								Dictionary<string, int> dictionary;
								(dictionary = TimelineAnalytics._timelineSceneInfo.trackCount)["Other"] = dictionary["Other"] + 1;
							}
							if (current2.clips.Any((TimelineClip x) => x.recordable))
							{
								TimelineAnalytics._timelineSceneInfo.numRecorded++;
							}
							else
							{
								AnimationTrack animationTrack = current2 as AnimationTrack;
								if (animationTrack != null)
								{
									if (animationTrack.CanConvertToClipMode())
									{
										TimelineAnalytics._timelineSceneInfo.numRecorded++;
									}
								}
							}
						}
					}
				}
			}
		}

		private class TimelineAnalyticsPostProcess : IPostprocessBuild, IOrderedCallback
		{
			public int callbackOrder
			{
				get
				{
					return 0;
				}
			}

			public void OnPostprocessBuild(BuildTarget target, string path)
			{
				if (TimelineAnalytics._timelineSceneInfo.uniqueDirectors.Count > 0)
				{
					TimelineEventInfo timelineEventInfo = new TimelineEventInfo(TimelineAnalytics._timelineSceneInfo);
					EditorAnalytics.SendEventTimelineInfo(timelineEventInfo);
				}
			}
		}

		private static TimelineSceneInfo _timelineSceneInfo = new TimelineSceneInfo();
	}
}
