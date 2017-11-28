using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal static class TimelineUtility
	{
		public static void SaveSequence(TimelineAsset timeline, string path)
		{
			string assetPath = AssetDatabase.GetAssetPath(timeline);
			if (assetPath.Length == 0)
			{
				AssetDatabase.CreateAsset(timeline, AssetDatabase.GenerateUniqueAssetPath(path + "/" + timeline.get_name() + ".playable"));
			}
			foreach (TrackAsset current in timeline.tracks)
			{
				string assetPath2 = AssetDatabase.GetAssetPath(current);
				if (assetPath2.Length == 0)
				{
					TimelineCreateUtilities.SaveAssetIntoObject(current, timeline);
					TimelineClip[] clips = current.clips;
					for (int i = 0; i < clips.Length; i++)
					{
						TimelineClip timelineClip = clips[i];
						string assetPath3 = AssetDatabase.GetAssetPath(timelineClip.asset);
						if (assetPath3.Length == 0)
						{
							TimelineCreateUtilities.SaveAssetIntoObject(timelineClip.asset, current);
						}
						if (timelineClip.curves != null)
						{
							string assetPath4 = AssetDatabase.GetAssetPath(timelineClip.curves);
							if (assetPath4.Length == 0)
							{
								TimelineCreateUtilities.SaveAssetIntoObject(timelineClip.curves, current);
							}
						}
					}
				}
			}
		}

		public static void SaveSequence(TimelineAsset timeline)
		{
			string text = AssetDatabase.GetAssetPath(Selection.get_activeObject());
			if (text == "")
			{
				text = "Assets";
			}
			else if (Path.GetExtension(text) != "")
			{
				text = text.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.get_activeObject())), "");
			}
			TimelineUtility.SaveSequence(timeline, text);
		}

		public static void ReorderTracks(List<TrackAsset> allTracks, List<TrackAsset> tracks, TrackAsset insertAfterAsset, bool up)
		{
			foreach (TrackAsset current in tracks)
			{
				allTracks.Remove(current);
			}
			int num = allTracks.IndexOf(insertAfterAsset);
			num = ((!up) ? (num + 1) : Math.Max(num, 0));
			allTracks.InsertRange(num, tracks);
		}

		public static TrackAsset GetSceneReferenceTrack(TrackAsset asset)
		{
			TrackAsset result;
			if (asset == null)
			{
				result = null;
			}
			else if (asset.isSubTrack)
			{
				result = TimelineUtility.GetSceneReferenceTrack(asset.parent as TrackAsset);
			}
			else
			{
				result = asset;
			}
			return result;
		}

		public static bool TrackHasAnimationCurves(TrackAsset track)
		{
			bool result;
			for (int i = 0; i < track.clips.Length; i++)
			{
				AnimationClip animationClip = track.clips[i].curves;
				AnimationClip animationClip2 = track.clips[i].animationClip;
				if (animationClip != null && animationClip.get_empty())
				{
					animationClip = null;
				}
				if (animationClip2 != null && animationClip2.get_empty())
				{
					animationClip2 = null;
				}
				if (animationClip2 != null && (animationClip2.get_hideFlags() & 8) != null)
				{
					animationClip2 = null;
				}
				if (!track.clips[i].recordable)
				{
					animationClip2 = null;
				}
				if (animationClip != null || animationClip2 != null)
				{
					result = true;
					return result;
				}
			}
			result = (track.animClip != null);
			return result;
		}

		public static GameObject GetSceneGameObject(PlayableDirector director, TrackAsset asset)
		{
			GameObject result;
			if (director == null || asset == null)
			{
				result = null;
			}
			else
			{
				asset = TimelineUtility.GetSceneReferenceTrack(asset);
				GameObject gameObject = director.GetGenericBinding(asset) as GameObject;
				Component component = director.GetGenericBinding(asset) as Component;
				if (component != null)
				{
					gameObject = component.get_gameObject();
				}
				result = gameObject;
			}
			return result;
		}

		public static void SetSceneGameObject(PlayableDirector director, TrackAsset asset, GameObject go)
		{
			if (!(director == null) && !(asset == null))
			{
				asset = TimelineUtility.GetSceneReferenceTrack(asset);
				IEnumerable<PlayableBinding> outputs = asset.get_outputs();
				if (outputs.Count<PlayableBinding>() != 0)
				{
					PlayableBinding playableBinding = outputs.First<PlayableBinding>();
					if (playableBinding.get_streamType() == null || playableBinding.get_sourceBindingType() == typeof(GameObject))
					{
						TimelineHelpers.AddRequiredComponent(go, asset);
						TimelineUtility.SetBindingInDirector(director, asset, go);
					}
					else
					{
						TimelineUtility.SetBindingInDirector(director, asset, TimelineHelpers.AddRequiredComponent(go, asset));
					}
				}
			}
		}

		public static void SetBindingInDirector(PlayableDirector director, Object bindTo, Object objectToBind)
		{
			if (!(director == null) && !(bindTo == null))
			{
				TimelineUndo.PushUndo(director, "PlayableDirector Binding");
				director.SetGenericBinding(bindTo, objectToBind);
			}
		}

		public static PlayableDirector[] GetDirectorsInSceneUsingAsset(PlayableAsset asset)
		{
			List<PlayableDirector> list = new List<PlayableDirector>();
			PlayableDirector[] array = Resources.FindObjectsOfTypeAll(typeof(PlayableDirector)) as PlayableDirector[];
			PlayableDirector[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				PlayableDirector playableDirector = array2[i];
				if ((playableDirector.get_hideFlags() & 15) == null)
				{
					string assetPath = AssetDatabase.GetAssetPath(playableDirector.get_transform().get_root().get_gameObject());
					if (string.IsNullOrEmpty(assetPath))
					{
						if (asset == null || (asset != null && playableDirector.get_playableAsset() == asset))
						{
							list.Add(playableDirector);
						}
					}
				}
			}
			return list.ToArray();
		}

		public static PlayableDirector GetDirectorComponentForGameObject(GameObject gameObject)
		{
			return (!(gameObject != null)) ? null : gameObject.GetComponent<PlayableDirector>();
		}

		public static TimelineAsset GetTimelineAssetForDirectorComponent(PlayableDirector director)
		{
			return (!(director != null)) ? null : (director.get_playableAsset() as TimelineAsset);
		}

		public static bool IsPrefabOrAsset(Object obj)
		{
			return EditorUtility.IsPersistent(obj) || (obj.get_hideFlags() & 8) != 0;
		}

		internal static ScriptableObject CreateAsset(Type t, string assetName)
		{
			string text = AssetDatabase.GetAssetPath(Selection.get_activeObject());
			if (text == "")
			{
				text = "Assets";
			}
			else if (Path.GetExtension(text) != "")
			{
				text = text.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.get_activeObject())), "");
			}
			ScriptableObject scriptableObject = ScriptableObject.CreateInstance(t);
			AssetDatabase.CreateAsset(scriptableObject, AssetDatabase.GenerateUniqueAssetPath(text + "/" + assetName));
			Selection.set_activeObject(scriptableObject);
			return scriptableObject;
		}

		internal static T CreateAsset<T>(string assetName) where T : ScriptableObject
		{
			return TimelineUtility.CreateAsset(typeof(T), assetName) as T;
		}

		internal static string PropertyToString(SerializedProperty property)
		{
			SerializedPropertyType propertyType = property.get_propertyType();
			string result;
			switch (propertyType + 1)
			{
			case 0:
				result = string.Empty;
				break;
			case 1:
				result = property.get_intValue().ToString();
				break;
			case 2:
				result = ((!property.get_boolValue()) ? "0" : "1");
				break;
			case 3:
				result = property.get_floatValue().ToString();
				break;
			case 4:
				result = property.get_stringValue();
				break;
			case 5:
				result = property.get_colorValue().ToString();
				break;
			case 6:
				result = string.Empty;
				break;
			case 7:
				result = property.get_intValue().ToString();
				break;
			case 8:
				result = property.get_intValue().ToString();
				break;
			case 9:
				result = property.get_vector2Value().ToString();
				break;
			case 10:
				result = property.get_vector3Value().ToString();
				break;
			case 11:
				result = property.get_vector4Value().ToString();
				break;
			case 12:
				result = property.get_rectValue().ToString();
				break;
			case 13:
				result = property.get_intValue().ToString();
				break;
			case 14:
				result = property.get_intValue().ToString();
				break;
			case 15:
				result = property.get_animationCurveValue().ToString();
				break;
			case 16:
				result = property.get_boundsValue().ToString();
				break;
			case 17:
				result = property.get_gradientValue().ToString();
				break;
			case 18:
				result = property.get_quaternionValue().ToString();
				break;
			default:
				Debug.LogWarning("Unknown Property Type: " + property.get_propertyType());
				result = string.Empty;
				break;
			}
			return result;
		}
	}
}
