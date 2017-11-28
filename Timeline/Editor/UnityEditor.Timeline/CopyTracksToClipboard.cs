using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[HideInMenu]
	internal class CopyTracksToClipboard : TrackAction
	{
		public static bool Do(TimelineWindow.TimelineState state, TrackAsset[] tracks)
		{
			CopyTracksToClipboard copyTracksToClipboard = new CopyTracksToClipboard();
			return copyTracksToClipboard.Execute(state, tracks);
		}

		public override bool Execute(TimelineWindow.TimelineState state, TrackAsset[] tracks)
		{
			Clipboard.AddDataCollection(from x in tracks
			select x);
			return true;
		}
	}
}
