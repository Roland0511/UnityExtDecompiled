using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class SimpleDragClipHandle : DragClipHandle
	{
		protected override void OnMouseDrag(Event evt, TimelineWindow.TimelineState state, TimelineClipHandle handle)
		{
			ManipulateEdges edges = ManipulateEdges.Both;
			float delta = evt.get_delta().x / state.timeAreaScale.x;
			TimelineUndo.PushUndo(handle.clip.clip.parentTrack, "Trim Clip");
			TimelineClipHandle.DragDirection direction = handle.direction;
			if (direction != TimelineClipHandle.DragDirection.Right)
			{
				if (direction == TimelineClipHandle.DragDirection.Left)
				{
					double num = this.m_FrameSnap.ApplyOffset(handle.clip.clip.start, delta, state);
					if (num > 0.0)
					{
						double num2 = num - handle.clip.clip.start;
						handle.clip.clip.start = num;
						if (handle.clip.clip.duration - num2 > TimelineClip.kMinDuration)
						{
							handle.clip.clip.duration -= num2;
						}
					}
					edges = ManipulateEdges.Left;
				}
			}
			else
			{
				double val = this.m_FrameSnap.ApplyOffset(handle.clip.clip.duration, delta, state);
				handle.clip.clip.duration = Math.Max(val, TimelineClip.kMinDuration);
				edges = ManipulateEdges.Right;
			}
			if (this.m_MagnetEngine != null && evt.get_modifiers() != 1)
			{
				this.m_MagnetEngine.Snap(evt.get_delta().x, edges);
			}
		}

		protected override void OnAttractedEdge(TimelineClip clip, AttractedEdge edge, double time, double duration)
		{
			if (edge != AttractedEdge.Right)
			{
				if (edge != AttractedEdge.Left)
				{
					if (time > 0.0)
					{
						clip.start = time;
						clip.duration = Math.Max(duration, TimelineClip.kMinDuration);
					}
				}
				else
				{
					double num = time - clip.start;
					if (time > 0.0)
					{
						clip.start = time;
						if (clip.duration - num > TimelineClip.kMinDuration)
						{
							clip.duration -= num;
						}
					}
				}
			}
			else
			{
				clip.duration = time - clip.start;
				clip.duration = Math.Max(clip.duration, TimelineClip.kMinDuration);
			}
		}
	}
}
