using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Timeline;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor
{
	internal class TimelineClipUnion
	{
		public List<TimelineClipGUI> m_Members = new List<TimelineClipGUI>();

		public Rect m_BoundingRect;

		public Rect m_Union;

		public double m_Start;

		public double m_Duration;

		public bool m_InitUnionRect = true;

		public void Add(TimelineClipGUI clip)
		{
			this.m_Members.Add(clip);
			if (this.m_Members.Count == 1)
			{
				this.m_BoundingRect = clip.clippedRect;
			}
			else
			{
				this.m_BoundingRect = RectUtils.Encompass(this.m_BoundingRect, clip.bounds);
			}
		}

		public void Draw(Rect parentRect, TimelineWindow.TimelineState state)
		{
			if (this.m_InitUnionRect)
			{
				this.m_Start = (from c in this.m_Members
				orderby c.clip.start
				select c).First<TimelineClipGUI>().clip.start;
				this.m_Duration = this.m_Members.Sum((TimelineClipGUI c) => c.clip.duration);
				this.m_InitUnionRect = false;
			}
			this.m_Union = new Rect((float)this.m_Start * state.timeAreaScale.x, 0f, (float)this.m_Duration * state.timeAreaScale.x, 0f);
			this.m_Union.set_xMin(this.m_Union.get_xMin() + (state.timeAreaTranslation.x + parentRect.get_x()));
			this.m_Union.set_xMax(this.m_Union.get_xMax() + (state.timeAreaTranslation.x + parentRect.get_x()));
			this.m_Union.set_y(parentRect.get_y() + 4f);
			this.m_Union.set_height(parentRect.get_height() - 8f);
			if (this.m_Union.get_x() < parentRect.get_xMin())
			{
				float num = parentRect.get_xMin() - this.m_Union.get_x();
				this.m_Union.set_x(parentRect.get_xMin());
				this.m_Union.set_width(this.m_Union.get_width() - num);
			}
			if (this.m_Union.get_xMax() >= parentRect.get_xMin())
			{
				if (this.m_Union.get_xMin() <= parentRect.get_xMax())
				{
					EditorGUI.DrawRect(this.m_Union, DirectorStyles.Instance.customSkin.colorClipUnion);
				}
			}
		}

		public static List<TimelineClipUnion> Build(List<TimelineClipGUI> clips)
		{
			List<TimelineClipUnion> list = new List<TimelineClipUnion>();
			List<TimelineClipUnion> result;
			if (clips == null)
			{
				result = list;
			}
			else
			{
				TimelineClipUnion timelineClipUnion = null;
				foreach (TimelineClipGUI current in clips)
				{
					Rect rect;
					if (timelineClipUnion == null)
					{
						timelineClipUnion = new TimelineClipUnion();
						timelineClipUnion.Add(current);
						list.Add(timelineClipUnion);
					}
					else if (RectUtils.Intersection(current.bounds, timelineClipUnion.m_BoundingRect, ref rect))
					{
						timelineClipUnion.Add(current);
					}
					else
					{
						timelineClipUnion = new TimelineClipUnion();
						timelineClipUnion.Add(current);
						list.Add(timelineClipUnion);
					}
				}
				result = list;
			}
			return result;
		}
	}
}
