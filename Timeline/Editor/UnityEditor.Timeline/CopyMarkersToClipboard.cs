using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[HideInMenu]
	internal class CopyMarkersToClipboard : ItemAction<TimelineMarker>
	{
		public static bool Do(TimelineWindow.TimelineState state, TimelineMarker theMarker)
		{
			TimelineMarker[] items = new TimelineMarker[]
			{
				theMarker
			};
			return ItemAction<TimelineMarker>.DoInternal(typeof(CopyMarkersToClipboard), state, items);
		}

		public static bool Do(TimelineWindow.TimelineState state, TimelineMarker[] markers)
		{
			return ItemAction<TimelineMarker>.DoInternal(typeof(CopyMarkersToClipboard), state, markers);
		}

		public override bool Execute(TimelineWindow.TimelineState state, TimelineMarker[] markers)
		{
			Clipboard.AddDataCollection(from x in markers
			select EditorItemFactory.GetEditorMarker(x));
			return true;
		}
	}
}
