using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal static class AnimatedParameterExtensions
	{
		private static readonly string kDefaultClipName = "Parameters";

		private static SerializedObject s_CachedObject;

		private static SerializedObject GetSerializedObject(TimelineClip clip)
		{
			SerializedObject result;
			if (clip == null)
			{
				result = null;
			}
			else if (!(clip.asset is IPlayableAsset))
			{
				result = null;
			}
			else
			{
				ScriptableObject scriptableObject = clip.asset as ScriptableObject;
				if (scriptableObject == null)
				{
					result = null;
				}
				else
				{
					if (AnimatedParameterExtensions.s_CachedObject == null || AnimatedParameterExtensions.s_CachedObject.get_targetObject() != clip.asset)
					{
						AnimatedParameterExtensions.s_CachedObject = new SerializedObject(scriptableObject);
					}
					result = AnimatedParameterExtensions.s_CachedObject;
				}
			}
			return result;
		}

		private static bool IsKeyable(Type t, string parameterName)
		{
			string name = parameterName;
			int num = parameterName.IndexOf('.');
			if (num > 0)
			{
				name = parameterName.Substring(0, num);
			}
			FieldInfo fieldInfo = t.GetField(name) ?? t.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
			return fieldInfo == null || !fieldInfo.IsDefined(typeof(NotKeyableAttribute), true);
		}

		public static bool IsAnimatable(SerializedPropertyType t)
		{
			bool result;
			switch (t)
			{
			case 1:
			case 2:
			case 4:
			case 8:
			case 9:
			case 10:
				goto IL_3E;
			case 3:
			case 5:
			case 6:
			case 7:
				IL_31:
				if (t != 17)
				{
					result = false;
					return result;
				}
				goto IL_3E;
			}
			goto IL_31;
			IL_3E:
			result = true;
			return result;
		}

		private static bool MatchBinding(EditorCurveBinding binding, string parameterName)
		{
			bool result;
			if (binding.propertyName == parameterName)
			{
				result = true;
			}
			else
			{
				int num = binding.propertyName.IndexOf('.');
				result = (num > 0 && parameterName.Length == num && binding.propertyName.StartsWith(parameterName));
			}
			return result;
		}

		public static bool HasAnyAnimatableParameters(this TimelineClip clip)
		{
			bool result;
			if (clip.asset == null || Attribute.IsDefined(clip.asset.GetType(), typeof(NotKeyableAttribute)))
			{
				result = false;
			}
			else if (!clip.HasScriptPlayable())
			{
				result = false;
			}
			else
			{
				SerializedObject serializedObject = AnimatedParameterExtensions.GetSerializedObject(clip);
				if (serializedObject == null)
				{
					result = false;
				}
				else
				{
					SerializedProperty iterator = serializedObject.GetIterator();
					bool flag = true;
					bool flag2 = clip.asset is IPlayableBehaviour;
					bool flag3 = false;
					while (iterator.NextVisible(flag))
					{
						if (AnimatedParameterExtensions.IsAnimatable(iterator.get_propertyType()) && AnimatedParameterExtensions.IsKeyable(clip.asset.GetType(), iterator.get_propertyPath()))
						{
							flag3 |= (flag2 || clip.IsAnimatablePath(iterator.get_propertyPath()));
						}
					}
					result = flag3;
				}
			}
			return result;
		}

		public static bool IsParameterAnimatable(this TimelineClip clip, string parameterName)
		{
			bool result;
			if (clip.asset == null || Attribute.IsDefined(clip.asset.GetType(), typeof(NotKeyableAttribute)))
			{
				result = false;
			}
			else if (!clip.HasScriptPlayable())
			{
				result = false;
			}
			else
			{
				SerializedObject serializedObject = AnimatedParameterExtensions.GetSerializedObject(clip);
				if (serializedObject == null)
				{
					result = false;
				}
				else
				{
					bool flag = clip.asset is IPlayableBehaviour;
					SerializedProperty serializedProperty = serializedObject.FindProperty(parameterName);
					result = (serializedProperty != null && AnimatedParameterExtensions.IsAnimatable(serializedProperty.get_propertyType()) && AnimatedParameterExtensions.IsKeyable(clip.asset.GetType(), parameterName) && (flag || clip.IsAnimatablePath(serializedProperty.get_propertyPath())));
				}
			}
			return result;
		}

		public static bool IsParameterAnimated(this TimelineClip clip, string parameterName)
		{
			bool result;
			if (clip == null)
			{
				result = false;
			}
			else if (clip.curves == null)
			{
				result = false;
			}
			else
			{
				EditorCurveBinding binding = clip.GetCurveBinding(parameterName);
				EditorCurveBinding[] bindings = AnimationClipCurveCache.Instance.GetCurveInfo(clip.curves).bindings;
				result = bindings.Any((EditorCurveBinding x) => AnimatedParameterExtensions.MatchBinding(x, binding.propertyName));
			}
			return result;
		}

		public static EditorCurveBinding GetCurveBinding(this TimelineClip clip, string parameterName)
		{
			string animatedParameterBindingName = AnimatedParameterExtensions.GetAnimatedParameterBindingName(clip, parameterName);
			return EditorCurveBinding.FloatCurve(string.Empty, AnimatedParameterExtensions.GetAnimationType(clip), animatedParameterBindingName);
		}

		private static Type GetAnimationType(TimelineClip clip)
		{
			Type result;
			if (clip != null && clip.asset != null && clip.asset != null)
			{
				result = clip.asset.GetType();
			}
			else
			{
				result = typeof(TimelineAsset);
			}
			return result;
		}

		private static string GetAnimatedParameterBindingName(TimelineClip clip, string parameterName)
		{
			string result;
			if (clip == null || clip.asset == null || clip.asset is IPlayableBehaviour)
			{
				result = parameterName;
			}
			else
			{
				IEnumerable<FieldInfo> scriptPlayableFields = AnimatedParameterExtensions.GetScriptPlayableFields(clip.asset as IPlayableAsset);
				foreach (FieldInfo current in scriptPlayableFields)
				{
					if (parameterName.StartsWith(current.Name))
					{
						if (parameterName.Length > current.Name.Length && parameterName[current.Name.Length] == '.')
						{
							result = parameterName.Substring(current.Name.Length + 1);
							return result;
						}
					}
				}
				result = parameterName;
			}
			return result;
		}

		public static bool AddAnimatedParameterValueAt(this TimelineClip clip, string parameterName, float value, float time)
		{
			bool result;
			if (!clip.IsParameterAnimatable(parameterName))
			{
				result = false;
			}
			else
			{
				AnimatedParameterExtensions.CreateCurvesIfRequired(clip, null);
				EditorCurveBinding curveBinding = clip.GetCurveBinding(parameterName);
				AnimationCurve animationCurve = AnimationUtility.GetEditorCurve(clip.curves, curveBinding) ?? new AnimationCurve();
				SerializedObject serializedObject = AnimatedParameterExtensions.GetSerializedObject(clip);
				SerializedProperty serializedProperty = serializedObject.FindProperty(parameterName);
				bool stepped = serializedProperty.get_propertyType() == 1 || serializedProperty.get_propertyType() == null || serializedProperty.get_propertyType() == 7;
				CurveEditUtility.AddKeyFrameToCurve(animationCurve, time, clip.curves.get_frameRate(), value, stepped);
				AnimationUtility.SetEditorCurve(clip.curves, curveBinding, animationCurve);
				result = true;
			}
			return result;
		}

		internal static void CreateCurvesIfRequired(TimelineClip clip, TrackAsset parentTrack = null)
		{
			if (clip.curves == null)
			{
				if (parentTrack == null)
				{
					parentTrack = clip.parentTrack;
				}
				clip.AllocateAnimatedParameterCurves();
				clip.curves.set_name(AnimationTrackRecorder.GetUniqueRecordedClipName(clip.parentTrack, AnimatedParameterExtensions.kDefaultClipName));
				string assetPath = AssetDatabase.GetAssetPath(clip.parentTrack);
				if (!string.IsNullOrEmpty(assetPath))
				{
					TimelineHelpers.SaveAnimClipIntoObject(clip.curves, clip.parentTrack);
					EditorUtility.SetDirty(clip.parentTrack);
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(clip.parentTrack));
					AssetDatabase.Refresh();
				}
			}
		}

		private static bool InternalAddParameter(TimelineClip clip, string parameterName, ref EditorCurveBinding binding, out SerializedProperty property)
		{
			property = null;
			bool result;
			if (clip.IsParameterAnimated(parameterName))
			{
				result = false;
			}
			else
			{
				SerializedObject serializedObject = AnimatedParameterExtensions.GetSerializedObject(clip);
				if (serializedObject == null)
				{
					result = false;
				}
				else
				{
					property = serializedObject.FindProperty(parameterName);
					if (property == null || !AnimatedParameterExtensions.IsAnimatable(property.get_propertyType()))
					{
						result = false;
					}
					else
					{
						AnimatedParameterExtensions.CreateCurvesIfRequired(clip, null);
						binding = clip.GetCurveBinding(parameterName);
						result = true;
					}
				}
			}
			return result;
		}

		public static bool AddAnimatedParameter(this TimelineClip clip, string parameterName)
		{
			EditorCurveBinding sourceBinding = default(EditorCurveBinding);
			SerializedProperty prop;
			bool result;
			if (!AnimatedParameterExtensions.InternalAddParameter(clip, parameterName, ref sourceBinding, out prop))
			{
				result = false;
			}
			else
			{
				float num = (float)clip.duration;
				CurveEditUtility.AddKey(clip.curves, sourceBinding, prop, 0.0);
				CurveEditUtility.AddKey(clip.curves, sourceBinding, prop, (double)num);
				result = true;
			}
			return result;
		}

		public static bool RemoveAnimatedParameter(this TimelineClip clip, string parameterName)
		{
			bool result;
			if (!clip.IsParameterAnimated(parameterName) || clip.curves == null)
			{
				result = false;
			}
			else
			{
				EditorCurveBinding curveBinding = clip.GetCurveBinding(parameterName);
				AnimationUtility.SetEditorCurve(clip.curves, curveBinding, null);
				result = true;
			}
			return result;
		}

		public static AnimationCurve GetAnimatedParameter(this TimelineClip clip, string parameterName)
		{
			AnimationCurve result;
			if (clip == null || clip.curves == null)
			{
				result = null;
			}
			else
			{
				ScriptableObject scriptableObject = clip.asset as ScriptableObject;
				if (scriptableObject == null)
				{
					result = null;
				}
				else
				{
					EditorCurveBinding curveBinding = clip.GetCurveBinding(parameterName);
					result = AnimationUtility.GetEditorCurve(clip.curves, curveBinding);
				}
			}
			return result;
		}

		public static bool SetAnimatedParameter(this TimelineClip clip, string parameterName, AnimationCurve curve)
		{
			bool result;
			if (!clip.IsParameterAnimated(parameterName) && !clip.AddAnimatedParameter(parameterName))
			{
				result = false;
			}
			else
			{
				EditorCurveBinding curveBinding = clip.GetCurveBinding(parameterName);
				AnimationUtility.SetEditorCurve(clip.curves, curveBinding, curve);
				result = true;
			}
			return result;
		}

		internal static bool HasScriptPlayable(this TimelineClip clip)
		{
			bool result;
			if (clip.asset == null)
			{
				result = false;
			}
			else
			{
				IPlayableBehaviour playableBehaviour = clip.asset as IPlayableBehaviour;
				result = (playableBehaviour != null || AnimatedParameterExtensions.GetScriptPlayableFields(clip.asset as IPlayableAsset).Any<FieldInfo>());
			}
			return result;
		}

		internal static bool IsAnimatablePath(this TimelineClip clip, string path)
		{
			return !(clip.asset == null) && AnimatedParameterExtensions.GetScriptPlayableFields(clip.asset as IPlayableAsset).Any((FieldInfo f) => path.StartsWith(f.Name) && path.Length > f.Name.Length && path[f.Name.Length] == '.');
		}

		internal static IEnumerable<FieldInfo> GetScriptPlayableFields(IPlayableAsset asset)
		{
			IEnumerable<FieldInfo> result;
			if (asset == null)
			{
				result = new FieldInfo[0];
			}
			else
			{
				result = from f in asset.GetType().GetFields()
				where f.IsPublic && !f.IsStatic && typeof(IPlayableBehaviour).IsAssignableFrom(f.FieldType)
				select f;
			}
			return result;
		}
	}
}
