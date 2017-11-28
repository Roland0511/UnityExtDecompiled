using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class PropertyCollector : IPropertyCollector
	{
		private readonly Stack<GameObject> m_ObjectStack = new Stack<GameObject>();

		public void Reset()
		{
			this.m_ObjectStack.Clear();
		}

		public void PushActiveGameObject(GameObject gameObject)
		{
			this.m_ObjectStack.Push(gameObject);
		}

		public void PopActiveGameObject()
		{
			this.m_ObjectStack.Pop();
		}

		public void AddFromClip(AnimationClip clip)
		{
			GameObject gameObject = this.m_ObjectStack.Peek();
			if (gameObject != null && clip != null)
			{
				this.AddFromClip(gameObject, clip);
			}
		}

		public void AddFromName<T>(string name) where T : Component
		{
			GameObject gameObject = this.m_ObjectStack.Peek();
			if (gameObject != null)
			{
				this.AddFromName<T>(gameObject, name);
			}
		}

		public void AddFromName(string name)
		{
			GameObject gameObject = this.m_ObjectStack.Peek();
			if (gameObject != null)
			{
				this.AddFromName(gameObject, name);
			}
		}

		public void AddFromClip(GameObject obj, AnimationClip clip)
		{
			if (!Application.get_isPlaying())
			{
				PropertyCollector.AddPropertiesFromClip(obj, clip);
			}
		}

		public void AddFromName<T>(GameObject obj, string name) where T : Component
		{
			if (!Application.get_isPlaying())
			{
				PropertyCollector.AddPropertiesFromName(obj, typeof(T), name);
			}
		}

		public void AddFromName(GameObject obj, string name)
		{
			if (!Application.get_isPlaying())
			{
				PropertyCollector.AddPropertiesFromName(obj, name);
			}
		}

		public void AddFromComponent(GameObject obj, Component component)
		{
			if (!Application.get_isPlaying())
			{
				if (!(obj == null) && !(component == null))
				{
					SerializedObject serializedObject = new SerializedObject(component);
					SerializedProperty iterator = serializedObject.GetIterator();
					while (iterator.NextVisible(true))
					{
						if (!iterator.get_hasVisibleChildren() && AnimatedParameterExtensions.IsAnimatable(iterator.get_propertyType()))
						{
							PropertyCollector.AddPropertyModification(component, iterator.get_propertyPath());
						}
					}
				}
			}
		}

		private static void AddPropertiesFromClip(GameObject go, AnimationClip clip)
		{
			if (go != null && clip != null)
			{
				AnimationMode.InitializePropertyModificationForGameObject(go, clip);
				if (clip.get_hasRootMotion())
				{
					PropertyCollector.AddPropertyModification(go.get_transform(), "m_LocalPosition.x");
					PropertyCollector.AddPropertyModification(go.get_transform(), "m_LocalPosition.y");
					PropertyCollector.AddPropertyModification(go.get_transform(), "m_LocalPosition.z");
					PropertyCollector.AddPropertyModification(go.get_transform(), "m_LocalRotation.x");
					PropertyCollector.AddPropertyModification(go.get_transform(), "m_LocalRotation.y");
					PropertyCollector.AddPropertyModification(go.get_transform(), "m_LocalRotation.w");
					PropertyCollector.AddPropertyModification(go.get_transform(), "m_LocalRotation.z");
				}
			}
		}

		private static void AddPropertiesFromName(GameObject go, string property)
		{
			if (!(go == null))
			{
				PropertyCollector.AddPropertyModification(go, property);
			}
		}

		private static void AddPropertiesFromName(GameObject go, Type compType, string property)
		{
			if (!(go == null))
			{
				Component component = go.GetComponent(compType);
				if (!(component == null))
				{
					PropertyCollector.AddPropertyModification(component, property);
				}
			}
		}

		public void AddObjectProperties(Object obj, AnimationClip clip)
		{
			if (!(obj == null) && !(clip == null))
			{
				IPlayableAsset playableAsset = obj as IPlayableAsset;
				IPlayableBehaviour playableBehaviour = obj as IPlayableBehaviour;
				if (playableAsset != null)
				{
					if (playableBehaviour == null)
					{
						this.AddSerializedPlayableModifications(playableAsset, clip);
					}
					else
					{
						AnimationMode.InitializePropertyModificationForObject(obj, clip);
					}
				}
			}
		}

		private void AddSerializedPlayableModifications(IPlayableAsset asset, AnimationClip clip)
		{
			Object @object = asset as Object;
			if (!(@object == null))
			{
				AnimationModeDriver previewDriver = TimelineWindow.TimelineState.previewDriver;
				if (!(previewDriver == null) && AnimationMode.InAnimationMode(previewDriver))
				{
					EditorCurveBinding[] bindings = AnimationClipCurveCache.Instance.GetCurveInfo(clip).bindings;
					List<FieldInfo> list = AnimatedParameterExtensions.GetScriptPlayableFields(asset).ToList<FieldInfo>();
					EditorCurveBinding[] array = bindings;
					for (int i = 0; i < array.Length; i++)
					{
						EditorCurveBinding editorCurveBinding = array[i];
						foreach (FieldInfo current in list)
						{
							DrivenPropertyManager.RegisterProperty(previewDriver, @object, current.Name + "." + editorCurveBinding.propertyName);
						}
					}
				}
			}
		}

		private static void AddPropertyModification(GameObject obj, string propertyName)
		{
			AnimationModeDriver previewDriver = TimelineWindow.TimelineState.previewDriver;
			if (!(previewDriver == null) && AnimationMode.InAnimationMode(previewDriver))
			{
				DrivenPropertyManager.RegisterProperty(previewDriver, obj, propertyName);
			}
		}

		private static void AddPropertyModification(Component comp, string name)
		{
			if (!(comp == null))
			{
				AnimationModeDriver previewDriver = TimelineWindow.TimelineState.previewDriver;
				if (!(previewDriver == null) && AnimationMode.InAnimationMode(previewDriver))
				{
					DrivenPropertyManager.RegisterProperty(previewDriver, comp, name);
				}
			}
		}
	}
}
