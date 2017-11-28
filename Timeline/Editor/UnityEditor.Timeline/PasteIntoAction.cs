using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[DisplayName("Paste Into"), SeparatorMenuItem(SeparatorMenuItemPosition.After)]
	internal class PasteIntoAction : TimelineAction
	{
		public static bool Do(TimelineWindow.TimelineState state, TrackAsset track)
		{
			PasteIntoAction.DoPasteClips(state, track);
			PasteIntoAction.DoPasteMarkers(state, track);
			state.Refresh();
			return true;
		}

		private static void DoPasteClips(TimelineWindow.TimelineState state, TrackAsset track)
		{
			IOrderedEnumerable<TimelineClip> orderedEnumerable = from x in Clipboard.GetData<EditorClip>()
			select x.clip into x
			orderby x.start
			select x;
			if (orderedEnumerable.Any<TimelineClip>())
			{
				double num = ((track.clips.Length != 0) ? track.clips.Last<TimelineClip>().end : 0.0) - orderedEnumerable.FirstOrDefault<TimelineClip>().start;
				foreach (TimelineClip current in orderedEnumerable)
				{
					if (track.IsCompatibleWithItem(current))
					{
						current.DuplicateAtTime(track, current.start + num, state.currentDirector);
					}
				}
			}
		}

		private static void DoPasteMarkers(TimelineWindow.TimelineState state, TrackAsset track)
		{
			if (track is ITimelineMarkerContainer)
			{
				List<TimelineMarker> list = (from x in Clipboard.GetData<EditorMarker>()
				select x.theMarker into x
				orderby x.time
				select x).ToList<TimelineMarker>();
				double num = 0.0;
				if (list.Count<TimelineMarker>() == 0)
				{
					num = 0.0;
				}
				else
				{
					num = list.Last<TimelineMarker>().time + 0.5;
				}
				TimelineMarker timelineMarker = null;
				foreach (TimelineMarker current in list)
				{
					if (current != null)
					{
						if (timelineMarker != null)
						{
							num += current.time - timelineMarker.time;
						}
						MarkerModifiers.DuplicateAtTime(current, track, state.currentDirector, num);
						timelineMarker = current;
					}
				}
			}
		}

		public override MenuActionDisplayState GetDisplayState(TimelineWindow.TimelineState state)
		{
			return (!PasteIntoAction.CanPasteInto(state)) ? MenuActionDisplayState.Disabled : MenuActionDisplayState.Visible;
		}

		public override bool Execute(TimelineWindow.TimelineState state)
		{
			TrackAsset track = SelectionManager.SelectedTracks().First<TrackAsset>();
			return PasteIntoAction.Do(state, track);
		}

		private static bool CanPasteInto(TimelineWindow.TimelineState state)
		{
			IEnumerable<TrackAsset> enumerable = SelectionManager.SelectedTracks();
			return enumerable.Count<TrackAsset>() == 1 && PasteIntoAction.CanPasteItemsInto(enumerable);
		}

		private static bool CanPasteItemsInto(IEnumerable<TrackAsset> tracks)
		{
			List<ITimelineItem> list = (from x in Clipboard.GetData<IEditorItem>()
			select x.item).ToList<ITimelineItem>();
			bool result;
			if (list.Count == 0)
			{
				result = false;
			}
			else
			{
				TrackAsset trackAsset = tracks.First<TrackAsset>();
				TrackAsset parentTrack = list[0].parentTrack;
				foreach (ITimelineItem current in list)
				{
					if (current.parentTrack != parentTrack && !trackAsset.IsCompatibleWithItem(current))
					{
						result = false;
						return result;
					}
				}
				result = true;
			}
			return result;
		}
	}
}
