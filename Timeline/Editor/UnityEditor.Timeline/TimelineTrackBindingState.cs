using System;

namespace UnityEditor.Timeline
{
	internal enum TimelineTrackBindingState
	{
		Valid,
		NoGameObjectBound,
		BoundGameObjectIsDisabled,
		NoValidComponentOnBoundGameObject,
		RequiredComponentOnBoundGameObjectIsDisabled
	}
}
