using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal abstract class ItemAction<T> : MenuItemActionBase where T : ITimelineItem
	{
		private static List<ItemAction<T>> s_ActionClasses;

		public static List<ItemAction<T>> actions
		{
			get
			{
				if (ItemAction<T>.s_ActionClasses == null)
				{
					ItemAction<T>.s_ActionClasses = (from x in MenuItemActionBase.GetActionsOfType(typeof(ItemAction<T>))
					select (ItemAction<T>)x.GetConstructors()[0].Invoke(null)).ToList<ItemAction<T>>();
				}
				return ItemAction<T>.s_ActionClasses;
			}
		}

		public abstract bool Execute(TimelineWindow.TimelineState state, T[] items);

		public virtual MenuActionDisplayState GetDisplayState(TimelineWindow.TimelineState state, T[] items)
		{
			return (items.Length <= 0) ? MenuActionDisplayState.Disabled : MenuActionDisplayState.Visible;
		}

		public virtual bool CanExecute(TimelineWindow.TimelineState state, T[] items)
		{
			return this.GetDisplayState(state, items) == MenuActionDisplayState.Visible;
		}

		public static bool HandleShortcut(TimelineWindow.TimelineState state, Event evt, T item)
		{
			T[] items = new T[]
			{
				item
			};
			bool result;
			foreach (ItemAction<T> current in ItemAction<T>.actions)
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
						bool flag = current.Execute(state, items);
						state.Refresh();
						state.Evaluate();
						result = flag;
						return result;
					}
				}
			}
			result = false;
			return result;
		}

		public static void AddToMenu(GenericMenu menu, TimelineWindow.TimelineState state)
		{
			T[] items = (from x in SelectionManager.SelectedItemGUI()
			select x.selectableObject).OfType<T>().ToArray<T>();
			if (items.Length >= 1)
			{
				menu.AddSeparator("");
				ItemAction<T>.actions.ForEach(delegate(ItemAction<T> action)
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
						MenuActionDisplayState displayState = action.GetDisplayState(state, items);
						if (displayState == MenuActionDisplayState.Visible)
						{
							if (separator != null && separator.before)
							{
								menu.AddSeparator(text);
							}
							menu.AddItem(new GUIContent(text2), false, delegate(object f)
							{
								action.Execute(state, items);
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
		}

		protected static bool DoInternal(Type t, TimelineWindow.TimelineState state, T[] items)
		{
			ItemAction<T> itemAction = (ItemAction<T>)t.GetConstructors()[0].Invoke(null);
			return itemAction.CanExecute(state, items) && itemAction.Execute(state, items);
		}
	}
}
