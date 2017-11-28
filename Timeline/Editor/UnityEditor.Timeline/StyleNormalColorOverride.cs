using System;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class StyleNormalColorOverride : IDisposable
	{
		private readonly GUIStyle m_Style;

		private readonly Color m_OldColor;

		public StyleNormalColorOverride(GUIStyle style, Color newColor)
		{
			this.m_Style = style;
			this.m_OldColor = style.get_normal().get_textColor();
			style.get_normal().set_textColor(newColor);
		}

		public void Dispose()
		{
			this.m_Style.get_normal().set_textColor(this.m_OldColor);
		}
	}
}
