using System;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal delegate bool PendingUpdateDelegate(ITimelineState state, Event currentEvent);
}
