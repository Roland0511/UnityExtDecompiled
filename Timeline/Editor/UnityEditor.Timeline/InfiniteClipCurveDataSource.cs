using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class InfiniteClipCurveDataSource : CurveDataSource
	{
		private readonly AnimationTrack m_AnimationTrack;

		public override AnimationClip animationClip
		{
			get
			{
				return this.m_AnimationTrack.animClip;
			}
		}

		public override float start
		{
			get
			{
				return 0f;
			}
		}

		public override float timeScale
		{
			get
			{
				return 1f;
			}
		}

		public InfiniteClipCurveDataSource(TimelineTrackGUI trackGui) : base(trackGui)
		{
			this.m_AnimationTrack = (trackGui.track as AnimationTrack);
		}
	}
}
