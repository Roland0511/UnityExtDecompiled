using System;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class FrameSnap
	{
		private double m_CurrentOffset;

		private double m_LastOffsetApplied;

		public double lastOffsetApplied
		{
			get
			{
				return this.m_LastOffsetApplied;
			}
		}

		public void Reset()
		{
			this.m_CurrentOffset = 0.0;
		}

		public double ApplyOffset(double currentValue, float delta, TimelineWindow.TimelineState state)
		{
			if (state.frameSnap)
			{
				double num = currentValue;
				this.m_LastOffsetApplied = 0.0;
				bool flag = true;
				this.m_CurrentOffset += (double)delta;
				if (!TimeUtility.OnFrameBoundary(currentValue, (double)state.frameRate))
				{
					double frames = TimeUtility.ToExactFrames(currentValue, (double)state.frameRate) - (double)TimeUtility.ToFrames(currentValue, (double)state.frameRate);
					double num2 = TimeUtility.FromFrames(frames, (double)state.frameRate);
					if (Math.Abs(this.m_CurrentOffset) >= Math.Abs(num2))
					{
						currentValue += num2;
						this.m_CurrentOffset -= num2;
					}
					else
					{
						flag = false;
					}
				}
				if (flag)
				{
					double num3 = (double)TimeUtility.ToFrames(this.m_CurrentOffset, (double)state.frameRate);
					this.m_CurrentOffset = TimeUtility.FromFrames(TimeUtility.ToExactFrames(this.m_CurrentOffset, (double)state.frameRate) - num3, (double)state.frameRate);
					currentValue = TimeUtility.FromFrames(num3 + (double)TimeUtility.ToFrames(currentValue, (double)state.frameRate), (double)state.frameRate);
				}
				this.m_LastOffsetApplied = currentValue - num;
			}
			else
			{
				this.m_LastOffsetApplied = (double)delta;
				currentValue += (double)delta;
			}
			return currentValue;
		}
	}
}
