using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class TimelineAnimationUtilities
	{
		public enum OffsetEditMode
		{
			None = -1,
			Translation,
			Rotation
		}

		public struct RigidTransform
		{
			public Vector3 position;

			public Quaternion rotation;

			public static TimelineAnimationUtilities.RigidTransform identity
			{
				get
				{
					return TimelineAnimationUtilities.RigidTransform.Compose(Vector3.get_zero(), Quaternion.get_identity());
				}
			}

			public static TimelineAnimationUtilities.RigidTransform Compose(Vector3 pos, Quaternion rot)
			{
				TimelineAnimationUtilities.RigidTransform result;
				result.position = pos;
				result.rotation = rot;
				return result;
			}

			public static TimelineAnimationUtilities.RigidTransform Mul(TimelineAnimationUtilities.RigidTransform a, TimelineAnimationUtilities.RigidTransform b)
			{
				TimelineAnimationUtilities.RigidTransform result;
				result.rotation = a.rotation * b.rotation;
				result.position = a.position + a.rotation * b.position;
				return result;
			}

			public static TimelineAnimationUtilities.RigidTransform Inverse(TimelineAnimationUtilities.RigidTransform a)
			{
				TimelineAnimationUtilities.RigidTransform result;
				result.rotation = Quaternion.Inverse(a.rotation);
				result.position = result.rotation * -a.position;
				return result;
			}
		}

		public static bool ValidateOffsetAvailabitity(PlayableDirector director, Animator animator)
		{
			return !(director == null) && !(animator == null);
		}

		public static TimelineClip GetPreviousClip(TimelineClip clip)
		{
			TimelineClip timelineClip = null;
			TimelineClip[] clips = clip.parentTrack.clips;
			for (int i = 0; i < clips.Length; i++)
			{
				TimelineClip timelineClip2 = clips[i];
				if (timelineClip2.start < clip.start && (timelineClip == null || timelineClip2.start >= timelineClip.start))
				{
					timelineClip = timelineClip2;
				}
			}
			return timelineClip;
		}

		public static TimelineClip GetNextClip(TimelineClip clip)
		{
			return (from c in clip.parentTrack.clips
			where c.start > clip.start
			orderby c.start
			select c).FirstOrDefault<TimelineClip>();
		}

		public static void ComputeClipWorldSpaceOffset(PlayableDirector director, TimelineClip clip, out Vector3 clipPositionOffset, out Quaternion clipRotationOffset)
		{
			GameObject sceneGameObject = TimelineUtility.GetSceneGameObject(director, clip.parentTrack);
			double time = director.get_time();
			TrackAsset parentTrack = clip.parentTrack;
			TimelineClip[] clips = parentTrack.clips;
			director.Stop();
			for (int i = 0; i < clips.Length; i++)
			{
				parentTrack.RemoveClip(clips[i]);
			}
			parentTrack.AddClip(clip);
			double start = clip.start;
			double blendInDuration = clip.blendInDuration;
			clip.blendInDuration = 0.0;
			clip.start = 1.0;
			director.Play();
			director.set_time(0.0);
			director.Evaluate();
			clipPositionOffset = sceneGameObject.get_transform().get_position();
			clipRotationOffset = sceneGameObject.get_transform().get_rotation();
			AnimationPlayableAsset animationPlayableAsset = clip.asset as AnimationPlayableAsset;
			if (!(animationPlayableAsset == null))
			{
				clipPositionOffset += clipRotationOffset * animationPlayableAsset.position;
				clipRotationOffset *= animationPlayableAsset.rotation;
				director.Stop();
				clip.start = start;
				clip.blendInDuration = blendInDuration;
				parentTrack.RemoveClip(clip);
				for (int j = 0; j < clips.Length; j++)
				{
					parentTrack.AddClip(clips[j]);
				}
				director.Play();
				director.set_time(time);
				director.Evaluate();
			}
		}

		public static void ComputeTrackOffsets(PlayableDirector director, TimelineClip clip, out Vector3 parentPositionOffset, out Quaternion parentRotationOffset, out Vector3 positionOffset, out Quaternion rotationOffset)
		{
			GameObject sceneGameObject = TimelineUtility.GetSceneGameObject(director, clip.parentTrack);
			double time = director.get_time();
			TrackAsset parentTrack = clip.parentTrack;
			TimelineClip[] clips = parentTrack.clips;
			director.Stop();
			for (int i = 0; i < clips.Length; i++)
			{
				parentTrack.RemoveClip(clips[i]);
			}
			parentTrack.AddClip(clip);
			double start = clip.start;
			double blendInDuration = clip.blendInDuration;
			clip.blendInDuration = 0.0;
			clip.start = 1.0;
			director.Play();
			director.set_time(1.0);
			director.Evaluate();
			positionOffset = sceneGameObject.get_transform().get_position();
			rotationOffset = sceneGameObject.get_transform().get_rotation();
			director.set_time(0.0);
			director.Evaluate();
			parentPositionOffset = sceneGameObject.get_transform().get_position();
			parentRotationOffset = sceneGameObject.get_transform().get_rotation();
			director.Stop();
			clip.start = start;
			clip.blendInDuration = blendInDuration;
			parentTrack.RemoveClip(clip);
			for (int j = 0; j < clips.Length; j++)
			{
				parentTrack.AddClip(clips[j]);
			}
			director.Play();
			director.set_time(time);
			director.Evaluate();
		}

		public static TimelineAnimationUtilities.RigidTransform UpdateClipOffsets(AnimationPlayableAsset asset, AnimationTrack track, Transform transform, Vector3 globalPosition, Quaternion globalRotation)
		{
			Matrix4x4 worldToLocalMatrix = transform.get_worldToLocalMatrix();
			Matrix4x4 matrix4x = Matrix4x4.TRS(asset.position, asset.rotation, Vector3.get_one());
			Matrix4x4 matrix4x2 = Matrix4x4.TRS(track.position, track.rotation, Vector3.get_one());
			if (transform.get_parent() != null)
			{
				matrix4x2 = transform.get_parent().get_localToWorldMatrix() * matrix4x2;
			}
			Vector3 position = transform.get_position();
			Quaternion rotation = transform.get_rotation();
			transform.set_position(globalPosition);
			transform.set_rotation(globalRotation);
			Matrix4x4 localToWorldMatrix = transform.get_localToWorldMatrix();
			transform.set_position(position);
			transform.set_rotation(rotation);
			Matrix4x4 matrix4x3 = matrix4x2.get_inverse() * localToWorldMatrix * worldToLocalMatrix * matrix4x2 * matrix4x;
			return TimelineAnimationUtilities.RigidTransform.Compose(matrix4x3.GetColumn(3), MathUtils.QuaternionFromMatrix(matrix4x3));
		}

		public static TimelineAnimationUtilities.RigidTransform GetTrackOffsets(AnimationTrack track, Transform transform)
		{
			Vector3 vector = track.position;
			Quaternion quaternion = track.rotation;
			if (transform != null && transform.get_parent() != null)
			{
				vector = transform.get_parent().TransformPoint(vector);
				quaternion = transform.get_parent().get_rotation() * quaternion;
				MathUtils.QuaternionNormalize(ref quaternion);
			}
			return TimelineAnimationUtilities.RigidTransform.Compose(vector, quaternion);
		}

		public static void UpdateTrackOffset(AnimationTrack track, Transform transform, TimelineAnimationUtilities.RigidTransform offsets)
		{
			if (transform != null && transform.get_parent() != null)
			{
				offsets.position = transform.get_parent().InverseTransformPoint(offsets.position);
				offsets.rotation = Quaternion.Inverse(transform.get_parent().get_rotation()) * offsets.rotation;
				MathUtils.QuaternionNormalize(ref offsets.rotation);
			}
			track.position = offsets.position;
			track.rotation = offsets.rotation;
			track.UpdateClipOffsets();
		}

		private static MatchTargetFields GetMatchFields(TimelineClip clip)
		{
			AnimationTrack animationTrack = clip.parentTrack as AnimationTrack;
			MatchTargetFields result;
			if (animationTrack == null)
			{
				result = MatchTargetFieldConstants.None;
			}
			else
			{
				AnimationPlayableAsset animationPlayableAsset = clip.asset as AnimationPlayableAsset;
				MatchTargetFields matchTargetFields = animationTrack.matchTargetFields;
				if (animationPlayableAsset != null && animationPlayableAsset.useTrackMatchFields)
				{
					matchTargetFields = animationPlayableAsset.matchTargetFields;
				}
				result = matchTargetFields;
			}
			return result;
		}

		private static void WriteMatchFields(AnimationPlayableAsset asset, TimelineAnimationUtilities.RigidTransform result, MatchTargetFields fields)
		{
			Vector3 position = asset.position;
			position.x = ((!fields.HasAny(MatchTargetFields.PositionX)) ? position.x : result.position.x);
			position.y = ((!fields.HasAny(MatchTargetFields.PositionY)) ? position.y : result.position.y);
			position.z = ((!fields.HasAny(MatchTargetFields.PositionZ)) ? position.z : result.position.z);
			asset.position = position;
			if (fields.HasAny(MatchTargetFieldConstants.Rotation))
			{
				Vector3 eulerAngles = asset.rotation.get_eulerAngles();
				Vector3 eulerAngles2 = result.rotation.get_eulerAngles();
				eulerAngles.x = ((!fields.HasAny(MatchTargetFields.RotationX)) ? eulerAngles.x : eulerAngles2.x);
				eulerAngles.y = ((!fields.HasAny(MatchTargetFields.RotationY)) ? eulerAngles.y : eulerAngles2.y);
				eulerAngles.z = ((!fields.HasAny(MatchTargetFields.RotationZ)) ? eulerAngles.z : eulerAngles2.z);
				asset.rotation = Quaternion.Euler(eulerAngles);
			}
		}

		public static void MatchPrevious(TimelineClip currentClip, Transform matchPoint, PlayableDirector director)
		{
			MatchTargetFields matchFields = TimelineAnimationUtilities.GetMatchFields(currentClip);
			if (matchFields != MatchTargetFieldConstants.None && !(matchPoint == null))
			{
				double time = director.get_time();
				TimelineClip previousClip = TimelineAnimationUtilities.GetPreviousClip(currentClip);
				if (previousClip != null && currentClip != previousClip)
				{
					AnimationTrack animationTrack = currentClip.parentTrack as AnimationTrack;
					double blendInDuration = currentClip.blendInDuration;
					currentClip.blendInDuration = 0.0;
					double blendOutDuration = previousClip.blendOutDuration;
					previousClip.blendOutDuration = 0.0;
					director.Stop();
					animationTrack.RemoveClip(currentClip);
					director.Play();
					double num = (currentClip.start <= previousClip.end) ? currentClip.start : previousClip.end;
					director.set_time(num - 1E-05);
					director.Evaluate();
					Vector3 position = matchPoint.get_position();
					Quaternion rotation = matchPoint.get_rotation();
					director.Stop();
					animationTrack.AddClip(currentClip);
					animationTrack.RemoveClip(previousClip);
					director.Play();
					director.set_time(currentClip.start + 1E-05);
					director.Evaluate();
					AnimationPlayableAsset asset = currentClip.asset as AnimationPlayableAsset;
					TimelineAnimationUtilities.RigidTransform result = TimelineAnimationUtilities.UpdateClipOffsets(asset, animationTrack, matchPoint, position, rotation);
					TimelineAnimationUtilities.WriteMatchFields(asset, result, matchFields);
					currentClip.blendInDuration = blendInDuration;
					previousClip.blendOutDuration = blendOutDuration;
					director.Stop();
					animationTrack.AddClip(previousClip);
					director.Play();
					director.set_time(time);
					director.Evaluate();
				}
			}
		}

		public static void MatchNext(TimelineClip currentClip, Transform matchPoint, PlayableDirector director)
		{
			MatchTargetFields matchFields = TimelineAnimationUtilities.GetMatchFields(currentClip);
			if (matchFields != MatchTargetFieldConstants.None && !(matchPoint == null))
			{
				double time = director.get_time();
				TimelineClip nextClip = TimelineAnimationUtilities.GetNextClip(currentClip);
				if (nextClip != null && currentClip != nextClip)
				{
					AnimationTrack animationTrack = currentClip.parentTrack as AnimationTrack;
					double blendOutDuration = currentClip.blendOutDuration;
					double blendInDuration = nextClip.blendInDuration;
					currentClip.blendOutDuration = 0.0;
					nextClip.blendInDuration = 0.0;
					director.Stop();
					animationTrack.RemoveClip(currentClip);
					director.Play();
					director.set_time(nextClip.start + 1E-05);
					director.Evaluate();
					Vector3 position = matchPoint.get_position();
					Quaternion rotation = matchPoint.get_rotation();
					director.Stop();
					animationTrack.AddClip(currentClip);
					animationTrack.RemoveClip(nextClip);
					director.Play();
					director.set_time(Math.Min(nextClip.start, currentClip.end - 1E-05));
					director.Evaluate();
					AnimationPlayableAsset asset = currentClip.asset as AnimationPlayableAsset;
					TimelineAnimationUtilities.RigidTransform result = TimelineAnimationUtilities.UpdateClipOffsets(asset, animationTrack, matchPoint, position, rotation);
					TimelineAnimationUtilities.WriteMatchFields(asset, result, matchFields);
					currentClip.blendOutDuration = blendOutDuration;
					nextClip.blendInDuration = blendInDuration;
					director.Stop();
					animationTrack.AddClip(nextClip);
					director.Play();
					director.set_time(time);
					director.Evaluate();
				}
			}
		}

		public static TimelineWindowTimeControl CreateTimeController(TimelineWindow.TimelineState state, TimelineClip clip)
		{
			AnimationWindow window = EditorWindow.GetWindow<AnimationWindow>();
			TimelineWindowTimeControl timelineWindowTimeControl = ScriptableObject.CreateInstance<TimelineWindowTimeControl>();
			timelineWindowTimeControl.Init(state.GetWindow(), window.get_state(), clip);
			return timelineWindowTimeControl;
		}

		public static TimelineWindowTimeControl CreateTimeController(TimelineWindow.TimelineState state, TimelineWindowTimeControl.ClipData clipData)
		{
			AnimationWindow window = EditorWindow.GetWindow<AnimationWindow>();
			TimelineWindowTimeControl timelineWindowTimeControl = ScriptableObject.CreateInstance<TimelineWindowTimeControl>();
			timelineWindowTimeControl.Init(state.GetWindow(), window.get_state(), clipData);
			return timelineWindowTimeControl;
		}

		public static void EditAnimationClipWithTimeController(AnimationClip animationClip, TimelineWindowTimeControl timeController, Object sourceObject)
		{
			AnimationWindow window = EditorWindow.GetWindow<AnimationWindow>();
			window.EditSequencerClip(animationClip, sourceObject, timeController);
		}

		public static int GetAnimationWindowCurrentFrame()
		{
			AnimationWindow window = EditorWindow.GetWindow<AnimationWindow>();
			int result;
			if (window)
			{
				result = window.get_state().get_currentFrame();
			}
			else
			{
				result = -1;
			}
			return result;
		}

		public static void SetAnimationWindowCurrentFrame(int frame)
		{
			AnimationWindow window = EditorWindow.GetWindow<AnimationWindow>();
			if (window)
			{
				window.get_state().set_currentFrame(frame);
			}
		}
	}
}
