using System;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class TimelineClipCurveDataSource : CurveDataSource
	{
		private readonly TimelineClipGUI m_ClipGUI;

		public override AnimationClip animationClip
		{
			get
			{
				return this.m_ClipGUI.clip.animationClip ?? this.m_ClipGUI.clip.curves;
			}
		}

		public override float start
		{
			get
			{
				return (float)(this.m_ClipGUI.clip.start - this.m_ClipGUI.clip.clipIn / this.m_ClipGUI.clip.timeScale);
			}
		}

		public override float timeScale
		{
			get
			{
				return (float)this.m_ClipGUI.clip.timeScale;
			}
		}

		public TimelineClipCurveDataSource(TimelineClipGUI clipGUI) : base(clipGUI.parentTrackGUI)
		{
			this.m_ClipGUI = clipGUI;
		}
	}
}
