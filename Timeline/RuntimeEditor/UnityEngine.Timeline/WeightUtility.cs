using System;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	internal static class WeightUtility
	{
		public static float NormalizeMixer(Playable mixer)
		{
			float result;
			if (!PlayableExtensions.IsValid<Playable>(mixer))
			{
				result = 0f;
			}
			else
			{
				int inputCount = PlayableExtensions.GetInputCount<Playable>(mixer);
				float num = 0f;
				for (int i = 0; i < inputCount; i++)
				{
					num += PlayableExtensions.GetInputWeight<Playable>(mixer, i);
				}
				if (num > Mathf.Epsilon && num < 1f)
				{
					for (int j = 0; j < inputCount; j++)
					{
						PlayableExtensions.SetInputWeight<Playable>(mixer, j, PlayableExtensions.GetInputWeight<Playable>(mixer, j) / num);
					}
				}
				result = Mathf.Clamp01(num);
			}
			return result;
		}
	}
}
