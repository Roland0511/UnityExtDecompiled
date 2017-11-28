using System;

namespace UnityEngine.Timeline
{
	[Serializable]
	internal class TimelineMarker : ITimelineItem
	{
		[SerializeField]
		private string m_Key;

		[HideInInspector, SerializeField]
		private TrackAsset m_ParentTrack;

		[SerializeField, TimeField]
		private double m_Time;

		[HideInInspector, SerializeField]
		private bool m_Selected;

		double ITimelineItem.start
		{
			get
			{
				return this.m_Time;
			}
		}

		public string key
		{
			get
			{
				return this.m_Key;
			}
			internal set
			{
				if (this.m_Key != value && this.parentTrack != null && this.parentTrack.timelineAsset != null)
				{
					this.parentTrack.timelineAsset.Invalidate();
				}
				this.m_Key = value;
			}
		}

		public double time
		{
			get
			{
				return this.m_Time;
			}
			set
			{
				this.m_Time = value;
			}
		}

		public bool selected
		{
			get
			{
				return this.m_Selected;
			}
			set
			{
				this.m_Selected = value;
			}
		}

		public TrackAsset parentTrack
		{
			get
			{
				return this.m_ParentTrack;
			}
		}

		public TimelineMarker(string key, double time, TrackAsset parentTrack)
		{
			if (key == null)
			{
				key = string.Empty;
			}
			this.m_Key = key;
			this.m_Time = time;
			this.m_ParentTrack = parentTrack;
		}

		int ITimelineItem.Hash()
		{
			return this.m_Time.GetHashCode() ^ this.m_Key.GetHashCode();
		}
	}
}
