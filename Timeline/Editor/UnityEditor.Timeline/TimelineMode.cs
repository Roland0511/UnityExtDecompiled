using System;
using System.Linq;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal abstract class TimelineMode
	{
		public struct HeaderState
		{
			public TimelineModeGUIState breadCrumb;

			public TimelineModeGUIState sequenceSelector;

			public TimelineModeGUIState options;
		}

		public struct TrackOptionsState
		{
			public TimelineModeGUIState newButton;

			public TimelineModeGUIState editAsAssetButton;
		}

		public TimelineMode.HeaderState headerState
		{
			get;
			protected set;
		}

		public TimelineMode.TrackOptionsState trackOptionsState
		{
			get;
			protected set;
		}

		public abstract bool ShouldShowPlayRange(TimelineWindow.TimelineState state);

		public abstract bool ShouldShowTimeCursor(TimelineWindow.TimelineState state);

		public virtual bool ShouldShowTimeArea(TimelineWindow.TimelineState state)
		{
			return state.timeline != null && state.timeline.tracks.Any<TrackAsset>();
		}

		public abstract TimelineModeGUIState TrackState(TimelineWindow.TimelineState state);

		public abstract TimelineModeGUIState ToolbarState(TimelineWindow.TimelineState state);
	}
}
