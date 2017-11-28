using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace UnityEditor.Timeline
{
	internal class MenuItemActionBase
	{
		protected static bool s_ShowActionTriggeredByShortcut = false;

		protected static IEnumerable<Type> GetActionsOfType(Type actionType)
		{
			return from type in EditorAssemblies.get_loadedTypes()
			where !type.IsGenericType && !type.IsNested && !type.IsAbstract && type.IsSubclassOf(actionType)
			select type;
		}

		protected static string GetDisplayName(MenuItemActionBase action)
		{
			object[] customAttributes = action.GetType().GetCustomAttributes(typeof(ShortcutAttribute), true);
			object[] customAttributes2 = action.GetType().GetCustomAttributes(typeof(DisplayNameAttribute), true);
			StringBuilder stringBuilder = new StringBuilder();
			if (customAttributes2.Length > 0)
			{
				stringBuilder.Append((customAttributes2[0] as DisplayNameAttribute).DisplayName);
			}
			else
			{
				stringBuilder.Append(action.GetType().Name);
			}
			if (customAttributes.Length > 0)
			{
				stringBuilder.Append("\t\t");
			}
			for (int num = 0; num != customAttributes.Length; num++)
			{
				if (num > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(customAttributes[num]);
			}
			return stringBuilder.ToString();
		}

		protected static CategoryAttribute GetCategoryAttribute(MenuItemActionBase action)
		{
			object[] customAttributes = action.GetType().GetCustomAttributes(typeof(CategoryAttribute), true);
			CategoryAttribute result;
			if (customAttributes.Length > 0)
			{
				result = (customAttributes[0] as CategoryAttribute);
			}
			else
			{
				result = null;
			}
			return result;
		}

		protected static SeparatorMenuItemAttribute GetSeparator(MenuItemActionBase action)
		{
			object[] customAttributes = action.GetType().GetCustomAttributes(typeof(SeparatorMenuItemAttribute), true);
			SeparatorMenuItemAttribute result;
			if (customAttributes.Length > 0)
			{
				result = (customAttributes[0] as SeparatorMenuItemAttribute);
			}
			else
			{
				result = null;
			}
			return result;
		}

		protected static bool IsHiddenInMenu(MenuItemActionBase action)
		{
			object[] customAttributes = action.GetType().GetCustomAttributes(typeof(HideInMenuAttribute), true);
			return customAttributes.Length > 0;
		}
	}
}
