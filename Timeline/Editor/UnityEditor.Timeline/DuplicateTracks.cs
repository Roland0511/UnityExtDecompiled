using System;
using System.Collections.Generic;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[HideInMenu]
	internal class DuplicateTracks : TrackAction
	{
		public override bool Execute(TimelineWindow.TimelineState state, TrackAsset[] tracks)
		{
			HashSet<TrackAsset> hashSet = new HashSet<TrackAsset>();
			for (int i = 0; i < tracks.Length; i++)
			{
				TrackAsset trackAsset = tracks[i];
				TrackAsset trackAsset2 = trackAsset.parent as TrackAsset;
				bool flag = false;
				while (trackAsset2 != null && !flag)
				{
					if (hashSet.Contains(trackAsset2))
					{
						flag = true;
					}
					trackAsset2 = (trackAsset2.parent as TrackAsset);
				}
				if (!flag)
				{
					hashSet.Add(trackAsset);
				}
			}
			foreach (TrackAsset current in hashSet)
			{
				current.Duplicate(state.currentDirector, null);
			}
			state.Refresh();
			return true;
		}
	}
}
