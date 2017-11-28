using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal abstract class TrackAction : MenuItemActionBase
	{
		private static List<TrackAction> s_ActionClasses;

		private static List<TrackAction> actions
		{
			get
			{
				if (TrackAction.s_ActionClasses == null)
				{
					TrackAction.s_ActionClasses = (from x in MenuItemActionBase.GetActionsOfType(typeof(TrackAction))
					select (TrackAction)x.GetConstructors()[0].Invoke(null)).ToList<TrackAction>();
				}
				return TrackAction.s_ActionClasses;
			}
		}

		public abstract bool Execute(TimelineWindow.TimelineState state, TrackAsset[] tracks);

		public virtual MenuActionDisplayState GetDisplayState(TimelineWindow.TimelineState state, TrackAsset[] tracks)
		{
			return (tracks.Length <= 0) ? MenuActionDisplayState.Disabled : MenuActionDisplayState.Visible;
		}

		public virtual bool CanExecute(TimelineWindow.TimelineState state, TrackAsset[] tracks)
		{
			return this.GetDisplayState(state, tracks) == MenuActionDisplayState.Visible;
		}

		public static bool InvokeByName(string actionName, TimelineWindow.TimelineState state, TrackAsset[] tracks)
		{
			return TrackAction.actions.First((TrackAction x) => x.GetType().Name == actionName).Execute(state, tracks);
		}

		public static bool InvokeByName(string actionName, TimelineWindow.TimelineState state, TrackAsset track)
		{
			TrackAsset[] tracks = new TrackAsset[]
			{
				track
			};
			return TrackAction.actions.First((TrackAction x) => x.GetType().Name == actionName).Execute(state, tracks);
		}

		public static void AddToMenu(GenericMenu menu, TimelineWindow.TimelineState state)
		{
			TrackAsset[] tracks = SelectionManager.SelectedTracks().ToArray<TrackAsset>();
			TrackAction.actions.ForEach(delegate(TrackAction action)
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
					MenuActionDisplayState displayState = action.GetDisplayState(state, tracks);
					if (displayState == MenuActionDisplayState.Visible)
					{
						menu.AddItem(new GUIContent(text2), false, delegate(object f)
						{
							action.Execute(state, tracks);
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

		public static bool HandleShortcut(TimelineWindow.TimelineState state, Event evt, TrackAsset track)
		{
			List<TrackAsset> list = SelectionManager.SelectedTracks().ToList<TrackAsset>();
			if (list.All((TrackAsset x) => x != track))
			{
				list.Add(track);
			}
			bool result;
			foreach (TrackAction current in TrackAction.actions)
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
						result = current.Execute(state, list.ToArray());
						return result;
					}
				}
			}
			result = false;
			return result;
		}
	}
}
