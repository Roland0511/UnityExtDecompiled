using System;
using System.Collections.Generic;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	[SupportsChildTracks(null, 2147483647), TrackClipType(typeof(TrackAsset)), TrackMediaType(TimelineAsset.MediaType.Group)]
	[Serializable]
	public class GroupTrack : TrackAsset
	{
		internal override bool compilable
		{
			get
			{
				return false;
			}
		}

		public override IEnumerable<PlayableBinding> outputs
		{
			get
			{
				return PlayableBinding.None;
			}
		}
	}
}
