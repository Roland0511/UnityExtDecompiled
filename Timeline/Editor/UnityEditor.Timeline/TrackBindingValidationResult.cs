using System;
using System.Runtime.InteropServices;

namespace UnityEditor.Timeline
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct TrackBindingValidationResult
	{
		public TimelineTrackBindingState bindingState
		{
			get;
			private set;
		}

		public string bindingName
		{
			get;
			private set;
		}

		public TrackBindingValidationResult(TimelineTrackBindingState state, string bindName = null)
		{
			this.bindingState = state;
			this.bindingName = bindName;
		}

		public static implicit operator bool(TrackBindingValidationResult result)
		{
			return result.IsValid();
		}

		public bool IsValid()
		{
			return this.bindingState == TimelineTrackBindingState.Valid;
		}
	}
}
