using System;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class TimeAreaItem : Control
	{
		public enum Alignment
		{
			Center,
			Left,
			Right
		}

		private GUIContent m_HeaderContent = new GUIContent();

		private GUIStyle m_Style;

		private Rect m_BoundingRect;

		private Tooltip m_Tooltip;

		private bool m_ShowTooltip;

		public TimeAreaItem.Alignment alignment
		{
			get;
			set;
		}

		public Color headColor
		{
			get;
			set;
		}

		public Color lineColor
		{
			get;
			set;
		}

		public bool dottedLine
		{
			get;
			set;
		}

		public bool drawLine
		{
			get;
			set;
		}

		public bool drawHead
		{
			get;
			set;
		}

		public bool canMoveHead
		{
			get;
			set;
		}

		public string tooltip
		{
			get;
			set;
		}

		public Vector2 boundOffset
		{
			get;
			set;
		}

		private float widgetHeight
		{
			get
			{
				float fixedHeight = this.m_Style.get_fixedHeight();
				float result;
				if (fixedHeight < 1f)
				{
					result = (float)this.m_Style.get_normal().get_background().get_height();
				}
				else
				{
					result = fixedHeight;
				}
				return result;
			}
		}

		private float widgetWidth
		{
			get
			{
				float fixedWidth = this.m_Style.get_fixedWidth();
				float result;
				if (fixedWidth < 1f)
				{
					result = (float)this.m_Style.get_normal().get_background().get_width();
				}
				else
				{
					result = fixedWidth;
				}
				return result;
			}
		}

		public override Rect bounds
		{
			get
			{
				Rect boundingRect = this.m_BoundingRect;
				boundingRect.set_y(TimelineWindow.instance.state.timeAreaRect.get_yMax() - this.widgetHeight);
				boundingRect.set_position(boundingRect.get_position() + this.boundOffset);
				return boundingRect;
			}
		}

		public bool showTooltip
		{
			get
			{
				return this.m_ShowTooltip;
			}
			set
			{
				this.m_ShowTooltip = value;
			}
		}

		public TimeAreaItem(GUIStyle style, Action<double, bool> onDrag)
		{
			this.m_Style = style;
			this.dottedLine = false;
			this.headColor = Color.get_white();
			Scrub m = new Scrub(onDrag);
			base.AddManipulator(m);
			this.lineColor = this.m_Style.get_normal().get_textColor();
			this.drawLine = true;
			this.drawHead = true;
			this.canMoveHead = false;
			this.tooltip = string.Empty;
			this.alignment = TimeAreaItem.Alignment.Center;
			this.boundOffset = Vector2.get_zero();
			this.m_Tooltip = new Tooltip(DirectorStyles.Instance.displayBackground, DirectorStyles.Instance.tinyFont);
			base.MouseUp += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				if (evt.get_button() == 0 && this.showTooltip)
				{
					this.showTooltip = false;
				}
				return false;
			};
		}

		public void Draw(Rect rect, TimelineWindow.TimelineState state, double time)
		{
			Vector2 min = rect.get_min();
			min.y += 4f;
			min.x = state.TimeToPixel(time);
			TimeAreaItem.Alignment alignment = this.alignment;
			if (alignment != TimeAreaItem.Alignment.Center)
			{
				if (alignment != TimeAreaItem.Alignment.Left)
				{
					if (alignment == TimeAreaItem.Alignment.Right)
					{
						this.m_BoundingRect = new Rect(min.x, min.y, this.widgetWidth, this.widgetHeight);
					}
				}
				else
				{
					this.m_BoundingRect = new Rect(min.x - this.widgetWidth, min.y, this.widgetWidth, this.widgetHeight);
				}
			}
			else
			{
				this.m_BoundingRect = new Rect(min.x - this.widgetWidth / 2f, min.y, this.widgetWidth, this.widgetHeight);
			}
			if (Event.get_current().get_type() == 7)
			{
				if (this.m_BoundingRect.get_xMax() < state.timeAreaRect.get_xMin())
				{
					return;
				}
				if (this.m_BoundingRect.get_xMin() > state.timeAreaRect.get_xMax())
				{
					return;
				}
			}
			float num = state.timeAreaRect.get_yMax() - DirectorStyles.kDurationGuiThickness;
			Vector3 p = new Vector3(min.x, num, 0f);
			Vector3 p2 = new Vector3(min.x, num + Mathf.Min(rect.get_height(), state.windowHeight), 0f);
			if (this.drawLine)
			{
				if (this.dottedLine)
				{
					Graphics.DrawDottedLine(p, p2, 5f, this.lineColor);
				}
				else
				{
					Rect rect2 = Rect.MinMaxRect(p.x - 0.5f, p.y, p2.x + 0.5f, p2.y);
					EditorGUI.DrawRect(rect2, this.lineColor);
				}
			}
			if (this.drawHead)
			{
				Color color = GUI.get_color();
				GUI.set_color(this.headColor);
				GUI.Box(this.bounds, this.m_HeaderContent, this.m_Style);
				GUI.set_color(color);
				if (this.canMoveHead)
				{
					EditorGUIUtility.AddCursorRect(this.bounds, 8);
				}
			}
			if (this.showTooltip)
			{
				this.m_Tooltip.text = state.TimeAsString(time, "F2");
				Vector2 position = this.bounds.get_position();
				position.y = state.timeAreaRect.get_y();
				position.y -= this.m_Tooltip.bounds.get_height();
				position.x -= Mathf.Abs(this.m_Tooltip.bounds.get_width() - this.bounds.get_width()) / 2f;
				Rect bounds = this.bounds;
				bounds.set_position(position);
				this.m_Tooltip.bounds = bounds;
				this.m_Tooltip.Draw();
			}
		}
	}
}
