using System;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal static class Shortcuts
	{
		public static void AddAction(string name, KeyCode key, EventModifiers modifiers = 0)
		{
			UserAction.AddBinding(name, key, modifiers);
		}

		public static void AddAction(string name, KeyCode[] keys, EventModifiers modifiers = 0)
		{
			for (int i = 0; i < keys.Length; i++)
			{
				KeyCode actionKey = keys[i];
				UserAction.AddBinding(name, actionKey, modifiers);
			}
		}

		public static bool IsPressed(string actionName, Event evt)
		{
			return UserAction.GetAction(actionName).IsPressed(evt);
		}

		public static bool IsReleased(string actionName, Event evt)
		{
			return UserAction.GetAction(actionName).IsReleased(evt);
		}
	}
}
