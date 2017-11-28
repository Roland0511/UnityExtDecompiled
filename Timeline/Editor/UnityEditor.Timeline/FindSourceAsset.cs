using System;
using System.ComponentModel;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[DisplayName("Find Source Asset")]
	internal class FindSourceAsset : ItemAction<TimelineClip>
	{
		public override MenuActionDisplayState GetDisplayState(TimelineWindow.TimelineState state, TimelineClip[] clips)
		{
			MenuActionDisplayState result;
			if (clips.Length > 1)
			{
				result = MenuActionDisplayState.Disabled;
			}
			else if (clips[0].underlyingAsset == null || clips[0].underlyingAsset is TimelineAsset)
			{
				result = MenuActionDisplayState.Disabled;
			}
			else
			{
				string assetPath = AssetDatabase.GetAssetPath(clips[0].underlyingAsset);
				result = ((assetPath == null || assetPath.Length <= 0) ? MenuActionDisplayState.Disabled : MenuActionDisplayState.Visible);
			}
			return result;
		}

		public override bool Execute(TimelineWindow.TimelineState state, TimelineClip[] clips)
		{
			EditorGUIUtility.PingObject(clips[0].underlyingAsset);
			return true;
		}
	}
}
