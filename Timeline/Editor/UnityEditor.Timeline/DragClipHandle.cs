using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class DragClipHandle : Manipulator
	{
		internal class DragClipAttractionHandler : IAttractionHandler
		{
			private FrameSnap m_FrameSnap;

			public DragClipAttractionHandler(FrameSnap frameSnap)
			{
				this.m_FrameSnap = frameSnap;
			}

			public void OnAttractedEdge(IAttractable attractable, AttractedEdge edge, double time, double duration)
			{
				TimelineClipGUI timelineClipGUI = attractable as TimelineClipGUI;
				if (timelineClipGUI != null)
				{
					TimelineClip clip = timelineClipGUI.clip;
					if (edge == AttractedEdge.Right)
					{
						clip.duration = time - clip.start;
						clip.duration = Math.Max(clip.duration, TimelineClip.kMinDuration);
					}
					else if (edge == AttractedEdge.Left)
					{
						double num = time - clip.start;
						double newValue = clip.clipIn + num * clip.timeScale;
						if (DragClipHandle.SetClipIn(clip, newValue, num))
						{
							this.m_FrameSnap.Reset();
						}
					}
					else
					{
						clip.start = time;
						clip.duration = duration;
					}
				}
			}
		}

		protected FrameSnap m_FrameSnap;

		protected bool m_IsCaptured;

		protected Ripple m_Ripple;

		protected string m_OverlayText = "";

		protected double m_OriginalDuration;

		protected double m_OriginalTimeScale;

		protected bool m_UndoSaved;

		protected MagnetEngine m_MagnetEngine;

		protected List<string> m_OverlayStrings = new List<string>();

		private static readonly double kEpsilon = 1E-07;

		protected virtual void OnAttractedEdge(TimelineClip clip, AttractedEdge edge, double time, double duration)
		{
		}

		private static void ManipulateBlending(TimelineClipHandle handle, Event evt, float scale)
		{
			if (handle.direction == TimelineClipHandle.DragDirection.Right)
			{
				float num = evt.get_delta().x / scale;
				handle.clip.clip.easeOutDuration -= (double)num;
			}
			else
			{
				float num2 = evt.get_delta().x / scale;
				handle.clip.clip.easeInDuration += (double)num2;
			}
		}

		public override void Init(IControl parent)
		{
			this.m_IsCaptured = false;
			this.m_Ripple = null;
			this.m_OriginalDuration = 0.0;
			this.m_OriginalTimeScale = 0.0;
			this.m_UndoSaved = false;
			this.m_MagnetEngine = null;
			this.m_FrameSnap = new FrameSnap();
			parent.MouseDown += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				TimelineClipHandle timelineClipHandle = target as TimelineClipHandle;
				bool result;
				if (timelineClipHandle == null)
				{
					result = base.IgnoreEvent();
				}
				else
				{
					this.OnMouseDown(evt, state, timelineClipHandle);
					result = base.ConsumeEvent();
				}
				return result;
			};
			parent.MouseUp += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				TimelineClipHandle timelineClipHandle = target as TimelineClipHandle;
				bool result;
				if (!this.m_IsCaptured || timelineClipHandle == null)
				{
					result = base.IgnoreEvent();
				}
				else
				{
					this.OnMouseUp(evt, state, timelineClipHandle);
					result = base.ConsumeEvent();
				}
				return result;
			};
			parent.MouseDrag += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				this.m_OverlayText = "";
				this.m_OverlayStrings.Clear();
				TimelineClipHandle timelineClipHandle = target as TimelineClipHandle;
				bool result;
				if (!this.m_IsCaptured || timelineClipHandle == null)
				{
					result = base.IgnoreEvent();
				}
				else
				{
					this.OnMouseDrag(evt, state, timelineClipHandle);
					this.RefreshOverlayStrings(timelineClipHandle, state);
					if (Selection.get_activeObject() != null)
					{
						EditorUtility.SetDirty(Selection.get_activeObject());
					}
					state.UpdateRootPlayableDuration(state.duration);
					result = base.ConsumeEvent();
				}
				return result;
			};
			parent.Overlay += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				TimelineClipHandle timelineClipHandle = target as TimelineClipHandle;
				bool result;
				if (timelineClipHandle == null)
				{
					result = base.IgnoreEvent();
				}
				else
				{
					this.OnOverlay(evt, timelineClipHandle);
					result = base.ConsumeEvent();
				}
				return result;
			};
		}

		protected virtual void OnMouseUp(Event evt, TimelineWindow.TimelineState state, TimelineClipHandle handle)
		{
			this.m_UndoSaved = false;
			this.m_MagnetEngine = null;
			state.captured.Clear();
			this.m_Ripple = null;
		}

		protected virtual void OnMouseDown(Event evt, TimelineWindow.TimelineState state, TimelineClipHandle handle)
		{
			if (!state.captured.Contains(handle))
			{
				state.captured.Add(handle);
			}
			this.m_IsCaptured = true;
			this.m_UndoSaved = false;
			SelectionManager.Add(handle.clip.clip);
			if (state.edgeSnaps)
			{
				this.m_MagnetEngine = new MagnetEngine(handle.clip, new DragClipHandle.DragClipAttractionHandler(this.m_FrameSnap), state);
			}
			this.m_OriginalDuration = handle.clip.clip.duration;
			this.m_OriginalTimeScale = handle.clip.clip.timeScale;
			this.m_FrameSnap.Reset();
		}

		protected virtual void OnMouseDrag(Event evt, TimelineWindow.TimelineState state, TimelineClipHandle handle)
		{
			if (evt.get_modifiers() == 2)
			{
				DragClipHandle.ManipulateBlending(handle, evt, state.timeAreaScale.x);
			}
			else
			{
				if (!this.m_UndoSaved)
				{
					TimelineUndo.PushUndo(handle.clip.parentTrackGUI.track, "Trim Clip");
					this.m_UndoSaved = true;
				}
				float num = evt.get_delta().x / state.timeAreaScale.x;
				if (this.m_Ripple != null)
				{
					this.m_Ripple.Run(num, state);
				}
				ManipulateEdges edges;
				if (handle.direction == TimelineClipHandle.DragDirection.Right)
				{
					double val = this.m_FrameSnap.ApplyOffset(handle.clip.clip.duration, num, state);
					handle.clip.clip.duration = Math.Max(val, TimelineClip.kMinDuration);
					if (handle.clip.clip.SupportsSpeedMultiplier() && evt.get_modifiers() == 1)
					{
						double num2 = this.m_OriginalDuration / handle.clip.clip.duration;
						handle.clip.clip.timeScale = this.m_OriginalTimeScale * num2;
					}
					edges = ManipulateEdges.Right;
				}
				else
				{
					double num3 = handle.clip.clip.clipIn / handle.clip.clip.timeScale;
					if (num3 > 0.0 && num3 + (double)num < 0.0)
					{
						num = (float)(-(float)num3);
					}
					if (this.m_MagnetEngine == null || !this.m_MagnetEngine.isSnapped)
					{
						double newValue = this.m_FrameSnap.ApplyOffset(num3, num, state) * handle.clip.clip.timeScale;
						DragClipHandle.SetClipIn(handle.clip.clip, newValue, this.m_FrameSnap.lastOffsetApplied);
					}
					edges = ManipulateEdges.Left;
				}
				if (this.m_MagnetEngine != null && evt.get_modifiers() != 1)
				{
					this.m_MagnetEngine.Snap(evt.get_delta().x, edges);
				}
				handle.clip.clip.duration = Math.Max(handle.clip.clip.duration, TimelineClip.kMinDuration);
			}
		}

		protected virtual void OnOverlay(Event evt, TimelineClipHandle handle)
		{
			if (this.m_OverlayStrings.Count > 0)
			{
				Vector2 vector = TimelineWindow.styles.tinyFont.CalcSize(new GUIContent(this.m_OverlayStrings[0]));
				Rect rect = new Rect(evt.get_mousePosition().x - vector.x / 2f, handle.clip.parentTrackGUI.boundingRect.get_y() + handle.clip.parentTrackGUI.boundingRect.get_height() + 42f, vector.x, 40f);
				GUILayout.BeginArea(rect);
				GUILayout.BeginVertical(new GUILayoutOption[0]);
				GUI.Label(rect, GUIContent.none, TimelineWindow.styles.displayBackground);
				foreach (string current in this.m_OverlayStrings)
				{
					GUILayout.Label(current, TimelineWindow.styles.tinyFont, new GUILayoutOption[0]);
				}
				GUILayout.EndVertical();
				GUILayout.EndArea();
			}
			if (this.m_MagnetEngine != null)
			{
				this.m_MagnetEngine.OnGUI();
			}
		}

		protected void RefreshOverlayStrings(TimelineClipHandle handle, TimelineWindow.TimelineState state)
		{
			double num = (this.m_OriginalTimeScale - handle.clip.clip.timeScale) * 100.0;
			double num2 = handle.clip.clip.duration - this.m_OriginalDuration;
			this.m_OverlayText = "";
			if (Math.Abs(num) > DragClipHandle.kEpsilon)
			{
				this.m_OverlayText = "speed: " + (handle.clip.clip.timeScale * 100.0).ToString("f2") + "%";
				this.m_OverlayText += " (";
				if (num > 0.0)
				{
					this.m_OverlayText += "+";
				}
				this.m_OverlayText = this.m_OverlayText + num.ToString("f2") + "%";
				this.m_OverlayText += ")";
				this.m_OverlayStrings.Add(this.m_OverlayText);
			}
			this.m_OverlayText = "";
			if (Math.Abs(num2) > DragClipHandle.kEpsilon)
			{
				if (!state.timeInFrames)
				{
					this.m_OverlayText = this.m_OverlayText + " duration: " + handle.clip.clip.duration.ToString("f2") + "s";
					this.m_OverlayText += " (";
					if (num2 > 0.0)
					{
						this.m_OverlayText += "+";
					}
					this.m_OverlayText = this.m_OverlayText + num2.ToString("f2") + "s";
					this.m_OverlayText += ")";
					this.m_OverlayStrings.Add(this.m_OverlayText);
				}
				else
				{
					double num3 = handle.clip.clip.duration * (double)state.frameRate;
					this.m_OverlayText = this.m_OverlayText + " duration: " + num3.ToString("f2") + "frames";
					this.m_OverlayText += " (";
					if (num2 > 0.0)
					{
						this.m_OverlayText += "+";
					}
					double num4 = num2 * (double)state.frameRate;
					this.m_OverlayText = this.m_OverlayText + num4.ToString("f2") + "frames";
					this.m_OverlayText += ")";
					this.m_OverlayStrings.Add(this.m_OverlayText);
				}
			}
		}

		private static bool SetClipIn(TimelineClip sourceClip, double newValue, double startDelta)
		{
			bool result;
			if (sourceClip.start + startDelta < 0.0)
			{
				result = false;
			}
			else
			{
				double kMaxTimeValue = TimelineClip.kMaxTimeValue;
				double num = sourceClip.duration - startDelta;
				if (newValue >= 0.0 && newValue < kMaxTimeValue && num > 0.0)
				{
					sourceClip.clipIn = newValue;
					sourceClip.start += startDelta;
					sourceClip.duration = num;
					result = true;
				}
				else
				{
					result = false;
				}
			}
			return result;
		}
	}
}
