using System;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	[TrackBindingType(typeof(GameObject)), TrackClipType(typeof(ActivationPlayableAsset)), TrackMediaType(TimelineAsset.MediaType.Script)]
	[Serializable]
	public class ActivationTrack : TrackAsset
	{
		public enum PostPlaybackState
		{
			Active,
			Inactive,
			Revert,
			LeaveAsIs
		}

		[SerializeField]
		private ActivationTrack.PostPlaybackState m_PostPlaybackState;

		private ActivationMixerPlayable m_ActivationMixer;

		internal override bool compilable
		{
			get
			{
				return this.isEmpty || base.compilable;
			}
		}

		public ActivationTrack.PostPlaybackState postPlaybackState
		{
			get
			{
				return this.m_PostPlaybackState;
			}
			set
			{
				this.m_PostPlaybackState = value;
				this.UpdateTrackMode();
			}
		}

		public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
		{
			ScriptPlayable<ActivationMixerPlayable> scriptPlayable = ActivationMixerPlayable.Create(graph, inputCount);
			this.m_ActivationMixer = scriptPlayable.GetBehaviour();
			PlayableDirector component = go.GetComponent<PlayableDirector>();
			this.UpdateBoundGameObject(component);
			this.UpdateTrackMode();
			return scriptPlayable;
		}

		private void UpdateBoundGameObject(PlayableDirector director)
		{
			if (director != null)
			{
				GameObject gameObject = director.GetGenericBinding(this) as GameObject;
				if (gameObject != null && this.m_ActivationMixer != null)
				{
					this.m_ActivationMixer.boundGameObject = gameObject;
				}
			}
		}

		internal void UpdateTrackMode()
		{
			if (this.m_ActivationMixer != null)
			{
				this.m_ActivationMixer.postPlaybackState = this.m_PostPlaybackState;
			}
		}

		public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
		{
			GameObject gameObjectBinding = base.GetGameObjectBinding(director);
			if (gameObjectBinding != null)
			{
				driver.AddFromName(gameObjectBinding, "m_IsActive");
			}
		}
	}
}
