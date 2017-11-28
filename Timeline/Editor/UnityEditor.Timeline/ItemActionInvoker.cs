using System;
using System.Linq;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal static class ItemActionInvoker
	{
		public static bool InvokeByName<T>(string actionName, TimelineWindow.TimelineState state, ITimelineItem[] items) where T : class, ITimelineItem
		{
			T[] array = items.OfType<T>().ToArray<T>();
			bool result;
			if (!array.Any<T>())
			{
				result = false;
			}
			else
			{
				ItemAction<T> itemAction = ItemAction<T>.actions.First((ItemAction<T> x) => x.GetType().Name == actionName);
				result = (itemAction != null && itemAction.Execute(state, array));
			}
			return result;
		}

		public static bool InvokeByName<T>(string actionName, TimelineWindow.TimelineState state, ITimelineItem item) where T : class, ITimelineItem
		{
			ITimelineItem[] items = new ITimelineItem[]
			{
				item
			};
			return ItemActionInvoker.InvokeByName<T>(actionName, state, items);
		}

		public static bool InvokeByName<T>(string actionName, TimelineWindow.TimelineState state, T item) where T : class, ITimelineItem
		{
			ITimelineItem[] items = new ITimelineItem[]
			{
				item
			};
			return ItemActionInvoker.InvokeByName<T>(actionName, state, items);
		}

		public static bool InvokeByName<T>(string actionName, TimelineWindow.TimelineState state, T[] items) where T : class, ITimelineItem
		{
			ITimelineItem[] items2 = items.Cast<ITimelineItem>().ToArray<ITimelineItem>();
			return ItemActionInvoker.InvokeByName<T>(actionName, state, items2);
		}
	}
}
