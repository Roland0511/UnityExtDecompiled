using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[CanEditMultipleObjects, CustomEditor(typeof(ControlPlayableAsset))]
	internal class ControlPlayableInspector : Editor
	{
		private static class Styles
		{
			private static string s_DisabledBecauseOfSelfControlTooltip = "Must be disabled when the Source Game Object references the same PlayableDirector component that is being controlled";

			public static readonly GUIContent activationContent = EditorGUIUtility.TextContent("Control Activation|When checked the clip will control the active state of the source game object");

			public static readonly GUIContent activationDisabledContent = EditorGUIUtility.TextContent("Control Activation|" + ControlPlayableInspector.Styles.s_DisabledBecauseOfSelfControlTooltip);

			public static readonly GUIContent prefabContent = EditorGUIUtility.TextContent("Prefab|A prefab to instantiate as a child object of the source game object");

			public static readonly GUIContent advancedContent = EditorGUIUtility.TextContent("Advanced");

			public static readonly GUIContent updateParticleSystemsContent = EditorGUIUtility.TextContent("Control Particle Systems|Synchronize the time between the clip and any particle systems on the game object");

			public static readonly GUIContent updatePlayableDirectorContent = EditorGUIUtility.TextContent("Control Playable Directors|Synchronize the time between the clip and any playable directors on the game object");

			public static readonly GUIContent updatePlayableDirectorDisabledContent = EditorGUIUtility.TextContent("Control Playable Directors|" + ControlPlayableInspector.Styles.s_DisabledBecauseOfSelfControlTooltip);

			public static readonly GUIContent updateITimeControlContent = EditorGUIUtility.TextContent("Control ITimeControl|Synchronize the time between the clip and any Script that implements the ITimeControl interface on the game object");

			public static readonly GUIContent updateHierarchy = EditorGUIUtility.TextContent("Control Children|Search child game objects for particle systems and playable directors");

			public static readonly GUIContent randomSeedContent = EditorGUIUtility.TextContent("Random Seed|A random seem to provide the particle systems for consistent previews");

			public static readonly GUIContent postPlayableContent = EditorGUIUtility.TextContent("Post Playback|The active state to the leave the game object when the timeline is finished. \n\nRevert will leave the game object in the state it was prior to the timeline being run");
		}

		private SerializedProperty m_SourceObject;

		private SerializedProperty m_PrefabObject;

		private SerializedProperty m_UpdateParticle;

		private SerializedProperty m_UpdateDirector;

		private SerializedProperty m_UpdateITimeControl;

		private SerializedProperty m_SearchHierarchy;

		private SerializedProperty m_UseActivation;

		private SerializedProperty m_PostPlayback;

		private SerializedProperty m_RandomSeed;

		private GUIContent m_SourceObjectLabel = new GUIContent();

		public void OnEnable()
		{
			this.m_SourceObject = base.get_serializedObject().FindProperty("sourceGameObject");
			this.m_PrefabObject = base.get_serializedObject().FindProperty("prefabGameObject");
			this.m_UpdateParticle = base.get_serializedObject().FindProperty("updateParticle");
			this.m_UpdateDirector = base.get_serializedObject().FindProperty("updateDirector");
			this.m_UpdateITimeControl = base.get_serializedObject().FindProperty("updateITimeControl");
			this.m_SearchHierarchy = base.get_serializedObject().FindProperty("searchHierarchy");
			this.m_UseActivation = base.get_serializedObject().FindProperty("active");
			this.m_PostPlayback = base.get_serializedObject().FindProperty("postPlayback");
			this.m_RandomSeed = base.get_serializedObject().FindProperty("particleRandomSeed");
		}

		public override void OnInspectorGUI()
		{
			base.get_serializedObject().Update();
			this.m_SourceObjectLabel.set_text(this.m_SourceObject.get_displayName());
			if (this.m_PrefabObject.get_objectReferenceValue() != null)
			{
				this.m_SourceObjectLabel.set_text("Parent Object");
			}
			bool flag = false;
			EditorGUI.BeginChangeCheck();
			EditorGUI.BeginChangeCheck();
			if (this.m_SourceObject.get_hasMultipleDifferentValues())
			{
				EditorGUI.set_showMixedValue(true);
			}
			EditorGUILayout.PropertyField(this.m_SourceObject, this.m_SourceObjectLabel, new GUILayoutOption[0]);
			EditorGUI.set_showMixedValue(false);
			GameObject gameObject = this.m_SourceObject.get_exposedReferenceValue() as GameObject;
			flag = (this.m_PrefabObject.get_objectReferenceValue() == null && TimelineWindow.instance != null && TimelineWindow.instance.state != null && TimelineWindow.instance.state.currentDirector != null && gameObject == TimelineWindow.instance.state.currentDirector.get_gameObject());
			if (EditorGUI.EndChangeCheck())
			{
				if (!flag)
				{
					this.DisablePlayOnAwake(gameObject);
				}
			}
			if (flag)
			{
				EditorGUILayout.HelpBox("The Source Game Object references the same PlayableDirector component being controlled.", 2);
			}
			EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
			EditorGUILayout.PropertyField(this.m_PrefabObject, ControlPlayableInspector.Styles.prefabContent, new GUILayoutOption[0]);
			EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
			using (new EditorGUI.DisabledScope(flag))
			{
				EditorGUILayout.PropertyField(this.m_UseActivation, (!flag) ? ControlPlayableInspector.Styles.activationContent : ControlPlayableInspector.Styles.activationDisabledContent, new GUILayoutOption[0]);
				if (this.m_UseActivation.get_boolValue())
				{
					EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
					EditorGUILayout.PropertyField(this.m_PostPlayback, ControlPlayableInspector.Styles.postPlayableContent, new GUILayoutOption[0]);
					EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
				}
			}
			this.m_SourceObject.set_isExpanded(EditorGUILayout.Foldout(this.m_SourceObject.get_isExpanded(), ControlPlayableInspector.Styles.advancedContent));
			if (this.m_SourceObject.get_isExpanded())
			{
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
				using (new EditorGUI.DisabledScope(flag && !this.m_SearchHierarchy.get_boolValue()))
				{
					EditorGUILayout.PropertyField(this.m_UpdateDirector, (!flag) ? ControlPlayableInspector.Styles.updatePlayableDirectorContent : ControlPlayableInspector.Styles.updatePlayableDirectorDisabledContent, new GUILayoutOption[0]);
				}
				EditorGUILayout.PropertyField(this.m_UpdateParticle, ControlPlayableInspector.Styles.updateParticleSystemsContent, new GUILayoutOption[0]);
				if (this.m_UpdateParticle.get_boolValue())
				{
					EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
					EditorGUILayout.PropertyField(this.m_RandomSeed, ControlPlayableInspector.Styles.randomSeedContent, new GUILayoutOption[0]);
					EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
				}
				EditorGUILayout.PropertyField(this.m_UpdateITimeControl, ControlPlayableInspector.Styles.updateITimeControlContent, new GUILayoutOption[0]);
				EditorGUILayout.PropertyField(this.m_SearchHierarchy, ControlPlayableInspector.Styles.updateHierarchy, new GUILayoutOption[0]);
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
			}
			if (EditorGUI.EndChangeCheck())
			{
				base.get_serializedObject().ApplyModifiedProperties();
			}
		}

		public void DisablePlayOnAwake(GameObject sourceObject)
		{
			if (sourceObject != null && this.m_UpdateDirector.get_boolValue())
			{
				if (this.m_SearchHierarchy.get_boolValue())
				{
					PlayableDirector[] componentsInChildren = sourceObject.GetComponentsInChildren<PlayableDirector>();
					PlayableDirector[] array = componentsInChildren;
					for (int i = 0; i < array.Length; i++)
					{
						PlayableDirector director = array[i];
						this.DisablePlayOnAwake(director);
					}
				}
				else
				{
					this.DisablePlayOnAwake(sourceObject.GetComponent<PlayableDirector>());
				}
			}
		}

		public void DisablePlayOnAwake(PlayableDirector director)
		{
			if (!(director == null))
			{
				SerializedObject serializedObject = new SerializedObject(director);
				SerializedProperty serializedProperty = serializedObject.FindProperty("m_InitialState");
				serializedProperty.set_enumValueIndex(0);
				serializedObject.ApplyModifiedProperties();
			}
		}
	}
}
