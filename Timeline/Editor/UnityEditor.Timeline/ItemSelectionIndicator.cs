using System;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class ItemSelectionIndicator
	{
		private Tooltip m_BeginSelectionTooltip = new Tooltip(DirectorStyles.Instance.displayBackground, DirectorStyles.Instance.tinyFont);

		private Tooltip m_EndSelectionTooltip = new Tooltip(DirectorStyles.Instance.displayBackground, DirectorStyles.Instance.tinyFont);

		public void Draw(double beginTime, double endTime, MagnetEngine magnetEngine)
		{
			TimelineWindow.TimelineState state = TimelineWindow.instance.state;
			Rect timeAreaBounds = TimelineWindow.instance.timeAreaBounds;
			timeAreaBounds.set_xMin(Mathf.Max(TimelineWindow.instance.timeAreaBounds.get_xMin(), state.TimeToTimeAreaPixel(beginTime)));
			timeAreaBounds.set_xMax(state.TimeToTimeAreaPixel(endTime));
			Rect position = TimelineWindow.instance.get_position();
			using (new GUIViewportScope(TimelineWindow.instance.timeAreaBounds))
			{
				Color textColor = DirectorStyles.Instance.selectedStyle.get_focused().get_textColor();
				textColor.a = 0.12f;
				EditorGUI.DrawRect(timeAreaBounds, textColor);
				this.m_BeginSelectionTooltip.text = state.TimeAsString(beginTime, "F2");
				this.m_EndSelectionTooltip.text = state.TimeAsString(endTime, "F2");
				Rect bounds = this.m_BeginSelectionTooltip.bounds;
				bounds.set_xMin(timeAreaBounds.get_xMin() - bounds.get_width() / 2f);
				bounds.set_y(timeAreaBounds.get_y());
				this.m_BeginSelectionTooltip.bounds = bounds;
				bounds = this.m_EndSelectionTooltip.bounds;
				bounds.set_xMin(timeAreaBounds.get_xMax() - bounds.get_width() / 2f);
				bounds.set_y(timeAreaBounds.get_y());
				this.m_EndSelectionTooltip.bounds = bounds;
				if (beginTime >= 0.0)
				{
					this.m_BeginSelectionTooltip.Draw();
				}
				this.m_EndSelectionTooltip.Draw();
			}
			if (beginTime >= 0.0)
			{
				if (magnetEngine == null || !magnetEngine.IsSnappedAtTime((double)state.PixelToTime(timeAreaBounds.get_xMin())))
				{
					Graphics.DrawDottedLine(new Vector3(timeAreaBounds.get_xMin(), timeAreaBounds.get_yMax(), 0f), new Vector3(timeAreaBounds.get_xMin(), timeAreaBounds.get_yMax() + position.get_height()), 4f, Color.get_black());
				}
				if (magnetEngine == null || !magnetEngine.IsSnappedAtTime((double)state.PixelToTime(timeAreaBounds.get_xMax())))
				{
					Graphics.DrawDottedLine(new Vector3(timeAreaBounds.get_xMax(), timeAreaBounds.get_yMax(), 0f), new Vector3(timeAreaBounds.get_xMax(), timeAreaBounds.get_yMax() + position.get_height()), 4f, Color.get_black());
				}
			}
		}
	}
}
