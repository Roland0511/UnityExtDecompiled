using System;
using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	[NotKeyable]
	[Serializable]
	public class AnimationPlayableAsset : PlayableAsset, ITimelineClipAsset, IPropertyPreview
	{
		[SerializeField]
		private AnimationClip m_Clip;

		[SerializeField]
		private Vector3 m_Position = Vector3.get_zero();

		[SerializeField]
		private Quaternion m_Rotation = Quaternion.get_identity();

		[SerializeField]
		private bool m_UseTrackMatchFields = false;

		[SerializeField]
		private MatchTargetFields m_MatchTargetFields = MatchTargetFieldConstants.All;

		[SerializeField]
		private bool m_RemoveStartOffset = true;

		private AnimationClipPlayable m_AnimationClipPlayable;

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

		public bool useTrackMatchFields
		{
			get
			{
				return this.m_UseTrackMatchFields;
			}
			set
			{
				this.m_UseTrackMatchFields = value;
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
				this.m_MatchTargetFields = value;
			}
		}

		internal bool removeStartOffset
		{
			get
			{
				return this.m_RemoveStartOffset;
			}
			set
			{
				this.m_RemoveStartOffset = value;
			}
		}

		public AnimationClip clip
		{
			get
			{
				return this.m_Clip;
			}
			set
			{
				if (value != null)
				{
					base.set_name("AnimationPlayableAsset of " + value.get_name());
				}
				this.m_Clip = value;
			}
		}

		public override double duration
		{
			get
			{
				double result;
				if (this.clip == null)
				{
					result = 1.7976931348623157E+308;
				}
				else
				{
					double num = (double)this.clip.get_length();
					if (this.clip.get_frameRate() > 0f)
					{
						double num2 = (double)Mathf.Round(this.clip.get_length() * this.clip.get_frameRate());
						num = num2 / (double)this.clip.get_frameRate();
					}
					result = num;
				}
				return result;
			}
		}

		public override IEnumerable<PlayableBinding> outputs
		{
			get
			{
				AnimationPlayableAsset.<>c__Iterator0 <>c__Iterator = new AnimationPlayableAsset.<>c__Iterator0();
				AnimationPlayableAsset.<>c__Iterator0 expr_07 = <>c__Iterator;
				expr_07.$PC = -2;
				return expr_07;
			}
		}

		private bool applyRootMotion
		{
			get
			{
				return this.m_Position != Vector3.get_zero() || this.m_Rotation != Quaternion.get_identity() || (this.m_Clip != null && this.m_Clip.get_hasRootMotion());
			}
		}

		public ClipCaps clipCaps
		{
			get
			{
				ClipCaps clipCaps = ClipCaps.All;
				if (this.m_Clip == null || !this.m_Clip.get_isLooping())
				{
					clipCaps &= ~ClipCaps.Looping;
				}
				return clipCaps;
			}
		}

		public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
		{
			AnimationClipPlayable animationClipPlayable = AnimationClipPlayable.Create(graph, this.m_Clip);
			this.m_AnimationClipPlayable = animationClipPlayable;
			this.m_AnimationClipPlayable.SetRemoveStartOffset(this.removeStartOffset);
			Playable result = animationClipPlayable;
			if (this.applyRootMotion)
			{
				AnimationOffsetPlayable animationOffsetPlayable = AnimationOffsetPlayable.Create(graph, this.m_Position, this.m_Rotation, 1);
				graph.Connect<AnimationClipPlayable, AnimationOffsetPlayable>(animationClipPlayable, 0, animationOffsetPlayable, 0);
				result = animationOffsetPlayable;
			}
			this.LiveLink();
			return result;
		}

		public void LiveLink()
		{
		}

		public void ResetOffsets()
		{
			this.position = Vector3.get_zero();
			this.rotation = Quaternion.get_identity();
		}

		public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
		{
			driver.AddFromClip(this.m_Clip);
		}
	}
}
