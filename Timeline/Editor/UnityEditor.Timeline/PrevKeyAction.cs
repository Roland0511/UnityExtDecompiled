using System;
using UnityEditor.Timeline.Utilities;

namespace UnityEditor.Timeline
{
	[HideInMenu, Shortcut("PrevKey")]
	internal class PrevKeyAction : TimelineAction
	{
		public override bool Execute(TimelineWindow.TimelineState state)
		{
			KeyTraverser keyTraverser = new KeyTraverser(state.timeline, 0.01f / state.frameRate);
			float prevKey = keyTraverser.GetPrevKey((float)state.time, state.dirtyStamp);
			if ((double)prevKey != state.time)
			{
				state.time = (double)prevKey;
			}
			return true;
		}
	}
}
