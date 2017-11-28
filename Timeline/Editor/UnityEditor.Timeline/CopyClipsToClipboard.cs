using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[HideInMenu]
	internal class CopyClipsToClipboard : ItemAction<TimelineClip>
	{
		public static bool Do(TimelineWindow.TimelineState state, TimelineClip clip)
		{
			TimelineClip[] items = new TimelineClip[]
			{
				clip
			};
			return ItemAction<TimelineClip>.DoInternal(typeof(CopyClipsToClipboard), state, items);
		}

		public static bool Do(TimelineWindow.TimelineState state, TimelineClip[] clips)
		{
			return ItemAction<TimelineClip>.DoInternal(typeof(CopyClipsToClipboard), state, clips);
		}

		public override bool Execute(TimelineWindow.TimelineState state, TimelineClip[] clips)
		{
			Clipboard.AddDataCollection(from x in clips
			select EditorItemFactory.GetEditorClip(x));
			return true;
		}
	}
}
