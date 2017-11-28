using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class TrackheadContextMenu : Manipulator
	{
		public override void Init(IControl parent)
		{
			parent.ContextClick += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				float tolerance = 0.25f / state.frameRate;
				GenericMenu genericMenu = new GenericMenu();
				genericMenu.AddItem(EditorGUIUtility.TextContent("Insert/Frame/Single"), false, delegate
				{
					Gaps.Insert(state.timeline, state.time, (double)(1f / state.frameRate), tolerance);
					state.Refresh(true);
				});
				int[] array = new int[]
				{
					5,
					10,
					25,
					100
				};
				for (int num = 0; num != array.Length; num++)
				{
					float f = (float)array[num];
					genericMenu.AddItem(EditorGUIUtility.TextContent("Insert/Frame/" + array[num] + " Frames"), false, delegate
					{
						Gaps.Insert(state.timeline, state.time, (double)(f / state.frameRate), tolerance);
						state.Refresh(true);
					});
				}
				Vector2 playRangeTime = state.playRangeTime;
				if (playRangeTime.y > playRangeTime.x)
				{
					genericMenu.AddItem(EditorGUIUtility.TextContent("Insert/Selected Time"), false, delegate
					{
						Gaps.Insert(state.timeline, (double)playRangeTime.x, (double)(playRangeTime.y - playRangeTime.x), tolerance);
						state.Refresh(true);
					});
				}
				genericMenu.AddItem(EditorGUIUtility.TextContent("Select/Clips Ending Before"), false, delegate
				{
					TrackheadContextMenu.SelectMenuCallback((TimelineClip x) => x.end < state.time + (double)tolerance, state);
				});
				genericMenu.AddItem(EditorGUIUtility.TextContent("Select/Clips Starting Before"), false, delegate
				{
					TrackheadContextMenu.SelectMenuCallback((TimelineClip x) => x.start < state.time + (double)tolerance, state);
				});
				genericMenu.AddItem(EditorGUIUtility.TextContent("Select/Clips Ending After"), false, delegate
				{
					TrackheadContextMenu.SelectMenuCallback((TimelineClip x) => x.end - state.time >= (double)(-(double)tolerance), state);
				});
				genericMenu.AddItem(EditorGUIUtility.TextContent("Select/Clips Starting After"), false, delegate
				{
					TrackheadContextMenu.SelectMenuCallback((TimelineClip x) => x.start - state.time >= (double)(-(double)tolerance), state);
				});
				genericMenu.AddItem(EditorGUIUtility.TextContent("Select/Clips Intersecting"), false, delegate
				{
					TrackheadContextMenu.SelectMenuCallback((TimelineClip x) => x.start <= state.time && state.time <= x.end, state);
				});
				genericMenu.AddItem(EditorGUIUtility.TextContent("Select/Blends Intersecting"), false, delegate
				{
					TrackheadContextMenu.SelectMenuCallback((TimelineClip x) => TrackheadContextMenu.SelectBlendingIntersecting(x, state.time), state);
				});
				genericMenu.ShowAsContext();
				return base.ConsumeEvent();
			};
		}

		private static bool SelectBlendingIntersecting(TimelineClip clip, double time)
		{
			return clip.start <= time && time <= clip.end && (time <= clip.start + clip.blendInDuration || time >= clip.end - clip.blendOutDuration);
		}

		private static void SelectMenuCallback(Func<TimelineClip, bool> selector, TimelineWindow.TimelineState state)
		{
			List<TimelineClipGUI> allClipGuis = state.GetWindow().treeView.allClipGuis;
			if (allClipGuis != null)
			{
				SelectionManager.Clear();
				for (int num = 0; num != allClipGuis.Count; num++)
				{
					TimelineClipGUI timelineClipGUI = allClipGuis[num];
					if (timelineClipGUI != null && timelineClipGUI.clip != null && selector(timelineClipGUI.clip))
					{
						SelectionManager.Add(timelineClipGUI.clip);
					}
				}
			}
		}
	}
}
