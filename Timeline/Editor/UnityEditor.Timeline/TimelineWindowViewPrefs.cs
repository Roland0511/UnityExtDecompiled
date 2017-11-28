using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[FilePath]
	internal class TimelineWindowViewPrefs : ScriptableObjectViewPrefs<TimelineAssetViewModel>
	{
		private static TimelineWindowViewPrefs s_Instance;

		public static TimelineWindowViewPrefs instance
		{
			get
			{
				if (TimelineWindowViewPrefs.s_Instance == null)
				{
					TimelineWindowViewPrefs.s_Instance = ScriptableObject.CreateInstance<TimelineWindowViewPrefs>();
					TimelineWindowViewPrefs.s_Instance.set_hideFlags(61);
				}
				return TimelineWindowViewPrefs.s_Instance;
			}
		}

		public static TimelineAssetViewModel GetTimelineAssetViewData(TimelineAsset obj)
		{
			TimelineAssetViewModel result;
			if (obj == null)
			{
				result = TimelineWindowViewPrefs.instance.CreateNewViewModel();
			}
			else
			{
				TimelineWindowViewPrefs.instance.SetActiveAsset(obj);
				result = TimelineWindowViewPrefs.instance.activeViewModel;
			}
			return result;
		}

		public static TrackViewModelData GetTrackViewModelData(TrackAsset track)
		{
			TrackViewModelData result;
			if (track == null)
			{
				result = new TrackViewModelData();
			}
			else if (track.timelineAsset == null)
			{
				result = new TrackViewModelData();
			}
			else
			{
				TimelineAssetViewModel timelineAssetViewData = TimelineWindowViewPrefs.GetTimelineAssetViewData(track.timelineAsset);
				TrackViewModelData trackViewModelData;
				if (timelineAssetViewData.tracksViewModelData.TryGetValue(track, out trackViewModelData))
				{
					result = trackViewModelData;
				}
				else
				{
					trackViewModelData = new TrackViewModelData();
					timelineAssetViewData.tracksViewModelData[track] = trackViewModelData;
					result = trackViewModelData;
				}
			}
			return result;
		}

		public static bool IsTrackCollapsed(TrackAsset track)
		{
			return track == null || TimelineWindowViewPrefs.GetTrackViewModelData(track).collapsed;
		}

		public static void SetTrackCollapsed(TrackAsset track, bool collapsed)
		{
			if (!(track == null))
			{
				TimelineWindowViewPrefs.GetTrackViewModelData(track).collapsed = collapsed;
			}
		}

		public static bool GetShowInlineCurves(TrackAsset track)
		{
			return !(track == null) && TimelineWindowViewPrefs.GetTrackViewModelData(track).showInlineCurves;
		}

		public static void SetShowInlineCurves(TrackAsset track, bool inlineOn)
		{
			if (!(track == null))
			{
				TimelineWindowViewPrefs.GetTrackViewModelData(track).showInlineCurves = inlineOn;
			}
		}

		public static float GetInlineCurveHeight(TrackAsset asset)
		{
			float result;
			if (asset == null)
			{
				result = TrackViewModelData.k_DefaultinlineAnimationCurveHeight;
			}
			else
			{
				result = TimelineWindowViewPrefs.GetTrackViewModelData(asset).inlineAnimationCurveHeight;
			}
			return result;
		}

		public static void SetInlineCurveHeight(TrackAsset asset, float height)
		{
			if (asset != null)
			{
				TimelineWindowViewPrefs.GetTrackViewModelData(asset).inlineAnimationCurveHeight = height;
			}
		}

		public static void Save()
		{
			if (TimelineWindowViewPrefs.instance.activeAsset != null)
			{
				TimelineWindowViewPrefs.instance.Save(TimelineWindowViewPrefs.instance.activeAsset, TimelineWindowViewPrefs.instance.activeViewModel);
			}
		}
	}
}
