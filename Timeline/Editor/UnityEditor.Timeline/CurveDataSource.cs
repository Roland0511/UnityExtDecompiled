using System;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal abstract class CurveDataSource
	{
		protected readonly TimelineTrackGUI m_TrackGUI;

		protected bool infiniteEditor = true;

		public abstract AnimationClip animationClip
		{
			get;
		}

		public abstract float start
		{
			get;
		}

		public abstract float timeScale
		{
			get;
		}

		protected CurveDataSource(TimelineTrackGUI trackGUI)
		{
			this.m_TrackGUI = trackGUI;
		}

		public void SetHeight(float height)
		{
			this.m_TrackGUI.SetHeight(height);
		}

		public Rect GetBackgroundRect(TimelineWindow.TimelineState state)
		{
			Rect boundingRect = this.m_TrackGUI.boundingRect;
			return new Rect(state.timeAreaTranslation.x + boundingRect.get_xMin(), boundingRect.get_y(), (float)state.timeline.get_duration() * state.timeAreaScale.x, boundingRect.get_height());
		}
	}
}
