using System;

namespace UnityEditor.Timeline
{
	internal class TimelineInactiveMode : TimelineMode
	{
		public TimelineInactiveMode()
		{
			base.headerState = new TimelineMode.HeaderState
			{
				breadCrumb = TimelineModeGUIState.Disabled,
				options = TimelineModeGUIState.Enabled,
				sequenceSelector = TimelineModeGUIState.Disabled
			};
			base.trackOptionsState = new TimelineMode.TrackOptionsState
			{
				newButton = TimelineModeGUIState.Disabled,
				editAsAssetButton = TimelineModeGUIState.Enabled
			};
		}

		public override bool ShouldShowPlayRange(TimelineWindow.TimelineState state)
		{
			return false;
		}

		public override bool ShouldShowTimeCursor(TimelineWindow.TimelineState state)
		{
			return false;
		}

		public override TimelineModeGUIState ToolbarState(TimelineWindow.TimelineState state)
		{
			return TimelineModeGUIState.Disabled;
		}

		public override TimelineModeGUIState TrackState(TimelineWindow.TimelineState state)
		{
			return TimelineModeGUIState.Disabled;
		}
	}
}
