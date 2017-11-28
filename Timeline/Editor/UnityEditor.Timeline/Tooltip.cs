using System;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class Tooltip
	{
		private GUIStyle m_Font;

		private float m_Pad = 4f;

		private GUIContent m_TextContent;

		private Color m_ForeColor = Color.get_white();

		private Rect m_Bounds;

		public GUIStyle style
		{
			get;
			set;
		}

		public string text
		{
			get;
			set;
		}

		public GUIStyle font
		{
			get
			{
				GUIStyle result;
				if (this.m_Font != null)
				{
					result = this.m_Font;
				}
				else if (this.style != null)
				{
					result = this.style;
				}
				else
				{
					this.m_Font = new GUIStyle();
					this.m_Font.set_font(EditorStyles.get_label().get_font());
					result = this.m_Font;
				}
				return result;
			}
			set
			{
				this.m_Font = value;
			}
		}

		public float pad
		{
			get
			{
				return this.m_Pad;
			}
			set
			{
				this.m_Pad = value;
			}
		}

		private GUIContent textContent
		{
			get
			{
				if (this.m_TextContent == null)
				{
					this.m_TextContent = new GUIContent();
				}
				this.m_TextContent.set_text(this.text);
				return this.m_TextContent;
			}
		}

		public Color foreColor
		{
			get
			{
				return this.m_ForeColor;
			}
			set
			{
				this.m_ForeColor = value;
			}
		}

		public Rect bounds
		{
			get
			{
				Vector2 vector = this.font.CalcSize(this.textContent);
				this.m_Bounds.set_width(vector.x + 2f * this.pad);
				this.m_Bounds.set_height(vector.y + 2f);
				return this.m_Bounds;
			}
			set
			{
				this.m_Bounds = value;
			}
		}

		public Tooltip(GUIStyle theStyle, GUIStyle font)
		{
			this.style = theStyle;
			this.m_Font = font;
		}

		public Tooltip()
		{
			this.style = null;
			this.m_Font = null;
		}

		public void Draw()
		{
			if (!string.IsNullOrEmpty(this.text))
			{
				if (this.style != null)
				{
					using (new GUIColorOverride(DirectorStyles.Instance.customSkin.colorTooltipBackground))
					{
						GUI.Label(this.bounds, GUIContent.none, this.style);
					}
				}
				Rect bounds = this.bounds;
				bounds.set_x(bounds.get_x() + this.pad);
				bounds.set_width(bounds.get_width() - this.pad);
				using (new GUIColorOverride(this.foreColor))
				{
					GUI.Label(bounds, this.textContent, this.font);
				}
			}
		}
	}
}
