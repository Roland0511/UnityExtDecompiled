using System;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal struct GUIColorOverride : IDisposable
	{
		private readonly Color m_OldColor;

		public GUIColorOverride(Color newColor)
		{
			this.m_OldColor = GUI.get_color();
			GUI.set_color(newColor);
		}

		public void Dispose()
		{
			GUI.set_color(this.m_OldColor);
		}
	}
}
