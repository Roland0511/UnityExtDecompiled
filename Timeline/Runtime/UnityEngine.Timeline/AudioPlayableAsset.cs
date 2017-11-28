using System;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	[Serializable]
	public class AudioPlayableAsset : PlayableAsset, ITimelineClipAsset
	{
		[SerializeField]
		private AudioClip m_Clip;

		[SerializeField]
		private bool m_Loop;

		[HideInInspector, SerializeField]
		private float m_bufferingTime = 0.1f;

		internal float bufferingTime
		{
			get
			{
				return this.m_bufferingTime;
			}
			set
			{
				this.m_bufferingTime = value;
			}
		}

		public AudioClip clip
		{
			get
			{
				return this.m_Clip;
			}
			set
			{
				this.m_Clip = value;
			}
		}

		public override double duration
		{
			get
			{
				double result;
				if (this.m_Clip == null)
				{
					result = base.get_duration();
				}
				else
				{
					result = (double)this.m_Clip.get_samples() / (double)this.m_Clip.get_frequency();
				}
				return result;
			}
		}

		public override IEnumerable<PlayableBinding> outputs
		{
			get
			{
				AudioPlayableAsset.<>c__Iterator0 <>c__Iterator = new AudioPlayableAsset.<>c__Iterator0();
				AudioPlayableAsset.<>c__Iterator0 expr_07 = <>c__Iterator;
				expr_07.$PC = -2;
				return expr_07;
			}
		}

		public ClipCaps clipCaps
		{
			get
			{
				return ClipCaps.ClipIn | ClipCaps.SpeedMultiplier | ClipCaps.Blending | ((!this.m_Loop) ? ClipCaps.None : ClipCaps.Looping);
			}
		}

		public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
		{
			Playable result;
			if (this.m_Clip == null)
			{
				result = Playable.get_Null();
			}
			else
			{
				result = AudioClipPlayable.Create(graph, this.m_Clip, this.m_Loop);
			}
			return result;
		}
	}
}
