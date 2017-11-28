using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class TimelineRecording
	{
		internal class RecordingState : IAnimationRecordingState
		{
			public GameObject activeGameObject
			{
				get;
				set;
			}

			public GameObject activeRootGameObject
			{
				get;
				set;
			}

			public AnimationClip activeAnimationClip
			{
				get;
				set;
			}

			public bool addZeroFrame
			{
				get
				{
					return false;
				}
			}

			public int currentFrame
			{
				get;
				set;
			}

			public void SaveCurve(AnimationWindowCurve curve)
			{
				Undo.RegisterCompleteObjectUndo(this.activeAnimationClip, "Edit Curve");
				AnimationRecording.SaveModifiedCurve(curve, this.activeAnimationClip);
			}

			public void AddPropertyModification(EditorCurveBinding binding, PropertyModification propertyModification, bool keepPrefabOverride)
			{
				AnimationMode.AddPropertyModification(binding, propertyModification, keepPrefabOverride);
			}

			public bool DiscardModification(PropertyModification modification)
			{
				return false;
			}
		}

		private static readonly List<PropertyModification> s_TempPropertyModifications = new List<PropertyModification>(6);

		private static readonly TimelineRecording.RecordingState s_RecordState = new TimelineRecording.RecordingState();

		private static readonly AnimationTrackRecorder s_TrackRecorder = new AnimationTrackRecorder();

		private static readonly List<UndoPropertyModification> s_UnprocessedMods = new List<UndoPropertyModification>();

		private static readonly List<UndoPropertyModification> s_ModsToProcess = new List<UndoPropertyModification>();

		private static AnimationTrack s_LastTrackWarning;

		public const string kLocalPosition = "m_LocalPosition";

		public const string kLocalRotation = "m_LocalRotation";

		public const string kLocalEulerHint = "m_LocalEulerAnglesHint";

		private const string kRotationWarning = "You are recording with an initial rotation offset. This may result in a misrepresentation of euler angles. When recording transform properties, it is recommended to reset rotation prior to recording";

		[CompilerGenerated]
		private static Func<PropertyModification, UndoPropertyModification> <>f__mg$cache0;

		internal static UndoPropertyModification[] ProcessUndoModification(UndoPropertyModification[] modifications, TimelineWindow.TimelineState state)
		{
			UndoPropertyModification[] result;
			if (TimelineRecording.HasAnyPlayableAssetModifications(modifications))
			{
				result = TimelineRecording.ProcessPlayableAssetModification(modifications, state);
			}
			else
			{
				result = TimelineRecording.ProcessMonoBehaviourModification(modifications, state);
			}
			return result;
		}

		private static Object GetTarget(UndoPropertyModification undo)
		{
			Object result;
			if (undo.currentValue != null)
			{
				result = undo.currentValue.target;
			}
			else if (undo.previousValue != null)
			{
				result = undo.previousValue.target;
			}
			else
			{
				result = null;
			}
			return result;
		}

		private static TrackAsset GetTrackForGameObject(GameObject gameObject, TimelineWindow.TimelineState state)
		{
			TrackAsset result;
			if (gameObject == null)
			{
				result = null;
			}
			else
			{
				PlayableDirector currentDirector = state.currentDirector;
				if (currentDirector == null)
				{
					result = null;
				}
				else
				{
					int num = 2147483647;
					TrackAsset trackAsset = null;
					IEnumerable<TrackAsset> flattenedTracks = state.timeline.flattenedTracks;
					foreach (TrackAsset current in flattenedTracks)
					{
						if (current.GetType() == typeof(AnimationTrack))
						{
							if (state.IsArmedForRecord(current))
							{
								GameObject sceneGameObject = TimelineUtility.GetSceneGameObject(currentDirector, current);
								if (sceneGameObject != null)
								{
									int childLevel = TimelineRecording.GetChildLevel(sceneGameObject, gameObject);
									if (childLevel != -1 && childLevel < num)
									{
										trackAsset = current;
										num = childLevel;
									}
								}
							}
						}
					}
					if (trackAsset && !state.IsArmedForRecord(trackAsset))
					{
						trackAsset = null;
					}
					result = trackAsset;
				}
			}
			return result;
		}

		public static TrackAsset GetRecordingTrack(SerializedProperty property, TimelineWindow.TimelineState state)
		{
			SerializedObject serializedObject = property.get_serializedObject();
			Component component = serializedObject.get_targetObject() as Component;
			TrackAsset result;
			if (component == null)
			{
				result = null;
			}
			else
			{
				GameObject gameObject = component.get_gameObject();
				result = TimelineRecording.GetTrackForGameObject(gameObject, state);
			}
			return result;
		}

		private static void GatherModifications(SerializedProperty property, List<PropertyModification> modifications)
		{
			if (property.get_hasChildren())
			{
				SerializedProperty serializedProperty = property.Copy();
				SerializedProperty endProperty = property.GetEndProperty(false);
				while (serializedProperty.Next(true) && !SerializedProperty.EqualContents(serializedProperty, endProperty))
				{
					TimelineRecording.GatherModifications(serializedProperty, modifications);
				}
			}
			bool flag = property.get_propertyType() == 5;
			bool flag2 = property.get_propertyType() == 2 || property.get_propertyType() == 1 || property.get_propertyType() == 0;
			if (flag || flag2)
			{
				SerializedObject serializedObject = property.get_serializedObject();
				PropertyModification propertyModification = new PropertyModification();
				propertyModification.target = serializedObject.get_targetObject();
				propertyModification.propertyPath = property.get_propertyPath();
				if (flag)
				{
					propertyModification.value = string.Empty;
					propertyModification.objectReference = property.get_objectReferenceValue();
				}
				else
				{
					propertyModification.value = TimelineUtility.PropertyToString(property);
				}
				if (serializedObject.get_targetObject() is Component)
				{
					GameObject gameObject = ((Component)serializedObject.get_targetObject()).get_gameObject();
					EditorCurveBinding editorCurveBinding;
					if (AnimationUtility.PropertyModificationToEditorCurveBinding(propertyModification, gameObject, ref editorCurveBinding) != null)
					{
						modifications.Add(propertyModification);
					}
				}
				else
				{
					modifications.Add(propertyModification);
				}
			}
		}

		public static bool CanRecord(SerializedProperty property, TimelineWindow.TimelineState state)
		{
			bool result;
			if (TimelineRecording.IsPlayableAssetProperty(property))
			{
				result = AnimatedParameterExtensions.IsAnimatable(property.get_propertyType());
			}
			else if (TimelineRecording.GetRecordingTrack(property, state) == null)
			{
				result = false;
			}
			else
			{
				TimelineRecording.s_TempPropertyModifications.Clear();
				TimelineRecording.GatherModifications(property, TimelineRecording.s_TempPropertyModifications);
				result = TimelineRecording.s_TempPropertyModifications.Any<PropertyModification>();
			}
			return result;
		}

		public static void AddKey(SerializedProperty prop, TimelineWindow.TimelineState state)
		{
			TimelineRecording.s_TempPropertyModifications.Clear();
			TimelineRecording.GatherModifications(prop, TimelineRecording.s_TempPropertyModifications);
			if (TimelineRecording.s_TempPropertyModifications.Any<PropertyModification>())
			{
				TimelineRecording.AddKey(TimelineRecording.s_TempPropertyModifications, state);
			}
		}

		public static void AddKey(IEnumerable<PropertyModification> modifications, TimelineWindow.TimelineState state)
		{
			if (TimelineRecording.<>f__mg$cache0 == null)
			{
				TimelineRecording.<>f__mg$cache0 = new Func<PropertyModification, UndoPropertyModification>(TimelineRecording.PropertyModificationToUndoPropertyModification);
			}
			UndoPropertyModification[] modifications2 = modifications.Select(TimelineRecording.<>f__mg$cache0).ToArray<UndoPropertyModification>();
			TimelineRecording.ProcessUndoModification(modifications2, state);
		}

		private static UndoPropertyModification PropertyModificationToUndoPropertyModification(PropertyModification prop)
		{
			UndoPropertyModification result = default(UndoPropertyModification);
			result.previousValue = prop;
			result.currentValue = new PropertyModification
			{
				objectReference = prop.objectReference,
				propertyPath = prop.propertyPath,
				target = prop.target,
				value = prop.value
			};
			result.set_keepPrefabOverride(true);
			return result;
		}

		private static AnimationClip GetRecordingClip(TrackAsset asset, TimelineWindow.TimelineState state, out double startTime, out double timeScale)
		{
			startTime = 0.0;
			timeScale = 1.0;
			TimelineClip timelineClip = asset.FindRecordingClipAtTime(state.time);
			AnimationClip result = asset.FindRecordingAnimationClipAtTime(state.time);
			if (timelineClip != null)
			{
				startTime = timelineClip.start;
				timeScale = timelineClip.timeScale;
			}
			return result;
		}

		private static bool GetClipAndRelativeTime(Object target, TimelineWindow.TimelineState state, out AnimationClip outClip, out double keyTime, out bool keyInRange)
		{
			outClip = null;
			keyTime = 0.0;
			keyInRange = false;
			double num = 0.0;
			double num2 = 1.0;
			AnimationClip animationClip = null;
			IPlayableAsset playableAsset = target as IPlayableAsset;
			Component component = target as Component;
			if (playableAsset != null)
			{
				TimelineClip timelineClip = TimelineRecording.FindClipWithAsset(state.timeline, playableAsset, state.currentDirector);
				if (timelineClip != null && state.IsArmedForRecord(timelineClip.parentTrack))
				{
					AnimatedParameterExtensions.CreateCurvesIfRequired(timelineClip, null);
					animationClip = timelineClip.curves;
					num = timelineClip.start;
					num2 = timelineClip.timeScale;
				}
			}
			else if (component != null)
			{
				TrackAsset trackForGameObject = TimelineRecording.GetTrackForGameObject(component.get_gameObject(), state);
				if (trackForGameObject != null)
				{
					animationClip = TimelineRecording.GetRecordingClip(trackForGameObject, state, out num, out num2);
				}
			}
			bool result;
			if (animationClip == null)
			{
				result = false;
			}
			else
			{
				keyTime = (state.time - num) * num2;
				outClip = animationClip;
				keyInRange = (keyTime >= 0.0 && keyTime <= (double)animationClip.get_length() * num2 + 9.9999997473787516E-06);
				result = true;
			}
			return result;
		}

		public static bool HasCurve(IEnumerable<PropertyModification> modifications, Object target, TimelineWindow.TimelineState state)
		{
			return TimelineRecording.GetKeyTimes(target, modifications, state).Any<double>();
		}

		public static bool HasKey(IEnumerable<PropertyModification> modifications, Object target, TimelineWindow.TimelineState state)
		{
			AnimationClip clip;
			double num;
			bool flag;
			return TimelineRecording.GetClipAndRelativeTime(target, state, out clip, out num, out flag) && TimelineRecording.GetKeyTimes(target, modifications, state).Any((double t) => CurveEditUtility.KeyCompare((float)state.time, (float)t, clip.get_frameRate()) == 0);
		}

		private static bool HasBinding(Object target, PropertyModification modification, AnimationClip clip, out EditorCurveBinding binding)
		{
			Component component = target as Component;
			IPlayableAsset playableAsset = target as IPlayableAsset;
			bool result;
			if (component != null)
			{
				Type type = AnimationUtility.PropertyModificationToEditorCurveBinding(modification, component.get_gameObject(), ref binding);
				binding = RotationCurveInterpolation.RemapAnimationBindingForRotationCurves(binding, clip);
				result = (type != null);
			}
			else if (playableAsset != null)
			{
				binding = EditorCurveBinding.FloatCurve(string.Empty, target.GetType(), modification.propertyPath);
				result = true;
			}
			else
			{
				binding = default(EditorCurveBinding);
				result = false;
			}
			return result;
		}

		public static void RemoveKey(Object target, IEnumerable<PropertyModification> modifications, TimelineWindow.TimelineState state)
		{
			AnimationClip animationClip;
			double num;
			bool flag;
			if (TimelineRecording.GetClipAndRelativeTime(target, state, out animationClip, out num, out flag) && flag)
			{
				TimelineUndo.PushUndo(animationClip, "Remove Key");
				foreach (PropertyModification current in modifications)
				{
					EditorCurveBinding editorCurveBinding;
					if (TimelineRecording.HasBinding(target, current, animationClip, out editorCurveBinding))
					{
						if (editorCurveBinding.get_isPPtrCurve())
						{
							CurveEditUtility.RemoveObjectKey(animationClip, editorCurveBinding, num);
						}
						else
						{
							AnimationCurve editorCurve = AnimationUtility.GetEditorCurve(animationClip, editorCurveBinding);
							if (editorCurve != null)
							{
								CurveEditUtility.RemoveKeyFrameFromCurve(editorCurve, (float)num, animationClip.get_frameRate());
								AnimationUtility.SetEditorCurve(animationClip, editorCurveBinding, editorCurve);
							}
						}
					}
				}
			}
		}

		private static HashSet<double> GetKeyTimes(Object target, IEnumerable<PropertyModification> modifications, TimelineWindow.TimelineState state)
		{
			HashSet<double> hashSet = new HashSet<double>();
			AnimationClip animationClip;
			double num;
			bool flag;
			TimelineRecording.GetClipAndRelativeTime(target, state, out animationClip, out num, out flag);
			HashSet<double> result;
			if (animationClip == null)
			{
				result = hashSet;
			}
			else
			{
				Component component = target as Component;
				IPlayableAsset playableAsset = target as IPlayableAsset;
				AnimationClipCurveInfo curveInfo = AnimationClipCurveCache.Instance.GetCurveInfo(animationClip);
				TimelineClip timelineClip = null;
				if (component != null)
				{
					timelineClip = TimelineRecording.GetTrackForGameObject(component.get_gameObject(), state).FindRecordingClipAtTime(state.time);
				}
				else if (playableAsset != null)
				{
					timelineClip = TimelineRecording.FindClipWithAsset(state.timeline, playableAsset, state.currentDirector);
				}
				foreach (PropertyModification current in modifications)
				{
					EditorCurveBinding binding;
					if (TimelineRecording.HasBinding(target, current, animationClip, out binding))
					{
						IEnumerable<double> enumerable = new HashSet<double>();
						if (binding.get_isPPtrCurve())
						{
							ObjectReferenceKeyframe[] objectCurveForBinding = curveInfo.GetObjectCurveForBinding(binding);
							if (objectCurveForBinding != null)
							{
								enumerable = from x in objectCurveForBinding
								select (double)x.time;
							}
						}
						else
						{
							AnimationCurve curveForBinding = curveInfo.GetCurveForBinding(binding);
							if (curveForBinding != null)
							{
								enumerable = from x in curveForBinding.get_keys()
								select (double)x.get_time();
							}
						}
						if (timelineClip != null)
						{
							foreach (double current2 in enumerable)
							{
								double num2 = timelineClip.FromLocalTimeUnbound(current2);
								if (num2 >= timelineClip.start - 1E-05 && num2 <= timelineClip.end + 1E-05)
								{
									hashSet.Add(num2);
								}
							}
						}
						else
						{
							hashSet.UnionWith(enumerable);
						}
					}
				}
				result = hashSet;
			}
			return result;
		}

		public static void NextKey(Object target, IEnumerable<PropertyModification> modifications, TimelineWindow.TimelineState state)
		{
			HashSet<double> keyTimes = TimelineRecording.GetKeyTimes(target, modifications, state);
			if (keyTimes.Count != 0)
			{
				IEnumerable<double> source = from x in keyTimes
				where x > state.time + 1E-05
				select x;
				if (source.Any<double>())
				{
					state.time = source.Min();
				}
			}
		}

		public static void PrevKey(Object target, IEnumerable<PropertyModification> modifications, TimelineWindow.TimelineState state)
		{
			HashSet<double> keyTimes = TimelineRecording.GetKeyTimes(target, modifications, state);
			if (keyTimes.Count != 0)
			{
				IEnumerable<double> source = from x in keyTimes
				where x < state.time - 1E-05
				select x;
				if (source.Any<double>())
				{
					state.time = source.Max();
				}
			}
		}

		public static void RemoveCurve(Object target, IEnumerable<PropertyModification> modifications, TimelineWindow.TimelineState state)
		{
			AnimationClip animationClip = null;
			double num = 0.0;
			bool flag = false;
			if (TimelineRecording.GetClipAndRelativeTime(target, state, out animationClip, out num, out flag))
			{
				TimelineUndo.PushUndo(animationClip, "Remove Curve");
				foreach (PropertyModification current in modifications)
				{
					EditorCurveBinding editorCurveBinding;
					if (TimelineRecording.HasBinding(target, current, animationClip, out editorCurveBinding))
					{
						if (editorCurveBinding.get_isPPtrCurve())
						{
							AnimationUtility.SetObjectReferenceCurve(animationClip, editorCurveBinding, null);
						}
						else
						{
							AnimationUtility.SetEditorCurve(animationClip, editorCurveBinding, null);
						}
					}
				}
			}
		}

		[DebuggerHidden]
		public static IEnumerable<GameObject> GetRecordableGameObjects(TimelineWindow.TimelineState state)
		{
			TimelineRecording.<GetRecordableGameObjects>c__Iterator0 <GetRecordableGameObjects>c__Iterator = new TimelineRecording.<GetRecordableGameObjects>c__Iterator0();
			<GetRecordableGameObjects>c__Iterator.state = state;
			TimelineRecording.<GetRecordableGameObjects>c__Iterator0 expr_0E = <GetRecordableGameObjects>c__Iterator;
			expr_0E.$PC = -2;
			return expr_0E;
		}

		internal static UndoPropertyModification[] ProcessMonoBehaviourModification(UndoPropertyModification[] modifications, TimelineWindow.TimelineState state)
		{
			UndoPropertyModification[] result;
			if (state == null || state.currentDirector == null)
			{
				result = modifications;
			}
			else
			{
				TimelineRecording.s_UnprocessedMods.Clear();
				TimelineRecording.s_TrackRecorder.PrepareForRecord(state);
				TimelineRecording.s_ModsToProcess.Clear();
				TimelineRecording.s_ModsToProcess.AddRange(modifications.Reverse<UndoPropertyModification>());
				while (TimelineRecording.s_ModsToProcess.Count > 0)
				{
					UndoPropertyModification undoPropertyModification = TimelineRecording.s_ModsToProcess[TimelineRecording.s_ModsToProcess.Count - 1];
					TimelineRecording.s_ModsToProcess.RemoveAt(TimelineRecording.s_ModsToProcess.Count - 1);
					GameObject gameObjectFromModification = TimelineRecording.GetGameObjectFromModification(undoPropertyModification);
					TrackAsset trackForGameObject = TimelineRecording.GetTrackForGameObject(gameObjectFromModification, state);
					if (trackForGameObject != null)
					{
						double num = 0.0;
						AnimationClip animationClip = TimelineRecording.s_TrackRecorder.PrepareTrack(trackForGameObject, state, gameObjectFromModification, out num);
						if (animationClip == null)
						{
							TimelineRecording.s_ModsToProcess.Reverse();
							result = TimelineRecording.s_ModsToProcess.ToArray();
							return result;
						}
						TimelineRecording.s_RecordState.activeAnimationClip = animationClip;
						TimelineRecording.s_RecordState.activeRootGameObject = state.GetSceneReference(trackForGameObject);
						TimelineRecording.s_RecordState.activeGameObject = gameObjectFromModification;
						TimelineRecording.s_RecordState.currentFrame = Mathf.RoundToInt((float)num);
						EditorUtility.SetDirty(animationClip);
						UndoPropertyModification[] array = TimelineRecording.GatherRelatedModifications(undoPropertyModification, TimelineRecording.s_ModsToProcess);
						Animator component = TimelineRecording.s_RecordState.activeRootGameObject.GetComponent<Animator>();
						AnimationTrack track = trackForGameObject as AnimationTrack;
						TimelineRecording.UpdatePreviewMode(array, gameObjectFromModification);
						TimelineRecording.AddTrackOffset(track, array, animationClip, component);
						TimelineRecording.AddClipOffset(track, array, TimelineRecording.s_TrackRecorder.recordClip, component);
						bool flag = component != null && undoPropertyModification.currentValue != null && undoPropertyModification.currentValue.target == TimelineRecording.s_RecordState.activeRootGameObject.get_transform() && TimelineRecording.HasOffsets(track, TimelineRecording.s_TrackRecorder.recordClip);
						if (flag)
						{
							array = TimelineRecording.HandleEulerModifications(track, TimelineRecording.s_TrackRecorder.recordClip, animationClip, (float)TimelineRecording.s_RecordState.currentFrame * animationClip.get_frameRate(), array);
							TimelineRecording.RemoveOffsets(undoPropertyModification, track, TimelineRecording.s_TrackRecorder.recordClip, array);
						}
						UndoPropertyModification[] array2 = AnimationRecording.Process(TimelineRecording.s_RecordState, array);
						if (array2 != null && array2.Length != 0)
						{
							TimelineRecording.s_UnprocessedMods.AddRange(array2);
						}
						if (flag)
						{
							TimelineRecording.ReapplyOffsets(undoPropertyModification, track, TimelineRecording.s_TrackRecorder.recordClip, array);
						}
						TimelineRecording.s_TrackRecorder.FinializeTrack(trackForGameObject, state);
					}
					else
					{
						TimelineRecording.s_UnprocessedMods.Add(undoPropertyModification);
					}
				}
				TimelineRecording.s_TrackRecorder.FinalizeRecording(state);
				result = TimelineRecording.s_UnprocessedMods.ToArray();
			}
			return result;
		}

		internal static bool IsPosition(UndoPropertyModification modification)
		{
			bool result;
			if (modification.currentValue != null)
			{
				result = modification.currentValue.propertyPath.StartsWith("m_LocalPosition");
			}
			else
			{
				result = (modification.previousValue != null && modification.previousValue.propertyPath.StartsWith("m_LocalPosition"));
			}
			return result;
		}

		internal static bool IsRotation(UndoPropertyModification modification)
		{
			bool result;
			if (modification.currentValue != null)
			{
				result = (modification.currentValue.propertyPath.StartsWith("m_LocalRotation") || modification.currentValue.propertyPath.StartsWith("m_LocalEulerAnglesHint"));
			}
			else
			{
				result = (modification.previousValue != null && (modification.previousValue.propertyPath.StartsWith("m_LocalRotation") || modification.previousValue.propertyPath.StartsWith("m_LocalEulerAnglesHint")));
			}
			return result;
		}

		internal static bool IsPositionOrRotation(UndoPropertyModification modification)
		{
			return TimelineRecording.IsPosition(modification) || TimelineRecording.IsRotation(modification);
		}

		internal static void UpdatePreviewMode(UndoPropertyModification[] mods, GameObject go)
		{
			if (mods.Any((UndoPropertyModification x) => TimelineRecording.IsPositionOrRotation(x) && TimelineRecording.IsRootModification(x)))
			{
				bool flag = false;
				bool flag2 = false;
				for (int i = 0; i < mods.Length; i++)
				{
					UndoPropertyModification modification = mods[i];
					EditorCurveBinding editorCurveBinding = default(EditorCurveBinding);
					if (AnimationUtility.PropertyModificationToEditorCurveBinding(modification.previousValue, go, ref editorCurveBinding) != null)
					{
						flag |= TimelineRecording.IsPosition(modification);
						flag2 |= TimelineRecording.IsRotation(modification);
						AnimationMode.AddPropertyModification(editorCurveBinding, modification.previousValue, true);
					}
				}
				AnimationModeDriver previewDriver = TimelineWindow.TimelineState.previewDriver;
				if (previewDriver != null && AnimationMode.InAnimationMode(previewDriver))
				{
					if (flag)
					{
						DrivenPropertyManager.RegisterProperty(previewDriver, go.get_transform(), "m_LocalPosition.x");
						DrivenPropertyManager.RegisterProperty(previewDriver, go.get_transform(), "m_LocalPosition.y");
						DrivenPropertyManager.RegisterProperty(previewDriver, go.get_transform(), "m_LocalPosition.z");
					}
					else if (flag2)
					{
						DrivenPropertyManager.RegisterProperty(previewDriver, go.get_transform(), "m_LocalRotation.x");
						DrivenPropertyManager.RegisterProperty(previewDriver, go.get_transform(), "m_LocalRotation.y");
						DrivenPropertyManager.RegisterProperty(previewDriver, go.get_transform(), "m_LocalRotation.z");
						DrivenPropertyManager.RegisterProperty(previewDriver, go.get_transform(), "m_LocalRotation.w");
					}
				}
			}
		}

		internal static bool IsRootModification(UndoPropertyModification modification)
		{
			string source = string.Empty;
			if (modification.currentValue != null)
			{
				source = modification.currentValue.propertyPath;
			}
			else if (modification.previousValue != null)
			{
				source = modification.previousValue.propertyPath;
			}
			return !source.Contains('/') && !source.Contains('\\');
		}

		internal static bool ClipHasPositionOrRotation(AnimationClip clip)
		{
			bool result;
			if (clip == null || clip.get_empty())
			{
				result = false;
			}
			else
			{
				AnimationClipCurveInfo curveInfo = AnimationClipCurveCache.Instance.GetCurveInfo(clip);
				for (int i = 0; i < curveInfo.bindings.Length; i++)
				{
					if (curveInfo.bindings[i].get_type() == typeof(Transform) && (curveInfo.bindings[i].propertyName.StartsWith("m_LocalPosition") || curveInfo.bindings[i].propertyName.StartsWith("m_LocalRotation") || curveInfo.bindings[i].propertyName.StartsWith("localEuler")))
					{
						result = true;
						return result;
					}
				}
				result = false;
			}
			return result;
		}

		internal static TimelineAnimationUtilities.RigidTransform ComputeInitialClipOffsets(AnimationTrack track, UndoPropertyModification[] mods, Animator animator)
		{
			Vector3 pos = Vector3.get_zero();
			Quaternion rot = Quaternion.get_identity();
			if (mods[0].previousValue.target == animator.get_transform())
			{
				TimelineRecording.GetPreviousPositionAndRotation(mods, ref pos, ref rot);
			}
			else
			{
				pos = animator.get_transform().get_localPosition();
				rot = animator.get_transform().get_localRotation();
			}
			TimelineAnimationUtilities.RigidTransform rigidTransform = TimelineAnimationUtilities.RigidTransform.Compose(pos, rot);
			TimelineAnimationUtilities.RigidTransform a = (!track.applyOffsets) ? TimelineAnimationUtilities.RigidTransform.identity : TimelineAnimationUtilities.RigidTransform.Compose(track.position, track.rotation);
			rigidTransform = TimelineAnimationUtilities.RigidTransform.Mul(TimelineAnimationUtilities.RigidTransform.Inverse(a), rigidTransform);
			if (mods[0].previousValue.target == animator.get_transform())
			{
				TimelineRecording.SetPreviousPositionAndRotation(mods, a.position, a.rotation);
			}
			return rigidTransform;
		}

		internal static void AddTrackOffset(AnimationTrack track, UndoPropertyModification[] mods, AnimationClip clip, Animator animator)
		{
			bool arg_49_0;
			if (!track.inClipMode && !TimelineRecording.ClipHasPositionOrRotation(clip))
			{
				if (mods.Any((UndoPropertyModification x) => TimelineRecording.IsPositionOrRotation(x) && TimelineRecording.IsRootModification(x)))
				{
					arg_49_0 = (animator != null);
					goto IL_49;
				}
			}
			arg_49_0 = false;
			IL_49:
			bool flag = arg_49_0;
			if (flag)
			{
				TimelineAnimationUtilities.RigidTransform rigidTransform = TimelineRecording.ComputeInitialClipOffsets(track, mods, animator);
				track.openClipOffsetPosition = rigidTransform.position;
				track.openClipOffsetRotation = rigidTransform.rotation;
			}
		}

		internal static void AddClipOffset(AnimationTrack track, UndoPropertyModification[] mods, TimelineClip clip, Animator animator)
		{
			if (clip != null && !(clip.asset == null))
			{
				AnimationPlayableAsset animationPlayableAsset = clip.asset as AnimationPlayableAsset;
				bool arg_82_0;
				if (track.inClipMode && animationPlayableAsset != null && !TimelineRecording.ClipHasPositionOrRotation(animationPlayableAsset.clip))
				{
					if (mods.Any((UndoPropertyModification x) => TimelineRecording.IsPositionOrRotation(x) && TimelineRecording.IsRootModification(x)))
					{
						arg_82_0 = (animator != null);
						goto IL_82;
					}
				}
				arg_82_0 = false;
				IL_82:
				bool flag = arg_82_0;
				if (flag)
				{
					TimelineAnimationUtilities.RigidTransform rigidTransform = TimelineRecording.ComputeInitialClipOffsets(track, mods, animator);
					animationPlayableAsset.position = rigidTransform.position;
					animationPlayableAsset.rotation = rigidTransform.rotation;
				}
			}
		}

		internal static TimelineAnimationUtilities.RigidTransform GetLocalToTrack(AnimationTrack track, TimelineClip clip)
		{
			TimelineAnimationUtilities.RigidTransform result;
			if (track == null)
			{
				result = TimelineAnimationUtilities.RigidTransform.Compose(Vector3.get_zero(), Quaternion.get_identity());
			}
			else
			{
				AnimationPlayableAsset animationPlayableAsset = (clip != null) ? (clip.asset as AnimationPlayableAsset) : null;
				TimelineAnimationUtilities.RigidTransform a = (!track.applyOffsets) ? TimelineAnimationUtilities.RigidTransform.identity : TimelineAnimationUtilities.RigidTransform.Compose(track.position, track.rotation);
				TimelineAnimationUtilities.RigidTransform b = TimelineAnimationUtilities.RigidTransform.Compose(Vector3.get_zero(), Quaternion.get_identity());
				if (animationPlayableAsset != null)
				{
					b = TimelineAnimationUtilities.RigidTransform.Compose(animationPlayableAsset.position, animationPlayableAsset.rotation);
				}
				else
				{
					b = TimelineAnimationUtilities.RigidTransform.Compose(track.openClipOffsetPosition, track.openClipOffsetRotation);
				}
				result = TimelineAnimationUtilities.RigidTransform.Mul(a, b);
			}
			return result;
		}

		internal static bool HasOffsets(AnimationTrack track, TimelineClip clip)
		{
			bool result;
			if (track == null)
			{
				result = false;
			}
			else
			{
				bool flag = track.applyOffsets && (track.position != Vector3.get_zero() || track.rotation != Quaternion.get_identity());
				AnimationPlayableAsset animationPlayableAsset = (clip != null) ? (clip.asset as AnimationPlayableAsset) : null;
				if (animationPlayableAsset)
				{
					flag |= (animationPlayableAsset.position != Vector3.get_zero() || animationPlayableAsset.rotation != Quaternion.get_identity());
				}
				else
				{
					flag |= (track.openClipOffsetPosition != Vector3.get_zero() || track.openClipOffsetRotation != Quaternion.get_identity());
				}
				result = flag;
			}
			return result;
		}

		internal static void RemoveOffsets(UndoPropertyModification modification, AnimationTrack track, TimelineClip clip, UndoPropertyModification[] mods)
		{
			if (TimelineRecording.IsPositionOrRotation(modification))
			{
				GameObject gameObjectFromModification = TimelineRecording.GetGameObjectFromModification(modification);
				TimelineAnimationUtilities.RigidTransform b = TimelineAnimationUtilities.RigidTransform.Compose(gameObjectFromModification.get_transform().get_localPosition(), gameObjectFromModification.get_transform().get_localRotation());
				TimelineAnimationUtilities.RigidTransform localToTrack = TimelineRecording.GetLocalToTrack(track, clip);
				TimelineAnimationUtilities.RigidTransform a = TimelineAnimationUtilities.RigidTransform.Inverse(localToTrack);
				TimelineAnimationUtilities.RigidTransform rigidTransform = TimelineAnimationUtilities.RigidTransform.Mul(a, b);
				Vector3 localPosition = gameObjectFromModification.get_transform().get_localPosition();
				Quaternion localRotation = gameObjectFromModification.get_transform().get_localRotation();
				TimelineRecording.GetPreviousPositionAndRotation(mods, ref localPosition, ref localRotation);
				TimelineAnimationUtilities.RigidTransform rigidTransform2 = TimelineAnimationUtilities.RigidTransform.Mul(a, TimelineAnimationUtilities.RigidTransform.Compose(localPosition, localRotation));
				TimelineRecording.SetPreviousPositionAndRotation(mods, rigidTransform2.position, rigidTransform2.rotation);
				Vector3 localPosition2 = gameObjectFromModification.get_transform().get_localPosition();
				Quaternion localRotation2 = gameObjectFromModification.get_transform().get_localRotation();
				TimelineRecording.GetCurrentPositionAndRotation(mods, ref localPosition2, ref localRotation2);
				TimelineAnimationUtilities.RigidTransform rigidTransform3 = TimelineAnimationUtilities.RigidTransform.Mul(a, TimelineAnimationUtilities.RigidTransform.Compose(localPosition2, localRotation2));
				TimelineRecording.SetCurrentPositionAndRotation(mods, rigidTransform3.position, rigidTransform3.rotation);
				gameObjectFromModification.get_transform().set_localPosition(rigidTransform.position);
				gameObjectFromModification.get_transform().set_localRotation(rigidTransform.rotation);
			}
		}

		internal static void ReapplyOffsets(UndoPropertyModification modification, AnimationTrack track, TimelineClip clip, UndoPropertyModification[] mods)
		{
			if (TimelineRecording.IsPositionOrRotation(modification))
			{
				GameObject gameObjectFromModification = TimelineRecording.GetGameObjectFromModification(modification);
				TimelineAnimationUtilities.RigidTransform b = TimelineAnimationUtilities.RigidTransform.Compose(gameObjectFromModification.get_transform().get_localPosition(), gameObjectFromModification.get_transform().get_localRotation());
				TimelineAnimationUtilities.RigidTransform localToTrack = TimelineRecording.GetLocalToTrack(track, clip);
				TimelineAnimationUtilities.RigidTransform rigidTransform = TimelineAnimationUtilities.RigidTransform.Mul(localToTrack, b);
				Vector3 localPosition = gameObjectFromModification.get_transform().get_localPosition();
				Quaternion localRotation = gameObjectFromModification.get_transform().get_localRotation();
				TimelineRecording.GetPreviousPositionAndRotation(mods, ref localPosition, ref localRotation);
				TimelineAnimationUtilities.RigidTransform rigidTransform2 = TimelineAnimationUtilities.RigidTransform.Mul(localToTrack, TimelineAnimationUtilities.RigidTransform.Compose(localPosition, localRotation));
				TimelineRecording.SetPreviousPositionAndRotation(mods, rigidTransform2.position, rigidTransform2.rotation);
				Vector3 localPosition2 = gameObjectFromModification.get_transform().get_localPosition();
				Quaternion localRotation2 = gameObjectFromModification.get_transform().get_localRotation();
				TimelineRecording.GetCurrentPositionAndRotation(mods, ref localPosition2, ref localRotation2);
				TimelineAnimationUtilities.RigidTransform rigidTransform3 = TimelineAnimationUtilities.RigidTransform.Mul(localToTrack, TimelineAnimationUtilities.RigidTransform.Compose(localPosition2, localRotation2));
				TimelineRecording.SetCurrentPositionAndRotation(mods, rigidTransform3.position, rigidTransform3.rotation);
				gameObjectFromModification.get_transform().set_localPosition(rigidTransform.position);
				gameObjectFromModification.get_transform().set_localRotation(rigidTransform.rotation);
			}
		}

		private static UndoPropertyModification[] GatherRelatedModifications(UndoPropertyModification toMatch, List<UndoPropertyModification> list)
		{
			List<UndoPropertyModification> list2 = new List<UndoPropertyModification>
			{
				toMatch
			};
			for (int i = list.Count - 1; i >= 0; i--)
			{
				UndoPropertyModification item = list[i];
				if (item.previousValue.target == toMatch.previousValue.target && TimelineRecording.DoesPropertyPathMatch(item.previousValue.propertyPath, toMatch.previousValue.propertyPath))
				{
					list2.Add(item);
					list.RemoveAt(i);
				}
			}
			return list2.ToArray();
		}

		private static GameObject GetGameObjectFromModification(UndoPropertyModification mod)
		{
			GameObject result = null;
			if (mod.previousValue.target is GameObject)
			{
				result = (mod.previousValue.target as GameObject);
			}
			else if (mod.previousValue.target is Component)
			{
				result = (mod.previousValue.target as Component).get_gameObject();
			}
			return result;
		}

		private static int GetChildLevel(GameObject parent, GameObject child)
		{
			int num = 0;
			int result;
			while (child != null)
			{
				if (parent == child)
				{
					break;
				}
				if (child.get_transform().get_parent() == null)
				{
					result = -1;
					return result;
				}
				child = child.get_transform().get_parent().get_gameObject();
				num++;
			}
			if (child != null)
			{
				result = num;
				return result;
			}
			result = -1;
			return result;
		}

		private static bool DoesPropertyPathMatch(string a, string b)
		{
			return AnimationWindowUtility.GetPropertyGroupName(a).Equals(AnimationWindowUtility.GetPropertyGroupName(a));
		}

		internal static void GetPreviousPositionAndRotation(UndoPropertyModification[] mods, ref Vector3 position, ref Quaternion rotation)
		{
			Transform transform = mods[0].previousValue.target as Transform;
			if (transform == null)
			{
				transform = (Transform)mods[0].currentValue.target;
			}
			position = transform.get_localPosition();
			rotation = transform.get_localRotation();
			for (int i = 0; i < mods.Length; i++)
			{
				UndoPropertyModification undoPropertyModification = mods[i];
				string propertyPath = undoPropertyModification.previousValue.propertyPath;
				switch (propertyPath)
				{
				case "m_LocalPosition.x":
					position.x = float.Parse(undoPropertyModification.previousValue.value);
					break;
				case "m_LocalPosition.y":
					position.y = float.Parse(undoPropertyModification.previousValue.value);
					break;
				case "m_LocalPosition.z":
					position.z = float.Parse(undoPropertyModification.previousValue.value);
					break;
				case "m_LocalRotation.x":
					rotation.x = float.Parse(undoPropertyModification.previousValue.value);
					break;
				case "m_LocalRotation.y":
					rotation.y = float.Parse(undoPropertyModification.previousValue.value);
					break;
				case "m_LocalRotation.z":
					rotation.z = float.Parse(undoPropertyModification.previousValue.value);
					break;
				case "m_LocalRotation.w":
					rotation.w = float.Parse(undoPropertyModification.previousValue.value);
					break;
				}
			}
		}

		internal static void GetCurrentPositionAndRotation(UndoPropertyModification[] mods, ref Vector3 position, ref Quaternion rotation)
		{
			Transform transform = (Transform)mods[0].currentValue.target;
			position = transform.get_localPosition();
			rotation = transform.get_localRotation();
			for (int i = 0; i < mods.Length; i++)
			{
				UndoPropertyModification undoPropertyModification = mods[i];
				string propertyPath = undoPropertyModification.currentValue.propertyPath;
				switch (propertyPath)
				{
				case "m_LocalPosition.x":
					position.x = float.Parse(undoPropertyModification.currentValue.value);
					break;
				case "m_LocalPosition.y":
					position.y = float.Parse(undoPropertyModification.currentValue.value);
					break;
				case "m_LocalPosition.z":
					position.z = float.Parse(undoPropertyModification.currentValue.value);
					break;
				case "m_LocalRotation.x":
					rotation.x = float.Parse(undoPropertyModification.currentValue.value);
					break;
				case "m_LocalRotation.y":
					rotation.y = float.Parse(undoPropertyModification.currentValue.value);
					break;
				case "m_LocalRotation.z":
					rotation.z = float.Parse(undoPropertyModification.currentValue.value);
					break;
				case "m_LocalRotation.w":
					rotation.w = float.Parse(undoPropertyModification.currentValue.value);
					break;
				}
			}
		}

		internal static void SetPreviousPositionAndRotation(UndoPropertyModification[] mods, Vector3 pos, Quaternion rot)
		{
			for (int i = 0; i < mods.Length; i++)
			{
				UndoPropertyModification undoPropertyModification = mods[i];
				string propertyPath = undoPropertyModification.previousValue.propertyPath;
				switch (propertyPath)
				{
				case "m_LocalPosition.x":
					undoPropertyModification.previousValue.value = pos.x.ToString();
					break;
				case "m_LocalPosition.y":
					undoPropertyModification.previousValue.value = pos.y.ToString();
					break;
				case "m_LocalPosition.z":
					undoPropertyModification.previousValue.value = pos.z.ToString();
					break;
				case "m_LocalRotation.x":
					undoPropertyModification.previousValue.value = rot.x.ToString();
					break;
				case "m_LocalRotation.y":
					undoPropertyModification.previousValue.value = rot.y.ToString();
					break;
				case "m_LocalRotation.z":
					undoPropertyModification.previousValue.value = rot.z.ToString();
					break;
				case "m_LocalRotation.w":
					undoPropertyModification.previousValue.value = rot.w.ToString();
					break;
				}
			}
		}

		internal static void SetCurrentPositionAndRotation(UndoPropertyModification[] mods, Vector3 pos, Quaternion rot)
		{
			for (int i = 0; i < mods.Length; i++)
			{
				UndoPropertyModification undoPropertyModification = mods[i];
				string propertyPath = undoPropertyModification.previousValue.propertyPath;
				switch (propertyPath)
				{
				case "m_LocalPosition.x":
					undoPropertyModification.currentValue.value = pos.x.ToString();
					break;
				case "m_LocalPosition.y":
					undoPropertyModification.currentValue.value = pos.y.ToString();
					break;
				case "m_LocalPosition.z":
					undoPropertyModification.currentValue.value = pos.z.ToString();
					break;
				case "m_LocalRotation.x":
					undoPropertyModification.currentValue.value = rot.x.ToString();
					break;
				case "m_LocalRotation.y":
					undoPropertyModification.currentValue.value = rot.y.ToString();
					break;
				case "m_LocalRotation.z":
					undoPropertyModification.currentValue.value = rot.z.ToString();
					break;
				case "m_LocalRotation.w":
					undoPropertyModification.currentValue.value = rot.w.ToString();
					break;
				}
			}
		}

		internal static UndoPropertyModification[] HandleEulerModifications(AnimationTrack track, TimelineClip clip, AnimationClip animClip, float time, UndoPropertyModification[] mods)
		{
			UndoPropertyModification[] result;
			if (mods.Any((UndoPropertyModification x) => x.currentValue.propertyPath.StartsWith("m_LocalEulerAnglesHint") || x.currentValue.propertyPath.StartsWith("m_LocalRotation")))
			{
				TimelineAnimationUtilities.RigidTransform localToTrack = TimelineRecording.GetLocalToTrack(track, clip);
				if (localToTrack.rotation != Quaternion.get_identity())
				{
					if (TimelineRecording.s_LastTrackWarning != track)
					{
						TimelineRecording.s_LastTrackWarning = track;
						Debug.LogWarning("You are recording with an initial rotation offset. This may result in a misrepresentation of euler angles. When recording transform properties, it is recommended to reset rotation prior to recording");
					}
					Transform transform = mods[0].currentValue.target as Transform;
					if (transform != null)
					{
						TimelineAnimationUtilities.RigidTransform rigidTransform = TimelineAnimationUtilities.RigidTransform.Inverse(localToTrack);
						IEnumerable<UndoPropertyModification> first = from x in mods
						where !x.currentValue.propertyPath.StartsWith("m_LocalEulerAnglesHint")
						select x;
						IEnumerable<UndoPropertyModification> second = TimelineRecording.FindBestEulerHint(rigidTransform.rotation * transform.get_localRotation(), animClip, time, transform);
						result = first.Union(second).ToArray<UndoPropertyModification>();
						return result;
					}
					result = (from x in mods
					where !x.currentValue.propertyPath.StartsWith("m_LocalEulerAnglesHint")
					select x).ToArray<UndoPropertyModification>();
					return result;
				}
			}
			result = mods;
			return result;
		}

		internal static IEnumerable<UndoPropertyModification> FindBestEulerHint(Quaternion rotation, AnimationClip clip, float time, Transform transform)
		{
			Vector3 vector = rotation.get_eulerAngles();
			AnimationCurve editorCurve = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(string.Empty, typeof(Transform), "localEulerAnglesRaw.x"));
			AnimationCurve editorCurve2 = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(string.Empty, typeof(Transform), "localEulerAnglesRaw.y"));
			AnimationCurve editorCurve3 = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve(string.Empty, typeof(Transform), "localEulerAnglesRaw.z"));
			if (editorCurve != null)
			{
				vector.x = editorCurve.Evaluate(time);
			}
			if (editorCurve2 != null)
			{
				vector.y = editorCurve2.Evaluate(time);
			}
			if (editorCurve3 != null)
			{
				vector.z = editorCurve3.Evaluate(time);
			}
			vector = QuaternionCurveTangentCalculation.GetEulerFromQuaternion(rotation, vector);
			return new UndoPropertyModification[]
			{
				TimelineRecording.PropertyModificationToUndoPropertyModification(new PropertyModification
				{
					target = transform,
					propertyPath = "m_LocalEulerAnglesHint.x",
					value = vector.x.ToString()
				}),
				TimelineRecording.PropertyModificationToUndoPropertyModification(new PropertyModification
				{
					target = transform,
					propertyPath = "m_LocalEulerAnglesHint.y",
					value = vector.y.ToString()
				}),
				TimelineRecording.PropertyModificationToUndoPropertyModification(new PropertyModification
				{
					target = transform,
					propertyPath = "m_LocalEulerAnglesHint.z",
					value = vector.z.ToString()
				})
			};
		}

		internal static bool HasAnyPlayableAssetModifications(UndoPropertyModification[] modifications)
		{
			return modifications.Any((UndoPropertyModification x) => TimelineRecording.GetTarget(x) is IPlayableAsset);
		}

		internal static UndoPropertyModification[] ProcessPlayableAssetModification(UndoPropertyModification[] modifications, TimelineWindow.TimelineState state)
		{
			UndoPropertyModification[] result;
			if (state == null || state.currentDirector == null)
			{
				result = modifications;
			}
			else
			{
				List<UndoPropertyModification> list = new List<UndoPropertyModification>();
				for (int i = 0; i < modifications.Length; i++)
				{
					UndoPropertyModification undoPropertyModification = modifications[i];
					TimelineClip timelineClip = TimelineRecording.FindClipWithAsset(state.timeline, TimelineRecording.GetTarget(undoPropertyModification) as IPlayableAsset, state.currentDirector);
					if (timelineClip == null || !TimelineRecording.IsRecording(timelineClip, state) || !TimelineRecording.ProcessPlayableAssetRecording(undoPropertyModification, state, timelineClip))
					{
						list.Add(undoPropertyModification);
					}
				}
				if (list.Count<UndoPropertyModification>() != modifications.Length)
				{
					state.rebuildGraph = true;
					state.GetWindow().Repaint();
				}
				result = list.ToArray();
			}
			return result;
		}

		internal static TimelineClip FindClipWithAsset(TimelineAsset asset, IPlayableAsset target, PlayableDirector director)
		{
			TimelineClip result;
			if (target == null || asset == null || director == null)
			{
				result = null;
			}
			else
			{
				IEnumerable<TimelineClip> source = asset.flattenedTracks.SelectMany((TrackAsset x) => x.clips);
				result = source.FirstOrDefault((TimelineClip x) => x != null && x.asset != null && target == x.asset as IPlayableAsset);
			}
			return result;
		}

		internal static bool IsRecording(TimelineClip clip, TimelineWindow.TimelineState state)
		{
			return clip != null && clip.parentTrack != null && state.IsArmedForRecord(clip.parentTrack);
		}

		internal static bool ProcessPlayableAssetRecording(UndoPropertyModification mod, TimelineWindow.TimelineState state, TimelineClip clip)
		{
			bool result;
			if (mod.currentValue == null)
			{
				result = false;
			}
			else if (!clip.IsParameterAnimatable(mod.currentValue.propertyPath))
			{
				result = false;
			}
			else
			{
				double num = clip.ToLocalTimeUnbound(state.time);
				if (num < 0.0)
				{
					result = false;
				}
				else
				{
					float value = 0f;
					if (!float.TryParse(mod.currentValue.value, out value))
					{
						bool flag = false;
						if (!bool.TryParse(mod.currentValue.value, out flag))
						{
							result = false;
							return result;
						}
						value = (float)((!flag) ? 0 : 1);
					}
					bool flag2 = clip.AddAnimatedParameterValueAt(mod.currentValue.propertyPath, value, (float)num);
					if (flag2 && AnimationMode.InAnimationMode())
					{
						EditorCurveBinding curveBinding = clip.GetCurveBinding(mod.previousValue.propertyPath);
						AnimationMode.AddPropertyModification(curveBinding, mod.previousValue, true);
						clip.parentTrack.SetShowInlineCurves(true);
						if (state.GetWindow() != null && state.GetWindow().treeView != null)
						{
							state.GetWindow().treeView.CalculateRowRects();
						}
					}
					result = flag2;
				}
			}
			return result;
		}

		private static bool IsPlayableAssetProperty(SerializedProperty property)
		{
			return property.get_serializedObject().get_targetObject() is IPlayableAsset;
		}
	}
}
