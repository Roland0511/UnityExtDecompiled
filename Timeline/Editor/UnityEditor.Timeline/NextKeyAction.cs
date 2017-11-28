using System;
using UnityEditor.Timeline.Utilities;

namespace UnityEditor.Timeline
{
	[HideInMenu, Shortcut("NextKey")]
	internal class NextKeyAction : TimelineAction
	{
		public override bool Execute(TimelineWindow.TimelineState state)
		{
			KeyTraverser keyTraverser = new KeyTraverser(state.timeline, 0.01f / state.frameRate);
			float nextKey = keyTraverser.GetNextKey((float)state.time, state.dirtyStamp);
			if ((double)nextKey != state.time)
			{
				state.time = (double)nextKey;
			}
			return true;
		}
	}
}
