using System;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal delegate bool TimelineUIEvent(object target, Event e, TimelineWindow.TimelineState state);
}
