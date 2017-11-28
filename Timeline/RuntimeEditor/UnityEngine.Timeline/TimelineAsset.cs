using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	[Serializable]
	public class TimelineAsset : PlayableAsset, ISerializationCallbackReceiver, ITimelineClipAsset, IPropertyPreview
	{
		public enum MediaType
		{
			Animation,
			Audio,
			Video,
			Script,
			Hybrid,
			Group
		}

		public enum DurationMode
		{
			BasedOnClips,
			FixedLength
		}

		[Serializable]
		public class EditorSettings
		{
			internal static readonly float kDefaultFPS = 60f;

			[HideInInspector, SerializeField]
			public float fps = TimelineAsset.EditorSettings.kDefaultFPS;
		}

		[HideInInspector, SerializeField]
		private int m_NextId;

		[HideInInspector, SerializeField]
		private List<TrackAsset> m_Tracks;

		[HideInInspector, SerializeField]
		private double m_FixedDuration;

		[HideInInspector]
		[NonSerialized]
		private TrackAsset[] m_CacheOutputTracks;

		[HideInInspector]
		[NonSerialized]
		private List<TrackAsset> m_CacheFlattenedTracks;

		[HideInInspector, SerializeField]
		private TimelineAsset.EditorSettings m_EditorSettings = new TimelineAsset.EditorSettings();

		[SerializeField]
		private TimelineAsset.DurationMode m_DurationMode;

		public TimelineAsset.EditorSettings editorSettings
		{
			get
			{
				return this.m_EditorSettings;
			}
		}

		public override double duration
		{
			get
			{
				double result;
				if (this.m_DurationMode == TimelineAsset.DurationMode.BasedOnClips)
				{
					result = this.CalculateDuration();
				}
				else
				{
					result = this.m_FixedDuration;
				}
				return result;
			}
		}

		public double fixedDuration
		{
			get
			{
				DiscreteTime lhs = (DiscreteTime)this.m_FixedDuration;
				double result;
				if (lhs <= 0)
				{
					result = 0.0;
				}
				else
				{
					result = (double)lhs.OneTickBefore();
				}
				return result;
			}
			set
			{
				this.m_FixedDuration = Math.Max(0.0, value);
			}
		}

		public TimelineAsset.DurationMode durationMode
		{
			get
			{
				return this.m_DurationMode;
			}
			set
			{
				this.m_DurationMode = value;
			}
		}

		public override IEnumerable<PlayableBinding> outputs
		{
			get
			{
				TimelineAsset.<>c__Iterator0 <>c__Iterator = new TimelineAsset.<>c__Iterator0();
				<>c__Iterator.$this = this;
				TimelineAsset.<>c__Iterator0 expr_0E = <>c__Iterator;
				expr_0E.$PC = -2;
				return expr_0E;
			}
		}

		public ClipCaps clipCaps
		{
			get
			{
				ClipCaps clipCaps = ClipCaps.All;
				foreach (TrackAsset current in this.m_Tracks)
				{
					TimelineClip[] clips = current.clips;
					for (int i = 0; i < clips.Length; i++)
					{
						TimelineClip timelineClip = clips[i];
						clipCaps &= timelineClip.clipCaps;
					}
				}
				return clipCaps;
			}
		}

		public int outputTrackCount
		{
			get
			{
				this.GetOutputTracks();
				return this.m_CacheOutputTracks.Length;
			}
		}

		public int rootTrackCount
		{
			get
			{
				return this.m_Tracks.Count;
			}
		}

		internal IEnumerable<TrackAsset> flattenedTracks
		{
			get
			{
				if (this.m_CacheFlattenedTracks == null)
				{
					this.m_CacheFlattenedTracks = new List<TrackAsset>(this.m_Tracks);
					foreach (TrackAsset current in this.m_Tracks)
					{
						TimelineAsset.AddSubTracksRecursive(current, ref this.m_CacheFlattenedTracks);
					}
				}
				return this.m_CacheFlattenedTracks;
			}
		}

		internal List<TrackAsset> tracks
		{
			get
			{
				return this.m_Tracks;
			}
		}

		public TrackAsset GetRootTrack(int index)
		{
			return this.m_Tracks[index];
		}

		public IEnumerable<TrackAsset> GetRootTracks()
		{
			return this.m_Tracks;
		}

		public TrackAsset GetOutputTrack(int index)
		{
			this.UpdateOutputTrackCache();
			return this.m_CacheOutputTracks[index];
		}

		public IEnumerable<TrackAsset> GetOutputTracks()
		{
			this.UpdateOutputTrackCache();
			return this.m_CacheOutputTracks;
		}

		private void UpdateOutputTrackCache()
		{
			if (this.m_CacheOutputTracks == null)
			{
				List<TrackAsset> list = new List<TrackAsset>();
				foreach (TrackAsset current in this.flattenedTracks)
				{
					if (current != null && current.mediaType != TimelineAsset.MediaType.Group && !current.isSubTrack)
					{
						list.Add(current);
					}
				}
				this.m_CacheOutputTracks = list.ToArray();
			}
		}

		public void OnEnable()
		{
			if (this.m_Tracks == null)
			{
				this.m_Tracks = new List<TrackAsset>();
			}
			this.m_Tracks.RemoveAll((TrackAsset t) => t == null);
		}

		internal void AddTrackInternal(TrackAsset track)
		{
			this.m_Tracks.Add(track);
			track.parent = this;
			this.Invalidate();
		}

		internal void RemoveTrack(TrackAsset track)
		{
			this.m_Tracks.Remove(track);
			this.Invalidate();
			TrackAsset trackAsset = track.parent as TrackAsset;
			if (trackAsset != null)
			{
				trackAsset.RemoveSubTrack(track);
			}
		}

		internal int GenerateNewId()
		{
			this.m_NextId++;
			return base.GetInstanceID().GetHashCode().CombineHash(this.m_NextId.GetHashCode());
		}

		public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
		{
			bool autoRebalance = true;
			bool createOutputs = graph.GetPlayableCount() == 0;
			ScriptPlayable<TimelinePlayable> scriptPlayable = TimelinePlayable.Create(graph, this.GetOutputTracks(), go, autoRebalance, createOutputs);
			return (!PlayableExtensions.IsValid<ScriptPlayable<TimelinePlayable>>(scriptPlayable)) ? Playable.get_Null() : scriptPlayable;
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			this.Invalidate();
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
		}

		public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
		{
			IEnumerable<TrackAsset> outputTracks = this.GetOutputTracks();
			foreach (TrackAsset current in outputTracks)
			{
				current.GatherProperties(director, driver);
			}
		}

		internal void Invalidate()
		{
			this.m_CacheOutputTracks = null;
			this.m_CacheFlattenedTracks = null;
		}

		private double CalculateDuration()
		{
			IEnumerable<TrackAsset> flattenedTracks = this.flattenedTracks;
			DiscreteTime lhs = new DiscreteTime(0);
			foreach (TrackAsset current in flattenedTracks)
			{
				lhs = DiscreteTime.Max(lhs, (DiscreteTime)current.end);
			}
			double result;
			if (lhs <= 0)
			{
				result = 0.0;
			}
			else
			{
				result = (double)lhs.OneTickBefore();
			}
			return result;
		}

		private static void AddSubTracksRecursive(TrackAsset track, ref List<TrackAsset> allTracks)
		{
			if (!(track == null))
			{
				if (track.subTracks != null && track.subTracks.Count != 0)
				{
					allTracks.AddRange(track.subTracks);
					foreach (TrackAsset current in track.subTracks)
					{
						TimelineAsset.AddSubTracksRecursive(current, ref allTracks);
					}
				}
			}
		}

		public TrackAsset CreateTrack(Type type, TrackAsset parent, string name)
		{
			if (parent != null && parent.timelineAsset != this)
			{
				throw new InvalidOperationException("Addtrack cannot parent to a track not in the Timeline");
			}
			if (!typeof(TrackAsset).IsAssignableFrom(type))
			{
				throw new InvalidOperationException("Supplied type must be a track asset");
			}
			if (parent != null)
			{
				if (!TimelineCreateUtilities.ValidateParentTrack(parent, type))
				{
					throw new InvalidOperationException("Cannot assign a child of type " + type.Name + "to a parent of type " + parent.GetType().Name);
				}
			}
			PlayableAsset playableAsset = (!(parent != null)) ? this : parent;
			TimelineUndo.PushUndo(playableAsset, "Create Track");
			string text = name;
			if (string.IsNullOrEmpty(text))
			{
				text = type.Name;
				text = ObjectNames.NicifyVariableName(text);
			}
			string text2 = TimelineCreateUtilities.GenerateUniqueActorName(this, text);
			TrackAsset trackAsset = this.AllocateTrack(parent, text2, type);
			if (trackAsset != null)
			{
				trackAsset.set_name(text2);
				TimelineCreateUtilities.SaveAssetIntoObject(trackAsset, playableAsset);
			}
			return trackAsset;
		}

		public T CreateTrack<T>(TrackAsset parent, string name) where T : TrackAsset, new()
		{
			return (T)((object)this.CreateTrack(typeof(T), parent, name));
		}

		public bool DeleteClip(TimelineClip clip)
		{
			bool result;
			if (clip == null || clip.parentTrack == null)
			{
				result = false;
			}
			else if (this != clip.parentTrack.timelineAsset)
			{
				Debug.LogError("Cannot delete a clip from this timeline");
				result = false;
			}
			else
			{
				TimelineUndo.PushUndo(clip.parentTrack, "Delete Clip");
				if (clip.curves != null)
				{
					TimelineUndo.PushDestroyUndo(this, clip.parentTrack, clip.curves, "Delete Curves");
				}
				if (clip.asset != null)
				{
					this.DeleteRecordedAnimation(clip);
					string assetPath = AssetDatabase.GetAssetPath(clip.asset);
					if (assetPath == AssetDatabase.GetAssetPath(this))
					{
						TimelineUndo.PushDestroyUndo(this, clip.parentTrack, clip.asset, "Delete Clip Asset");
					}
				}
				TrackAsset parentTrack = clip.parentTrack;
				parentTrack.RemoveClip(clip);
				parentTrack.CalculateExtrapolationTimes();
				result = true;
			}
			return result;
		}

		public bool DeleteTrack(TrackAsset track)
		{
			bool result;
			if (track.timelineAsset != this)
			{
				result = false;
			}
			else
			{
				TimelineUndo.PushUndo(track, "Delete Track");
				TimelineUndo.PushUndo(this, "Delete Track");
				TrackAsset trackAsset = track.parent as TrackAsset;
				if (trackAsset != null)
				{
					TimelineUndo.PushUndo(trackAsset, "Delete Track");
				}
				if (track.subTracks != null)
				{
					IEnumerable<TrackAsset> childTracks = track.GetChildTracks();
					foreach (TrackAsset current in childTracks)
					{
						this.DeleteTrack(current);
					}
				}
				this.DeleteRecordingClip(track);
				List<TimelineClip> list = new List<TimelineClip>(track.clips);
				foreach (TimelineClip current2 in list)
				{
					this.DeleteClip(current2);
				}
				this.RemoveTrack(track);
				TimelineUndo.PushDestroyUndo(this, this, track, "Delete Track");
				result = true;
			}
			return result;
		}

		internal TrackAsset AllocateTrack(TrackAsset trackAssetParent, string trackName, Type trackType)
		{
			if (trackAssetParent != null && trackAssetParent.timelineAsset != this)
			{
				throw new InvalidOperationException("Addtrack cannot parent to a track not in the Timeline");
			}
			if (!typeof(TrackAsset).IsAssignableFrom(trackType))
			{
				throw new InvalidOperationException("Supplied type must be a track asset");
			}
			TrackAsset trackAsset = (TrackAsset)ScriptableObject.CreateInstance(trackType);
			trackAsset.set_name(trackName);
			if (trackAssetParent != null)
			{
				trackAssetParent.AddChild(trackAsset);
			}
			else
			{
				this.AddTrackInternal(trackAsset);
			}
			return trackAsset;
		}

		private void DeleteRecordingClip(TrackAsset track)
		{
			AnimationTrack animationTrack = track as AnimationTrack;
			if (!(animationTrack == null) && !(animationTrack.animClip == null))
			{
				TimelineUndo.PushDestroyUndo(this, track, animationTrack.animClip, "Delete Track");
			}
		}

		private void DeleteRecordedAnimation(TimelineClip clip)
		{
			if (clip != null)
			{
				if (clip.curves != null)
				{
					TimelineUndo.PushDestroyUndo(this, clip.parentTrack, clip.curves, "Delete Parameters");
				}
				if (clip.recordable)
				{
					AnimationPlayableAsset animationPlayableAsset = clip.asset as AnimationPlayableAsset;
					if (!(animationPlayableAsset == null) && !(animationPlayableAsset.clip == null))
					{
						TimelineUndo.PushDestroyUndo(this, animationPlayableAsset, animationPlayableAsset.clip, "Delete Recording");
					}
				}
			}
		}
	}
}
