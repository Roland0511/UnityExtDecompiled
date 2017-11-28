using System;
using UnityEditor;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	internal static class AnimationDefaultTrack
	{
		private static string k_DefaultHumanoidClipPath;

		private static AnimationClip s_DefaultHumanoidClip;

		public static AnimationClip defaultHumanoidClip
		{
			get
			{
				return AnimationDefaultTrack.s_DefaultHumanoidClip;
			}
		}

		static AnimationDefaultTrack()
		{
			AnimationDefaultTrack.k_DefaultHumanoidClipPath = "Editors/TimelineWindow/HumanoidDefault.anim";
			AnimationDefaultTrack.s_DefaultHumanoidClip = null;
			AnimationDefaultTrack.s_DefaultHumanoidClip = (EditorGUIUtility.LoadRequired(AnimationDefaultTrack.k_DefaultHumanoidClipPath) as AnimationClip);
			if (AnimationDefaultTrack.s_DefaultHumanoidClip == null)
			{
				Debug.LogError("Could not load default humanoid animation clip for Timeline");
			}
		}

		public static Animator GetAnimator(TrackAsset asset, PlayableDirector director)
		{
			AnimationTrack animationTrack = asset as AnimationTrack;
			Animator result;
			if (animationTrack != null && !animationTrack.isSubTrack)
			{
				Object genericBinding = director.GetGenericBinding(animationTrack);
				if (genericBinding == null)
				{
					result = null;
				}
				else
				{
					Animator animator = genericBinding as Animator;
					GameObject gameObject = genericBinding as GameObject;
					if (animator == null && gameObject != null)
					{
						animator = gameObject.GetComponent<Animator>();
					}
					result = animator;
				}
			}
			else
			{
				result = null;
			}
			return result;
		}

		public static void AddDefaultHumanoidTrack(PlayableGraph graph, Playable rootPlayable, Animator animator)
		{
			if (!(AnimationDefaultTrack.defaultHumanoidClip == null))
			{
				Playable playable = new Playable(AnimationPlayableGraphExtensions.CreateAnimationMotionXToDeltaPlayable(graph));
				PlayableExtensions.SetInputCount<Playable>(playable, 1);
				AnimationOffsetPlayable animationOffsetPlayable = AnimationOffsetPlayable.Create(graph, Vector3.get_zero(), Quaternion.get_identity(), 1);
				AnimationClipPlayable animationClipPlayable = AnimationClipPlayable.Create(graph, AnimationDefaultTrack.defaultHumanoidClip);
				PlayableExtensions.SetTime<AnimationClipPlayable>(animationClipPlayable, 0.0);
				PlayableExtensions.SetSpeed<AnimationClipPlayable>(animationClipPlayable, 0.0);
				graph.Connect<AnimationOffsetPlayable, Playable>(animationOffsetPlayable, 0, playable, 0);
				PlayableExtensions.SetInputWeight<Playable>(playable, 0, 1f);
				graph.Connect<AnimationClipPlayable, AnimationOffsetPlayable>(animationClipPlayable, 0, animationOffsetPlayable, 0);
				PlayableExtensions.SetInputWeight<AnimationOffsetPlayable>(animationOffsetPlayable, 0, 1f);
				int inputCount = PlayableExtensions.GetInputCount<Playable>(rootPlayable);
				PlayableExtensions.SetInputCount<Playable>(rootPlayable, inputCount + 1);
				graph.Connect<Playable, Playable>(playable, 0, rootPlayable, inputCount);
				PlayableExtensions.SetInputWeight<Playable>(rootPlayable, inputCount, 1f);
				AnimationPlayableOutput animationPlayableOutput = AnimationPlayableOutput.Create(graph, "DefaultHumanoid", animator);
				PlayableOutputExtensions.SetSourcePlayable<AnimationPlayableOutput, Playable>(animationPlayableOutput, rootPlayable);
				PlayableOutputExtensions.SetSourceInputPort<AnimationPlayableOutput>(animationPlayableOutput, inputCount);
			}
		}
	}
}
