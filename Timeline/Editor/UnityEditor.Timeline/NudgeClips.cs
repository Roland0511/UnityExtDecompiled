using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class NudgeClips : Manipulator
	{
		public override void Init(IControl parent)
		{
			parent.KeyDown += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				bool result;
				if (state.IsEditingASubItem())
				{
					result = base.IgnoreEvent();
				}
				else if ((evt.get_modifiers() & 15) != null)
				{
					result = base.IgnoreEvent();
				}
				else
				{
					bool flag = evt.get_keyCode() == 49 || evt.get_keyCode() == 257;
					bool flag2 = evt.get_keyCode() == 50 || evt.get_keyCode() == 258;
					if (!evt.get_isKey() || (!flag && !flag2))
					{
						result = base.IgnoreEvent();
					}
					else if (SelectionManager.Count() == 0)
					{
						result = base.IgnoreEvent();
					}
					else
					{
						double offset = (!flag2) ? -1.0 : 1.0;
						bool flag3 = false;
						foreach (TimelineClip current in SelectionManager.SelectedItems<TimelineClip>())
						{
							flag3 |= this.NudgeClip(current, state, offset);
						}
						if (flag3)
						{
							state.Evaluate();
							result = base.ConsumeEvent();
						}
						else
						{
							result = base.IgnoreEvent();
						}
					}
				}
				return result;
			};
		}

		private bool NudgeClip(TimelineClip clip, TimelineWindow.TimelineState state, double offset)
		{
			bool result;
			if (clip == null)
			{
				result = false;
			}
			else
			{
				TimelineUndo.PushUndo(clip.parentTrack, "Nudge Clip");
				if (state.frameSnap)
				{
					clip.start = TimeUtility.FromFrames((double)TimeUtility.ToFrames(clip.start, (double)state.frameRate) + offset, (double)state.frameRate);
				}
				else
				{
					clip.start += offset / (double)state.frameRate;
				}
				EditorUtility.SetDirty(clip.parentTrack);
				result = true;
			}
			return result;
		}
	}
}
