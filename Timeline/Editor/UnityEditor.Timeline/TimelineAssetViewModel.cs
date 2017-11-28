using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[Serializable]
	internal class TimelineAssetViewModel : ScriptableObject, ISerializationCallbackReceiver
	{
		public static readonly float k_DefaultTrackHeight = 30f;

		public static readonly float k_DefaultTrackScale = 1f;

		internal static readonly Vector2 kTimeAreaDefaultRange = new Vector2(-5f, 5f);

		[SerializeField]
		public bool timeInFrames = true;

		[SerializeField]
		public Vector2 timeAreaShownRange = TimelineAssetViewModel.kTimeAreaDefaultRange;

		[SerializeField]
		public bool showAudioWaveform = true;

		[SerializeField]
		public float trackHeight = TimelineAssetViewModel.k_DefaultTrackHeight;

		[SerializeField]
		public float trackScale = TimelineAssetViewModel.k_DefaultTrackScale;

		[SerializeField]
		public bool playRangeEnabled = false;

		[SerializeField]
		public Vector2 timeAreaPlayRange = TimelineAssetViewModel.kNoPlayRangeSet;

		public static readonly Vector2 kNoPlayRangeSet = new Vector2(3.40282347E+38f, 3.40282347E+38f);

		public Dictionary<TrackAsset, TrackViewModelData> tracksViewModelData = new Dictionary<TrackAsset, TrackViewModelData>();

		[SerializeField]
		private List<TrackAsset> m_Keys = new List<TrackAsset>();

		[SerializeField]
		private List<TrackViewModelData> m_Vals = new List<TrackViewModelData>();

		public void OnBeforeSerialize()
		{
			this.m_Keys.Clear();
			this.m_Vals.Clear();
			foreach (KeyValuePair<TrackAsset, TrackViewModelData> current in this.tracksViewModelData)
			{
				if (current.Key != null && current.Value != null && (current.Key.get_hideFlags() & 52) == null)
				{
					this.m_Keys.Add(current.Key);
					this.m_Vals.Add(current.Value);
				}
			}
		}

		public void OnAfterDeserialize()
		{
		}

		public void OnEnable()
		{
			if (this.m_Keys.Count == this.m_Vals.Count)
			{
				this.tracksViewModelData.Clear();
				for (int i = 0; i < this.m_Keys.Count; i++)
				{
					if (this.m_Keys[i] != null)
					{
						this.tracksViewModelData[this.m_Keys[i]] = this.m_Vals[i];
					}
				}
			}
			this.m_Keys.Clear();
			this.m_Vals.Clear();
		}
	}
}
