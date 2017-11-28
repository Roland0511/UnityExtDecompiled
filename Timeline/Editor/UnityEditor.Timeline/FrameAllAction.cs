using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[HideInMenu, Shortcut("FrameAll")]
	internal class FrameAllAction : TimelineAction
	{
		public override bool Execute(TimelineWindow.TimelineState state)
		{
			bool result;
			if (state.IsEditingASubItem())
			{
				result = false;
			}
			else
			{
				TimelineWindow window = state.GetWindow();
				if (window == null || window.treeView == null)
				{
					result = false;
				}
				else
				{
					TrackAsset[] visibleTracks = window.treeView.visibleTracks;
					if (visibleTracks.Length == 0)
					{
						result = false;
					}
					else
					{
						float num = 3.40282347E+38f;
						float num2 = -3.40282347E+38f;
						TrackAsset[] array = visibleTracks;
						for (int i = 0; i < array.Length; i++)
						{
							TrackAsset trackAsset = array[i];
							double num3;
							double num4;
							trackAsset.GetSequenceTime(out num3, out num4);
							num = Mathf.Min(num, (float)num3);
							num2 = Mathf.Max(num2, (float)(num3 + num4));
						}
						float num5 = num2 - Math.Max(0f, num);
						if (num5 > 0f)
						{
							state.SetTimeAreaShownRange(Mathf.Max(-10f, num - num5 * 0.1f), num2 + num5 * 0.1f);
						}
						else
						{
							state.SetTimeAreaShownRange(0f, 100f);
						}
						state.Evaluate();
						result = true;
					}
				}
			}
			return result;
		}
	}
}
