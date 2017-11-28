using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Timeline
{
	[HideInMenu, Shortcut("FrameSelection")]
	internal class FrameSelectedAction : TimelineAction
	{
		public override bool Execute(TimelineWindow.TimelineState state)
		{
			bool result;
			if (state.IsEditingASubItem())
			{
				result = false;
			}
			else if (SelectionManager.Count() == 0)
			{
				result = false;
			}
			else
			{
				float num = 3.40282347E+38f;
				float num2 = -3.40282347E+38f;
				IEnumerable<TimelineClipGUI> enumerable = SelectionManager.SelectedClipGUI();
				if (!enumerable.Any<TimelineClipGUI>())
				{
					result = false;
				}
				else
				{
					foreach (TimelineClipGUI current in enumerable)
					{
						num = Mathf.Min(num, (float)current.clip.start);
						num2 = Mathf.Max(num2, (float)current.clip.start + (float)current.clip.duration);
						if (current.clipCurveEditor != null)
						{
							current.clipCurveEditor.FrameClip();
						}
					}
					float num3 = num2 - num;
					if (Mathf.Abs(num3) < 1.401298E-45f)
					{
						num3 = 1f;
					}
					state.SetTimeAreaShownRange(Mathf.Max(num - num3 * 0.2f, -10f), num2 + num3 * 0.2f);
					state.Evaluate();
					result = true;
				}
			}
			return result;
		}
	}
}
