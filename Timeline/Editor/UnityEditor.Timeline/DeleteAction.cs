using System;
using System.ComponentModel;
using System.Linq;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[DisplayName("Delete"), Shortcut("Delete")]
	internal class DeleteAction : TimelineAction
	{
		public override bool Execute(TimelineWindow.TimelineState state)
		{
			bool result;
			if (SelectionManager.GetCurrentInlineEditorCurve() != null)
			{
				result = false;
			}
			else
			{
				ITimelineItem[] array = (from x in SelectionManager.SelectedItemGUI()
				select x.selectableObject as ITimelineItem).ToArray<ITimelineItem>();
				if (array.Length > 0)
				{
					ItemActionInvoker.InvokeByName<TimelineClip>("DeleteClips", state, array);
					ItemActionInvoker.InvokeByName<TimelineMarker>("DeleteMarkers", state, array);
				}
				TrackAsset[] array2 = SelectionManager.SelectedTracks().ToArray<TrackAsset>();
				if (array2.Length > 0)
				{
					TrackAction.InvokeByName("DeleteTracks", state, array2);
				}
				result = (array.Length > 0 || array2.Length > 0);
			}
			return result;
		}
	}
}
