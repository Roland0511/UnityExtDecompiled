using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal interface ITimelineState
	{
		bool timeInFrames
		{
			get;
			set;
		}

		Vector2 playRangeTime
		{
			get;
			set;
		}

		bool canRecord
		{
			get;
		}

		bool recording
		{
			get;
		}

		bool playing
		{
			get;
			set;
		}

		bool previewMode
		{
			get;
			set;
		}

		float playbackSpeed
		{
			get;
			set;
		}

		float frameRate
		{
			get;
			set;
		}

		double time
		{
			get;
			set;
		}

		int frame
		{
			get;
			set;
		}

		bool showAudioWaveform
		{
			get;
			set;
		}

		TimelineAsset timeline
		{
			get;
		}

		TrackAsset rootTrack
		{
			get;
		}

		EditorWindow editorWindow
		{
			get;
		}

		PlayableDirector currentDirector
		{
			get;
			set;
		}

		bool rebuildGraph
		{
			get;
			set;
		}

		void Refresh();

		float TimeToTimeAreaPixel(double time);

		float TimeToScreenSpacePixel(double time);

		float TimeToPixel(double time);

		float PixelToTime(float pixel);

		string TimeAsString(double timeValue, string format);

		float TimeAreaPixelToTime(float pixel);

		float ScreenSpacePixelToTimeAreaTime(float pixel);

		Component GetBindingForTrack(TrackAsset trackAsset);

		void AddStartFrameDelegate(PendingUpdateDelegate callback);

		void AddEndFrameDelegate(PendingUpdateDelegate callback);

		void SetCurrentSequence(TimelineAsset asset);

		double SnapToFrameIfRequired(double time);
	}
}
