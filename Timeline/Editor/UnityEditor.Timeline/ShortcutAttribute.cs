using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Timeline
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
	internal class ShortcutAttribute : Attribute
	{
		private readonly string m_ActionName;

		public ShortcutAttribute(string actionName)
		{
			this.m_ActionName = actionName;
		}

		public bool IsRecognized(Event evt)
		{
			return Shortcuts.IsPressed(this.m_ActionName, evt);
		}

		public override string ToString()
		{
			List<UserAction.BindingSource> bindings = UserAction.GetAction(this.m_ActionName).bindings;
			return (bindings.Count <= 0) ? "" : bindings[0].ToString();
		}
	}
}
