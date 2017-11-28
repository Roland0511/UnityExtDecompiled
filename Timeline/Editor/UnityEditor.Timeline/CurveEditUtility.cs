using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal static class CurveEditUtility
	{
		private static bool IsRotationKey(EditorCurveBinding binding)
		{
			return binding.propertyName.Contains("localEulerAnglesRaw");
		}

		public static void AddKey(AnimationClip clip, EditorCurveBinding sourceBinding, SerializedProperty prop, double time)
		{
			if (sourceBinding.get_isPPtrCurve())
			{
				CurveEditUtility.AddObjectKey(clip, sourceBinding, prop, time);
			}
			else if (CurveEditUtility.IsRotationKey(sourceBinding))
			{
				CurveEditUtility.AddRotationKey(clip, sourceBinding, prop, time);
			}
			else
			{
				CurveEditUtility.AddFloatKey(clip, sourceBinding, prop, time);
			}
		}

		private static void AddObjectKey(AnimationClip clip, EditorCurveBinding sourceBinding, SerializedProperty prop, double time)
		{
			if (prop.get_propertyType() == 5)
			{
				ObjectReferenceKeyframe[] array = null;
				AnimationClipCurveInfo curveInfo = AnimationClipCurveCache.Instance.GetCurveInfo(clip);
				int num = Array.IndexOf<EditorCurveBinding>(curveInfo.objectBindings, sourceBinding);
				if (num >= 0)
				{
					array = curveInfo.objectCurves[num];
					int num2 = CurveEditUtility.EvaluateIndex(array, (float)time);
					if (CurveEditUtility.KeyCompare(array[num2].time, (float)time, clip.get_frameRate()) == 0)
					{
						array[num2].value = prop.get_objectReferenceValue();
					}
					else if (num2 < array.Length - 1 && CurveEditUtility.KeyCompare(array[num2 + 1].time, (float)time, clip.get_frameRate()) == 0)
					{
						array[num2 + 1].value = prop.get_objectReferenceValue();
					}
					else
					{
						if (time > (double)array[0].time)
						{
							num2++;
						}
						ArrayUtility.Insert<ObjectReferenceKeyframe>(ref array, num2, new ObjectReferenceKeyframe
						{
							time = (float)time,
							value = prop.get_objectReferenceValue()
						});
					}
				}
				else
				{
					array = new ObjectReferenceKeyframe[1];
					array[0].time = (float)time;
					array[0].value = prop.get_objectReferenceValue();
				}
				AnimationUtility.SetObjectReferenceCurve(clip, sourceBinding, array);
				EditorUtility.SetDirty(clip);
			}
		}

		private static void AddRotationKey(AnimationClip clip, EditorCurveBinding sourceBind, SerializedProperty prop, double time)
		{
			if (prop.get_propertyType() == 17)
			{
				List<AnimationCurve> list = new List<AnimationCurve>();
				List<EditorCurveBinding> list2 = new List<EditorCurveBinding>();
				AnimationClipCurveInfo curveInfo = AnimationClipCurveCache.Instance.GetCurveInfo(clip);
				for (int i = 0; i < curveInfo.bindings.Length; i++)
				{
					if (sourceBind.get_type() == curveInfo.bindings[i].get_type())
					{
						if (curveInfo.bindings[i].propertyName.Contains("localEuler"))
						{
							list2.Add(curveInfo.bindings[i]);
							list.Add(curveInfo.curves[i]);
						}
					}
				}
				Vector3 localEulerAngles = ((Transform)prop.get_serializedObject().get_targetObject()).get_localEulerAngles();
				if (list2.Count == 0)
				{
					string propertyGroupName = AnimationWindowUtility.GetPropertyGroupName(sourceBind.propertyName);
					list2.Add(EditorCurveBinding.FloatCurve(sourceBind.path, sourceBind.get_type(), propertyGroupName + ".x"));
					list2.Add(EditorCurveBinding.FloatCurve(sourceBind.path, sourceBind.get_type(), propertyGroupName + ".y"));
					list2.Add(EditorCurveBinding.FloatCurve(sourceBind.path, sourceBind.get_type(), propertyGroupName + ".z"));
					AnimationCurve animationCurve = new AnimationCurve();
					AnimationCurve animationCurve2 = new AnimationCurve();
					AnimationCurve animationCurve3 = new AnimationCurve();
					CurveEditUtility.AddKeyFrameToCurve(animationCurve, (float)time, clip.get_frameRate(), localEulerAngles.x, false);
					CurveEditUtility.AddKeyFrameToCurve(animationCurve2, (float)time, clip.get_frameRate(), localEulerAngles.y, false);
					CurveEditUtility.AddKeyFrameToCurve(animationCurve3, (float)time, clip.get_frameRate(), localEulerAngles.z, false);
					list.Add(animationCurve);
					list.Add(animationCurve2);
					list.Add(animationCurve3);
				}
				for (int j = 0; j < list2.Count; j++)
				{
					char c = list2[j].propertyName.Last<char>();
					float value = localEulerAngles.x;
					if (c == 'y')
					{
						value = localEulerAngles.y;
					}
					else if (c == 'z')
					{
						value = localEulerAngles.z;
					}
					CurveEditUtility.AddKeyFrameToCurve(list[j], (float)time, clip.get_frameRate(), value, false);
				}
				CurveEditUtility.UpdateEditorCurves(clip, list2, list);
			}
		}

		private static void AddFloatKey(AnimationClip clip, EditorCurveBinding sourceBind, SerializedProperty prop, double time)
		{
			List<AnimationCurve> list = new List<AnimationCurve>();
			List<EditorCurveBinding> list2 = new List<EditorCurveBinding>();
			bool flag = false;
			AnimationClipCurveInfo curveInfo = AnimationClipCurveCache.Instance.GetCurveInfo(clip);
			for (int i = 0; i < curveInfo.bindings.Length; i++)
			{
				EditorCurveBinding item = curveInfo.bindings[i];
				if (item.get_type() == sourceBind.get_type())
				{
					SerializedProperty serializedProperty = null;
					AnimationCurve animationCurve = curveInfo.curves[i];
					if (prop.get_propertyPath().Equals(item.propertyName))
					{
						serializedProperty = prop;
					}
					else if (item.propertyName.Contains(prop.get_propertyPath()))
					{
						serializedProperty = prop.get_serializedObject().FindProperty(item.propertyName);
					}
					if (serializedProperty != null)
					{
						float keyValue = CurveEditUtility.GetKeyValue(serializedProperty);
						if (!float.IsNaN(keyValue))
						{
							flag = true;
							CurveEditUtility.AddKeyFrameToCurve(animationCurve, (float)time, clip.get_frameRate(), keyValue, serializedProperty.get_propertyType() == 1);
							list.Add(animationCurve);
							list2.Add(item);
						}
					}
				}
			}
			if (!flag)
			{
				string propertyGroupName = AnimationWindowUtility.GetPropertyGroupName(sourceBind.propertyName);
				if (!prop.get_hasChildren())
				{
					float keyValue2 = CurveEditUtility.GetKeyValue(prop);
					if (!float.IsNaN(keyValue2))
					{
						list2.Add(EditorCurveBinding.FloatCurve(sourceBind.path, sourceBind.get_type(), sourceBind.propertyName));
						AnimationCurve animationCurve2 = new AnimationCurve();
						CurveEditUtility.AddKeyFrameToCurve(animationCurve2, (float)time, clip.get_frameRate(), keyValue2, prop.get_propertyType() == 1);
						list.Add(animationCurve2);
					}
				}
				else if (prop.get_propertyType() == 4)
				{
					list2.Add(EditorCurveBinding.FloatCurve(sourceBind.path, sourceBind.get_type(), propertyGroupName + ".r"));
					list2.Add(EditorCurveBinding.FloatCurve(sourceBind.path, sourceBind.get_type(), propertyGroupName + ".g"));
					list2.Add(EditorCurveBinding.FloatCurve(sourceBind.path, sourceBind.get_type(), propertyGroupName + ".b"));
					list2.Add(EditorCurveBinding.FloatCurve(sourceBind.path, sourceBind.get_type(), propertyGroupName + ".a"));
					Color colorValue = prop.get_colorValue();
					for (int j = 0; j < 4; j++)
					{
						AnimationCurve animationCurve3 = new AnimationCurve();
						CurveEditUtility.AddKeyFrameToCurve(animationCurve3, (float)time, clip.get_frameRate(), colorValue.get_Item(j), prop.get_propertyType() == 1);
						list.Add(animationCurve3);
					}
				}
				else
				{
					prop = prop.Copy();
					IEnumerator enumerator = prop.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							SerializedProperty serializedProperty2 = (SerializedProperty)enumerator.Current;
							list2.Add(EditorCurveBinding.FloatCurve(sourceBind.path, sourceBind.get_type(), serializedProperty2.get_propertyPath()));
							AnimationCurve animationCurve4 = new AnimationCurve();
							CurveEditUtility.AddKeyFrameToCurve(animationCurve4, (float)time, clip.get_frameRate(), CurveEditUtility.GetKeyValue(serializedProperty2), serializedProperty2.get_propertyType() == 1);
							list.Add(animationCurve4);
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
			CurveEditUtility.UpdateEditorCurves(clip, list2, list);
		}

		public static void RemoveKey(AnimationClip clip, EditorCurveBinding sourceBinding, SerializedProperty prop, double time)
		{
			if (sourceBinding.get_isPPtrCurve())
			{
				CurveEditUtility.RemoveObjectKey(clip, sourceBinding, time);
			}
			else if (CurveEditUtility.IsRotationKey(sourceBinding))
			{
				CurveEditUtility.RemoveRotationKey(clip, sourceBinding, prop, time);
			}
			else
			{
				CurveEditUtility.RemoveFloatKey(clip, sourceBinding, prop, time);
			}
		}

		public static void RemoveObjectKey(AnimationClip clip, EditorCurveBinding sourceBinding, double time)
		{
			AnimationClipCurveInfo curveInfo = AnimationClipCurveCache.Instance.GetCurveInfo(clip);
			int num = Array.IndexOf<EditorCurveBinding>(curveInfo.objectBindings, sourceBinding);
			if (num >= 0)
			{
				ObjectReferenceKeyframe[] array = curveInfo.objectCurves[num];
				int keyframeAtTime = CurveEditUtility.GetKeyframeAtTime(array, (float)time, clip.get_frameRate());
				if (keyframeAtTime >= 0)
				{
					ArrayUtility.RemoveAt<ObjectReferenceKeyframe>(ref array, keyframeAtTime);
					AnimationUtility.SetObjectReferenceCurve(clip, sourceBinding, array);
					EditorUtility.SetDirty(clip);
				}
			}
		}

		private static void RemoveRotationKey(AnimationClip clip, EditorCurveBinding sourceBind, SerializedProperty prop, double time)
		{
			if (prop.get_propertyType() == 17)
			{
				List<AnimationCurve> list = new List<AnimationCurve>();
				List<EditorCurveBinding> list2 = new List<EditorCurveBinding>();
				AnimationClipCurveInfo curveInfo = AnimationClipCurveCache.Instance.GetCurveInfo(clip);
				for (int i = 0; i < curveInfo.bindings.Length; i++)
				{
					if (sourceBind.get_type() == curveInfo.bindings[i].get_type())
					{
						if (curveInfo.bindings[i].propertyName.Contains("localEuler"))
						{
							list2.Add(curveInfo.bindings[i]);
							list.Add(curveInfo.curves[i]);
						}
					}
				}
				foreach (AnimationCurve current in list)
				{
					CurveEditUtility.RemoveKeyFrameFromCurve(current, (float)time, clip.get_frameRate());
				}
				CurveEditUtility.UpdateEditorCurves(clip, list2, list);
			}
		}

		private static void RemoveFloatKey(AnimationClip clip, EditorCurveBinding sourceBind, SerializedProperty prop, double time)
		{
			List<AnimationCurve> list = new List<AnimationCurve>();
			List<EditorCurveBinding> list2 = new List<EditorCurveBinding>();
			AnimationClipCurveInfo curveInfo = AnimationClipCurveCache.Instance.GetCurveInfo(clip);
			for (int i = 0; i < curveInfo.bindings.Length; i++)
			{
				EditorCurveBinding item = curveInfo.bindings[i];
				if (item.get_type() == sourceBind.get_type())
				{
					SerializedProperty serializedProperty = null;
					AnimationCurve animationCurve = curveInfo.curves[i];
					if (prop.get_propertyPath().Equals(item.propertyName))
					{
						serializedProperty = prop;
					}
					else if (item.propertyName.Contains(prop.get_propertyPath()))
					{
						serializedProperty = prop.get_serializedObject().FindProperty(item.propertyName);
					}
					if (serializedProperty != null)
					{
						CurveEditUtility.RemoveKeyFrameFromCurve(animationCurve, (float)time, clip.get_frameRate());
						list.Add(animationCurve);
						list2.Add(item);
					}
				}
			}
			CurveEditUtility.UpdateEditorCurves(clip, list2, list);
		}

		private static void UpdateEditorCurve(AnimationClip clip, EditorCurveBinding binding, AnimationCurve curve)
		{
			if (curve.get_keys().Length == 0)
			{
				AnimationUtility.SetEditorCurve(clip, binding, null);
			}
			else
			{
				AnimationUtility.SetEditorCurve(clip, binding, curve);
			}
		}

		private static void UpdateEditorCurves(AnimationClip clip, List<EditorCurveBinding> bindings, List<AnimationCurve> curves)
		{
			if (curves.Count != 0)
			{
				for (int i = 0; i < curves.Count; i++)
				{
					CurveEditUtility.UpdateEditorCurve(clip, bindings[i], curves[i]);
				}
				EditorUtility.SetDirty(clip);
			}
		}

		public static void RemoveCurves(AnimationClip clip, SerializedProperty prop)
		{
			if (!(clip == null) && prop != null)
			{
				List<EditorCurveBinding> list = new List<EditorCurveBinding>();
				AnimationClipCurveInfo curveInfo = AnimationClipCurveCache.Instance.GetCurveInfo(clip);
				for (int i = 0; i < curveInfo.bindings.Length; i++)
				{
					EditorCurveBinding item = curveInfo.bindings[i];
					if (prop.get_propertyPath().Equals(item.propertyName) || item.propertyName.Contains(prop.get_propertyPath()))
					{
						list.Add(item);
					}
				}
				for (int j = 0; j < list.Count; j++)
				{
					AnimationUtility.SetEditorCurve(clip, list[j], null);
				}
			}
		}

		public static void AddKeyFrameToCurve(AnimationCurve curve, float time, float framerate, float value, bool stepped)
		{
			Keyframe keyframe = default(Keyframe);
			bool flag = true;
			int num = CurveEditUtility.GetKeyframeAtTime(curve, time, framerate);
			if (num != -1)
			{
				flag = false;
				keyframe = curve.get_Item(num);
				curve.RemoveKey(num);
			}
			keyframe.set_value(value);
			keyframe.set_time(CurveEditUtility.GetKeyTime(time, framerate));
			num = curve.AddKey(keyframe);
			if (stepped)
			{
				AnimationUtility.SetKeyBroken(curve, num, stepped);
				AnimationUtility.SetKeyLeftTangentMode(curve, num, 3);
				AnimationUtility.SetKeyRightTangentMode(curve, num, 3);
				keyframe.set_outTangent(float.PositiveInfinity);
				keyframe.set_inTangent(float.PositiveInfinity);
			}
			else if (flag)
			{
				AnimationUtility.SetKeyLeftTangentMode(curve, num, 4);
				AnimationUtility.SetKeyRightTangentMode(curve, num, 4);
			}
			if (num != -1 && !stepped)
			{
				AnimationUtility.UpdateTangentsFromModeSurrounding(curve, num);
				AnimationUtility.SetKeyBroken(curve, num, false);
			}
		}

		public static bool RemoveKeyFrameFromCurve(AnimationCurve curve, float time, float framerate)
		{
			int keyframeAtTime = CurveEditUtility.GetKeyframeAtTime(curve, time, framerate);
			bool result;
			if (keyframeAtTime == -1)
			{
				result = false;
			}
			else
			{
				curve.RemoveKey(keyframeAtTime);
				result = true;
			}
			return result;
		}

		private static float GetKeyValue(SerializedProperty prop)
		{
			float result;
			switch (prop.get_propertyType())
			{
			case 0:
				result = (float)prop.get_intValue();
				break;
			case 1:
				result = ((!prop.get_boolValue()) ? 0f : 1f);
				break;
			case 2:
				result = prop.get_floatValue();
				break;
			default:
				Debug.LogError("Could not convert property type " + prop.get_propertyType() + " to float");
				result = float.NaN;
				break;
			}
			return result;
		}

		public static int GetKeyframeAtTime(AnimationCurve curve, float time, float frameRate)
		{
			float num = 0.5f / frameRate;
			Keyframe[] keys = curve.get_keys();
			int result;
			for (int i = 0; i < keys.Length; i++)
			{
				Keyframe keyframe = keys[i];
				if (keyframe.get_time() >= time - num && keyframe.get_time() < time + num)
				{
					result = i;
					return result;
				}
			}
			result = -1;
			return result;
		}

		public static int GetKeyframeAtTime(ObjectReferenceKeyframe[] curve, float time, float frameRate)
		{
			int result;
			if (curve == null || curve.Length == 0)
			{
				result = -1;
			}
			else
			{
				float num = 0.5f / frameRate;
				for (int i = 0; i < curve.Length; i++)
				{
					float time2 = curve[i].time;
					if (time2 >= time - num && time2 < time + num)
					{
						result = i;
						return result;
					}
				}
				result = -1;
			}
			return result;
		}

		public static float GetKeyTime(float time, float frameRate)
		{
			return Mathf.Round(time * frameRate) / frameRate;
		}

		public static int KeyCompare(float timeA, float timeB, float frameRate)
		{
			int result;
			if (Mathf.Abs(timeA - timeB) <= 0.5f / frameRate)
			{
				result = 0;
			}
			else
			{
				result = ((timeA >= timeB) ? 1 : -1);
			}
			return result;
		}

		public static Object Evaluate(ObjectReferenceKeyframe[] curve, float time)
		{
			return curve[CurveEditUtility.EvaluateIndex(curve, time)].value;
		}

		public static int EvaluateIndex(ObjectReferenceKeyframe[] curve, float time)
		{
			if (curve == null || curve.Length == 0)
			{
				throw new InvalidOperationException("Can not evaluate a PPtr curve with no entries");
			}
			int result;
			if (time <= curve[0].time)
			{
				result = 0;
			}
			else if (time >= curve.Last<ObjectReferenceKeyframe>().time)
			{
				result = curve.Length - 1;
			}
			else
			{
				int num = curve.Length - 1;
				int num2 = 0;
				while (num - num2 > 1)
				{
					int num3 = (num2 + num) / 2;
					if (Mathf.Approximately(curve[num3].time, time))
					{
						result = num3;
						return result;
					}
					if (curve[num3].time < time)
					{
						num2 = num3;
					}
					else if (curve[num3].time > time)
					{
						num = num3;
					}
				}
				result = num2;
			}
			return result;
		}

		public static void ShiftBySeconds(this AnimationClip clip, float time)
		{
			EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);
			EditorCurveBinding[] objectReferenceCurveBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
			EditorCurveBinding[] array = curveBindings;
			for (int i = 0; i < array.Length; i++)
			{
				EditorCurveBinding editorCurveBinding = array[i];
				AnimationCurve editorCurve = AnimationUtility.GetEditorCurve(clip, editorCurveBinding);
				Keyframe[] keys = editorCurve.get_keys();
				for (int j = 0; j < keys.Length; j++)
				{
					Keyframe[] expr_4C_cp_0 = keys;
					int expr_4C_cp_1 = j;
					expr_4C_cp_0[expr_4C_cp_1].set_time(expr_4C_cp_0[expr_4C_cp_1].get_time() + time);
				}
				editorCurve.set_keys(keys);
				AnimationUtility.SetEditorCurve(clip, editorCurveBinding, editorCurve);
			}
			EditorCurveBinding[] array2 = objectReferenceCurveBindings;
			for (int k = 0; k < array2.Length; k++)
			{
				EditorCurveBinding editorCurveBinding2 = array2[k];
				ObjectReferenceKeyframe[] objectReferenceCurve = AnimationUtility.GetObjectReferenceCurve(clip, editorCurveBinding2);
				for (int l = 0; l < objectReferenceCurve.Length; l++)
				{
					ObjectReferenceKeyframe[] expr_C5_cp_0 = objectReferenceCurve;
					int expr_C5_cp_1 = l;
					expr_C5_cp_0[expr_C5_cp_1].time = expr_C5_cp_0[expr_C5_cp_1].time + time;
				}
				AnimationUtility.SetObjectReferenceCurve(clip, editorCurveBinding2, objectReferenceCurve);
			}
			EditorUtility.SetDirty(clip);
		}

		public static void ScaleTime(this AnimationClip clip, float scale)
		{
			EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);
			EditorCurveBinding[] objectReferenceCurveBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
			EditorCurveBinding[] array = curveBindings;
			for (int i = 0; i < array.Length; i++)
			{
				EditorCurveBinding editorCurveBinding = array[i];
				AnimationCurve editorCurve = AnimationUtility.GetEditorCurve(clip, editorCurveBinding);
				Keyframe[] keys = editorCurve.get_keys();
				for (int j = 0; j < keys.Length; j++)
				{
					Keyframe[] expr_4C_cp_0 = keys;
					int expr_4C_cp_1 = j;
					expr_4C_cp_0[expr_4C_cp_1].set_time(expr_4C_cp_0[expr_4C_cp_1].get_time() * scale);
				}
				editorCurve.set_keys((from x in keys
				orderby x.get_time()
				select x).ToArray<Keyframe>());
				AnimationUtility.SetEditorCurve(clip, editorCurveBinding, editorCurve);
			}
			EditorCurveBinding[] array2 = objectReferenceCurveBindings;
			for (int k = 0; k < array2.Length; k++)
			{
				EditorCurveBinding editorCurveBinding2 = array2[k];
				ObjectReferenceKeyframe[] array3 = AnimationUtility.GetObjectReferenceCurve(clip, editorCurveBinding2);
				for (int l = 0; l < array3.Length; l++)
				{
					ObjectReferenceKeyframe[] expr_EC_cp_0 = array3;
					int expr_EC_cp_1 = l;
					expr_EC_cp_0[expr_EC_cp_1].time = expr_EC_cp_0[expr_EC_cp_1].time * scale;
				}
				array3 = (from x in array3
				orderby x.time
				select x).ToArray<ObjectReferenceKeyframe>();
				AnimationUtility.SetObjectReferenceCurve(clip, editorCurveBinding2, array3);
			}
			EditorUtility.SetDirty(clip);
		}

		public static AnimationCurve CreateMatchingCurve(AnimationCurve curve)
		{
			Keyframe[] keys = curve.get_keys();
			for (int num = 0; num != keys.Length; num++)
			{
				if (!float.IsPositiveInfinity(keys[num].get_inTangent()))
				{
					keys[num].set_inTangent(-keys[num].get_inTangent());
				}
				if (!float.IsPositiveInfinity(keys[num].get_outTangent()))
				{
					keys[num].set_outTangent(-keys[num].get_outTangent());
				}
				keys[num].set_value(1f - keys[num].get_value());
			}
			return new AnimationCurve(keys);
		}

		public static Keyframe[] SanitizeCurveKeys(Keyframe[] keys, bool easeIn)
		{
			if (keys.Length < 2)
			{
				if (easeIn)
				{
					keys = new Keyframe[]
					{
						new Keyframe(0f, 0f),
						new Keyframe(1f, 1f)
					};
				}
				else
				{
					keys = new Keyframe[]
					{
						new Keyframe(0f, 1f),
						new Keyframe(1f, 0f)
					};
				}
			}
			else if (easeIn)
			{
				keys[0].set_time(0f);
				keys[keys.Length - 1].set_time(1f);
				keys[keys.Length - 1].set_value(1f);
			}
			else
			{
				keys[0].set_time(0f);
				keys[0].set_value(1f);
				keys[keys.Length - 1].set_time(1f);
			}
			return keys;
		}
	}
}
