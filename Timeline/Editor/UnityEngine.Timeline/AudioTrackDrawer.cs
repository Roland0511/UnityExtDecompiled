using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Timeline;

namespace UnityEngine.Timeline
{
	[CustomTrackDrawer(typeof(AudioTrack))]
	internal class AudioTrackDrawer : TrackDrawer
	{
		private static Dictionary<TimelineClip, WaveformPreview> s_PersistentPreviews = new Dictionary<TimelineClip, WaveformPreview>();

		public override Color trackColor
		{
			get
			{
				return DirectorStyles.Instance.customSkin.colorAudio;
			}
		}

		public override GUIContent GetIcon()
		{
			return EditorGUIUtility.IconContent("AudioSource Icon");
		}

		protected override void DrawCustomClipBody(TrackDrawer.ClipDrawData drawData, Rect rect)
		{
			if (drawData.state.showAudioWaveform)
			{
				if (rect.get_width() > 0f)
				{
					TimelineClip clip = drawData.clip;
					AudioClip audioClip = clip.asset as AudioClip;
					if (audioClip == null)
					{
						AudioPlayableAsset audioPlayableAsset = drawData.clip.asset as AudioPlayableAsset;
						if (audioPlayableAsset != null)
						{
							audioClip = audioPlayableAsset.clip;
						}
						if (audioClip == null)
						{
							return;
						}
					}
					Rect rect2 = new Rect(Mathf.Ceil(rect.get_x()), Mathf.Ceil(rect.get_y()), Mathf.Ceil(rect.get_width()), Mathf.Ceil(rect.get_height()));
					WaveformPreview waveformPreview;
					if (!AudioTrackDrawer.s_PersistentPreviews.TryGetValue(drawData.clip, out waveformPreview) || audioClip != waveformPreview.presentedObject)
					{
						WaveformPreview waveformPreview2 = WaveformPreviewFactory.Create((int)rect2.get_width(), audioClip);
						AudioTrackDrawer.s_PersistentPreviews[drawData.clip] = waveformPreview2;
						waveformPreview = waveformPreview2;
						Color colorAudioWaveform = DirectorStyles.Instance.customSkin.colorAudioWaveform;
						Color backgroundColor = colorAudioWaveform;
						backgroundColor.a = 0f;
						waveformPreview.set_backgroundColor(backgroundColor);
						waveformPreview.set_waveColor(colorAudioWaveform);
						waveformPreview.SetChannelMode(0);
						waveformPreview.add_updated(new Action(drawData.state.editorWindow.Repaint));
					}
					waveformPreview.set_looping(drawData.clip.SupportsLooping());
					waveformPreview.SetTimeInfo(drawData.localVisibleStartTime, drawData.localVisibleEndTime - drawData.localVisibleStartTime);
					waveformPreview.OptimizeForSize(rect2.get_size());
					if (Event.get_current().get_type() == 7)
					{
						waveformPreview.ApplyModifications();
						waveformPreview.Render(rect2);
					}
				}
			}
		}
	}
}
