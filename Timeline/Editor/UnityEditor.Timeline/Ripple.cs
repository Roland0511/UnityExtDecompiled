using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class Ripple
	{
		[Flags]
		public enum RippleDirection
		{
			After = 2,
			Before = 4,
			All = 6
		}

		private readonly Ripple.RippleDirection m_Direction;

		private float m_RippleTotal;

		private float m_AccumulatedOffsetLeft;

		private float m_AccumulatedOffsetRight;

		private List<TimelineClip> m_RippleAfter = new List<TimelineClip>();

		private List<TimelineClip> m_RippleBefore = new List<TimelineClip>();

		public Ripple(Ripple.RippleDirection direction)
		{
			this.m_Direction = direction;
		}

		public void Init(TimelineClipGUI gui, TimelineWindow.TimelineState state)
		{
			TrackAsset track = gui.parentTrackGUI.track;
			var source = SelectionManager.SelectedClipGUI().GroupBy((TimelineClipGUI p) => p.parentTrackGUI, (TimelineClipGUI p) => p.clip, (TimelineTrackGUI key, IEnumerable<TimelineClip> g) => new
			{
				track = key,
				clips = g.ToList<TimelineClip>()
			});
			if (!source.Any(r => r.track.track == track && r.clips[0] != gui.clip))
			{
				this.m_RippleTotal = 0f;
				List<TimelineClip> exclude = SelectionManager.SelectedItems<TimelineClip>().ToList<TimelineClip>();
				exclude.Add(gui.clip);
				if ((this.m_Direction & Ripple.RippleDirection.After) == Ripple.RippleDirection.After)
				{
					this.m_RippleAfter = (from c in track.clips
					where !exclude.Contains(c) && c.start >= gui.clip.start
					select c).ToList<TimelineClip>();
				}
				if ((this.m_Direction & Ripple.RippleDirection.Before) == Ripple.RippleDirection.Before)
				{
					this.m_RippleBefore = (from c in track.clips
					where !exclude.Contains(c) && c.start < gui.clip.start
					select c).ToList<TimelineClip>();
				}
			}
		}

		public void Run(float offset, TimelineWindow.TimelineState state)
		{
			this.m_RippleTotal += offset;
			if (this.m_RippleTotal >= 0f)
			{
				this.m_AccumulatedOffsetRight += offset;
				foreach (TimelineClip current in this.m_RippleAfter)
				{
					if (current.start + (double)offset < 0.0)
					{
						break;
					}
					current.start += (double)offset;
				}
				if (this.m_AccumulatedOffsetLeft > 0f)
				{
					foreach (TimelineClip current2 in this.m_RippleBefore)
					{
						if (current2.start - (double)this.m_AccumulatedOffsetLeft < 0.0)
						{
							break;
						}
						current2.start -= (double)this.m_AccumulatedOffsetLeft;
					}
					this.m_AccumulatedOffsetLeft = 0f;
				}
			}
			else
			{
				this.m_AccumulatedOffsetLeft += offset;
				foreach (TimelineClip current3 in this.m_RippleBefore)
				{
					if (current3.start + (double)offset < 0.0)
					{
						break;
					}
					current3.start += (double)offset;
				}
				if (this.m_AccumulatedOffsetRight > 0f)
				{
					foreach (TimelineClip current4 in this.m_RippleAfter)
					{
						if (current4.start - (double)this.m_AccumulatedOffsetRight < 0.0)
						{
							break;
						}
						current4.start -= (double)this.m_AccumulatedOffsetRight;
					}
					this.m_AccumulatedOffsetRight = 0f;
				}
			}
		}
	}
}
