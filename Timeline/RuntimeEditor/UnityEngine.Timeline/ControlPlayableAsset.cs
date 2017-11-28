using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	[NotKeyable]
	[Serializable]
	public class ControlPlayableAsset : PlayableAsset, IPropertyPreview, ITimelineClipAsset
	{
		private static readonly int k_MaxRandInt = 10000;

		[SerializeField]
		public ExposedReference<GameObject> sourceGameObject;

		[SerializeField]
		public GameObject prefabGameObject;

		[SerializeField]
		public bool updateParticle = true;

		[SerializeField]
		public uint particleRandomSeed;

		[SerializeField]
		public bool updateDirector = true;

		[SerializeField]
		public bool updateITimeControl = true;

		[SerializeField]
		public bool searchHierarchy = true;

		[SerializeField]
		public bool active = true;

		[SerializeField]
		public ActivationControlPlayable.PostPlaybackState postPlayback = ActivationControlPlayable.PostPlaybackState.Revert;

		private PlayableAsset m_ControlDirectorAsset;

		private double m_Duration = PlayableBinding.DefaultDuration;

		private bool m_SupportLoop;

		public override double duration
		{
			get
			{
				return this.m_Duration;
			}
		}

		public ClipCaps clipCaps
		{
			get
			{
				return ClipCaps.ClipIn | ClipCaps.SpeedMultiplier | ((!this.m_SupportLoop) ? ClipCaps.None : ClipCaps.Looping);
			}
		}

		public void OnEnable()
		{
			if (this.particleRandomSeed == 0u)
			{
				this.particleRandomSeed = (uint)Random.Range(1, ControlPlayableAsset.k_MaxRandInt);
			}
		}

		public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
		{
			List<Playable> list = new List<Playable>();
			GameObject gameObject = this.sourceGameObject.Resolve(graph.GetResolver());
			if (this.prefabGameObject != null)
			{
				Transform parentTransform = (!(gameObject != null)) ? null : gameObject.get_transform();
				ScriptPlayable<PrefabControlPlayable> scriptPlayable = PrefabControlPlayable.Create(graph, this.prefabGameObject, parentTransform);
				gameObject = scriptPlayable.GetBehaviour().prefabInstance;
				list.Add(scriptPlayable);
			}
			this.m_Duration = PlayableBinding.DefaultDuration;
			this.m_SupportLoop = false;
			Playable result;
			if (gameObject == null)
			{
				result = Playable.Create(graph, 0);
			}
			else
			{
				IList<PlayableDirector> directors = this.GetDirectors(gameObject);
				IList<ParticleSystem> particleSystems = this.GetParticleSystems(gameObject);
				this.UpdateDurationAndLoopFlag(directors, particleSystems);
				PlayableDirector component = go.GetComponent<PlayableDirector>();
				if (component != null)
				{
					this.m_ControlDirectorAsset = component.get_playableAsset();
				}
				if (go == gameObject && this.prefabGameObject == null)
				{
					Debug.LogWarning("Control Playable (" + base.get_name() + ") is referencing the same PlayableDirector component than the one in which it is playing.");
					this.active = false;
					if (!this.searchHierarchy)
					{
						this.updateDirector = false;
					}
				}
				if (this.active)
				{
					this.CreateActivationPlayable(gameObject, graph, list);
				}
				if (this.updateDirector)
				{
					this.SearchHierarchyAndConnectDirector(directors, graph, list);
				}
				if (this.updateParticle)
				{
					this.SearchHiearchyAndConnectParticleSystem(particleSystems, graph, list);
				}
				if (this.updateITimeControl)
				{
					ControlPlayableAsset.SearchHierarchyAndConnectControlableScripts(ControlPlayableAsset.GetControlableScripts(gameObject), graph, list);
				}
				Playable playable = ControlPlayableAsset.ConnectPlayablesToMixer(graph, list);
				result = playable;
			}
			return result;
		}

		private static Playable ConnectPlayablesToMixer(PlayableGraph graph, List<Playable> playables)
		{
			Playable playable = Playable.Create(graph, playables.Count);
			for (int num = 0; num != playables.Count; num++)
			{
				ControlPlayableAsset.ConnectMixerAndPlayable(graph, playable, playables[num], num);
			}
			PlayableExtensions.SetPropagateSetTime<Playable>(playable, true);
			return playable;
		}

		private void CreateActivationPlayable(GameObject root, PlayableGraph graph, List<Playable> outplayables)
		{
			ScriptPlayable<ActivationControlPlayable> scriptPlayable = ActivationControlPlayable.Create(graph, root, this.postPlayback);
			if (PlayableExtensions.IsValid<ScriptPlayable<ActivationControlPlayable>>(scriptPlayable))
			{
				outplayables.Add(scriptPlayable);
			}
		}

		private void SearchHiearchyAndConnectParticleSystem(IEnumerable<ParticleSystem> particleSystems, PlayableGraph graph, List<Playable> outplayables)
		{
			foreach (ParticleSystem current in particleSystems)
			{
				if (current != null)
				{
					outplayables.Add(ParticleControlPlayable.Create(graph, current, this.particleRandomSeed));
				}
			}
		}

		private void SearchHierarchyAndConnectDirector(IEnumerable<PlayableDirector> directors, PlayableGraph graph, List<Playable> outplayables)
		{
			foreach (PlayableDirector current in directors)
			{
				if (current != null)
				{
					if (current.get_playableAsset() != this.m_ControlDirectorAsset)
					{
						outplayables.Add(DirectorControlPlayable.Create(graph, current));
					}
				}
			}
		}

		private static void SearchHierarchyAndConnectControlableScripts(IEnumerable<MonoBehaviour> controlableScripts, PlayableGraph graph, List<Playable> outplayables)
		{
			foreach (MonoBehaviour current in controlableScripts)
			{
				outplayables.Add(TimeControlPlayable.Create(graph, (ITimeControl)current));
			}
		}

		private static void ConnectMixerAndPlayable(PlayableGraph graph, Playable mixer, Playable playable, int portIndex)
		{
			graph.Connect<Playable, Playable>(playable, 0, mixer, portIndex);
			PlayableExtensions.SetInputWeight<Playable, Playable>(mixer, playable, 1f);
		}

		private IList<ParticleSystem> GetParticleSystems(GameObject gameObject)
		{
			List<ParticleSystem> list = new List<ParticleSystem>();
			if (gameObject != null)
			{
				ParticleSystem component = gameObject.GetComponent<ParticleSystem>();
				if (component != null)
				{
					list.Add(component);
				}
				else if (this.searchHierarchy)
				{
					int childCount = gameObject.get_transform().get_childCount();
					for (int i = 0; i < childCount; i++)
					{
						IList<ParticleSystem> particleSystems = this.GetParticleSystems(gameObject.get_transform().GetChild(i).get_gameObject());
						foreach (ParticleSystem current in particleSystems)
						{
							list.Add(current);
						}
					}
				}
			}
			return list;
		}

		private IList<PlayableDirector> GetDirectors(GameObject gameObject)
		{
			List<PlayableDirector> list = new List<PlayableDirector>();
			if (gameObject != null)
			{
				if (this.searchHierarchy)
				{
					PlayableDirector[] componentsInChildren = gameObject.GetComponentsInChildren<PlayableDirector>(true);
					PlayableDirector[] array = componentsInChildren;
					for (int i = 0; i < array.Length; i++)
					{
						PlayableDirector item = array[i];
						list.Add(item);
					}
				}
				else
				{
					PlayableDirector component = gameObject.GetComponent<PlayableDirector>();
					if (component != null)
					{
						list.Add(component);
					}
				}
			}
			return list;
		}

		[DebuggerHidden]
		private static IEnumerable<MonoBehaviour> GetControlableScripts(GameObject root)
		{
			ControlPlayableAsset.<GetControlableScripts>c__Iterator0 <GetControlableScripts>c__Iterator = new ControlPlayableAsset.<GetControlableScripts>c__Iterator0();
			<GetControlableScripts>c__Iterator.root = root;
			ControlPlayableAsset.<GetControlableScripts>c__Iterator0 expr_0E = <GetControlableScripts>c__Iterator;
			expr_0E.$PC = -2;
			return expr_0E;
		}

		private void UpdateDurationAndLoopFlag(IList<PlayableDirector> directors, IList<ParticleSystem> particleSystems)
		{
			if (directors.Count == 1 && particleSystems.Count == 0)
			{
				PlayableDirector playableDirector = directors[0];
				if (playableDirector.get_playableAsset() != null)
				{
					this.m_Duration = playableDirector.get_playableAsset().get_duration();
					this.m_SupportLoop = (playableDirector.get_extrapolationMode() == 1);
				}
			}
			else if (particleSystems.Count == 1 && directors.Count == 0)
			{
				ParticleSystem particleSystem = particleSystems[0];
				this.m_Duration = (double)particleSystem.get_main().get_duration();
				this.m_SupportLoop = particleSystem.get_main().get_loop();
			}
		}

		public void GatherProperties(PlayableDirector director, IPropertyCollector driver)
		{
			GameObject gameObject = this.sourceGameObject.Resolve(director);
			if (gameObject != null)
			{
				if (this.updateParticle)
				{
					foreach (ParticleSystem current in this.GetParticleSystems(gameObject))
					{
						driver.AddFromName<ParticleSystem>(current.get_gameObject(), "randomSeed");
						driver.AddFromName<ParticleSystem>(current.get_gameObject(), "autoRandomSeed");
					}
				}
				if (this.active)
				{
					driver.AddFromName(gameObject, "m_IsActive");
				}
				if (this.updateITimeControl)
				{
					foreach (MonoBehaviour current2 in ControlPlayableAsset.GetControlableScripts(gameObject))
					{
						IPropertyPreview propertyPreview = current2 as IPropertyPreview;
						if (propertyPreview != null)
						{
							propertyPreview.GatherProperties(director, driver);
						}
						else
						{
							driver.AddFromComponent(current2.get_gameObject(), current2);
						}
					}
				}
			}
		}
	}
}
