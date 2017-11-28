using System;

namespace UnityEngine.Timeline
{
	public static class TrackAssetExtensions
	{
		public static GroupTrack GetGroup(this TrackAsset asset)
		{
			GroupTrack result;
			if (asset == null)
			{
				result = null;
			}
			else
			{
				result = (asset.parent as GroupTrack);
			}
			return result;
		}

		public static void SetGroup(this TrackAsset asset, GroupTrack group)
		{
			if (!(asset == null) && !(asset == group) && !(asset.parent == group))
			{
				if (group != null && asset.timelineAsset != group.timelineAsset)
				{
					throw new InvalidOperationException("Cannot assign to a group in a different timeline");
				}
				TimelineUndo.PushUndo(asset, "Reparent");
				TimelineAsset timelineAsset = asset.timelineAsset;
				TrackAsset trackAsset = asset.parent as TrackAsset;
				TimelineAsset timelineAsset2 = asset.parent as TimelineAsset;
				if (trackAsset != null || timelineAsset2 != null)
				{
					TimelineUndo.PushUndo(asset.parent, "Reparent");
					if (timelineAsset2 != null)
					{
						timelineAsset2.RemoveTrack(asset);
					}
					else
					{
						trackAsset.RemoveSubTrack(asset);
					}
				}
				if (group == null)
				{
					TimelineUndo.PushUndo(timelineAsset, "Reparent");
					asset.parent = asset.timelineAsset;
					timelineAsset.AddTrackInternal(asset);
				}
				else
				{
					TimelineUndo.PushUndo(group, "Reparent");
					group.AddChild(asset);
				}
			}
		}
	}
}
