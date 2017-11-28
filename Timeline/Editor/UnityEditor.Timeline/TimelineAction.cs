using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal abstract class TimelineAction : MenuItemActionBase
	{
		private static List<TimelineAction> s_ActionClasses;

		private static List<TimelineAction> actions
		{
			get
			{
				if (TimelineAction.s_ActionClasses == null)
				{
					TimelineAction.s_ActionClasses = (from x in MenuItemActionBase.GetActionsOfType(typeof(TimelineAction))
					select (TimelineAction)x.GetConstructors()[0].Invoke(null)).ToList<TimelineAction>();
				}
				return TimelineAction.s_ActionClasses;
			}
		}

		public abstract bool Execute(TimelineWindow.TimelineState state);

		public virtual MenuActionDisplayState GetDisplayState(TimelineWindow.TimelineState state)
		{
			return MenuActionDisplayState.Visible;
		}

		public virtual bool CanExecute(TimelineWindow.TimelineState state)
		{
			return this.GetDisplayState(state) == MenuActionDisplayState.Visible;
		}

		public static bool InvokeByName(string actionName, TimelineWindow.TimelineState state)
		{
			TimelineAction timelineAction = TimelineAction.actions.FirstOrDefault((TimelineAction x) => x.GetType().Name == actionName);
			return timelineAction != null && timelineAction.CanExecute(state) && timelineAction.Execute(state);
		}

		public static void AddToMenu(GenericMenu menu, TimelineWindow.TimelineState state)
		{
			TimelineAction.actions.ForEach(delegate(TimelineAction action)
			{
				string text = string.Empty;
				CategoryAttribute categoryAttribute = MenuItemActionBase.GetCategoryAttribute(action);
				if (categoryAttribute == null)
				{
					text = string.Empty;
				}
				else
				{
					text = categoryAttribute.Category;
					if (!text.EndsWith("/"))
					{
						text += "/";
					}
				}
				string displayName = MenuItemActionBase.GetDisplayName(action);
				string text2 = text + displayName;
				SeparatorMenuItemAttribute separator = MenuItemActionBase.GetSeparator(action);
				bool flag = !MenuItemActionBase.IsHiddenInMenu(action);
				if (flag)
				{
					MenuActionDisplayState displayState = action.GetDisplayState(state);
					if (displayState == MenuActionDisplayState.Visible)
					{
						menu.AddItem(new GUIContent(text2), false, delegate(object f)
						{
							action.Execute(state);
						}, action);
					}
					if (displayState == MenuActionDisplayState.Disabled)
					{
						menu.AddDisabledItem(new GUIContent(text2));
					}
					if (displayState != MenuActionDisplayState.Hidden && separator != null && separator.after)
					{
						menu.AddSeparator(text);
					}
				}
			});
		}

		public static bool HandleShortcut(TimelineWindow.TimelineState state, Event evt)
		{
			bool result;
			if (EditorGUI.IsEditingTextField())
			{
				result = false;
			}
			else
			{
				foreach (TimelineAction current in TimelineAction.actions)
				{
					object[] customAttributes = current.GetType().GetCustomAttributes(typeof(ShortcutAttribute), true);
					object[] array = customAttributes;
					for (int i = 0; i < array.Length; i++)
					{
						ShortcutAttribute shortcutAttribute = (ShortcutAttribute)array[i];
						if (shortcutAttribute.IsRecognized(evt))
						{
							if (MenuItemActionBase.s_ShowActionTriggeredByShortcut)
							{
								Debug.Log(current.GetType().Name);
							}
							result = current.Execute(state);
							return result;
						}
					}
				}
				result = false;
			}
			return result;
		}

		protected static bool DoInternal(Type t, TimelineWindow.TimelineState state)
		{
			TimelineAction timelineAction = (TimelineAction)t.GetConstructors()[0].Invoke(null);
			return timelineAction.CanExecute(state) && timelineAction.Execute(state);
		}
	}
}
