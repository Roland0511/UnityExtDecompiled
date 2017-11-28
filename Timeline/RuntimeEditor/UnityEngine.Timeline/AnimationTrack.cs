using System;
using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	[SupportsChildTracks(typeof(AnimationTrack), 1), TrackClipType(typeof(AnimationPlayableAsset)), TrackClipType(typeof(AnimationClip)), TrackMediaType(TimelineAsset.MediaType.Animation)]
	[Serializable]
	public class AnimationTrack : TrackAsset
	{
		protected AnimationPlayableAsset m_AnimationPlayableAsset;

		protected TimelineClip m_FakeAnimClip;

		[SerializeField]
		private TimelineClip.ClipExtrapolation m_OpenClipPreExtrapolation = TimelineClip.ClipExtrapolation.None;

		[SerializeField]
		private TimelineClip.ClipExtrapolation m_OpenClipPostExtrapolation = TimelineClip.ClipExtrapolation.None;

		[SerializeField]
		private Vector3 m_OpenClipOffsetPosition = Vector3.get_zero();

		[SerializeField]
		private Quaternion m_OpenClipOffsetRotation = Quaternion.get_identity();

		[SerializeField]
		private double m_OpenClipTimeOffset;

		[SerializeField]
		private MatchTargetFields m_MatchTargetFields = MatchTargetFieldConstants.All;

		[SerializeField]
		private Vector3 m_Position = Vector3.get_zero();

		[SerializeField]
		private Quaternion m_Rotation = Quaternion.get_identity();

		[SerializeField]
		private bool m_ApplyOffsets;

		[SerializeField]
		private AvatarMask m_AvatarMask;

		[SerializeField]
		private bool m_ApplyAvatarMask = true;

		private AnimationOffsetPlayable m_ClipOffset;

		public Vector3 position
		{
			get
			{
				return this.m_Position;
			}
			set
			{
				this.m_Position = value;
			}
		}

		public Quaternion rotation
		{
			get
			{
				return this.m_Rotation;
			}
			set
			{
				this.m_Rotation = value;
			}
		}

		public bool applyOffsets
		{
			get
			{
				return this.m_ApplyOffsets;
			}
			set
			{
				this.m_ApplyOffsets = value;
			}
		}

		public MatchTargetFields matchTargetFields
		{
			get
			{
				return this.m_MatchTargetFields;
			}
			set
			{
				this.m_MatchTargetFields = (value & MatchTargetFieldConstants.All);
			}
		}

		private bool compilableIsolated
		{
			get
			{
				return !base.muted && (this.m_Clips.Count > 0 || (base.animClip != null && !base.animClip.get_empty()));
			}
		}

		public AvatarMask avatarMask
		{
			get
			{
				return this.m_AvatarMask;
			}
			set
			{
				this.m_AvatarMask = value;
			}
		}

		public bool applyAvatarMask
		{
			get
			{
				return this.m_ApplyAvatarMask;
			}
			set
			{
				this.m_ApplyAvatarMask = value;
			}
		}

		internal override bool compilable
		{
			get
			{
				bool result;
				if (this.compilableIsolated)
				{
					result = true;
				}
				else
				{
					foreach (TrackAsset current in base.subTracks)
					{
						if (current.compilable)
						{
							result = true;
							return result;
						}
					}
					result = false;
				}
				return result;
			}
		}

		public override IEnumerable<PlayableBinding> outputs
		{
			get
			{
				AnimationTrack.<>c__Iterator0 <>c__Iterator = new AnimationTrack.<>c__Iterator0();
				<>c__Iterator.$this = this;
				AnimationTrack.<>c__Iterator0 expr_0E = <>c__Iterator;
				expr_0E.$PC = -2;
				return expr_0E;
			}
		}

		public bool inClipMode
		{
			get
			{
				return base.clips != null && base.clips.Length != 0;
			}
		}

		public Vector3 openClipOffsetPosition
		{
			get
			{
				return this.m_OpenClipOffsetPosition;
			}
			set
			{
				this.m_OpenClipOffsetPosition = value;
			}
		}

		public Quaternion openClipOffsetRotation
		{
			get
			{
				return this.m_OpenClipOffsetRotation;
			}
			set
			{
				this.m_OpenClipOffsetRotation = value;
			}
		}

		internal double openClipTimeOffset
		{
			get
			{
				return this.m_OpenClipTimeOffset;
			}
			set
			{
				this.m_OpenClipTimeOffset = value;
			}
		}

		public TimelineClip.ClipExtrapolation openClipPreExtrapolation
		{
			get
			{
				return this.m_OpenClipPreExtrapolation;
			}
			set
			{
				this.m_OpenClipPreExtrapolation = value;
			}
		}

		public TimelineClip.ClipExtrapolation openClipPostExtrapolation
		{
			get
			{
				return this.m_OpenClipPostExtrapolation;
			}
			set
			{
				this.m_OpenClipPostExtrapolation = value;
			}
		}

		[ContextMenu("Reset Offsets")]
		private void ResetOffsets()
		{
			this.m_Position = Vector3.get_zero();
			this.m_Rotation = Quaternion.get_identity();
			this.UpdateClipOffsets();
		}

		public TimelineClip CreateClip(AnimationClip clip)
		{
			TimelineClip result;
			if (clip == null)
			{
				result = null;
			}
			else
			{
				TimelineClip timelineClip = base.CreateDefaultClip();
				this.AssignAnimationClip(timelineClip, clip);
				result = timelineClip;
			}
			return result;
		}

		internal void UpdateClipOffsets()
		{
			if (PlayableExtensions.IsValid<AnimationOffsetPlayable>(this.m_ClipOffset))
			{
				this.m_ClipOffset.SetPosition(this.m_Position);
				this.m_ClipOffset.SetRotation(this.m_Rotation);
			}
		}

		internal override void OnCreateClipFromAsset(Object asset, TimelineClip clip)
		{
			AnimationClip animationClip = asset as AnimationClip;
			if (animationClip != null)
			{
				if (animationClip.get_legacy())
				{
					throw new InvalidOperationException("Legacy Animation Clips are not supported");
				}
				AnimationPlayableAsset animationPlayableAsset = ScriptableObject.CreateInstance<AnimationPlayableAsset>();
				TimelineCreateUtilities.SaveAssetIntoObject(animationPlayableAsset, this);
				animationPlayableAsset.clip = animationClip;
				clip.asset = animationPlayableAsset;
				this.AssignAnimationClip(clip, animationClip);
			}
		}

		internal Playable CompileTrackPlayable(PlayableGraph graph, TrackAsset track, GameObject go, IntervalTree<RuntimeElement> tree)
		{
			AnimationMixerPlayable animationMixerPlayable = AnimationMixerPlayable.Create(graph, track.clips.Length, false);
			for (int i = 0; i < track.clips.Length; i++)
			{
				TimelineClip timelineClip = track.clips[i];
				PlayableAsset playableAsset = timelineClip.asset as PlayableAsset;
				if (!(playableAsset == null))
				{
					if (timelineClip.recordable)
					{
						AnimationPlayableAsset animationPlayableAsset = playableAsset as AnimationPlayableAsset;
						if (animationPlayableAsset != null)
						{
							animationPlayableAsset.removeStartOffset = !timelineClip.recordable;
						}
					}
					Playable playable = playableAsset.CreatePlayable(graph, go);
					RuntimeClip item = new RuntimeClip(timelineClip, playable, animationMixerPlayable);
					tree.Add(item);
					graph.Connect<Playable, AnimationMixerPlayable>(playable, 0, animationMixerPlayable, i);
					PlayableExtensions.SetInputWeight<AnimationMixerPlayable>(animationMixerPlayable, i, 0f);
				}
			}
			return this.ApplyTrackOffset(graph, animationMixerPlayable);
		}

		internal override Playable OnCreatePlayableGraph(PlayableGraph graph, GameObject go, IntervalTree<RuntimeElement> tree)
		{
			if (base.isSubTrack)
			{
				throw new InvalidOperationException("Nested animation tracks should never be asked to create a graph directly");
			}
			List<AnimationTrack> list = new List<AnimationTrack>();
			if (this.compilableIsolated)
			{
				list.Add(this);
			}
			for (int i = 0; i < base.subTracks.Count; i++)
			{
				AnimationTrack animationTrack = base.subTracks[i] as AnimationTrack;
				if (animationTrack != null && animationTrack.compilable)
				{
					list.Add(animationTrack);
				}
			}
			Playable playable = new Playable(AnimationPlayableGraphExtensions.CreateAnimationMotionXToDeltaPlayable(graph));
			AnimationLayerMixerPlayable animationLayerMixerPlayable = AnimationTrack.CreateGroupMixer(graph, go, list.Count);
			graph.Connect<AnimationLayerMixerPlayable, Playable>(animationLayerMixerPlayable, 0, playable, 0);
			PlayableExtensions.SetInputWeight<Playable>(playable, 0, 1f);
			for (int j = 0; j < list.Count; j++)
			{
				Playable playable2 = (!list[j].inClipMode) ? list[j].CreateInfiniteTrackPlayable(graph, go, tree) : this.CompileTrackPlayable(graph, list[j], go, tree);
				graph.Connect<Playable, AnimationLayerMixerPlayable>(playable2, 0, animationLayerMixerPlayable, j);
				PlayableExtensions.SetInputWeight<AnimationLayerMixerPlayable>(animationLayerMixerPlayable, j, (float)((!list[j].inClipMode) ? 1 : 0));
				if (list[j].applyAvatarMask && list[j].avatarMask != null)
				{
					animationLayerMixerPlayable.SetLayerMaskFromAvatarMask((uint)j, list[j].avatarMask);
				}
			}
			return playable;
		}

		private static AnimationLayerMixerPlayable CreateGroupMixer(PlayableGraph graph, GameObject go, int inputCount)
		{
			return AnimationLayerMixerPlayable.Create(graph, inputCount);
		}

		private Playable CreateInfiniteTrackPlayable(PlayableGraph graph, GameObject go, IntervalTree<RuntimeElement> tree)
		{
			Playable result;
			if (base.animClip == null)
			{
				result = Playable.get_Null();
			}
			else
			{
				if (this.m_FakeAnimClip == null || this.m_AnimationPlayableAsset == null)
				{
					this.m_AnimationPlayableAsset = ScriptableObject.CreateInstance<AnimationPlayableAsset>();
					this.m_FakeAnimClip = new TimelineClip(null)
					{
						asset = this.m_AnimationPlayableAsset,
						displayName = "Animation Clip",
						timeScale = 1.0,
						start = 0.0,
						postExtrapolationMode = TimelineClip.ClipExtrapolation.Hold,
						preExtrapolationMode = TimelineClip.ClipExtrapolation.Hold
					};
					this.m_FakeAnimClip.SetPostExtrapolationTime(TimelineClip.kMaxTimeValue);
				}
				this.m_AnimationPlayableAsset.clip = base.animClip;
				this.m_AnimationPlayableAsset.position = this.m_OpenClipOffsetPosition;
				this.m_AnimationPlayableAsset.rotation = this.m_OpenClipOffsetRotation;
				this.m_AnimationPlayableAsset.removeStartOffset = false;
				this.m_FakeAnimClip.start = 0.0;
				this.m_FakeAnimClip.SetPreExtrapolationTime(0.0);
				this.m_FakeAnimClip.duration = (double)base.animClip.get_length();
				AnimationMixerPlayable animationMixerPlayable = AnimationMixerPlayable.Create(graph, 1, false);
				Playable playable = this.m_AnimationPlayableAsset.CreatePlayable(graph, go);
				tree.Add(new RuntimeClip(this.m_FakeAnimClip, playable, animationMixerPlayable));
				graph.Connect<Playable, AnimationMixerPlayable>(playable, 0, animationMixerPlayable, 0);
				PlayableExtensions.SetInputWeight<AnimationMixerPlayable>(animationMixerPlayable, 0, 1f);
				result = this.ApplyTrackOffset(graph, animationMixerPlayable);
			}
			return result;
		}

		private Playable ApplyTrackOffset(PlayableGraph graph, Playable root)
		{
			this.m_ClipOffset = AnimationOffsetPlayable.get_Null();
			Playable result;
			if (!this.m_ApplyOffsets)
			{
				result = root;
			}
			else
			{
				AnimationOffsetPlayable animationOffsetPlayable = AnimationOffsetPlayable.Create(graph, this.m_Position, this.m_Rotation, 1);
				this.m_ClipOffset = animationOffsetPlayable;
				graph.Connect<Playable, AnimationOffsetPlayable>(root, 0, animationOffsetPlayable, 0);
				PlayableExtensions.SetInputWeight<AnimationOffsetPlayable>(animationOffsetPlayable, 0, 1f);
				result = animationOffsetPlayable;
			}
			return result;
		}

		internal override void GetEvaluationTime(out double outStart, out double outDuration)
		{
			if (this.inClipMode)
			{
				base.GetEvaluationTime(out outStart, out outDuration);
			}
			else
			{
				outStart = 0.0;
				outDuration = TimelineClip.kMaxTimeValue;
			}
		}

		internal override void GetSequenceTime(out double outStart, out double outDuration)
		{
			if (this.inClipMode)
			{
				base.GetSequenceTime(out outStart, out outDuration);
			}
			else
			{
				outStart = 0.0;
				outDuration = 0.0;
				if (base.animClip != null)
				{
					outDuration = (double)base.animClip.get_length();
				}
			}
		}

		private void AssignAnimationClip(TimelineClip clip, AnimationClip animClip)
		{
			if (clip != null && !(animClip == null))
			{
				if (animClip.get_legacy())
				{
					throw new InvalidOperationException("Legacy Animation Clips are not supported");
				}
				if (animClip.get_frameRate() > 0f)
				{
					double num = (double)Mathf.Round(animClip.get_length() * animClip.get_frameRate());
					clip.duration = num / (double)animClip.get_frameRate();
				}
				else
				{
					clip.duration = (double)animClip.get_length();
				}
				TimelineClip.ClipExtrapolation clipExtrapolation = TimelineClip.ClipExtrapolation.None;
				if (!base.isSubTrack)
				{
					clipExtrapolation = TimelineClip.ClipExtrapolation.Hold;
				}
				AnimationPlayableAsset animationPlayableAsset = clip.asset as AnimationPlayableAsset;
				if (animationPlayableAsset != null)
				{
					animationPlayableAsset.clip = animClip;
				}
				clip.underlyingAsset = animClip;
				clip.preExtrapolationMode = clipExtrapolation;
				clip.postExtrapolationMode = clipExtrapolation;
			}
		}

		public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
		{
			base.GatherProperties(director, driver);
			Animator animator = AnimationDefaultTrack.GetAnimator(this, director);
			if (animator != null && animator.get_isHuman())
			{
				driver.AddFromClip(animator.get_gameObject(), AnimationDefaultTrack.defaultHumanoidClip);
			}
		}
	}
}
