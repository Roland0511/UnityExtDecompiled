using System;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal interface IControl
	{
		event TimelineUIEvent MouseDown;

		event TimelineUIEvent MouseDrag;

		event TimelineUIEvent MouseWheel;

		event TimelineUIEvent MouseUp;

		event TimelineUIEvent DoubleClick;

		event TimelineUIEvent KeyDown;

		event TimelineUIEvent KeyUp;

		event TimelineUIEvent DragPerform;

		event TimelineUIEvent DragExited;

		event TimelineUIEvent DragUpdated;

		event TimelineUIEvent Overlay;

		event TimelineUIEvent ContextClick;

		event TimelineUIEvent ValidateCommand;

		event TimelineUIEvent ExecuteCommand;

		bool OnEvent(Event evt, TimelineWindow.TimelineState state, bool isCaptureSession);

		void DrawOverlays(Event evt, TimelineWindow.TimelineState state);

		bool IsMouseOver(Vector2 mousePosition);
	}
}
