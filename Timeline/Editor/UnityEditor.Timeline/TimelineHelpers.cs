using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal static class TimelineHelpers
	{
		public static readonly TrackType GroupTrackType = new TrackType(typeof(GroupTrack), TimelineAsset.MediaType.Group);

		private static TrackType[] s_CachedMixableTypes;

		private static Type[] s_StandaloneAssetTypes;

		private static List<Type> s_SubClassesOfTrackDrawer;

		public static Vector2 sInvalidMousePosition = new Vector2(float.PositiveInfinity, float.PositiveInfinity);

		[CompilerGenerated]
		private static Func<TrackType, IEnumerable<Type>> <>f__mg$cache0;

		private static ScriptableObject CloneReferencedPlayableAsset(ScriptableObject original, PlayableDirector directorInstance)
		{
			ScriptableObject scriptableObject = Object.Instantiate<ScriptableObject>(original);
			if (scriptableObject == null || !(scriptableObject is IPlayableAsset))
			{
				throw new InvalidCastException("could not cast instantiated object into IPlayableAsset");
			}
			if (directorInstance != null)
			{
				SerializedObject serializedObject = new SerializedObject(original);
				SerializedObject serializedObject2 = new SerializedObject(scriptableObject);
				SerializedProperty iterator = serializedObject.GetIterator();
				if (iterator.Next(true))
				{
					do
					{
						serializedObject2.CopyFromSerializedProperty(iterator);
					}
					while (iterator.Next(false));
				}
				serializedObject2.ApplyModifiedProperties();
				EditorUtility.SetDirty(directorInstance);
			}
			if (directorInstance != null)
			{
				List<FieldInfo> list = (from f in scriptableObject.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
				where f.FieldType.IsGenericType && f.FieldType.GetGenericTypeDefinition() == typeof(ExposedReference)
				select f).ToList<FieldInfo>();
				foreach (FieldInfo current in list)
				{
					object value = current.GetValue(scriptableObject);
					FieldInfo field = value.GetType().GetField("exposedName");
					if (field != null)
					{
						PropertyName propertyName = (PropertyName)field.GetValue(value);
						bool flag = false;
						Object referenceValue = directorInstance.GetReferenceValue(propertyName, ref flag);
						if (flag)
						{
							PropertyName propertyName2 = new PropertyName(GUID.Generate().ToString());
							directorInstance.SetReferenceValue(propertyName2, referenceValue);
							field.SetValue(value, propertyName2);
						}
					}
					current.SetValue(scriptableObject, value);
				}
			}
			IEnumerable<FieldInfo> enumerable = from f in scriptableObject.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			where !f.IsNotSerialized && f.FieldType == typeof(AnimationClip)
			select f;
			foreach (FieldInfo current2 in enumerable)
			{
				current2.SetValue(scriptableObject, TimelineHelpers.CloneAnimationClipIfRequired(current2.GetValue(scriptableObject) as AnimationClip, original));
			}
			return scriptableObject;
		}

		private static void SaveCloneToOriginalAsset(Object original, Object clone)
		{
			string assetPath = AssetDatabase.GetAssetPath(original);
			Object @object = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
			if (@object != null)
			{
				TimelineCreateUtilities.SaveAssetIntoObject(clone, @object);
				EditorUtility.SetDirty(@object);
			}
		}

		private static AnimationClip CloneAnimationClipIfRequired(AnimationClip clip, Object owner)
		{
			AnimationClip result;
			if (clip == null)
			{
				result = null;
			}
			else
			{
				string assetPath = AssetDatabase.GetAssetPath(clip);
				string assetPath2 = AssetDatabase.GetAssetPath(owner);
				bool flag = assetPath == assetPath2;
				if (flag)
				{
					AnimationClip animationClip = Object.Instantiate<AnimationClip>(clip);
					animationClip.set_name(AnimationTrackRecorder.GetUniqueRecordedClipName(owner, clip.get_name()));
					animationClip.set_hideFlags(clip.get_hideFlags());
					if ((clip.get_hideFlags() & 52) != 52 && assetPath2.Length > 0)
					{
						TimelineHelpers.SaveAnimClipIntoObject(animationClip, owner);
					}
					EditorUtility.SetDirty(owner);
					clip = animationClip;
				}
				result = clip;
			}
			return result;
		}

		public static TimelineClip Clone(TimelineClip clip, PlayableDirector directorInstance)
		{
			EditorClip editorClip = EditorItemFactory.GetEditorClip(clip);
			TimelineClip clip2 = Object.Instantiate<EditorClip>(editorClip).clip;
			SelectionManager.Remove(clip2);
			clip2.parentTrack = null;
			clip2.ClearAnimatedParameterCurves();
			if (clip.curves != null)
			{
				AnimatedParameterExtensions.CreateCurvesIfRequired(clip2, clip.parentTrack);
				EditorUtility.CopySerialized(clip.curves, clip2.curves);
			}
			ScriptableObject scriptableObject = clip2.asset as ScriptableObject;
			if (scriptableObject != null && clip2.asset is IPlayableAsset)
			{
				ScriptableObject scriptableObject2 = TimelineHelpers.CloneReferencedPlayableAsset(scriptableObject, directorInstance);
				TimelineHelpers.SaveCloneToOriginalAsset(scriptableObject, scriptableObject2);
				clip2.asset = scriptableObject2;
				AnimationPlayableAsset animationPlayableAsset = scriptableObject2 as AnimationPlayableAsset;
				if (clip2.recordable && animationPlayableAsset != null && animationPlayableAsset.clip != null)
				{
					clip2.displayName = animationPlayableAsset.clip.get_name();
				}
			}
			return clip2;
		}

		public static TimelineMarker Clone(TimelineMarker theMarker)
		{
			return new TimelineMarker(theMarker.key, theMarker.time, null);
		}

		public static TrackAsset Clone(PlayableAsset parent, TrackAsset trackAsset, PlayableDirector directorInstance)
		{
			TrackAsset result;
			if (trackAsset == null)
			{
				result = null;
			}
			else
			{
				TimelineAsset timelineAsset = trackAsset.timelineAsset;
				if (timelineAsset == null)
				{
					result = null;
				}
				else
				{
					TrackAsset trackAsset2 = Object.Instantiate<TrackAsset>(trackAsset);
					trackAsset2.SetClips(new List<TimelineClip>());
					trackAsset2.parent = parent;
					trackAsset2.subTracks = new List<TrackAsset>();
					string[] array = (from x in timelineAsset.flattenedTracks
					select x.get_name()).ToArray<string>();
					trackAsset2.set_name(ObjectNames.GetUniqueName(array, trackAsset.get_name()));
					if (trackAsset.animClip != null)
					{
						trackAsset2.animClip = TimelineHelpers.CloneAnimationClipIfRequired(trackAsset.animClip, trackAsset);
					}
					TimelineClip[] clips = trackAsset.clips;
					for (int i = 0; i < clips.Length; i++)
					{
						TimelineClip clip = clips[i];
						TimelineClip timelineClip = TimelineHelpers.Clone(clip, directorInstance);
						timelineClip.parentTrack = trackAsset2;
						trackAsset2.AddClip(timelineClip);
					}
					trackAsset2.SetCollapsed(trackAsset.GetCollapsed());
					if (SelectionManager.Contains(trackAsset))
					{
						SelectionManager.Remove(trackAsset);
						SelectionManager.Add(trackAsset2);
					}
					result = trackAsset2;
				}
			}
			return result;
		}

		public static bool IsCircularRef(TimelineAsset baseSeq, TimelineAsset other)
		{
			bool result;
			if (baseSeq == other)
			{
				result = true;
			}
			else
			{
				foreach (TrackAsset current in other.flattenedTracks)
				{
					TimelineClip[] clips = current.clips;
					for (int i = 0; i < clips.Length; i++)
					{
						TimelineClip timelineClip = clips[i];
						if (timelineClip.asset == baseSeq)
						{
							result = true;
							return result;
						}
					}
				}
				result = false;
			}
			return result;
		}

		public static TrackType[] GetMixableTypes()
		{
			TrackType[] result;
			if (TimelineHelpers.s_CachedMixableTypes != null)
			{
				result = TimelineHelpers.s_CachedMixableTypes;
			}
			else
			{
				TimelineHelpers.s_CachedMixableTypes = (from x in EditorAssemblies.get_loadedTypes()
				where !x.IsAbstract && typeof(TrackAsset).IsAssignableFrom(x)
				select x into t
				select new TrackType(t, TimelineHelpers.GetMediaTypeFromType(t))).ToArray<TrackType>();
				result = TimelineHelpers.s_CachedMixableTypes;
			}
			return result;
		}

		public static bool IsTypeSupportedByTrack(TrackType trackType, Type objectType)
		{
			TrackType[] trackTypeHandle = TimelineHelpers.GetTrackTypeHandle(objectType);
			return trackTypeHandle.Contains(trackType);
		}

		public static TimelineAsset.MediaType GetMediaTypeFromType(Type type)
		{
			object[] customAttributes = type.GetCustomAttributes(typeof(TrackMediaType), true);
			return (!customAttributes.Any<object>()) ? TimelineAsset.MediaType.Script : ((TrackMediaType)customAttributes[0]).m_MediaType;
		}

		public static TrackType[] GetTrackTypeHandle(Type toBeHandled)
		{
			Type[] array = (from assemblyType in EditorAssemblies.get_loadedTypes()
			where assemblyType.IsSubclassOf(typeof(TrackAsset))
			select assemblyType).ToArray<Type>();
			List<TrackType> list = new List<TrackType>();
			Type[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				Type type = array2[i];
				object[] customAttributes = type.GetCustomAttributes(typeof(TrackClipTypeAttribute), true);
				object[] array3 = customAttributes;
				for (int j = 0; j < array3.Length; j++)
				{
					object obj = array3[j];
					Type inspectedType = ((TrackClipTypeAttribute)obj).inspectedType;
					if (inspectedType == toBeHandled || inspectedType.IsAssignableFrom(toBeHandled))
					{
						TrackType item = new TrackType(type, TimelineHelpers.GetMediaTypeFromType(type));
						list.Add(item);
					}
				}
			}
			if (toBeHandled == typeof(MonoScript))
			{
				list.Add(new TrackType(typeof(PlayableTrack), TimelineHelpers.GetMediaTypeFromType(typeof(PlayableTrack))));
			}
			return list.ToArray();
		}

		public static IEnumerable<Type> GetTypesHandledByTrackType(TrackType trackType)
		{
			object[] customAttributes = trackType.trackType.GetCustomAttributes(typeof(TrackClipTypeAttribute), true);
			return from a in customAttributes
			select ((TrackClipTypeAttribute)a).inspectedType;
		}

		public static Type[] GetAllStandalonePlayableAssets()
		{
			IEnumerable<TrackType> arg_23_0 = TimelineHelpers.GetMixableTypes();
			if (TimelineHelpers.<>f__mg$cache0 == null)
			{
				TimelineHelpers.<>f__mg$cache0 = new Func<TrackType, IEnumerable<Type>>(TimelineHelpers.GetTypesHandledByTrackType);
			}
			IEnumerable<Type> second = arg_23_0.SelectMany(TimelineHelpers.<>f__mg$cache0);
			IEnumerable<Type> first = from assemblyType in EditorAssemblies.get_loadedTypes()
			where typeof(IPlayableAsset).IsAssignableFrom(assemblyType) && typeof(ScriptableObject).IsAssignableFrom(assemblyType) && assemblyType.Assembly.FullName.Contains("Assembly-CSharp")
			select assemblyType;
			TimelineHelpers.s_StandaloneAssetTypes = first.Except(second).ToArray<Type>();
			return TimelineHelpers.s_StandaloneAssetTypes;
		}

		public static TrackType TrackTypeFromType(Type t)
		{
			return new TrackType(t, TimelineHelpers.GetMediaTypeFromType(t));
		}

		public static Type GetCustomDrawer(Type trackType)
		{
			if (TimelineHelpers.s_SubClassesOfTrackDrawer == null)
			{
				TimelineHelpers.s_SubClassesOfTrackDrawer = EditorAssemblies.SubclassesOf(typeof(TrackDrawer)).ToList<Type>();
			}
			Type result;
			foreach (Type current in TimelineHelpers.s_SubClassesOfTrackDrawer)
			{
				CustomTrackDrawerAttribute customTrackDrawerAttribute = Attribute.GetCustomAttribute(current, typeof(CustomTrackDrawerAttribute), false) as CustomTrackDrawerAttribute;
				if (customTrackDrawerAttribute != null && customTrackDrawerAttribute.assetType == trackType)
				{
					result = current;
					return result;
				}
			}
			result = typeof(TrackDrawer);
			return result;
		}

		public static string GenerateUniqueActorName(TimelineAsset timeline, string prefix)
		{
			string result;
			if (!timeline.tracks.Exists((TrackAsset x) => x.get_name() == prefix))
			{
				result = prefix;
			}
			else
			{
				int num = 1;
				string newName = prefix + num;
				while (timeline.tracks.Exists((TrackAsset x) => x.get_name() == newName))
				{
					num++;
					newName = prefix + num;
				}
				result = newName;
			}
			return result;
		}

		public static bool HaveSameContainerAsset(Object assetA, Object assetB)
		{
			return !(assetA == null) && !(assetB == null) && (((assetA.get_hideFlags() & 52) != null && (assetB.get_hideFlags() & 52) != null) || AssetDatabase.GetAssetPath(assetA) == AssetDatabase.GetAssetPath(assetB));
		}

		public static void SaveAnimClipIntoObject(AnimationClip clip, Object asset)
		{
			if ((asset.get_hideFlags() & 52) != null)
			{
				clip.set_hideFlags(clip.get_hideFlags() | 52);
			}
			else
			{
				AssetDatabase.AddObjectToAsset(clip, asset);
			}
		}

		public static TrackAsset GetGroup(object o)
		{
			TrackAsset result;
			if (o == null)
			{
				result = null;
			}
			else
			{
				TrackAsset trackAsset = o as TrackAsset;
				TimelineGroupGUI timelineGroupGUI = o as TimelineGroupGUI;
				if (trackAsset == null)
				{
					if (timelineGroupGUI != null)
					{
						if (timelineGroupGUI.track.GetType() == TimelineHelpers.GroupTrackType.trackType)
						{
							result = timelineGroupGUI.track;
							return result;
						}
						trackAsset = (timelineGroupGUI.track.parent as TrackAsset);
					}
				}
				while (trackAsset != null)
				{
					if (trackAsset.GetType() == TimelineHelpers.GroupTrackType.trackType)
					{
						result = trackAsset;
						return result;
					}
					trackAsset = (trackAsset.parent as TrackAsset);
				}
				result = null;
			}
			return result;
		}

		public static Component AddRequiredComponent(GameObject go, TrackAsset asset)
		{
			Component result;
			if (go == null || asset == null)
			{
				result = null;
			}
			else
			{
				IEnumerable<PlayableBinding> outputs = asset.get_outputs();
				if (!outputs.Any<PlayableBinding>())
				{
					result = null;
				}
				else
				{
					PlayableBinding playableBinding = outputs.First<PlayableBinding>();
					if (playableBinding.get_streamType() == null)
					{
						Animator animator = go.GetComponent<Animator>();
						if (animator == null)
						{
							animator = Undo.AddComponent<Animator>(go);
							animator.set_applyRootMotion(true);
						}
						result = animator;
					}
					else if (playableBinding.get_streamType() == 1)
					{
						AudioSource audioSource = go.GetComponent<AudioSource>();
						if (audioSource == null)
						{
							audioSource = Undo.AddComponent<AudioSource>(go);
						}
						result = audioSource;
					}
					else if (playableBinding.get_streamType() == 3 && typeof(Component).IsAssignableFrom(playableBinding.get_sourceBindingType()))
					{
						Component component = go.GetComponent(playableBinding.get_sourceBindingType());
						if (component == null)
						{
							component = Undo.AddComponent(go, playableBinding.get_sourceBindingType());
						}
						result = component;
					}
					else
					{
						result = null;
					}
				}
			}
			return result;
		}

		public static double GetTrackEndTime(TrackAsset track)
		{
			double num = 0.0;
			TimelineClip[] clips = track.clips;
			for (int i = 0; i < clips.Length; i++)
			{
				TimelineClip timelineClip = clips[i];
				if (timelineClip != null && !double.IsPositiveInfinity(timelineClip.duration))
				{
					num = Math.Max(num, timelineClip.start + timelineClip.duration);
				}
			}
			return num;
		}

		public static double FindBestInsertionTime(ITimelineState state, TimelineClip clip, TrackAsset track)
		{
			float num = state.TimeToTimeAreaPixel(state.time);
			return TimelineHelpers.FindBestInsertionTime((TimelineWindow.TimelineState)state, clip, track, new Vector2(num, num));
		}

		public static double FindBestInsertionTime(ITimelineState state, TimelineClip clip, TrackAsset track, Vector2 mousePosition)
		{
			double droppedTime = state.SnapToFrameIfRequired((double)state.ScreenSpacePixelToTimeAreaTime(mousePosition.x));
			TimelineClip timelineClip = (from c in track.clips
			where c != clip && c.start - TimeUtility.kTimeEpsilon <= droppedTime
			orderby c.start
			select c).LastOrDefault<TimelineClip>();
			double result;
			if (timelineClip != null)
			{
				double num = timelineClip.start + timelineClip.duration;
				double num2 = (double)state.TimeAreaPixelToTime(0f);
				if (num < num2)
				{
					result = droppedTime;
				}
				else if (!float.IsPositiveInfinity(mousePosition.x) && droppedTime > num)
				{
					result = droppedTime;
				}
				else
				{
					result = num;
				}
			}
			else
			{
				timelineClip = (from c in track.clips
				where c != clip
				orderby c.start + c.duration
				select c).LastOrDefault<TimelineClip>();
				if (timelineClip != null)
				{
					result = timelineClip.start + timelineClip.duration;
				}
				else
				{
					result = 0.0;
				}
			}
			return result;
		}

		public static string GetTrackCategoryName(TrackType trackType)
		{
			string result;
			if (trackType.trackType == null || trackType.trackType.Namespace == null)
			{
				result = "";
			}
			else if (trackType.trackType.Namespace.Contains("UnityEngine"))
			{
				result = "";
			}
			else
			{
				result = trackType.trackType.Namespace;
			}
			return result;
		}

		public static string GetTrackMenuName(TrackType trackType)
		{
			return ObjectNames.NicifyVariableName(trackType.trackType.Name);
		}

		public static double GetLoopDuration(TimelineClip clip)
		{
			double clipAssetDuration = clip.clipAssetDuration;
			double result;
			if (clipAssetDuration == 1.7976931348623157E+308 || double.IsInfinity(clipAssetDuration))
			{
				result = clipAssetDuration;
			}
			else
			{
				result = clipAssetDuration / clip.timeScale;
			}
			return result;
		}

		public static bool HasUsableAssetDuration(TimelineClip clip)
		{
			double clipAssetDuration = clip.clipAssetDuration;
			return clipAssetDuration < TimelineClip.kMaxTimeValue && !double.IsInfinity(clipAssetDuration);
		}

		public static double[] GetLoopTimes(TimelineClip clip)
		{
			double[] result;
			if (!TimelineHelpers.HasUsableAssetDuration(clip))
			{
				result = new double[]
				{
					-clip.clipIn / clip.timeScale
				};
			}
			else
			{
				List<double> list = new List<double>();
				double loopDuration = TimelineHelpers.GetLoopDuration(clip);
				if (loopDuration <= TimeUtility.kTimeEpsilon)
				{
					result = new double[0];
				}
				else
				{
					double num = -clip.clipIn / clip.timeScale;
					double num2 = num + loopDuration;
					list.Add(num);
					while (num2 < clip.duration - TimelineWindow.TimelineState.kTimeEpsilon)
					{
						list.Add(num2);
						num2 += loopDuration;
					}
					result = list.ToArray();
				}
			}
			return result;
		}

		public static TimelineClip CreateClipOnTrack(Object asset, TrackAsset parentTrack, ITimelineState state, Vector2 mousePosition)
		{
			double end = parentTrack.end;
			TimelineClip timelineClip = parentTrack.CreateClipFromAsset(asset);
			if (timelineClip != null)
			{
				SelectionManager.Clear();
				timelineClip.timeScale = 1.0;
				if (!float.IsPositiveInfinity(mousePosition.x) && !float.IsPositiveInfinity(mousePosition.y))
				{
					timelineClip.start = (double)state.ScreenSpacePixelToTimeAreaTime(mousePosition.x);
				}
				else
				{
					timelineClip.start = state.SnapToFrameIfRequired(end);
				}
				timelineClip.start = Math.Max(0.0, timelineClip.start);
				timelineClip.mixInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
				timelineClip.mixOutCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
				SelectionManager.Add(timelineClip);
				parentTrack.CalculateExtrapolationTimes();
				state.Refresh();
			}
			return timelineClip;
		}

		public static TimelineClip CreateClipOnTrack(Type playableAssetType, TrackAsset parentTrack, ITimelineState state)
		{
			return TimelineHelpers.CreateClipOnTrack(playableAssetType, parentTrack, state, TimelineHelpers.sInvalidMousePosition);
		}

		public static TimelineClip CreateClipOnTrack(Type playableAssetType, TrackAsset parentTrack, ITimelineState state, Vector2 mousePosition)
		{
			TimelineClip result;
			if (!typeof(IPlayableAsset).IsAssignableFrom(playableAssetType) || !typeof(ScriptableObject).IsAssignableFrom(playableAssetType))
			{
				result = null;
			}
			else
			{
				ScriptableObject scriptableObject = ScriptableObject.CreateInstance(playableAssetType);
				if (scriptableObject == null)
				{
					throw new InvalidOperationException("Could not create an instance of the ScriptableObject type " + playableAssetType.Name);
				}
				scriptableObject.set_name(playableAssetType.Name);
				TimelineCreateUtilities.SaveAssetIntoObject(scriptableObject, parentTrack);
				result = TimelineHelpers.CreateClipOnTrack(scriptableObject, parentTrack, state, mousePosition);
			}
			return result;
		}

		public static bool NudgeClip(TimelineClip clip, TimelineWindow.TimelineState state, double offset)
		{
			bool result;
			if (clip == null)
			{
				result = false;
			}
			else
			{
				TimelineUndo.PushUndo(clip.parentTrack, "Nudge Clip");
				if (state.frameSnap)
				{
					clip.start = TimeUtility.FromFrames(Math.Max((double)TimeUtility.ToFrames(clip.start, (double)state.frameRate) + offset, 0.0), (double)state.frameRate);
				}
				else
				{
					clip.start += Math.Max(offset / (double)state.frameRate, 0.0);
				}
				EditorUtility.SetDirty(clip.parentTrack);
				result = true;
			}
			return result;
		}
	}
}
