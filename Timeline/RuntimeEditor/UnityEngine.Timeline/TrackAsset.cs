using System;
using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace UnityEngine.Timeline
{
	[IgnoreOnPlayableTrack]
	[Serializable]
	public abstract class TrackAsset : PlayableAsset, ISerializationCallbackReceiver, IPropertyPreview
	{
		[SerializeField]
		private bool m_Locked;

		[SerializeField]
		private bool m_Muted;

		[HideInInspector, SerializeField]
		private string m_CustomPlayableFullTypename = string.Empty;

		[HideInInspector, FormerlySerializedAs("m_animClip"), SerializeField]
		private AnimationClip m_AnimClip;

		[HideInInspector, SerializeField]
		private PlayableAsset m_Parent;

		[HideInInspector, SerializeField]
		private List<TrackAsset> m_Children;

		[NonSerialized]
		private int m_ItemsHash;

		[NonSerialized]
		private TimelineClip[] m_ClipsCache;

		private DiscreteTime m_Start;

		private DiscreteTime m_End;

		private TimelineAsset.MediaType m_MediaType;

		private static Dictionary<Type, TrackBindingTypeAttribute> s_TrackBindingTypeAttributeCache = new Dictionary<Type, TrackBindingTypeAttribute>();

		[HideInInspector, SerializeField]
		protected internal List<TimelineClip> m_Clips = new List<TimelineClip>();

		public double start
		{
			get
			{
				this.UpdateDuration();
				return (double)this.m_Start;
			}
		}

		public double end
		{
			get
			{
				this.UpdateDuration();
				return (double)this.m_End;
			}
		}

		public sealed override double duration
		{
			get
			{
				this.UpdateDuration();
				return (double)(this.m_End - this.m_Start);
			}
		}

		public bool muted
		{
			get
			{
				return this.m_Muted;
			}
			set
			{
				this.m_Muted = value;
			}
		}

		public TimelineAsset timelineAsset
		{
			get
			{
				TrackAsset trackAsset = this;
				TimelineAsset result;
				while (trackAsset != null)
				{
					if (trackAsset.parent == null)
					{
						result = null;
					}
					else
					{
						TimelineAsset timelineAsset = trackAsset.parent as TimelineAsset;
						if (!(timelineAsset != null))
						{
							trackAsset = (trackAsset.parent as TrackAsset);
							continue;
						}
						result = timelineAsset;
					}
					return result;
				}
				result = null;
				return result;
			}
		}

		public PlayableAsset parent
		{
			get
			{
				return this.m_Parent;
			}
			internal set
			{
				this.m_Parent = value;
			}
		}

		internal TimelineClip[] clips
		{
			get
			{
				if (this.m_Clips == null)
				{
					this.m_Clips = new List<TimelineClip>();
				}
				if (this.m_ClipsCache == null)
				{
					this.m_ClipsCache = this.m_Clips.ToArray();
				}
				return this.m_ClipsCache;
			}
		}

		public virtual bool isEmpty
		{
			get
			{
				return !this.hasClips;
			}
		}

		internal bool hasClips
		{
			get
			{
				return this.m_Clips != null && this.m_Clips.Count != 0;
			}
		}

		public bool isSubTrack
		{
			get
			{
				TrackAsset trackAsset = this.parent as TrackAsset;
				return trackAsset != null && trackAsset.GetType() == base.GetType();
			}
		}

		public override IEnumerable<PlayableBinding> outputs
		{
			get
			{
				TrackAsset.<>c__Iterator0 <>c__Iterator = new TrackAsset.<>c__Iterator0();
				<>c__Iterator.$this = this;
				TrackAsset.<>c__Iterator0 expr_0E = <>c__Iterator;
				expr_0E.$PC = -2;
				return expr_0E;
			}
		}

		internal string customPlayableTypename
		{
			get
			{
				return this.m_CustomPlayableFullTypename;
			}
			set
			{
				this.m_CustomPlayableFullTypename = value;
			}
		}

		internal AnimationClip animClip
		{
			get
			{
				return this.m_AnimClip;
			}
			set
			{
				this.m_AnimClip = value;
			}
		}

		internal List<TrackAsset> subTracks
		{
			get
			{
				return this.m_Children;
			}
			set
			{
				this.m_Children = value;
			}
		}

		internal bool locked
		{
			get
			{
				return this.m_Locked;
			}
			set
			{
				this.m_Locked = value;
			}
		}

		internal TimelineAsset.MediaType mediaType
		{
			get
			{
				return this.m_MediaType;
			}
		}

		internal virtual bool compilable
		{
			get
			{
				return !this.muted && !this.isEmpty;
			}
		}

		protected TrackAsset()
		{
			this.m_MediaType = TrackAsset.GetMediaType(base.GetType());
		}

		public IEnumerable<TimelineClip> GetClips()
		{
			return this.clips;
		}

		public IEnumerable<TrackAsset> GetChildTracks()
		{
			return this.m_Children.ToArray();
		}

		public void OnBeforeSerialize()
		{
		}

		public void OnAfterDeserialize()
		{
			this.m_ClipsCache = null;
			this.InvalidateTimelineCaches();
		}

		public virtual void OnEnable()
		{
			if (this.m_Children == null)
			{
				this.m_Children = new List<TrackAsset>();
			}
			if (this.m_Clips == null)
			{
				this.m_Clips = new List<TimelineClip>();
			}
			if (this.m_Children.Count > 0)
			{
				List<TrackAsset> list = new List<TrackAsset>();
				foreach (TrackAsset current in this.m_Children)
				{
					if (current != null)
					{
						list.Add(current);
					}
				}
				this.m_Children = list;
			}
		}

		public virtual Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
		{
			return Playable.Create(graph, inputCount);
		}

		public sealed override Playable CreatePlayable(PlayableGraph graph, GameObject go)
		{
			return Playable.get_Null();
		}

		public TimelineClip CreateDefaultClip()
		{
			object[] customAttributes = base.GetType().GetCustomAttributes(typeof(TrackClipTypeAttribute), true);
			Type type = null;
			object[] array = customAttributes;
			for (int i = 0; i < array.Length; i++)
			{
				object obj = array[i];
				TrackClipTypeAttribute trackClipTypeAttribute = obj as TrackClipTypeAttribute;
				if (trackClipTypeAttribute != null && typeof(IPlayableAsset).IsAssignableFrom(trackClipTypeAttribute.inspectedType) && typeof(ScriptableObject).IsAssignableFrom(trackClipTypeAttribute.inspectedType))
				{
					type = trackClipTypeAttribute.inspectedType;
					break;
				}
			}
			TimelineClip result;
			if (type == null)
			{
				Debug.LogWarning("Cannot create a default clip for type " + base.GetType());
				result = null;
			}
			else
			{
				result = this.CreateClipOfType(type);
			}
			return result;
		}

		internal TimelineClip CreateClipOfType(Type type)
		{
			if (type == null || !typeof(IPlayableAsset).IsAssignableFrom(type) || !typeof(ScriptableObject).IsAssignableFrom(type))
			{
				throw new InvalidOperationException("Cannot create a clip for type " + type.Name);
			}
			ScriptableObject scriptableObject = ScriptableObject.CreateInstance(type);
			if (scriptableObject == null)
			{
				throw new InvalidOperationException("Could not create an instance of the ScriptableObject type " + type.Name);
			}
			scriptableObject.set_name(type.Name);
			TimelineCreateUtilities.SaveAssetIntoObject(scriptableObject, this);
			TimelineClip timelineClip = this.CreateNewClipContainerInternal();
			timelineClip.displayName = scriptableObject.get_name();
			timelineClip.asset = scriptableObject;
			TimelineClip result;
			try
			{
				this.OnCreateClipFromAsset(scriptableObject, timelineClip);
			}
			catch (Exception ex)
			{
				Debug.LogError(ex.Message, scriptableObject);
				result = null;
				return result;
			}
			this.AddClip(timelineClip);
			result = timelineClip;
			return result;
		}

		internal void AddClip(TimelineClip newClip)
		{
			if (!this.m_Clips.Contains(newClip))
			{
				this.m_Clips.Add(newClip);
				this.m_ClipsCache = null;
			}
		}

		internal Playable CreatePlayableGraph(PlayableGraph graph, GameObject go, IntervalTree<RuntimeElement> tree)
		{
			this.UpdateDuration();
			return this.OnCreatePlayableGraph(graph, go, tree);
		}

		internal virtual Playable OnCreatePlayableGraph(PlayableGraph graph, GameObject go, IntervalTree<RuntimeElement> tree)
		{
			if (tree == null)
			{
				throw new ArgumentException("IntervalTree argument cannot be null", "tree");
			}
			if (go == null)
			{
				throw new ArgumentException("GameObject argument cannot be null", "go");
			}
			Playable playable = this.CreateTrackMixer(graph, go, this.clips.Length);
			for (int i = 0; i < this.clips.Length; i++)
			{
				Playable playable2 = this.CreatePlayable(graph, go, this.clips[i]);
				if (PlayableExtensions.IsValid<Playable>(playable2))
				{
					PlayableExtensions.SetDuration<Playable>(playable2, this.clips[i].duration);
					RuntimeClip item = new RuntimeClip(this.clips[i], playable2, playable);
					tree.Add(item);
					graph.Connect<Playable, Playable>(playable2, 0, playable, i);
					PlayableExtensions.SetInputWeight<Playable>(playable, i, 0f);
				}
			}
			return playable;
		}

		internal virtual bool IsCompatibleWithItem(ITimelineItem item)
		{
			TimelineClip timelineClip = item as TimelineClip;
			bool result;
			if (timelineClip != null)
			{
				result = this.ValidateClipType(timelineClip.asset.GetType());
			}
			else
			{
				TimelineMarker timelineMarker = item as TimelineMarker;
				result = (timelineMarker != null && this is ITimelineMarkerContainer);
			}
			return result;
		}

		internal void SortClips()
		{
			this.m_Clips.Sort((TimelineClip clip1, TimelineClip clip2) => clip1.start.CompareTo(clip2.start));
			this.m_ClipsCache = null;
		}

		internal void SetClips(List<TimelineClip> timelineClips)
		{
			this.m_Clips = timelineClips;
			this.m_ClipsCache = timelineClips.ToArray();
		}

		internal TimelineClip CreateClipFromAsset(Object asset)
		{
			TimelineClip result;
			if (asset == null)
			{
				result = null;
			}
			else
			{
				TimelineUndo.PushUndo(this, "Create Clip");
				TimelineAsset timelineAsset = asset as TimelineAsset;
				if (timelineAsset == null && !this.ValidateClipType(asset.GetType()))
				{
					throw new InvalidOperationException(string.Format("Cannot create a clip for track {0} with clip type: {1}. Did you forget a ClipClass attribute?", base.GetType(), asset.GetType()));
				}
				TimelineClip timelineClip = this.CreateNewClipContainerInternal();
				timelineClip.underlyingAsset = asset;
				timelineClip.displayName = asset.get_name();
				timelineClip.asset = asset;
				try
				{
					this.OnCreateClipFromAsset(asset, timelineClip);
				}
				catch (Exception ex)
				{
					Debug.LogError(ex.Message, asset);
					this.RemoveClip(timelineClip);
					result = null;
					return result;
				}
				result = timelineClip;
			}
			return result;
		}

		internal virtual void OnCreateClipFromAsset(Object asset, TimelineClip clip)
		{
			clip.asset = asset;
			clip.underlyingAsset = asset;
			clip.displayName = asset.get_name();
			IPlayableAsset playableAsset = asset as IPlayableAsset;
			if (playableAsset != null)
			{
				double duration = playableAsset.get_duration();
				if (duration > 0.0 && !double.IsInfinity(duration))
				{
					clip.duration = duration;
				}
			}
		}

		internal TimelineClip CreateNewClipContainerInternal()
		{
			TimelineClip timelineClip = new TimelineClip(this);
			timelineClip.asset = null;
			double num = 0.0;
			for (int i = 0; i < this.m_Clips.Count - 1; i++)
			{
				double num2 = this.m_Clips[i].duration;
				if (double.IsInfinity(num2))
				{
					num2 = (double)TimelineClip.kDefaultClipDurationInSeconds;
				}
				num = Math.Max(num, this.m_Clips[i].start + num2);
			}
			timelineClip.mixInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
			timelineClip.mixOutCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
			timelineClip.start = num;
			timelineClip.duration = (double)TimelineClip.kDefaultClipDurationInSeconds;
			timelineClip.displayName = "untitled";
			return timelineClip;
		}

		internal void AddChild(TrackAsset child)
		{
			if (!(child == null))
			{
				this.m_Children.Add(child);
				child.parent = this;
				this.InvalidateTimelineCaches();
			}
		}

		internal bool AddChildAfter(TrackAsset child, TrackAsset other)
		{
			bool result;
			if (child == null)
			{
				result = false;
			}
			else
			{
				int num = this.m_Children.IndexOf(other);
				if (num >= 0 && num != this.m_Children.Count - 1)
				{
					this.m_Children.Insert(num + 1, child);
				}
				else
				{
					this.m_Children.Add(child);
				}
				this.InvalidateTimelineCaches();
				child.parent = this;
				result = true;
			}
			return result;
		}

		internal bool RemoveSubTrack(TrackAsset child)
		{
			bool result;
			if (this.m_Children.Remove(child))
			{
				this.InvalidateTimelineCaches();
				child.parent = null;
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

		internal void RemoveClip(TimelineClip clip)
		{
			this.m_Clips.Remove(clip);
			this.m_ClipsCache = null;
		}

		internal virtual void GetEvaluationTime(out double outStart, out double outDuration)
		{
			if (this.clips.Length == 0)
			{
				outStart = 0.0;
				outDuration = this.GetMarkerDuration();
			}
			else
			{
				outStart = 1.7976931348623157E+308;
				double num = 0.0;
				for (int i = 0; i < this.clips.Length; i++)
				{
					outStart = Math.Min(this.clips[i].start, outStart);
					num = Math.Max(this.clips[i].start + this.clips[i].duration, num);
				}
				outStart = Math.Max(outStart, 0.0);
				outDuration = Math.Max(this.GetMarkerDuration(), num - outStart);
			}
		}

		internal virtual void GetSequenceTime(out double outStart, out double outDuration)
		{
			this.GetEvaluationTime(out outStart, out outDuration);
		}

		public virtual void GatherProperties(PlayableDirector director, IPropertyCollector driver)
		{
			GameObject gameObjectBinding = this.GetGameObjectBinding(director);
			if (gameObjectBinding != null)
			{
				driver.PushActiveGameObject(gameObjectBinding);
			}
			if (this.animClip != null)
			{
				driver.AddFromClip(this.animClip);
			}
			TimelineClip[] clips = this.clips;
			for (int i = 0; i < clips.Length; i++)
			{
				TimelineClip timelineClip = clips[i];
				if (timelineClip.curves != null && timelineClip.asset != null)
				{
					driver.AddObjectProperties(timelineClip.asset, timelineClip.curves);
				}
				IPropertyPreview propertyPreview = timelineClip.asset as IPropertyPreview;
				if (propertyPreview != null)
				{
					propertyPreview.GatherProperties(director, driver);
				}
			}
			foreach (TrackAsset current in this.subTracks)
			{
				if (current != null)
				{
					current.GatherProperties(director, driver);
				}
			}
			if (gameObjectBinding != null)
			{
				driver.PopActiveGameObject();
			}
		}

		internal GameObject GetGameObjectBinding(PlayableDirector director)
		{
			GameObject result;
			if (director == null)
			{
				result = null;
			}
			else
			{
				Object genericBinding = director.GetGenericBinding(this);
				GameObject gameObject = genericBinding as GameObject;
				if (gameObject != null)
				{
					result = gameObject;
				}
				else
				{
					Component component = genericBinding as Component;
					if (component != null)
					{
						result = component.get_gameObject();
					}
					else
					{
						result = null;
					}
				}
			}
			return result;
		}

		private bool ValidateClipType(Type clipType)
		{
			object[] customAttributes = base.GetType().GetCustomAttributes(typeof(TrackClipTypeAttribute), true);
			bool result;
			for (int i = 0; i < customAttributes.Length; i++)
			{
				object obj = customAttributes[i];
				TrackClipTypeAttribute trackClipTypeAttribute = (TrackClipTypeAttribute)obj;
				if (trackClipTypeAttribute.inspectedType.IsAssignableFrom(clipType))
				{
					result = true;
					return result;
				}
			}
			result = false;
			return result;
		}

		protected internal virtual void UpdateDuration()
		{
			int h = (!(this.m_AnimClip != null)) ? 0 : ((int)(this.m_AnimClip.get_frameRate() * this.m_AnimClip.get_length()));
			int num = HashUtility.CombineHash(this.GetClipsHash(), this.GetMarkerHash(), h);
			if (num != this.m_ItemsHash)
			{
				this.m_ItemsHash = num;
				double num2;
				double num3;
				this.GetSequenceTime(out num2, out num3);
				this.m_Start = (DiscreteTime)num2;
				this.m_End = (DiscreteTime)(num2 + num3);
				this.CalculateExtrapolationTimes();
			}
		}

		protected internal virtual Playable CreatePlayable(PlayableGraph graph, GameObject go, TimelineClip clip)
		{
			IPlayableAsset playableAsset = clip.asset as IPlayableAsset;
			Playable result;
			if (playableAsset != null)
			{
				Playable playable = playableAsset.CreatePlayable(graph, go);
				if (PlayableExtensions.IsValid<Playable>(playable))
				{
					AnimationPlayableExtensions.SetAnimatedProperties<Playable>(playable, clip.curves);
					PlayableExtensions.SetSpeed<Playable>(playable, clip.timeScale);
				}
				result = playable;
			}
			else
			{
				result = Playable.get_Null();
			}
			return result;
		}

		private void InvalidateTimelineCaches()
		{
			TimelineAsset timelineAsset = this.timelineAsset;
			if (timelineAsset != null)
			{
				timelineAsset.Invalidate();
			}
		}

		protected internal virtual int Hash()
		{
			return this.clips.Length + (this.GetMarkerContainerHash() << 16);
		}

		private int GetClipsHash()
		{
			int num = 0;
			foreach (ITimelineItem current in this.m_Clips)
			{
				num = num.CombineHash(current.Hash());
			}
			return num;
		}

		private int GetMarkerContainerHash()
		{
			ITimelineMarkerContainer timelineMarkerContainer = this as ITimelineMarkerContainer;
			int result;
			if (timelineMarkerContainer == null)
			{
				result = 0;
			}
			else
			{
				TimelineMarker[] markers = timelineMarkerContainer.GetMarkers();
				result = ((markers != null) ? markers.Length : 0);
			}
			return result;
		}

		private int GetMarkerHash()
		{
			ITimelineMarkerContainer timelineMarkerContainer = this as ITimelineMarkerContainer;
			int num = 0;
			if (timelineMarkerContainer != null)
			{
				TimelineMarker[] markers = timelineMarkerContainer.GetMarkers();
				if (markers != null)
				{
					for (int i = 0; i < markers.Length; i++)
					{
						num = num.CombineHash(((ITimelineItem)markers[i]).Hash());
					}
				}
			}
			return num;
		}

		private double GetMarkerDuration()
		{
			ITimelineMarkerContainer timelineMarkerContainer = this as ITimelineMarkerContainer;
			double num = 0.0;
			if (timelineMarkerContainer != null)
			{
				TimelineMarker[] markers = timelineMarkerContainer.GetMarkers();
				if (markers != null)
				{
					for (int i = 0; i < markers.Length; i++)
					{
						num = Math.Max(num, markers[i].time);
					}
				}
			}
			return num;
		}

		private static TimelineAsset.MediaType GetMediaType(Type t)
		{
			object[] customAttributes = t.GetCustomAttributes(typeof(TrackMediaType), true);
			TimelineAsset.MediaType result;
			if (customAttributes.Length > 0 && customAttributes[0] is TrackMediaType)
			{
				result = ((TrackMediaType)customAttributes[0]).m_MediaType;
			}
			else
			{
				result = TimelineAsset.MediaType.Script;
			}
			return result;
		}
	}
}
