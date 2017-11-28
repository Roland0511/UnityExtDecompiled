using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class AnimationTrackKeyDataSource : IPropertyKeyDataSource
	{
		private readonly AnimationTrack m_Track;

		public AnimationTrackKeyDataSource(AnimationTrack track)
		{
			this.m_Track = track;
		}

		public float[] GetKeys()
		{
			float[] result;
			if (this.m_Track == null || this.m_Track.animClip == null)
			{
				result = null;
			}
			else
			{
				AnimationClipCurveInfo curveInfo = AnimationClipCurveCache.Instance.GetCurveInfo(this.m_Track.animClip);
				result = (from x in curveInfo.keyTimes
				select x + (float)this.m_Track.openClipTimeOffset).ToArray<float>();
			}
			return result;
		}

		public Dictionary<float, string> GetDescriptions()
		{
			Dictionary<float, string> dictionary = new Dictionary<float, string>();
			AnimationClipCurveInfo curveInfo = AnimationClipCurveCache.Instance.GetCurveInfo(this.m_Track.animClip);
			HashSet<string> hashSet = new HashSet<string>();
			EditorCurveBinding[] bindings = curveInfo.bindings;
			for (int i = 0; i < bindings.Length; i++)
			{
				EditorCurveBinding binding = bindings[i];
				string groupID = binding.GetGroupID();
				if (!hashSet.Contains(groupID))
				{
					CurveBindingGroup groupBinding = curveInfo.GetGroupBinding(groupID);
					string nicePropertyGroupDisplayName = AnimationWindowUtility.GetNicePropertyGroupDisplayName(binding.get_type(), binding.propertyName);
					float[] keyTimes = curveInfo.keyTimes;
					for (int j = 0; j < keyTimes.Length; j++)
					{
						float num = keyTimes[j];
						float num2 = num + (float)this.m_Track.openClipTimeOffset;
						string text = nicePropertyGroupDisplayName + " : " + groupBinding.GetDescription(num2);
						if (dictionary.ContainsKey(num2))
						{
							Dictionary<float, string> dictionary2;
							float key;
							(dictionary2 = dictionary)[key = num2] = dictionary2[key] + '\n' + text;
						}
						else
						{
							dictionary.Add(num2, text);
						}
					}
					hashSet.Add(groupID);
				}
			}
			return dictionary;
		}
	}
}
