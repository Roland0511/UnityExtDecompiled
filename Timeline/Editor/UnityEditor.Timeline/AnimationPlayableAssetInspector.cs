using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[CanEditMultipleObjects, CustomEditor(typeof(AnimationPlayableAsset))]
	internal class AnimationPlayableAssetInspector : Editor
	{
		private static class Styles
		{
			public static readonly GUIContent RotationText = EditorGUIUtility.TextContent("Rotation");

			public static readonly GUIContent AnimClipText = EditorGUIUtility.TextContent("Animation Clip");

			public static readonly GUIContent PositionIcon = EditorGUIUtility.IconContent("MoveTool");

			public static readonly GUIContent ClipOffsetTitle = EditorGUIUtility.TextContent("Clip Root Motion Offsets");

			public static readonly GUIContent MotionCurveWarning = EditorGUIUtility.TextContent("The animation clip does not have any motion curves, and may not playback as expected. Assign a Root Motion Node, or Generate Root Motion curves");

			public static readonly GUIContent LegacyError = EditorGUIUtility.TextContent("Legacy animation clips are not supported");

			public static readonly GUIContent AnimationClipName = EditorGUIUtility.TextContent("Animation Clip Name");
		}

		private TimelineWindow m_TimelineWindow;

		private GameObject m_Binding;

		private TimelineAnimationUtilities.OffsetEditMode m_OffsetEditMode = TimelineAnimationUtilities.OffsetEditMode.None;

		private EditorClip m_EditorClip;

		private SerializedProperty m_PositionProperty;

		private SerializedProperty m_RotationProperty;

		private SerializedProperty m_AnimClipProperty;

		private SerializedProperty m_UseTrackMatchFieldsProperty;

		private SerializedProperty m_MatchTargetFieldsProperty;

		private SerializedProperty m_TrackMatchTargetFieldsProperty;

		private SerializedObject m_TrackSerializedObject;

		private SerializedObject m_SerializedAnimClip;

		private SerializedProperty m_SerializedAnimClipName;

		private Vector3 m_LastPosition;

		private Quaternion m_LastRotation;

		public override void OnInspectorGUI()
		{
			base.get_serializedObject().Update();
			if (!this.m_TimelineWindow)
			{
				this.m_TimelineWindow = TimelineWindow.instance;
			}
			using (new EditorGUI.DisabledScope(true))
			{
				EditorGUILayout.PropertyField(this.m_AnimClipProperty, AnimationPlayableAssetInspector.Styles.AnimClipText, new GUILayoutOption[0]);
			}
			this.ShowRecordableClipRename();
			this.ShowAnimationClipWarnings();
			EditorGUI.BeginChangeCheck();
			if (base.get_targets().Length == 1)
			{
				using (new EditorGUI.DisabledScope(!this.ShouldShowOffsets()))
				{
					EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
					this.ClipRootMotionOffsetsGUI();
					this.ClipOffsetsMatchFieldsGUI();
					EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
				}
			}
			bool flag = EditorGUI.EndChangeCheck() || this.m_LastPosition != this.m_PositionProperty.get_vector3Value() || this.m_LastRotation != this.m_RotationProperty.get_quaternionValue();
			this.m_LastPosition = this.m_PositionProperty.get_vector3Value();
			this.m_LastRotation = this.m_RotationProperty.get_quaternionValue();
			if (flag)
			{
				base.get_serializedObject().ApplyModifiedProperties();
				((AnimationPlayableAsset)base.get_target()).LiveLink();
				if (TimelineWindow.instance != null && TimelineWindow.instance.state != null)
				{
					TimelineWindow.instance.state.Evaluate();
				}
			}
			base.get_serializedObject().ApplyModifiedProperties();
		}

		private void ClipOffsetsMatchFieldsGUI()
		{
			EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
			AnimationTrackInspector.MatchTargetsField(this.m_MatchTargetFieldsProperty, this.m_TrackMatchTargetFieldsProperty, this.m_UseTrackMatchFieldsProperty, true);
			EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
		}

		private void ClipRootMotionOffsetsGUI()
		{
			this.m_PositionProperty.set_isExpanded(EditorGUILayout.Foldout(this.m_PositionProperty.get_isExpanded(), AnimationPlayableAssetInspector.Styles.ClipOffsetTitle));
			if (this.m_PositionProperty.get_isExpanded())
			{
				float num = 0f;
				float num2 = 0f;
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
				TimelineAnimationUtilities.OffsetEditMode offsetEditMode = this.m_OffsetEditMode;
				AnimationTrackInspector.ShowMotionOffsetEditModeToolbar(ref this.m_OffsetEditMode);
				if (offsetEditMode != this.m_OffsetEditMode)
				{
					this.SetTimeToClip();
					SceneView.RepaintAll();
				}
				EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
				EditorGUILayout.PropertyField(this.m_PositionProperty, new GUILayoutOption[0]);
				GUI.get_skin().get_button().CalcMinMaxWidth(AnimationPlayableAssetInspector.Styles.PositionIcon, ref num, ref num2);
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
				AnimationPlayableAssetInspector.ShowRotationField(this.m_RotationProperty);
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.Space();
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
			}
		}

		private void Reevaluate()
		{
			if (this.m_TimelineWindow != null && this.m_TimelineWindow.state != null)
			{
				this.m_TimelineWindow.state.EvaluateImmediate();
			}
		}

		private void SetTimeToClip()
		{
			if (this.m_TimelineWindow != null && this.m_TimelineWindow.state != null)
			{
				this.m_TimelineWindow.state.time = Math.Min(this.m_EditorClip.clip.end, Math.Max(this.m_EditorClip.clip.start, this.m_TimelineWindow.state.time));
			}
		}

		public void OnEnable()
		{
			this.m_EditorClip = (Selection.get_activeObject() as EditorClip);
			SceneView.onSceneGUIDelegate = (SceneView.OnSceneFunc)Delegate.Combine(SceneView.onSceneGUIDelegate, new SceneView.OnSceneFunc(this.OnSceneGUI));
			this.m_PositionProperty = base.get_serializedObject().FindProperty("m_Position");
			this.m_PositionProperty.set_isExpanded(true);
			this.m_RotationProperty = base.get_serializedObject().FindProperty("m_Rotation");
			this.m_AnimClipProperty = base.get_serializedObject().FindProperty("m_Clip");
			this.m_UseTrackMatchFieldsProperty = base.get_serializedObject().FindProperty("m_UseTrackMatchFields");
			this.m_UseTrackMatchFieldsProperty.set_isExpanded(true);
			this.m_MatchTargetFieldsProperty = base.get_serializedObject().FindProperty("m_MatchTargetFields");
			this.m_MatchTargetFieldsProperty.set_isExpanded(true);
			this.m_LastPosition = this.m_PositionProperty.get_vector3Value();
			this.m_LastRotation = this.m_RotationProperty.get_quaternionValue();
			if (this.m_EditorClip != null && this.m_EditorClip.clip != null)
			{
				this.m_TrackSerializedObject = new SerializedObject(this.m_EditorClip.clip.parentTrack);
				this.m_TrackMatchTargetFieldsProperty = this.m_TrackSerializedObject.FindProperty("m_MatchTargetFields");
			}
		}

		private void OnDestroy()
		{
			SceneView.onSceneGUIDelegate = (SceneView.OnSceneFunc)Delegate.Remove(SceneView.onSceneGUIDelegate, new SceneView.OnSceneFunc(this.OnSceneGUI));
		}

		private void OnSceneGUI(SceneView sceneView)
		{
			this.DoManipulators();
		}

		private Transform GetTransform()
		{
			Transform result;
			if (this.m_Binding != null)
			{
				result = this.m_Binding.get_transform();
			}
			else
			{
				if (this.m_TimelineWindow != null && this.m_TimelineWindow.state != null && this.m_TimelineWindow.state.currentDirector != null && this.m_EditorClip != null && this.m_EditorClip.clip != null)
				{
					GameObject sceneGameObject = TimelineUtility.GetSceneGameObject(this.m_TimelineWindow.state.currentDirector, this.m_EditorClip.clip.parentTrack);
					this.m_Binding = sceneGameObject;
					if (sceneGameObject != null)
					{
						result = sceneGameObject.get_transform();
						return result;
					}
				}
				result = null;
			}
			return result;
		}

		private void DoManipulators()
		{
			if (!(this.m_EditorClip == null) && this.m_EditorClip.clip != null)
			{
				AnimationPlayableAsset animationPlayableAsset = this.m_EditorClip.clip.asset as AnimationPlayableAsset;
				AnimationTrack animationTrack = this.m_EditorClip.clip.parentTrack as AnimationTrack;
				Transform transform = this.GetTransform();
				if (transform != null && animationPlayableAsset != null && this.m_OffsetEditMode != TimelineAnimationUtilities.OffsetEditMode.None && animationTrack != null)
				{
					Vector3 vector = transform.get_position();
					Quaternion quaternion = transform.get_rotation();
					EditorGUI.BeginChangeCheck();
					if (this.m_OffsetEditMode == TimelineAnimationUtilities.OffsetEditMode.Translation)
					{
						vector = Handles.PositionHandle(vector, (Tools.get_pivotRotation() != 1) ? quaternion : Quaternion.get_identity());
					}
					else if (this.m_OffsetEditMode == TimelineAnimationUtilities.OffsetEditMode.Rotation)
					{
						quaternion = Handles.RotationHandle(quaternion, vector);
					}
					if (EditorGUI.EndChangeCheck())
					{
						TimelineAnimationUtilities.RigidTransform rigidTransform = TimelineAnimationUtilities.UpdateClipOffsets(animationPlayableAsset, animationTrack, transform, vector, quaternion);
						animationPlayableAsset.position = rigidTransform.position;
						animationPlayableAsset.rotation = rigidTransform.rotation;
						this.Reevaluate();
						base.Repaint();
					}
				}
			}
		}

		public static void ShowRotationField(SerializedProperty rotation)
		{
			Rect controlRect = EditorGUILayout.GetControlRect(new GUILayoutOption[0]);
			GUIContent gUIContent = EditorGUI.BeginProperty(controlRect, AnimationPlayableAssetInspector.Styles.RotationText, rotation);
			EditorGUI.BeginChangeCheck();
			Vector3 vector = EditorGUI.Vector3Field(controlRect, gUIContent, rotation.get_quaternionValue().get_eulerAngles());
			if (EditorGUI.EndChangeCheck())
			{
				rotation.set_quaternionValue(Quaternion.Euler(vector));
			}
			EditorGUI.EndProperty();
		}

		private void ShowAnimationClipWarnings()
		{
			AnimationClip animationClip = this.m_AnimClipProperty.get_objectReferenceValue() as AnimationClip;
			if (!(animationClip == null))
			{
				if (animationClip.get_legacy())
				{
					EditorGUILayout.HelpBox(AnimationPlayableAssetInspector.Styles.LegacyError.get_text(), 3);
				}
				else
				{
					bool flag = AnimationUtility.HasGenericRootTransform(animationClip);
					bool flag2 = AnimationUtility.HasMotionCurves(animationClip);
					bool flag3 = AnimationUtility.HasRootCurves(animationClip);
					if (flag && !flag2 && !flag3)
					{
						EditorGUILayout.HelpBox(AnimationPlayableAssetInspector.Styles.MotionCurveWarning.get_text(), 2);
					}
				}
			}
		}

		private bool ShouldShowOffsets()
		{
			AnimationClip animationClip = this.m_AnimClipProperty.get_objectReferenceValue() as AnimationClip;
			return !(animationClip == null) && (AnimationUtility.HasGenericRootTransform(animationClip) || AnimationUtility.HasMotionCurves(animationClip) || AnimationUtility.HasRootCurves(animationClip));
		}

		private void ShowRecordableClipRename()
		{
			if (base.get_targets().Length <= 1 && !(this.m_EditorClip == null) && this.m_EditorClip.clip != null && this.m_EditorClip.clip.recordable)
			{
				AnimationClip animationClip = this.m_AnimClipProperty.get_objectReferenceValue() as AnimationClip;
				if (!(animationClip == null) && AssetDatabase.IsSubAsset(animationClip))
				{
					if (this.m_SerializedAnimClip == null)
					{
						this.m_SerializedAnimClip = new SerializedObject(animationClip);
						this.m_SerializedAnimClipName = this.m_SerializedAnimClip.FindProperty("m_Name");
					}
					if (this.m_SerializedAnimClipName != null)
					{
						this.m_SerializedAnimClip.Update();
						EditorGUI.BeginChangeCheck();
						EditorGUILayout.DelayedTextField(this.m_SerializedAnimClipName, AnimationPlayableAssetInspector.Styles.AnimationClipName, new GUILayoutOption[0]);
						if (EditorGUI.EndChangeCheck())
						{
							this.m_SerializedAnimClip.ApplyModifiedProperties();
						}
					}
				}
			}
		}
	}
}
