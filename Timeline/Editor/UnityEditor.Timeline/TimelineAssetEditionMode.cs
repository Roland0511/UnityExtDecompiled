using System;

namespace UnityEditor.Timeline
{
	internal class TimelineAssetEditionMode : TimelineInactiveMode
	{
		public TimelineAssetEditionMode()
		{
			base.headerState = new TimelineMode.HeaderState
			{
				breadCrumb = TimelineModeGUIState.Enabled,
				options = TimelineModeGUIState.Enabled,
				sequenceSelector = TimelineModeGUIState.Enabled
			};
			base.trackOptionsState = new TimelineMode.TrackOptionsState
			{
				newButton = TimelineModeGUIState.Enabled,
				editAsAssetButton = TimelineModeGUIState.Enabled
			};
		}

		public override TimelineModeGUIState TrackState(TimelineWindow.TimelineState state)
		{
			return TimelineModeGUIState.Enabled;
		}
	}
}
