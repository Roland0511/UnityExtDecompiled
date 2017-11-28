using System;
using UnityEngine.Audio;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	internal class ScheduleRuntimeClip : RuntimeClipBase
	{
		private TimelineClip m_Clip;

		private Playable m_Playable;

		private AudioClipPlayable m_AudioClipPlayable;

		private Playable m_ParentMixer;

		private double m_StartDelay;

		private double m_FinishTail;

		private bool m_Started = false;

		public override double start
		{
			get
			{
				return Math.Max(0.0, this.m_Clip.start - this.m_StartDelay);
			}
		}

		public override double duration
		{
			get
			{
				return this.m_Clip.duration + this.m_FinishTail + this.m_Clip.start - this.start;
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
					if (playState == null && PlayableExtensions.IsValid<Playable>(this.m_ParentMixer))
					{
						PlayableExtensions.SetInputWeight<Playable, Playable>(this.m_ParentMixer, this.m_Playable, 0f);
					}
				}
				this.m_Started &= value;
			}
		}

		public ScheduleRuntimeClip(TimelineClip clip, Playable clipPlayable, Playable parentMixer, double startDelay = 0.2, double finishTail = 0.1)
		{
			this.Create(clip, clipPlayable, parentMixer, startDelay, finishTail);
		}

		public void SetTime(double time)
		{
			PlayableExtensions.SetTime<Playable>(this.m_Playable, time);
		}

		private void Create(TimelineClip clip, Playable clipPlayable, Playable parentMixer, double startDelay, double finishTail)
		{
			this.m_Clip = clip;
			this.m_Playable = clipPlayable;
			this.m_AudioClipPlayable = (AudioClipPlayable)clipPlayable;
			this.m_ParentMixer = parentMixer;
			this.m_StartDelay = startDelay;
			this.m_FinishTail = finishTail;
			PlayableExtensions.SetPlayState<Playable>(clipPlayable, 0);
		}

		public override void EvaluateAt(double localTime, FrameData frameData)
		{
			if (frameData.get_timeHeld())
			{
				this.enable = false;
			}
			else
			{
				bool flag = frameData.get_seekOccurred() || frameData.get_timeLooped() || frameData.get_evaluationType() == 0;
				if (localTime <= this.start + this.duration - this.m_FinishTail)
				{
					float num = this.clip.EvaluateMixIn(localTime) * this.clip.EvaluateMixOut(localTime);
					if (PlayableExtensions.IsValid<Playable>(this.mixer))
					{
						PlayableExtensions.SetInputWeight<Playable, Playable>(this.mixer, this.playable, num);
					}
					if (!this.m_Started || flag)
					{
						double num2 = this.clip.ToLocalTime(Math.Max(localTime, this.clip.start));
						double num3 = Math.Max(this.clip.start - localTime, 0.0) * this.clip.timeScale;
						double num4 = this.m_Clip.duration * this.clip.timeScale;
						this.m_AudioClipPlayable.Seek(num2, num3, num4);
						this.m_Started = true;
					}
				}
			}
		}
	}
}
