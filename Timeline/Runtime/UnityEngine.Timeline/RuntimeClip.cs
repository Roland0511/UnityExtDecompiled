using System;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	internal class RuntimeClip : RuntimeClipBase
	{
		private TimelineClip m_Clip;

		private Playable m_Playable;

		private Playable m_ParentMixer;

		public override double start
		{
			get
			{
				return this.m_Clip.extrapolatedStart;
			}
		}

		public override double duration
		{
			get
			{
				return this.m_Clip.extrapolatedDuration;
			}
		}

		public TimelineClip clip
		{
			get
			{
				return this.m_Clip;
			}
		}

		public Playable mixer
		{
			get
			{
				return this.m_ParentMixer;
			}
		}

		public Playable playable
		{
			get
			{
				return this.m_Playable;
			}
		}

		public override bool enable
		{
			set
			{
				PlayState playState = (!value) ? 0 : 1;
				PlayState playState2 = PlayableExtensions.GetPlayState<Playable>(this.m_Playable);
				if (playState != playState2)
				{
					PlayableExtensions.SetPlayState<Playable>(this.m_Playable, playState);
				}
				if (playState == null && PlayableExtensions.IsValid<Playable>(this.m_ParentMixer))
				{
					PlayableExtensions.SetInputWeight<Playable, Playable>(this.m_ParentMixer, this.m_Playable, 0f);
				}
			}
		}

		public RuntimeClip(TimelineClip clip, Playable clipPlayable, Playable parentMixer)
		{
			this.Create(clip, clipPlayable, parentMixer);
		}

		private void Create(TimelineClip clip, Playable clipPlayable, Playable parentMixer)
		{
			this.m_Clip = clip;
			this.m_Playable = clipPlayable;
			this.m_ParentMixer = parentMixer;
			PlayableExtensions.SetPlayState<Playable>(clipPlayable, 0);
		}

		public void SetTime(double time)
		{
			PlayableExtensions.SetTime<Playable>(this.m_Playable, time);
		}

		public void SetDuration(double duration)
		{
			PlayableExtensions.SetDuration<Playable>(this.m_Playable, duration);
		}

		public override void EvaluateAt(double localTime, FrameData frameData)
		{
			this.enable = true;
			float num;
			if (this.clip.IsPreExtrapolatedTime(localTime))
			{
				num = this.clip.EvaluateMixIn((double)((float)this.clip.start));
			}
			else if (this.clip.IsPostExtrapolatedTime(localTime))
			{
				num = this.clip.EvaluateMixOut((double)((float)this.clip.end));
			}
			else
			{
				num = this.clip.EvaluateMixIn(localTime) * this.clip.EvaluateMixOut(localTime);
			}
			if (PlayableExtensions.IsValid<Playable>(this.mixer))
			{
				PlayableExtensions.SetInputWeight<Playable, Playable>(this.mixer, this.playable, num);
			}
			double time = this.clip.ToLocalTime(localTime);
			if (time.CompareTo(0.0) >= 0)
			{
				this.SetTime(time);
			}
			this.SetDuration(this.clip.extrapolatedDuration);
		}
	}
}
