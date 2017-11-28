using System;
using System.Collections;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	public class PrefabControlPlayable : PlayableBehaviour
	{
		private GameObject m_Instance;

		public GameObject prefabInstance
		{
			get
			{
				return this.m_Instance;
			}
		}

		public static ScriptPlayable<PrefabControlPlayable> Create(PlayableGraph graph, GameObject prefabGameObject, Transform parentTransform)
		{
			ScriptPlayable<PrefabControlPlayable> result;
			if (prefabGameObject == null)
			{
				result = ScriptPlayable<PrefabControlPlayable>.get_Null();
			}
			else
			{
				ScriptPlayable<PrefabControlPlayable> scriptPlayable = ScriptPlayable<PrefabControlPlayable>.Create(graph, 0);
				scriptPlayable.GetBehaviour().Initialize(prefabGameObject, parentTransform);
				result = scriptPlayable;
			}
			return result;
		}

		public GameObject Initialize(GameObject prefabGameObject, Transform parentTransform)
		{
			if (prefabGameObject == null)
			{
				throw new ArgumentNullException("Prefab cannot be null");
			}
			if (this.m_Instance != null)
			{
				Debug.LogWarningFormat("Prefab Control Playable ({0}) has already been initialized with a Prefab ({1}).", new object[]
				{
					prefabGameObject.get_name(),
					this.m_Instance.get_name()
				});
			}
			else
			{
				this.m_Instance = Object.Instantiate<GameObject>(prefabGameObject, parentTransform, false);
				this.m_Instance.set_name(prefabGameObject.get_name() + " [Timeline]");
				this.m_Instance.SetActive(false);
				PrefabControlPlayable.SetHideFlagsRecursive(this.m_Instance);
			}
			return this.m_Instance;
		}

		public override void OnPlayableDestroy(Playable playable)
		{
			if (this.m_Instance)
			{
				if (Application.get_isPlaying())
				{
					Object.Destroy(this.m_Instance);
				}
				else
				{
					Object.DestroyImmediate(this.m_Instance);
				}
			}
		}

		public override void OnBehaviourPlay(Playable playable, FrameData info)
		{
			if (!(this.m_Instance == null))
			{
				this.m_Instance.SetActive(true);
			}
		}

		public override void OnBehaviourPause(Playable playable, FrameData info)
		{
			if (!(this.m_Instance == null))
			{
				this.m_Instance.SetActive(false);
			}
		}

		private static void SetHideFlagsRecursive(GameObject gameObject)
		{
			gameObject.set_hideFlags(20);
			if (!Application.get_isPlaying())
			{
				gameObject.set_hideFlags(gameObject.get_hideFlags() | 1);
			}
			IEnumerator enumerator = gameObject.get_transform().GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Transform transform = (Transform)enumerator.Current;
					PrefabControlPlayable.SetHideFlagsRecursive(transform.get_gameObject());
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = (enumerator as IDisposable)) != null)
				{
					disposable.Dispose();
				}
			}
		}
	}
}
