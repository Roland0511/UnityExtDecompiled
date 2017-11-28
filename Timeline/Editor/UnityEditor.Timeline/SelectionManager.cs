using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal static class SelectionManager
	{
		private static IClipCurveEditorOwner currentInlineEditorCurve;

		[CompilerGenerated]
		private static Func<Object, bool> <>f__mg$cache0;

		public static void Add(Object obj)
		{
			SelectionManager.currentInlineEditorCurve = null;
			if (!Selection.Contains(obj))
			{
				Selection.Add(obj);
			}
		}

		public static void Add(ITimelineItem item)
		{
			TimelineClip timelineClip = item as TimelineClip;
			if (timelineClip != null)
			{
				SelectionManager.Add(timelineClip);
			}
			else
			{
				TimelineMarker timelineMarker = item as TimelineMarker;
				if (timelineMarker != null)
				{
					SelectionManager.Add(timelineMarker);
				}
			}
		}

		public static void Add(TimelineClip item)
		{
			SelectionManager.Add(EditorItemFactory.GetEditorClip(item));
		}

		public static void Add(TimelineMarker item)
		{
			SelectionManager.Add(EditorItemFactory.GetEditorMarker(item));
		}

		public static void SelectInlineCurveEditor(IClipCurveEditorOwner selection)
		{
			SelectionManager.currentInlineEditorCurve = selection;
		}

		public static IClipCurveEditorOwner GetCurrentInlineEditorCurve()
		{
			return SelectionManager.currentInlineEditorCurve;
		}

		public static bool IsCurveEditorFocused(IClipCurveEditorOwner selection)
		{
			return selection == SelectionManager.currentInlineEditorCurve;
		}

		public static bool Contains(TrackAsset item)
		{
			return Selection.Contains(item);
		}

		public static bool Contains(ITimelineItem item)
		{
			TimelineClip timelineClip = item as TimelineClip;
			bool result;
			if (timelineClip != null)
			{
				result = SelectionManager.Contains(timelineClip);
			}
			else
			{
				TimelineMarker timelineMarker = item as TimelineMarker;
				result = (timelineMarker != null && SelectionManager.Contains(timelineMarker));
			}
			return result;
		}

		public static bool Contains(TimelineClip item)
		{
			return Selection.Contains(EditorItemFactory.GetEditorClip(item));
		}

		public static bool Contains(TimelineMarker item)
		{
			return Selection.Contains(EditorItemFactory.GetEditorMarker(item));
		}

		public static void Clear()
		{
			Selection.set_activeObject(null);
		}

		public static void Remove(ITimelineItem item)
		{
			TimelineClip timelineClip = item as TimelineClip;
			if (timelineClip != null)
			{
				SelectionManager.Remove(timelineClip);
			}
			else
			{
				TimelineMarker timelineMarker = item as TimelineMarker;
				if (timelineMarker != null)
				{
					SelectionManager.Remove(timelineMarker);
				}
			}
		}

		public static void Remove(TimelineClip item)
		{
			SelectionManager.Remove(EditorItemFactory.GetEditorClip(item));
		}

		public static void Remove(TimelineMarker item)
		{
			SelectionManager.Remove(EditorItemFactory.GetEditorMarker(item));
		}

		public static void Remove(Object item)
		{
			Selection.Remove(item);
		}

		public static void RemoveTimelineSelection()
		{
			Selection.set_objects((from s in Selection.get_objects()
			where !SelectionManager.IsTimelineType(s)
			select s).ToArray<Object>());
		}

		public static bool IsMultiSelect()
		{
			return SelectionManager.Count() > 1;
		}

		public static int Count()
		{
			IEnumerable<Object> arg_23_0 = Selection.get_objects();
			if (SelectionManager.<>f__mg$cache0 == null)
			{
				SelectionManager.<>f__mg$cache0 = new Func<Object, bool>(SelectionManager.IsTimelineType);
			}
			return arg_23_0.Where(SelectionManager.<>f__mg$cache0).Count<Object>();
		}

		public static IEnumerable<TrackAsset> SelectedTracks()
		{
			return Selection.GetFiltered<TrackAsset>(0);
		}

		private static IEnumerable<U> SelectedItemGUI<U, V>() where U : TimelineItemGUI where V : ITimelineItem
		{
			IEnumerable<ITimelineItem> enumerable = from x in Selection.GetFiltered<EditorItem<V>>(0)
			select x.item;
			List<U> list = new List<U>();
			foreach (ITimelineItem current in enumerable)
			{
				TimelineItemGUI timelineItemGUI;
				if (TimelineItemGUI.s_ItemToItemGUI.TryGetValue(current, out timelineItemGUI))
				{
					list.Add((U)((object)timelineItemGUI));
				}
			}
			return list;
		}

		public static IEnumerable<TimelineClipGUI> SelectedClipGUI()
		{
			return SelectionManager.SelectedItemGUI<TimelineClipGUI, TimelineClip>();
		}

		public static IEnumerable<TimelineMarkerGUI> SelectedMarkerGUI()
		{
			return SelectionManager.SelectedItemGUI<TimelineMarkerGUI, TimelineMarker>();
		}

		public static IEnumerable<T> SelectedItems<T>() where T : ITimelineItem
		{
			return (from x in Selection.GetFiltered<EditorItem<T>>(0)
			select x.item).Cast<T>();
		}

		public static IEnumerable<TimelineItemGUI> SelectedItemGUI()
		{
			IEnumerable<TimelineItemGUI> first = SelectionManager.SelectedClipGUI().Cast<TimelineItemGUI>();
			IEnumerable<TimelineItemGUI> second = SelectionManager.SelectedMarkerGUI().Cast<TimelineItemGUI>();
			return first.Union(second);
		}

		public static IEnumerable<TimelineTrackBaseGUI> SelectedTrackGUI()
		{
			IEnumerable<TrackAsset> tracks = SelectionManager.SelectedTracks();
			return from x in TimelineWindow.instance.allTracks
			where tracks.Contains(x.track)
			select x;
		}

		public static bool IsMouseHoveringOnItem()
		{
			return SelectionManager.SelectedItemGUI().Any((TimelineItemGUI b) => b.boundingRect.Contains(Event.get_current().get_mousePosition()));
		}

		private static bool IsTimelineType(Object o)
		{
			return o is TrackAsset || o is EditorClip || o is EditorMarker;
		}
	}
}
