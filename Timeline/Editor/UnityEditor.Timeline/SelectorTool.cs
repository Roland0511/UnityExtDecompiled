using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class SelectorTool : Manipulator
	{
		private bool m_DoubleClicked = false;

		private static bool CanClearSelection(Event evt, TimelineGroupGUI track)
		{
			return !SelectionManager.Contains(track.track) && evt.get_modifiers() != 2 && evt.get_modifiers() != 1 && evt.get_modifiers() != 8;
		}

		public override void Init(IControl parent)
		{
			parent.DoubleClick += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				this.m_DoubleClicked = true;
				bool result;
				if (state.IsEditingASubItem())
				{
					result = base.IgnoreEvent();
				}
				else
				{
					TimelineTrackGUI timelineTrackGUI = target as TimelineTrackGUI;
					if (timelineTrackGUI == null)
					{
						result = base.IgnoreEvent();
					}
					else if (evt.get_button() != 0)
					{
						result = base.IgnoreEvent();
					}
					else if (!timelineTrackGUI.indentedHeaderBounds.Contains(evt.get_mousePosition()))
					{
						result = base.IgnoreEvent();
					}
					else
					{
						bool flag = SelectionManager.SelectedTracks().Contains(timelineTrackGUI.track);
						foreach (TimelineItemGUI current in timelineTrackGUI.items)
						{
							if (flag)
							{
								SelectionManager.Add(current.item);
							}
							else
							{
								SelectionManager.Remove(current.item);
							}
						}
						result = base.ConsumeEvent();
					}
				}
				return result;
			};
			parent.MouseDown += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				this.m_DoubleClicked = false;
				bool result;
				if (state.IsCurrentEditingASequencerTextField())
				{
					result = base.IgnoreEvent();
				}
				else
				{
					TimelineGroupGUI timelineGroupGUI = target as TimelineGroupGUI;
					if (timelineGroupGUI == null)
					{
						result = base.IgnoreEvent();
					}
					else
					{
						if (target is TimelineTrackGUI)
						{
							TimelineTrackGUI timelineTrackGUI = target as TimelineTrackGUI;
							if (timelineTrackGUI.locked)
							{
								if (SelectorTool.CanClearSelection(evt, timelineGroupGUI))
								{
									SelectionManager.Clear();
								}
								SelectionManager.Add(timelineTrackGUI.track);
							}
							bool flag = timelineTrackGUI.items.Any((TimelineItemGUI x) => x.bounds.Contains(evt.get_mousePosition()));
							if (flag && !TimelineWindow.instance.sequenceHeaderBounds.Contains(evt.get_mousePosition()))
							{
								result = base.IgnoreEvent();
								return result;
							}
						}
						if (SelectorTool.CanClearSelection(evt, timelineGroupGUI))
						{
							SelectionManager.Clear();
						}
						IEnumerable<TrackAsset> source = SelectionManager.SelectedTracks();
						if (evt.get_modifiers() == 2 || evt.get_modifiers() == 8)
						{
							if (SelectionManager.Contains(timelineGroupGUI.track))
							{
								SelectionManager.Remove(timelineGroupGUI.track);
							}
							else
							{
								SelectionManager.Add(timelineGroupGUI.track);
							}
						}
						else if (evt.get_modifiers() == 1)
						{
							if (!source.Any<TrackAsset>() && !SelectionManager.Contains(timelineGroupGUI.track))
							{
								SelectionManager.Add(timelineGroupGUI.track);
							}
							else
							{
								bool flag2 = false;
								foreach (TimelineTrackBaseGUI current in TimelineWindow.instance.allTracks)
								{
									if (!flag2)
									{
										if (current == timelineGroupGUI || SelectionManager.Contains(current.track))
										{
											SelectionManager.Add(current.track);
											flag2 = true;
											continue;
										}
									}
									if (flag2)
									{
										if (current == timelineGroupGUI || SelectionManager.Contains(current.track))
										{
											SelectionManager.Add(current.track);
											flag2 = false;
											continue;
										}
									}
									if (flag2)
									{
										SelectionManager.Add(current.track);
									}
								}
							}
						}
						else
						{
							SelectionManager.Add(timelineGroupGUI.track);
						}
						result = base.IgnoreEvent();
					}
				}
				return result;
			};
			parent.MouseUp += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				bool result;
				if (this.m_DoubleClicked || evt.get_modifiers() != null || evt.get_button() != 0 || !SelectionManager.IsMultiSelect())
				{
					result = base.IgnoreEvent();
				}
				else
				{
					TimelineGroupGUI timelineGroupGUI = target as TimelineGroupGUI;
					if (timelineGroupGUI == null)
					{
						result = base.IgnoreEvent();
					}
					else if (!SelectionManager.Contains(timelineGroupGUI.track))
					{
						result = base.IgnoreEvent();
					}
					else
					{
						SelectionManager.Clear();
						SelectionManager.Add(timelineGroupGUI.track);
						result = base.ConsumeEvent();
					}
				}
				return result;
			};
		}
	}
}
