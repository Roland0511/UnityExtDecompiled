using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[CanEditMultipleObjects, CustomEditor(typeof(AnimationTrack))]
	internal class AnimationTrackInspector : TrackAssetInspector
	{
		internal static class Styles
		{
			public static GUIContent MatchTargetFieldsTitle = EditorGUIUtility.TextContent("Clip Offset Match Fields|Specify which transform fields to match");

			public static readonly GUIContent PositionIcon = EditorGUIUtility.IconContent("MoveTool");

			public static readonly GUIContent RotationIcon = EditorGUIUtility.IconContent("RotateTool");

			public static GUIContent XTitle = EditorGUIUtility.TextContent("X");

			public static GUIContent YTitle = EditorGUIUtility.TextContent("Y");

			public static GUIContent ZTitle = EditorGUIUtility.TextContent("Z");

			public static GUIContent PositionTitle = EditorGUIUtility.TextContent("Position");

			public static GUIContent RotationTitle = EditorGUIUtility.TextContent("Rotation");

			public static readonly GUIContent TrackOffsetTitle = EditorGUIUtility.TextContent("Apply Track Offsets|Root Motion Offsets values will be applied globally to all animation clips on the track that support root motion.\nThe offsets applied are in addition to any offsets on the clips.");

			public static readonly GUIContent DisableOptionsTitle = EditorGUIUtility.TextContent("Override Track Matching Fields");

			public static readonly GUIContent Blank = new GUIContent(" ");

			public static readonly GUIContent MatchTargetsFieldHelp = EditorGUIUtility.TextContent("Use \"{0}\" to set different Matching Fields for a \u0003single clip on a track. Apply Matching through the clip's right-click contextual menu.");

			public static readonly GUIContent TrackOffsetCannotBeEditedWarning = EditorGUIUtility.TextContent("Track offsets cannot be multi-edited.");
		}

		private TimelineAnimationUtilities.OffsetEditMode m_OffsetEditMode = TimelineAnimationUtilities.OffsetEditMode.None;

		private SerializedProperty m_MatchFieldsProperty;

		private SerializedProperty m_TrackPositionProperty;

		private SerializedProperty m_TrackRotationProperty;

		private SerializedProperty m_ApplyOffsetsProperty;

		private SerializedProperty m_AvatarMaskProperty;

		private SerializedProperty m_ApplyAvatarMaskProperty;

		private Animator m_PreviousAnimator;

		private Vector3 m_lastPosition;

		private Quaternion m_lastRotation;

		private void Evaluate()
		{
			if (base.timelineWindow.state != null && base.timelineWindow.state.currentDirector != null)
			{
				base.timelineWindow.state.currentDirector.Evaluate();
			}
		}

		private void RebuildGraph()
		{
			if (base.timelineWindow.state != null)
			{
				base.timelineWindow.state.rebuildGraph = true;
				base.timelineWindow.Repaint();
			}
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			base.get_serializedObject().Update();
			bool flag = this.m_lastPosition != this.m_TrackPositionProperty.get_vector3Value() || this.m_lastRotation != this.m_TrackRotationProperty.get_quaternionValue();
			this.m_lastPosition = this.m_TrackPositionProperty.get_vector3Value();
			this.m_lastRotation = this.m_TrackRotationProperty.get_quaternionValue();
			EditorGUI.BeginChangeCheck();
			this.DrawAvatarProperties();
			if (EditorGUI.EndChangeCheck())
			{
				this.RebuildGraph();
			}
			if (base.get_targets().Length == 1)
			{
				EditorGUI.BeginChangeCheck();
				EditorGUILayout.PropertyField(this.m_ApplyOffsetsProperty, AnimationTrackInspector.Styles.TrackOffsetTitle, new GUILayoutOption[0]);
				if (EditorGUI.EndChangeCheck())
				{
					this.RebuildGraph();
				}
				Animator animator = this.GetAnimator();
				bool flag2 = animator == null || !TimelineAnimationUtilities.ValidateOffsetAvailabitity(base.timelineWindow.state.currentDirector, animator) || !this.m_ApplyOffsetsProperty.get_boolValue();
				using (new EditorGUI.DisabledScope(flag2))
				{
					EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
					float num = 0f;
					float num2 = 0f;
					GUI.get_skin().get_button().CalcMinMaxWidth(AnimationTrackInspector.Styles.PositionIcon, ref num, ref num2);
					AnimationTrackInspector.ShowMotionOffsetEditModeToolbar(ref this.m_OffsetEditMode);
					SceneView.RepaintAll();
					EditorGUI.BeginChangeCheck();
					EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
					EditorGUILayout.PropertyField(this.m_TrackPositionProperty, new GUILayoutOption[0]);
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
					AnimationPlayableAssetInspector.ShowRotationField(this.m_TrackRotationProperty);
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.Space();
					EditorGUILayout.Space();
					EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
					flag |= EditorGUI.EndChangeCheck();
					if (flag)
					{
						AnimationTrack animationTrack = (AnimationTrack)base.get_target();
						animationTrack.UpdateClipOffsets();
						this.Evaluate();
					}
				}
			}
			else
			{
				GUILayout.Label(AnimationTrackInspector.Styles.TrackOffsetCannotBeEditedWarning, EditorStyles.get_helpBox(), new GUILayoutOption[0]);
			}
			AnimationTrackInspector.MatchTargetsField(this.m_MatchFieldsProperty, null, null, false);
			base.get_serializedObject().ApplyModifiedProperties();
		}

		private Animator GetAnimator()
		{
			Animator result;
			if (this.m_PreviousAnimator != null)
			{
				result = this.m_PreviousAnimator;
			}
			else
			{
				TrackAsset trackAsset = base.get_target() as TrackAsset;
				if (trackAsset != null && base.timelineWindow.state != null && base.timelineWindow.state.currentDirector != null)
				{
					GameObject sceneGameObject = TimelineUtility.GetSceneGameObject(base.timelineWindow.state.currentDirector, trackAsset);
					if (sceneGameObject != null && sceneGameObject.get_transform() != null)
					{
						Animator component = sceneGameObject.get_transform().GetComponent<Animator>();
						this.m_PreviousAnimator = component;
						if (component != null)
						{
							result = component;
							return result;
						}
					}
				}
				result = null;
			}
			return result;
		}

		public void DrawAvatarProperties()
		{
			EditorGUILayout.PropertyField(this.m_ApplyAvatarMaskProperty, new GUILayoutOption[0]);
			if (this.m_ApplyAvatarMaskProperty.get_hasMultipleDifferentValues() || this.m_ApplyAvatarMaskProperty.get_boolValue())
			{
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
				EditorGUILayout.PropertyField(this.m_AvatarMaskProperty, new GUILayoutOption[0]);
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
			}
			EditorGUILayout.Space();
		}

		public static void ShowMotionOffsetEditModeToolbar(ref TimelineAnimationUtilities.OffsetEditMode motionOffset)
		{
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.FlexibleSpace();
			GUILayout.FlexibleSpace();
			int num = GUILayout.Toolbar((int)motionOffset, new GUIContent[]
			{
				AnimationTrackInspector.Styles.PositionIcon,
				AnimationTrackInspector.Styles.RotationIcon
			}, new GUILayoutOption[0]);
			if (GUI.get_changed())
			{
				if (motionOffset == (TimelineAnimationUtilities.OffsetEditMode)num)
				{
					motionOffset = TimelineAnimationUtilities.OffsetEditMode.None;
				}
				else
				{
					motionOffset = (TimelineAnimationUtilities.OffsetEditMode)num;
				}
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.Space(3f);
		}

		public override void OnEnable()
		{
			base.OnEnable();
			SceneView.onSceneGUIDelegate = (SceneView.OnSceneFunc)Delegate.Combine(SceneView.onSceneGUIDelegate, new SceneView.OnSceneFunc(this.OnSceneGUI));
			this.m_MatchFieldsProperty = base.get_serializedObject().FindProperty("m_MatchTargetFields");
			this.m_MatchFieldsProperty.set_isExpanded(true);
			this.m_TrackPositionProperty = base.get_serializedObject().FindProperty("m_Position");
			this.m_TrackPositionProperty.set_isExpanded(true);
			this.m_TrackRotationProperty = base.get_serializedObject().FindProperty("m_Rotation");
			this.m_ApplyOffsetsProperty = base.get_serializedObject().FindProperty("m_ApplyOffsets");
			this.m_AvatarMaskProperty = base.get_serializedObject().FindProperty("m_AvatarMask");
			this.m_ApplyAvatarMaskProperty = base.get_serializedObject().FindProperty("m_ApplyAvatarMask");
			this.m_lastPosition = this.m_TrackPositionProperty.get_vector3Value();
			this.m_lastRotation = this.m_TrackRotationProperty.get_quaternionValue();
		}

		public override void OnDestroy()
		{
			base.OnDestroy();
			SceneView.onSceneGUIDelegate = (SceneView.OnSceneFunc)Delegate.Remove(SceneView.onSceneGUIDelegate, new SceneView.OnSceneFunc(this.OnSceneGUI));
		}

		private void OnSceneGUI(SceneView sceneView)
		{
			this.DoOffsetManipulator();
		}

		private void DoOffsetManipulator()
		{
			if (base.get_targets().Length <= 1)
			{
				if (!(base.timelineWindow == null) && base.timelineWindow.state != null && !(base.timelineWindow.state.currentDirector == null))
				{
					AnimationTrack animationTrack = base.get_target() as AnimationTrack;
					if (animationTrack != null && animationTrack.applyOffsets && this.m_OffsetEditMode != TimelineAnimationUtilities.OffsetEditMode.None)
					{
						GameObject sceneGameObject = TimelineUtility.GetSceneGameObject(base.timelineWindow.state.currentDirector, animationTrack);
						Transform transform = (!(sceneGameObject != null)) ? null : sceneGameObject.get_transform();
						TimelineAnimationUtilities.RigidTransform trackOffsets = TimelineAnimationUtilities.GetTrackOffsets(animationTrack, transform);
						EditorGUI.BeginChangeCheck();
						TimelineAnimationUtilities.OffsetEditMode offsetEditMode = this.m_OffsetEditMode;
						if (offsetEditMode != TimelineAnimationUtilities.OffsetEditMode.Translation)
						{
							if (offsetEditMode == TimelineAnimationUtilities.OffsetEditMode.Rotation)
							{
								trackOffsets.rotation = Handles.RotationHandle(trackOffsets.rotation, trackOffsets.position);
							}
						}
						else
						{
							trackOffsets.position = Handles.PositionHandle(trackOffsets.position, (Tools.get_pivotRotation() != 1) ? trackOffsets.rotation : Quaternion.get_identity());
						}
						if (EditorGUI.EndChangeCheck())
						{
							TimelineAnimationUtilities.UpdateTrackOffset(animationTrack, transform, trackOffsets);
							this.Evaluate();
							base.Repaint();
						}
					}
				}
			}
		}

		public static void MatchTargetsField(SerializedProperty property, SerializedProperty alternate, SerializedProperty disableOptions, bool showHelp = false)
		{
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			property.set_isExpanded(EditorGUILayout.Foldout(property.get_isExpanded(), AnimationTrackInspector.Styles.MatchTargetFieldsTitle));
			GUILayout.EndHorizontal();
			int num = 0;
			if (property.get_isExpanded())
			{
				if (showHelp)
				{
					string text = string.Format(AnimationTrackInspector.Styles.MatchTargetsFieldHelp.get_text(), AnimationTrackInspector.Styles.DisableOptionsTitle.get_text());
					EditorGUILayout.HelpBox(text, 1);
				}
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
				bool flag = false;
				if (alternate != null)
				{
					EditorGUILayout.PropertyField(disableOptions, AnimationTrackInspector.Styles.DisableOptionsTitle, new GUILayoutOption[0]);
					flag = !disableOptions.get_boolValue();
					if (flag)
					{
						property = alternate;
					}
				}
				using (new EditorGUI.DisabledScope(flag))
				{
					MatchTargetFields intValue = (MatchTargetFields)property.get_intValue();
					EditorGUI.BeginChangeCheck();
					Rect controlRect = EditorGUILayout.GetControlRect(false, 32f, new GUILayoutOption[0]);
					Rect rect = new Rect(controlRect.get_x(), controlRect.get_y(), controlRect.get_width(), 16f);
					EditorGUI.BeginProperty(controlRect, AnimationTrackInspector.Styles.MatchTargetFieldsTitle, property);
					float num2 = 0f;
					float num3 = 0f;
					EditorStyles.get_label().CalcMinMaxWidth(AnimationTrackInspector.Styles.XTitle, ref num2, ref num3);
					float num4 = num2 + 20f;
					GUILayout.BeginHorizontal(new GUILayoutOption[0]);
					Rect rect2 = EditorGUI.PrefixLabel(rect, AnimationTrackInspector.Styles.PositionTitle);
					int indentLevel = EditorGUI.get_indentLevel();
					EditorGUI.set_indentLevel(0);
					rect2.set_width(num4);
					num |= ((!EditorGUI.ToggleLeft(rect2, AnimationTrackInspector.Styles.XTitle, intValue.HasAny(MatchTargetFields.PositionX))) ? 0 : 1);
					rect2.set_x(rect2.get_x() + num4);
					num |= ((!EditorGUI.ToggleLeft(rect2, AnimationTrackInspector.Styles.YTitle, intValue.HasAny(MatchTargetFields.PositionY))) ? 0 : 2);
					rect2.set_x(rect2.get_x() + num4);
					num |= ((!EditorGUI.ToggleLeft(rect2, AnimationTrackInspector.Styles.ZTitle, intValue.HasAny(MatchTargetFields.PositionZ))) ? 0 : 4);
					EditorGUI.set_indentLevel(indentLevel);
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal(new GUILayoutOption[0]);
					rect.set_y(rect.get_y() + 16f);
					rect2 = EditorGUI.PrefixLabel(rect, AnimationTrackInspector.Styles.RotationTitle);
					EditorGUI.set_indentLevel(0);
					rect2.set_width(num4);
					num |= ((!EditorGUI.ToggleLeft(rect2, AnimationTrackInspector.Styles.XTitle, intValue.HasAny(MatchTargetFields.RotationX))) ? 0 : 8);
					rect2.set_x(rect2.get_x() + num4);
					num |= ((!EditorGUI.ToggleLeft(rect2, AnimationTrackInspector.Styles.YTitle, intValue.HasAny(MatchTargetFields.RotationY))) ? 0 : 16);
					rect2.set_x(rect2.get_x() + num4);
					num |= ((!EditorGUI.ToggleLeft(rect2, AnimationTrackInspector.Styles.ZTitle, intValue.HasAny(MatchTargetFields.RotationZ))) ? 0 : 32);
					EditorGUI.set_indentLevel(indentLevel);
					GUILayout.EndHorizontal();
					if (EditorGUI.EndChangeCheck())
					{
						property.set_intValue(num);
					}
					EditorGUI.EndProperty();
				}
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
			}
		}
	}
}
