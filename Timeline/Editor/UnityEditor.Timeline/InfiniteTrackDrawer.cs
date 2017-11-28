using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class InfiniteTrackDrawer : TrackDrawer
	{
		private readonly IPropertyKeyDataSource m_DataSource;

		private Rect m_TrackRect;

		public InfiniteTrackDrawer(IPropertyKeyDataSource dataSource)
		{
			this.m_DataSource = dataSource;
		}

		public bool CanDraw(TrackAsset track, TimelineWindow.TimelineState state)
		{
			float[] keys = this.m_DataSource.GetKeys();
			bool flag = track.clips.Length == 0;
			return keys != null || (state.IsArmedForRecord(track) && flag);
		}

		private static void DrawRecordBackground(Rect trackRect)
		{
			DirectorStyles instance = DirectorStyles.Instance;
			EditorGUI.DrawRect(trackRect, instance.customSkin.colorInfiniteTrackBackgroundRecording);
			Graphics.ShadowLabel(trackRect, instance.Elipsify(DirectorStyles.recordingLabel.get_text(), trackRect, instance.fontClip), instance.fontClip, Color.get_white(), Color.get_black());
		}

		public override bool DrawTrack(Rect trackRect, TrackAsset trackAsset, Vector2 visibleTime, ITimelineState state)
		{
			this.m_TrackRect = trackRect;
			TimelineWindow.TimelineState timelineState = (TimelineWindow.TimelineState)state;
			bool result;
			if (!this.CanDraw(trackAsset, timelineState))
			{
				result = true;
			}
			else
			{
				if (timelineState.recording && timelineState.IsArmedForRecord(trackAsset))
				{
					InfiniteTrackDrawer.DrawRecordBackground(trackRect);
				}
				GUI.Box(trackRect, GUIContent.none, DirectorStyles.Instance.infiniteTrack);
				Rect rect = trackRect;
				rect.set_yMin(rect.get_yMax());
				rect.set_height(15f);
				GUI.DrawTexture(rect, DirectorStyles.Instance.bottomShadow.get_normal().get_background(), 0);
				float[] keys = this.m_DataSource.GetKeys();
				if (keys != null && keys.Length > 0)
				{
					float[] array = keys;
					for (int i = 0; i < array.Length; i++)
					{
						float key = array[i];
						this.DrawKeyFrame(key, timelineState);
					}
				}
				result = true;
			}
			return result;
		}

		private void DrawKeyFrame(float key, TimelineWindow.TimelineState state)
		{
			float num = state.TimeToPixel((double)key);
			Rect rect = new Rect(num, this.m_TrackRect.get_yMin() + 3f, 1f, this.m_TrackRect.get_height() - 6f);
			if (this.m_TrackRect.Overlaps(rect))
			{
				float fixedWidth = DirectorStyles.Instance.keyframe.get_fixedWidth();
				float fixedHeight = DirectorStyles.Instance.keyframe.get_fixedHeight();
				Rect rect2 = rect;
				rect2.set_width(fixedWidth);
				rect2.set_height(fixedHeight);
				rect2.set_xMin(rect2.get_xMin() - fixedWidth / 2f);
				rect2.set_yMin(this.m_TrackRect.get_yMin() + (this.m_TrackRect.get_height() - fixedHeight) / 2f);
				GUI.Label(rect2, GUIContent.none, DirectorStyles.Instance.keyframe);
				EditorGUI.DrawRect(rect, DirectorStyles.Instance.customSkin.colorInfiniteClipLine);
			}
		}
	}
}
