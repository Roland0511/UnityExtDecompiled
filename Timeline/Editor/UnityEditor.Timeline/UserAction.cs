using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class UserAction
	{
		internal class BindingSource
		{
			public KeyCode key;

			public EventModifiers modifiers;

			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				if (this.modifiers != null)
				{
					if ((this.modifiers & 1) == 1)
					{
						stringBuilder.Append("Shift");
					}
					else if (Application.get_platform() == 7 && (this.modifiers & 2) == 2)
					{
						stringBuilder.Append("Ctrl");
					}
					else if (Application.get_platform() == null && (this.modifiers & 8) == 8)
					{
						stringBuilder.Append("Cmd");
					}
					else if ((this.modifiers & 4) == 4)
					{
						stringBuilder.Append("Alt");
					}
				}
				if (this.key != null)
				{
					if (this.modifiers != null && this.modifiers != 64)
					{
						stringBuilder.Append("+");
					}
					stringBuilder.Append(Enum.GetName(typeof(KeyCode), this.key));
				}
				return stringBuilder.ToString();
			}
		}

		private static readonly Dictionary<string, UserAction> s_Actions = new Dictionary<string, UserAction>();

		private readonly List<UserAction.BindingSource> m_Bindings = new List<UserAction.BindingSource>();

		public List<UserAction.BindingSource> bindings
		{
			get
			{
				return this.m_Bindings;
			}
		}

		public static UserAction GetAction(string actionName)
		{
			UserAction userAction;
			UserAction result;
			if (UserAction.s_Actions.TryGetValue(actionName, out userAction))
			{
				result = userAction;
			}
			else
			{
				userAction = new UserAction();
				UserAction.s_Actions[actionName] = userAction;
				result = userAction;
			}
			return result;
		}

		public static void AddBinding(string actionName, KeyCode actionKey, EventModifiers actionModifiers = 0)
		{
			UserAction.GetAction(actionName).m_Bindings.Add(new UserAction.BindingSource
			{
				key = actionKey,
				modifiers = actionModifiers
			});
		}

		public bool IsPressed(Event evt)
		{
			return evt.get_type() == 4 && this.m_Bindings.Any((UserAction.BindingSource x) => x.key == evt.get_keyCode() && x.modifiers == evt.get_modifiers());
		}

		public bool IsReleased(Event evt)
		{
			return evt.get_type() == 5 && this.m_Bindings.Any((UserAction.BindingSource x) => x.key == evt.get_keyCode() && x.modifiers == evt.get_modifiers());
		}
	}
}
