using System;

namespace UnityEditor.Timeline
{
	[HideInMenu]
	internal class SelectAllAction : TimelineAction
	{
		public override bool Execute(TimelineWindow.TimelineState state)
		{
			IClipCurveEditorOwner currentInlineEditorCurve = SelectionManager.GetCurrentInlineEditorCurve();
			bool result;
			if (currentInlineEditorCurve != null && currentInlineEditorCurve.clipCurveEditor != null)
			{
				currentInlineEditorCurve.clipCurveEditor.SelectAllKeys();
				result = true;
			}
			else
			{
				SelectionManager.Clear();
				state.GetWindow().allTracks.ForEach(delegate(TimelineTrackBaseGUI x)
				{
					SelectionManager.Add(x.track);
				});
				result = true;
			}
			return result;
		}
	}
}
