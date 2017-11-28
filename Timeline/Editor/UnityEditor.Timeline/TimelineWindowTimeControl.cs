using System;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class TimelineWindowTimeControl : IAnimationWindowControl
	{
		[Serializable]
		public struct ClipData
		{
			public float start;

			public float duration;

			public TrackAsset track;
		}

		[SerializeField]
		private TimelineWindowTimeControl.ClipData m_ClipData;

		[SerializeField]
		private TimelineClip m_Clip;

		[SerializeField]
		private AnimationWindowState m_AnimWindowState;

		public float start
		{
			get
			{
				float result;
				if (this.m_Clip != null)
				{
					result = (float)this.m_Clip.start;
				}
				else
				{
					result = this.m_ClipData.start;
				}
				return result;
			}
		}

		public float duration
		{
			get
			{
				float result;
				if (this.m_Clip != null)
				{
					result = (float)this.m_Clip.duration;
				}
				else
				{
					result = this.m_ClipData.duration;
				}
				return result;
			}
		}

		public TrackAsset track
		{
			get
			{
				TrackAsset result;
				if (this.m_Clip != null)
				{
					result = this.m_Clip.parentTrack;
				}
				else
				{
					result = this.m_ClipData.track;
				}
				return result;
			}
		}

		public TimelineWindow window
		{
			get
			{
				return TimelineWindow.instance;
			}
		}

		public TimelineWindow.TimelineState state
		{
			get
			{
				TimelineWindow.TimelineState result;
				if (this.window != null)
				{
					result = this.window.state;
				}
				else
				{
					result = null;
				}
				return result;
			}
		}

		public override AnimationKeyTime time
		{
			get
			{
				AnimationKeyTime result;
				if (this.state == null)
				{
					result = AnimationKeyTime.Time(0f, 0f);
				}
				else
				{
					result = AnimationKeyTime.Time((float)this.state.time - this.start, this.state.frameRate);
				}
				return result;
			}
		}

		public override bool canPlay
		{
			get
			{
				return this.state != null && this.state.previewMode;
			}
		}

		public override bool playing
		{
			get
			{
				return this.state != null && this.state.playing;
			}
		}

		public override bool canRecord
		{
			get
			{
				return this.state != null && this.state.canRecord;
			}
		}

		public override bool recording
		{
			get
			{
				return this.state != null && this.state.recording;
			}
		}

		public override bool canPreview
		{
			get
			{
				return false;
			}
		}

		public override bool previewing
		{
			get
			{
				return false;
			}
		}

		private void OnStateChange(object sender, TimelineWindow.StateEventArgs arg)
		{
			if (arg.state.dirtyStamp > 0 && this.m_AnimWindowState != null)
			{
				this.m_AnimWindowState.Repaint();
			}
		}

		public void Init(TimelineWindow window, AnimationWindowState state, TimelineClip clip)
		{
			this.m_Clip = clip;
			this.m_AnimWindowState = state;
		}

		public void Init(TimelineWindow window, AnimationWindowState state, TimelineWindowTimeControl.ClipData clip)
		{
			this.m_ClipData = clip;
			this.m_AnimWindowState = state;
		}

		public override void OnEnable()
		{
			if (this.window != null)
			{
				this.window.OnStateChange += new EventHandler<TimelineWindow.StateEventArgs>(this.OnStateChange);
			}
			base.OnEnable();
		}

		public void OnDisable()
		{
			if (this.window != null)
			{
				this.window.OnStateChange -= new EventHandler<TimelineWindow.StateEventArgs>(this.OnStateChange);
			}
		}

		private void ChangeTime(float time)
		{
			if (this.state != null && this.state.currentDirector != null)
			{
				this.state.time = (double)time + (double)this.start;
				this.window.Repaint();
			}
		}

		private void ChangeFrame(int frame)
		{
			if (this.state != null)
			{
				this.state.frame = frame;
				this.window.Repaint();
			}
		}

		public override void GoToTime(float time)
		{
			this.ChangeTime(time);
		}

		public override void GoToFrame(int frame)
		{
			this.ChangeFrame(frame);
		}

		public override void StartScrubTime()
		{
		}

		public override void EndScrubTime()
		{
		}

		public override void ScrubTime(float time)
		{
			this.ChangeTime(time);
		}

		public override void GoToPreviousFrame()
		{
			if (this.state != null)
			{
				this.ChangeFrame(this.state.frame - 1);
			}
		}

		public override void GoToNextFrame()
		{
			if (this.state != null)
			{
				this.ChangeFrame(this.state.frame + 1);
			}
		}

		private AnimationWindowCurve[] GetCurves()
		{
			List<AnimationWindowCurve> list = (!this.m_AnimWindowState.showCurveEditor || this.m_AnimWindowState.get_activeCurves().Count <= 0) ? this.m_AnimWindowState.get_allCurves() : this.m_AnimWindowState.get_activeCurves();
			return list.ToArray();
		}

		public override void GoToPreviousKeyframe()
		{
			float previousKeyframeTime = AnimationWindowUtility.GetPreviousKeyframeTime(this.GetCurves(), this.get_time().get_time(), this.m_AnimWindowState.get_clipFrameRate());
			this.GoToTime(this.m_AnimWindowState.SnapToFrame(previousKeyframeTime, 2));
		}

		public override void GoToNextKeyframe()
		{
			float nextKeyframeTime = AnimationWindowUtility.GetNextKeyframeTime(this.GetCurves(), this.get_time().get_time(), this.m_AnimWindowState.get_clipFrameRate());
			this.GoToTime(this.m_AnimWindowState.SnapToFrame(nextKeyframeTime, 2));
		}

		public override void GoToFirstKeyframe()
		{
			this.GoToTime(0f);
		}

		public override void GoToLastKeyframe()
		{
			this.GoToTime(this.duration);
		}

		private void SetPlaybackState(bool playbackState)
		{
			if (this.state != null && playbackState != this.state.playing)
			{
				TimelineWindow.instance.Simulate(playbackState);
				this.state.playing = playbackState;
			}
		}

		public override bool StartPlayback()
		{
			this.SetPlaybackState(true);
			return this.state != null && this.state.playing;
		}

		public override void StopPlayback()
		{
			this.SetPlaybackState(false);
		}

		public override bool PlaybackUpdate()
		{
			return false;
		}

		public override bool StartRecording(Object targetObject)
		{
			bool result;
			if (!this.get_canRecord())
			{
				result = false;
			}
			else if (Application.get_isPlaying())
			{
				result = false;
			}
			else if (this.state != null && this.track != null)
			{
				this.state.ArmForRecord(this.track);
				result = this.state.recording;
			}
			else
			{
				result = false;
			}
			return result;
		}

		public override void StopRecording()
		{
			if (!Application.get_isPlaying())
			{
				if (this.state != null && this.track != null)
				{
					this.state.UnarmForRecord(this.track);
				}
			}
		}

		public override void OnSelectionChanged()
		{
		}

		public override void ResampleAnimation()
		{
		}

		public override bool StartPreview()
		{
			if (this.state != null)
			{
				this.state.previewMode = true;
			}
			return this.state.previewMode;
		}

		public override void StopPreview()
		{
			if (this.state != null)
			{
				this.state.previewMode = false;
			}
		}

		public override void ProcessCandidates()
		{
		}

		public override void ClearCandidates()
		{
		}
	}
}
