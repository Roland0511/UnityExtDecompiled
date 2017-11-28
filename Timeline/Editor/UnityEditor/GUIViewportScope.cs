using System;
using UnityEngine;

namespace UnityEditor
{
	internal struct GUIViewportScope : IDisposable
	{
		private bool m_open;

		public GUIViewportScope(Rect position)
		{
			this.m_open = false;
			if (Event.get_current().get_type() == 7 || Event.get_current().get_type() == 8)
			{
				GUI.BeginClip(position, -position.get_min(), Vector2.get_zero(), false);
				this.m_open = true;
			}
		}

		public void Dispose()
		{
			this.CloseScope();
		}

		private void CloseScope()
		{
			if (this.m_open)
			{
				GUI.EndClip();
				this.m_open = false;
			}
		}
	}
}
