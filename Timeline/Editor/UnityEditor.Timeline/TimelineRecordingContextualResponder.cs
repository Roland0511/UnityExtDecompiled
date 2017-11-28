using System;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class TimelineRecordingContextualResponder : IAnimationContextualResponder
	{
		public TimelineWindow.TimelineState state
		{
			get;
			internal set;
		}

		public TimelineRecordingContextualResponder(TimelineWindow.TimelineState _state)
		{
			this.state = _state;
		}

		public bool HasAnyCandidates()
		{
			return false;
		}

		public bool HasAnyCurves()
		{
			return false;
		}

		public void AddCandidateKeys()
		{
		}

		public void AddAnimatedKeys()
		{
		}

		public bool IsAnimatable(PropertyModification[] modifications)
		{
			bool result;
			for (int i = 0; i < modifications.Length; i++)
			{
				IPlayableAsset playableAsset = modifications[i].target as IPlayableAsset;
				if (playableAsset != null)
				{
					TimelineClip timelineClip = TimelineRecording.FindClipWithAsset(this.state.timeline, playableAsset, this.state.currentDirector);
					if (timelineClip != null && timelineClip.IsParameterAnimatable(modifications[i].propertyPath))
					{
						result = true;
						return result;
					}
				}
			}
			foreach (GameObject current in TimelineRecording.GetRecordableGameObjects(this.state))
			{
				for (int j = 0; j < modifications.Length; j++)
				{
					PropertyModification propertyModification = modifications[j];
					if (AnimationWindowUtility.PropertyIsAnimatable(propertyModification.target, propertyModification.propertyPath, current))
					{
						result = true;
						return result;
					}
				}
			}
			result = false;
			return result;
		}

		public bool IsEditable(Object targetObject)
		{
			return true;
		}

		public bool KeyExists(PropertyModification[] modifications)
		{
			return modifications.Length != 0 && !(modifications[0].target == null) && TimelineRecording.HasKey(modifications, modifications[0].target, this.state);
		}

		public bool CandidateExists(PropertyModification[] modifications)
		{
			return true;
		}

		public bool CurveExists(PropertyModification[] modifications)
		{
			return modifications.Length != 0 && !(modifications[0].target == null) && TimelineRecording.HasCurve(modifications, modifications[0].target, this.state);
		}

		public void AddKey(PropertyModification[] modifications)
		{
			TimelineRecording.AddKey(modifications, this.state);
			this.state.Refresh(false);
		}

		public void RemoveKey(PropertyModification[] modifications)
		{
			if (modifications.Length != 0 && !(modifications[0].target == null))
			{
				TimelineRecording.RemoveKey(modifications[0].target, modifications, this.state);
				this.state.Refresh(false);
			}
		}

		public void RemoveCurve(PropertyModification[] modifications)
		{
			if (modifications.Length != 0 && !(modifications[0].target == null))
			{
				TimelineRecording.RemoveCurve(modifications[0].target, modifications, this.state);
				this.state.Refresh(false);
			}
		}

		public void GoToNextKeyframe(PropertyModification[] modifications)
		{
			if (modifications.Length != 0 && !(modifications[0].target == null))
			{
				TimelineRecording.NextKey(modifications[0].target, modifications, this.state);
				this.state.Refresh(false);
			}
		}

		public void GoToPreviousKeyframe(PropertyModification[] modifications)
		{
			TimelineRecording.PrevKey(modifications[0].target, modifications, this.state);
			this.state.Refresh(false);
		}
	}
}
