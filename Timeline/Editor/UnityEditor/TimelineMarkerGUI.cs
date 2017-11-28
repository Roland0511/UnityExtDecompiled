using System;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor
{
	internal class TimelineMarkerGUI : TimelineItemGUI, ISnappable, IAttractable
	{
		private int m_ProjectedClipHash;

		private TrackDrawer.MarkerDrawData m_MarkerDrawData;

		public TimelineMarker timelineMarker
		{
			get
			{
				return (TimelineMarker)this.m_EditorItem.item;
			}
		}

		public double triggerTime
		{
			get
			{
				return this.timelineMarker.time;
			}
			set
			{
				this.timelineMarker.time = value;
			}
		}

		public double start
		{
			get
			{
				return this.timelineMarker.time;
			}
		}

		public double end
		{
			get
			{
				return this.timelineMarker.time;
			}
		}

		public TimelineMarkerGUI(TimelineMarker theMarker, TimelineAsset timeline, TimelineTrackGUI parent) : base(parent)
		{
			this.m_EditorItem = EditorItemFactory.GetEditorMarker(theMarker);
			if (parent.drawer != null)
			{
				parent.drawer.ConfigureUIEvent(this);
			}
		}

		public override bool OnEvent(Event evt, TimelineWindow.TimelineState state, bool isCaptureSession)
		{
			return (base.parentTrackGUI == null || !base.parentTrackGUI.track.locked) && base.OnEvent(evt, state, isCaptureSession);
		}

		private int ComputeDirtyHash()
		{
			return this.timelineMarker.time.GetHashCode();
		}

		private void DrawEventByDrawer(Rect drawRect, GUIStyle style, TimelineWindow.TimelineState state)
		{
			this.m_MarkerDrawData.uiMarker = this;
			this.m_MarkerDrawData.timelineMarker = this.timelineMarker;
			this.m_MarkerDrawData.targetRect = drawRect;
			this.m_MarkerDrawData.unclippedRect = base.UnClippedRect;
			this.m_MarkerDrawData.selected = SelectionManager.Contains(this.timelineMarker);
			this.m_MarkerDrawData.style = style;
			this.m_MarkerDrawData.state = state;
			this.m_MarkerDrawData.selectedStyle = this.m_Styles.selectedStyle;
			base.parentTrackGUI.drawer.DrawEvent(this.m_MarkerDrawData);
		}

		public void DrawInto(Rect drawRect, GUIStyle style, TimelineWindow.TimelineState state)
		{
			GUI.BeginClip(drawRect);
			Rect drawRect2 = drawRect;
			drawRect2.set_x(0f);
			drawRect2.set_y(0f);
			this.DrawEventByDrawer(drawRect2, style, state);
			GUI.EndClip();
		}

		private void CalculateClipRectangle(TrackAsset parentTrackAsset, Rect trackRect, TimelineWindow.TimelineState state, int projectedClipHash)
		{
			if (this.m_ProjectedClipHash == projectedClipHash)
			{
				if (Event.get_current().get_type() == 7 && !parentTrackAsset.locked)
				{
					state.quadTree.Insert(this);
				}
			}
			else
			{
				this.m_ProjectedClipHash = projectedClipHash;
				Rect rect = this.RectToTimeline(trackRect, state);
				Rect rect2 = rect;
				rect2.set_width(DirectorStyles.Instance.eventIcon.get_fixedWidth());
				rect2.set_height(DirectorStyles.Instance.eventIcon.get_fixedHeight());
				rect2.set_x(rect2.get_x() - 0.5f * rect2.get_width());
				base.rect = rect2;
				this.m_UnclippedRect = rect;
				if (Event.get_current().get_type() == 7 && !parentTrackAsset.locked)
				{
					state.quadTree.Insert(this);
				}
				if (rect.get_x() < trackRect.get_xMin())
				{
					float num = trackRect.get_xMin() - rect.get_x();
					rect.set_x(trackRect.get_xMin());
					rect.set_width(rect.get_width() - num);
				}
				base.clippedRect = rect;
				if (rect.get_xMax() >= trackRect.get_xMin())
				{
					if (rect.get_xMin() <= trackRect.get_xMax())
					{
						if (base.clippedRect.get_width() < 2f)
						{
							this.m_ClippedRect.set_width(5f);
						}
					}
				}
			}
		}

		public virtual void Draw(Rect trackRect, TimelineWindow.TimelineState state, TrackDrawer drawer)
		{
			int num = this.ComputeDirtyHash();
			int num2 = state.timeAreaTranslation.GetHashCode() ^ state.timeAreaScale.GetHashCode() ^ trackRect.GetHashCode();
			this.CalculateClipRectangle(base.parentTrackGUI.track, trackRect, state, num ^ num2);
			this.DrawInto(base.rect, DirectorStyles.Instance.eventIcon, state);
		}

		public Rect RectToTimeline(Rect trackRect, TimelineWindow.TimelineState state)
		{
			Rect result = new Rect((float)this.timelineMarker.time * state.timeAreaScale.x, 0f, 1f * state.timeAreaScale.x, 0f);
			result.set_xMin(result.get_xMin() + (state.timeAreaTranslation.x + trackRect.get_xMin()));
			result.set_xMax(result.get_xMax() + (state.timeAreaTranslation.x + trackRect.get_xMin()));
			result.set_y(trackRect.get_y() + 2f);
			result.set_height(trackRect.get_height() - 4f);
			result.set_y(trackRect.get_y());
			result.set_height(trackRect.get_height());
			return result;
		}

		public IEnumerable<Edge> SnappableEdgesFor(IAttractable attractable)
		{
			List<Edge> list = new List<Edge>();
			IEnumerable<Edge> result;
			if (attractable == this)
			{
				result = list;
			}
			else
			{
				TimelineMarkerGUI timelineMarkerGUI = attractable as TimelineMarkerGUI;
				bool flag = (timelineMarkerGUI == null || !(timelineMarkerGUI.parentTrackGUI.track == base.parentTrackGUI.track)) && (!base.parentTrackGUI.get_hasChildren() || base.parentTrackGUI.isExpanded);
				if (flag)
				{
					list.Add(new Edge(this.timelineMarker.time));
				}
				result = list;
			}
			return result;
		}
	}
}
