using System;
using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	internal class AnimationOutputWeightProcessor : ITimelineEvaluateCallback
	{
		private struct WeightInfo
		{
			public Playable mixer;

			public Playable parentMixer;

			public int port;

			public bool modulate;
		}

		private AnimationPlayableOutput m_Output;

		private AnimationLayerMixerPlayable m_LayerMixer;

		private readonly List<AnimationOutputWeightProcessor.WeightInfo> m_Mixers = new List<AnimationOutputWeightProcessor.WeightInfo>();

		public AnimationOutputWeightProcessor(AnimationPlayableOutput output)
		{
			this.m_Output = output;
			this.FindMixers();
		}

		private void FindMixers()
		{
			this.m_Mixers.Clear();
			this.m_LayerMixer = AnimationLayerMixerPlayable.get_Null();
			Playable sourcePlayable = PlayableOutputExtensions.GetSourcePlayable<AnimationPlayableOutput>(this.m_Output);
			int sourceInputPort = PlayableOutputExtensions.GetSourceInputPort<AnimationPlayableOutput>(this.m_Output);
			if (PlayableExtensions.IsValid<Playable>(sourcePlayable) && sourceInputPort >= 0 && sourceInputPort < PlayableExtensions.GetInputCount<Playable>(sourcePlayable))
			{
				Playable input = PlayableExtensions.GetInput<Playable>(PlayableExtensions.GetInput<Playable>(sourcePlayable, sourceInputPort), 0);
				if (PlayableExtensions.IsValid<Playable>(input) && input.IsPlayableOfType<AnimationLayerMixerPlayable>())
				{
					this.m_LayerMixer = (AnimationLayerMixerPlayable)input;
					int inputCount = PlayableExtensions.GetInputCount<AnimationLayerMixerPlayable>(this.m_LayerMixer);
					for (int i = 0; i < inputCount; i++)
					{
						this.FindMixers(this.m_LayerMixer, i, PlayableExtensions.GetInput<AnimationLayerMixerPlayable>(this.m_LayerMixer, i));
					}
				}
			}
		}

		private void FindMixers(Playable parent, int port, Playable node)
		{
			if (PlayableExtensions.IsValid<Playable>(node))
			{
				Type playableType = node.GetPlayableType();
				if (playableType == typeof(AnimationMixerPlayable) || playableType == typeof(AnimationLayerMixerPlayable))
				{
					int inputCount = PlayableExtensions.GetInputCount<Playable>(node);
					for (int i = 0; i < inputCount; i++)
					{
						this.FindMixers(node, i, PlayableExtensions.GetInput<Playable>(node, i));
					}
					AnimationOutputWeightProcessor.WeightInfo item = new AnimationOutputWeightProcessor.WeightInfo
					{
						parentMixer = parent,
						mixer = node,
						port = port,
						modulate = (playableType == typeof(AnimationLayerMixerPlayable))
					};
					this.m_Mixers.Add(item);
				}
				else
				{
					int inputCount2 = PlayableExtensions.GetInputCount<Playable>(node);
					for (int j = 0; j < inputCount2; j++)
					{
						this.FindMixers(parent, port, PlayableExtensions.GetInput<Playable>(node, j));
					}
				}
			}
		}

		public void Evaluate()
		{
			for (int i = 0; i < this.m_Mixers.Count; i++)
			{
				AnimationOutputWeightProcessor.WeightInfo weightInfo = this.m_Mixers[i];
				float num = (!weightInfo.modulate) ? 1f : PlayableExtensions.GetInputWeight<Playable>(weightInfo.parentMixer, weightInfo.port);
				PlayableExtensions.SetInputWeight<Playable>(weightInfo.parentMixer, weightInfo.port, num * WeightUtility.NormalizeMixer(weightInfo.mixer));
			}
			PlayableOutputExtensions.SetWeight<AnimationPlayableOutput>(this.m_Output, WeightUtility.NormalizeMixer(this.m_LayerMixer));
		}
	}
}
