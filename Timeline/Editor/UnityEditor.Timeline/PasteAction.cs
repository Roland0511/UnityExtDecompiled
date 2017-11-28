using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[DisplayName("Paste"), Shortcut("Paste")]
	internal class PasteAction : TimelineAction
	{
		public static bool Do(TimelineWindow.TimelineState state)
		{
			return TimelineAction.DoInternal(typeof(PasteAction), state);
		}

		public override bool Execute(TimelineWindow.TimelineState state)
		{
			IEnumerable<EditorClip> data = Clipboard.GetData<EditorClip>();
			foreach (EditorClip current in data)
			{
				double end = current.clip.parentTrack.clips.Last<TimelineClip>().end;
				current.clip.DuplicateAtTime(current.clip.parentTrack, end, state.currentDirector);
			}
			IEnumerable<EditorMarker> data2 = Clipboard.GetData<EditorMarker>();
			foreach (EditorMarker current2 in data2)
			{
				double newTime = current2.theMarker.parentTrack.end + 0.5;
				MarkerModifiers.DuplicateAtTime(current2.theMarker, state.currentDirector, newTime);
			}
			IEnumerable<TrackAsset> data3 = Clipboard.GetData<TrackAsset>();
			foreach (TrackAsset current3 in data3)
			{
				current3.Duplicate(state.currentDirector, state.timeline);
			}
			state.Refresh();
			return true;
		}
	}
}
