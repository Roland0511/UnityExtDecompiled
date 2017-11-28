using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class AnimationTrackRecorder
	{
		public static readonly string kRecordClipDefaultName = "Recorded";

		private readonly List<AnimationClip> m_ProcessedClips = new List<AnimationClip>();

		private readonly List<GameObject> m_RebindList = new List<GameObject>();

		private bool m_RefreshState;

		private double m_ClipTime;

		private bool m_needRebuildRects;

		private readonly List<TrackAsset> m_TracksToProcess = new List<TrackAsset>();

		public TimelineClip recordClip
		{
			get;
			private set;
		}

		public void PrepareForRecord(TimelineWindow.TimelineState state)
		{
			this.m_ProcessedClips.Clear();
			this.m_RebindList.Clear();
			this.m_RefreshState = false;
			this.m_TracksToProcess.Clear();
		}

		public AnimationClip PrepareTrack(TrackAsset track, TimelineWindow.TimelineState state, GameObject gameObject, out double startTime)
		{
			AnimationTrack animationTrack = (AnimationTrack)track;
			AnimationClip result;
			if (!animationTrack.inClipMode)
			{
				AnimationClip orCreateClip = animationTrack.GetOrCreateClip();
				startTime = (double)orCreateClip.get_frameRate() * state.time;
				if (!this.m_TracksToProcess.Contains(animationTrack))
				{
					this.m_TracksToProcess.Add(animationTrack);
				}
				this.m_RebindList.Add(gameObject);
				if (orCreateClip.get_empty())
				{
					animationTrack.openClipTimeOffset = 0.0;
					animationTrack.openClipPreExtrapolation = TimelineClip.ClipExtrapolation.Hold;
					animationTrack.openClipPostExtrapolation = TimelineClip.ClipExtrapolation.Hold;
				}
				result = orCreateClip;
			}
			else
			{
				TimelineClip timelineClip = AnimationTrackRecorder.GetRecordingClipForTrack(track, state);
				if (timelineClip == null)
				{
					timelineClip = track.FindRecordingClipAtTime(state.time);
				}
				List<TimelineClip> list = (from x in track.clips
				where x.start <= state.time && x.end >= state.time
				select x).ToList<TimelineClip>();
				if (timelineClip == null)
				{
					if (list.Count != 0)
					{
						if (list.Count > 0)
						{
							if (list.Any((TimelineClip x) => x.recordable))
							{
								goto IL_12A;
							}
						}
						Debug.LogWarning("Cannot record on top of an imported animation clip");
						startTime = -1.0;
						result = null;
						return result;
					}
					IL_12A:
					timelineClip = AnimationTrackRecorder.AddRecordableClip(track, state);
					timelineClip.start = state.time;
					this.m_RebindList.Add(gameObject);
				}
				AnimationClip animationClip = timelineClip.animationClip;
				double num = state.time - timelineClip.start;
				if (num < 0.0)
				{
					Undo.RegisterCompleteObjectUndo(animationClip, "Record Key");
					TimelineUndo.PushUndo(track, "Prepend Key");
					AnimationTrackRecorder.ShiftAnimationClip(animationClip, (float)(-(float)num));
					timelineClip.start = state.time;
					timelineClip.duration += -num;
					num = 0.0;
					this.m_RefreshState = true;
				}
				this.m_ClipTime = num;
				this.recordClip = timelineClip;
				startTime = (double)TimeUtility.ToFrames(this.recordClip.ToLocalTimeUnbound(state.time), (double)animationClip.get_frameRate());
				this.m_needRebuildRects = animationClip.get_empty();
				result = animationClip;
			}
			return result;
		}

		public void FinializeTrack(TrackAsset track, TimelineWindow.TimelineState state)
		{
			AnimationTrack animationTrack = track as AnimationTrack;
			if (!animationTrack.inClipMode)
			{
				EditorUtility.SetDirty(animationTrack.GetOrCreateClip());
			}
			if (this.recordClip != null)
			{
				if (!this.m_ProcessedClips.Contains(this.recordClip.animationClip))
				{
					this.m_ProcessedClips.Add(this.recordClip.animationClip);
				}
				if (this.m_ClipTime > this.recordClip.duration)
				{
					TimelineUndo.PushUndo(track, "Add Key");
					this.recordClip.duration = this.m_ClipTime;
					this.m_RefreshState = true;
				}
				track.CalculateExtrapolationTimes();
			}
			this.recordClip = null;
			this.m_ClipTime = 0.0;
			if (this.m_needRebuildRects)
			{
				state.CalculateRowRects();
				this.m_needRebuildRects = false;
			}
		}

		public void FinalizeRecording(TimelineWindow.TimelineState state)
		{
			for (int num = 0; num != this.m_ProcessedClips.Count; num++)
			{
				AnimationTrackRecorder.ProcessTemporaryKeys(this.m_ProcessedClips[num]);
			}
			this.m_RefreshState |= this.m_TracksToProcess.Any<TrackAsset>();
			this.m_TracksToProcess.Clear();
			if (this.m_ProcessedClips.Count > 0 || this.m_RefreshState)
			{
				state.GetWindow().RebuildGraphIfNecessary(false);
			}
			state.RebindAnimators(this.m_RebindList);
			if (this.m_ProcessedClips.Count > 0 || this.m_RefreshState)
			{
				state.EvaluateImmediate();
			}
		}

		public static string GetUniqueRecordedClipName(Object owner, string name)
		{
			string assetPath = AssetDatabase.GetAssetPath(owner);
			string result;
			if (!string.IsNullOrEmpty(assetPath))
			{
				IEnumerable<string> source = from x in AssetDatabase.LoadAllAssetsAtPath(assetPath)
				where x != null
				select x.get_name();
				result = ObjectNames.GetUniqueName(source.ToArray<string>(), name);
			}
			else
			{
				TrackAsset trackAsset = owner as TrackAsset;
				if (trackAsset == null || trackAsset.clips.Length == 0)
				{
					result = name;
				}
				else
				{
					result = ObjectNames.GetUniqueName((from x in trackAsset.clips
					select x.displayName).ToArray<string>(), name);
				}
			}
			return result;
		}

		public static TimelineClip AddRecordableClip(TrackAsset parentTrack, TimelineWindow.TimelineState state)
		{
			TimelineAsset timeline = state.timeline;
			TimelineClip result;
			if (timeline == null)
			{
				Debug.LogError("Parent Track needs to be bound to an asset to add a recordable");
				result = null;
			}
			else
			{
				AnimationClip animationClip = new AnimationClip();
				animationClip.set_name(AnimationTrackRecorder.GetUniqueRecordedClipName(parentTrack, AnimationTrackRecorder.kRecordClipDefaultName));
				animationClip.set_frameRate(state.frameRate);
				AnimationUtility.SetGenerateMotionCurves(animationClip, true);
				Undo.RegisterCreatedObjectUndo(animationClip, "Create Clip");
				TimelineHelpers.SaveAnimClipIntoObject(animationClip, parentTrack);
				TimelineClip timelineClip = parentTrack.CreateClipFromAsset(animationClip);
				if (timelineClip != null)
				{
					timelineClip.recordable = true;
					timelineClip.displayName = animationClip.get_name();
					timelineClip.timeScale = 1.0;
					timelineClip.start = 0.0;
					timelineClip.duration = 0.0;
					timelineClip.mixInCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
					timelineClip.mixOutCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
					timelineClip.preExtrapolationMode = TimelineClip.ClipExtrapolation.Hold;
					timelineClip.postExtrapolationMode = TimelineClip.ClipExtrapolation.Hold;
					TimelineCreateUtilities.SaveAssetIntoObject(timelineClip.asset, parentTrack);
					state.Refresh();
				}
				result = timelineClip;
			}
			return result;
		}

		private static TimelineClip GetRecordingClipForTrack(TrackAsset track, TimelineWindow.TimelineState state)
		{
			return track.FindRecordingClipAtTime(state.time);
		}

		private static void ShiftAnimationClip(AnimationClip clip, float amount)
		{
			if (!(clip == null))
			{
				EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);
				EditorCurveBinding[] objectReferenceCurveBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
				EditorCurveBinding[] array = curveBindings;
				for (int i = 0; i < array.Length; i++)
				{
					EditorCurveBinding editorCurveBinding = array[i];
					AnimationCurve editorCurve = AnimationUtility.GetEditorCurve(clip, editorCurveBinding);
					editorCurve.set_keys(AnimationTrackRecorder.ShiftKeys(editorCurve.get_keys(), amount));
					AnimationUtility.SetEditorCurve(clip, editorCurveBinding, editorCurve);
				}
				EditorCurveBinding[] array2 = objectReferenceCurveBindings;
				for (int j = 0; j < array2.Length; j++)
				{
					EditorCurveBinding editorCurveBinding2 = array2[j];
					ObjectReferenceKeyframe[] array3 = AnimationUtility.GetObjectReferenceCurve(clip, editorCurveBinding2);
					array3 = AnimationTrackRecorder.ShiftObjectKeys(array3, amount);
					AnimationUtility.SetObjectReferenceCurve(clip, editorCurveBinding2, array3);
				}
				EditorUtility.SetDirty(clip);
			}
		}

		private static Keyframe[] ShiftKeys(Keyframe[] keys, float time)
		{
			Keyframe[] result;
			if (keys == null || keys.Length == 0 || time == 0f)
			{
				result = keys;
			}
			else
			{
				Keyframe[] array = new Keyframe[keys.Length + 1];
				array[0] = keys[0];
				array[0].set_inTangent(0f);
				array[0].set_outTangent(0f);
				for (int i = 0; i < keys.Length; i++)
				{
					array[i + 1] = keys[i];
					Keyframe[] expr_91_cp_0 = array;
					int expr_91_cp_1 = i + 1;
					expr_91_cp_0[expr_91_cp_1].set_time(expr_91_cp_0[expr_91_cp_1].get_time() + time);
				}
				array[1].set_inTangent(0f);
				result = array;
			}
			return result;
		}

		private static ObjectReferenceKeyframe[] ShiftObjectKeys(ObjectReferenceKeyframe[] keys, float time)
		{
			ObjectReferenceKeyframe[] result;
			if (keys == null || keys.Length == 0 || time == 0f)
			{
				result = keys;
			}
			else
			{
				ObjectReferenceKeyframe[] array = new ObjectReferenceKeyframe[keys.Length + 1];
				array[0] = keys[0];
				for (int i = 0; i < keys.Length; i++)
				{
					array[i + 1] = keys[i];
					ObjectReferenceKeyframe[] expr_6F_cp_0 = array;
					int expr_6F_cp_1 = i + 1;
					expr_6F_cp_0[expr_6F_cp_1].time = expr_6F_cp_0[expr_6F_cp_1].time + time;
				}
				result = array;
			}
			return result;
		}

		private static bool ProcessCurveBinding(AnimationClip clip, EditorCurveBinding binding)
		{
			float num = 1.5f;
			if ((double)clip.get_frameRate() > 0.0)
			{
				num = 1.5f / clip.get_frameRate();
			}
			bool result = false;
			AnimationCurve editorCurve = AnimationUtility.GetEditorCurve(clip, binding);
			Keyframe[] keys = editorCurve.get_keys();
			if (keys.Length == 3 && AnimationUtility.GetKeyBroken(keys[1]))
			{
				float num2 = keys[1].get_time() - keys[0].get_time();
				float num3 = keys[2].get_time() - keys[1].get_time();
				float num4 = Mathf.Min(num2, num3);
				if (num4 <= num)
				{
					if (num2 < num3)
					{
						editorCurve.RemoveKey(1);
					}
					else
					{
						editorCurve.RemoveKey(2);
					}
					keys = editorCurve.get_keys();
					AnimationUtility.SetKeyBroken(editorCurve, 0, false);
					AnimationUtility.SetKeyBroken(editorCurve, 1, false);
					AnimationUtility.SetKeyLeftTangentMode(editorCurve, 0, 1);
					AnimationUtility.SetKeyLeftTangentMode(editorCurve, 1, 1);
					AnimationUtility.SetKeyRightTangentMode(editorCurve, 0, 1);
					AnimationUtility.SetKeyRightTangentMode(editorCurve, 1, 1);
					editorCurve.set_keys(keys);
					AnimationUtility.UpdateTangentsFromMode(editorCurve);
					AnimationUtility.SetEditorCurve(clip, binding, editorCurve);
					result = true;
				}
			}
			else if (keys.Length == 1 && keys[0].get_time() == 0f)
			{
				editorCurve.AddKey(1f / clip.get_frameRate(), keys[0].get_value());
				keys = editorCurve.get_keys();
				AnimationUtility.SetKeyLeftTangentMode(editorCurve, 0, 1);
				AnimationUtility.SetKeyRightTangentMode(editorCurve, 0, 1);
				AnimationUtility.SetKeyBroken(editorCurve, 0, true);
				AnimationUtility.SetKeyBroken(editorCurve, 1, true);
				Keyframe[] arg_199_0_cp_0 = keys;
				int arg_199_0_cp_1 = 0;
				float num5 = 0f;
				keys[0].set_outTangent(num5);
				arg_199_0_cp_0[arg_199_0_cp_1].set_inTangent(num5);
				Keyframe[] arg_1BC_0_cp_0 = keys;
				int arg_1BC_0_cp_1 = 1;
				num5 = 0f;
				keys[1].set_outTangent(num5);
				arg_1BC_0_cp_0[arg_1BC_0_cp_1].set_inTangent(num5);
				editorCurve.set_keys(keys);
				AnimationUtility.SetEditorCurve(clip, binding, editorCurve);
				EditorUtility.SetDirty(clip);
				result = true;
			}
			else if (keys.Length == 2)
			{
				float num6 = keys[1].get_time() - keys[0].get_time();
				if (AnimationUtility.GetKeyBroken(keys[0]) && AnimationUtility.GetKeyBroken(keys[1]) && num6 > 0f && num6 < num)
				{
					keys[1].set_value(keys[0].get_value());
					editorCurve.set_keys(keys);
					AnimationUtility.SetEditorCurve(clip, binding, editorCurve);
					EditorUtility.SetDirty(clip);
					result = true;
				}
			}
			return result;
		}

		private static void ProcessTemporaryKeys(AnimationClip clip)
		{
			if (!(clip == null))
			{
				bool flag = false;
				EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);
				EditorCurveBinding[] array = curveBindings;
				for (int i = 0; i < array.Length; i++)
				{
					EditorCurveBinding editorCurveBinding = array[i];
					if (!editorCurveBinding.propertyName.Contains("LocalRotation.w"))
					{
						EditorCurveBinding binding = RotationCurveInterpolation.RemapAnimationBindingForRotationCurves(editorCurveBinding, clip);
						flag |= AnimationTrackRecorder.ProcessCurveBinding(clip, binding);
					}
				}
				if (flag)
				{
					EditorUtility.SetDirty(clip);
				}
			}
		}
	}
}
