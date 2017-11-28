using System;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	public class DirectorControlPlayable : PlayableBehaviour
	{
		public PlayableDirector director;

		public static ScriptPlayable<DirectorControlPlayable> Create(PlayableGraph graph, PlayableDirector director)
		{
			ScriptPlayable<DirectorControlPlayable> result;
			if (director == null)
			{
				result = ScriptPlayable<DirectorControlPlayable>.get_Null();
			}
			else
			{
				ScriptPlayable<DirectorControlPlayable> scriptPlayable = ScriptPlayable<DirectorControlPlayable>.Create(graph, 0);
				scriptPlayable.GetBehaviour().director = director;
				result = scriptPlayable;
			}
			return result;
		}

		public override void PrepareFrame(Playable playable, FrameData info)
		{
			if (!(this.director == null) && this.director.get_isActiveAndEnabled() && !(this.director.get_playableAsset() == null))
			{
				if (this.director.get_playableGraph().IsValid())
				{
					int rootPlayableCount = this.director.get_playableGraph().GetRootPlayableCount();
					for (int i = 0; i < rootPlayableCount; i++)
					{
						Playable rootPlayable = this.director.get_playableGraph().GetRootPlayable(i);
						if (PlayableExtensions.IsValid<Playable>(rootPlayable))
						{
							PlayableExtensions.SetSpeed<Playable>(rootPlayable, (double)info.get_effectiveSpeed());
						}
					}
				}
				if (info.get_evaluationType() == null)
				{
					if (Application.get_isPlaying())
					{
						this.director.Pause();
					}
					this.UpdateTime(playable);
					this.director.Evaluate();
				}
				else if (Application.get_isPlaying())
				{
					if (PlayableExtensions.GetTime<Playable>(playable) < this.director.get_time())
					{
						this.UpdateTime(playable);
					}
					this.director.Play();
				}
			}
		}

		public override void OnBehaviourPlay(Playable playable, FrameData info)
		{
			if (this.director != null && this.director.get_playableAsset() != null)
			{
				this.UpdateTime(playable);
				this.director.Evaluate();
				this.director.Play();
			}
		}

		public override void OnBehaviourPause(Playable playable, FrameData info)
		{
			if (this.director != null && this.director.get_playableAsset() != null)
			{
				this.director.Stop();
			}
		}

		private void UpdateTime(Playable playable)
		{
			double num = Math.Max(0.1, this.director.get_playableAsset().get_duration());
			DirectorWrapMode extrapolationMode = this.director.get_extrapolationMode();
			if (extrapolationMode != null)
			{
				if (extrapolationMode != 1)
				{
					if (extrapolationMode == 2)
					{
						this.director.set_time(PlayableExtensions.GetTime<Playable>(playable));
					}
				}
				else
				{
					this.director.set_time(Math.Max(0.0, PlayableExtensions.GetTime<Playable>(playable) % num));
				}
			}
			else
			{
				this.director.set_time(Math.Min(num, Math.Max(0.0, PlayableExtensions.GetTime<Playable>(playable))));
			}
		}
	}
}
