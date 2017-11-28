using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class RectangleSelect : Manipulator
	{
		private Vector2 m_Start = Vector2.get_zero();

		private Vector2 m_End = Vector2.get_zero();

		private bool m_IsCaptured;

		private Rect m_ActiveRect;

		private static readonly float k_SegmentSize = 2f;

		public override void Init(IControl parent)
		{
			parent.MouseDown += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				bool result;
				if (state.IsCurrentEditingASequencerTextField())
				{
					result = base.IgnoreEvent();
				}
				else if (evt.get_button() != 0 && evt.get_button() != 1)
				{
					result = base.IgnoreEvent();
				}
				else if (evt.get_modifiers() == 4)
				{
					result = base.IgnoreEvent();
				}
				else if (TimelineWindow.instance.sequenceHeaderBounds.Contains(evt.get_mousePosition()))
				{
					foreach (TimelineTrackBaseGUI current in TimelineWindow.instance.allTracks)
					{
						Rect headerBounds = current.headerBounds;
						headerBounds.set_y(headerBounds.get_y() + TimelineWindow.instance.treeviewBounds.get_y());
						if (headerBounds.Contains(evt.get_mousePosition()))
						{
							result = base.IgnoreEvent();
							return result;
						}
					}
					if (evt.get_modifiers() == null)
					{
						SelectionManager.Clear();
					}
					result = base.IgnoreEvent();
				}
				else
				{
					this.m_ActiveRect = TimelineWindow.instance.clipArea;
					if (!this.m_ActiveRect.Contains(evt.get_mousePosition()))
					{
						result = base.IgnoreEvent();
					}
					else
					{
						List<IBounds> elementsAtPosition = Manipulator.GetElementsAtPosition(state.quadTree, evt.get_mousePosition());
						bool flag;
						if (!RectangleSelect.CanStartRectableSelect(evt, elementsAtPosition, out flag))
						{
							if (flag)
							{
								RectangleSelect.HandleReselection(elementsAtPosition);
							}
							else
							{
								RectangleSelect.HandleSingleSelection(evt, state, elementsAtPosition);
							}
							result = base.IgnoreEvent();
						}
						else
						{
							state.captured.Add(target as IControl);
							this.m_IsCaptured = true;
							this.m_Start = evt.get_mousePosition();
							this.m_End = evt.get_mousePosition();
							if (RectangleSelect.CanClearSelection(evt))
							{
								SelectionManager.Clear();
							}
							result = base.IgnoreEvent();
						}
					}
				}
				return result;
			};
			parent.KeyDown += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				bool result;
				if (this.m_IsCaptured && evt.get_keyCode() == 27)
				{
					state.captured.Remove(target as IControl);
					this.m_IsCaptured = false;
					result = base.ConsumeEvent();
				}
				else
				{
					result = base.IgnoreEvent();
				}
				return result;
			};
			parent.MouseUp += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				bool result;
				if (!this.m_IsCaptured)
				{
					if (evt.get_modifiers() == null && evt.get_button() == 0 && SelectionManager.IsMultiSelect())
					{
						List<IBounds> elementsAtPosition = Manipulator.GetElementsAtPosition(state.quadTree, evt.get_mousePosition());
						TimelineItemGUI timelineItemGUI = RectangleSelect.PickItemGUI(elementsAtPosition);
						if (timelineItemGUI != null && SelectionManager.Contains(timelineItemGUI.item))
						{
							SelectionManager.Clear();
							SelectionManager.Add(timelineItemGUI.item);
						}
					}
					result = base.IgnoreEvent();
				}
				else
				{
					state.captured.Remove(target as IControl);
					this.m_IsCaptured = false;
					Rect r = this.CurrentSelectionRect();
					if (r.get_width() < 1f || r.get_height() < 1f)
					{
						result = base.IgnoreEvent();
					}
					else
					{
						List<IBounds> elementsInRectangle = Manipulator.GetElementsInRectangle(state.quadTree, r);
						if (elementsInRectangle.Count == 0)
						{
							result = base.IgnoreEvent();
						}
						else
						{
							if (RectangleSelect.CanClearSelection(evt))
							{
								SelectionManager.Clear();
							}
							foreach (IBounds current in elementsInRectangle)
							{
								if (!(current is TimelineGroupGUI))
								{
									if (current is TimelineItemGUI)
									{
										SelectionManager.Add(((TimelineItemGUI)current).item);
									}
								}
							}
							result = base.ConsumeEvent();
						}
					}
				}
				return result;
			};
			parent.MouseDrag += delegate(object target, Event evt, TimelineWindow.TimelineState state)
			{
				bool result;
				if (!this.m_IsCaptured)
				{
					result = base.IgnoreEvent();
				}
				else
				{
					this.m_End = evt.get_mousePosition();
					result = base.ConsumeEvent();
				}
				return result;
			};
			parent.Overlay += new TimelineUIEvent(this.DrawSelection);
		}

		private static bool CanClearSelection(Event evt)
		{
			return evt.get_modifiers() != 2 && evt.get_modifiers() != 8 && evt.get_modifiers() != 1;
		}

		private static bool CanStartRectableSelect(Event evt, List<IBounds> elements, out bool hasOneSelected)
		{
			hasOneSelected = false;
			bool result;
			foreach (IBounds current in elements)
			{
				if (current != null)
				{
					if (current is InlineCurveEditor)
					{
						hasOneSelected = true;
						result = false;
						return result;
					}
					if (current is TimelineItemGUI)
					{
						SelectionManager.Contains((current as TimelineItemGUI).item);
						if (SelectionManager.Contains((current as TimelineItemGUI).item) && evt.get_modifiers() == null)
						{
							SelectionManager.Add((current as TimelineItemGUI).item);
							hasOneSelected = true;
						}
						result = false;
						return result;
					}
				}
			}
			result = true;
			return result;
		}

		private static void HandleSingleSelection(Event evt, TimelineWindow.TimelineState state, List<IBounds> elements)
		{
			if (RectangleSelect.CanClearSelection(evt))
			{
				SelectionManager.Clear();
			}
			TimelineItemGUI timelineItemGUI = RectangleSelect.PickItemGUI(elements);
			if (evt.get_modifiers() == 1)
			{
				timelineItemGUI.parentTrackGUI.RangeSelectItems(timelineItemGUI, state);
			}
			else if (evt.get_modifiers() == 2 || evt.get_modifiers() == 8)
			{
				bool flag = SelectionManager.Contains(timelineItemGUI.item);
				if (flag)
				{
					SelectionManager.Remove(timelineItemGUI.item);
				}
				else
				{
					SelectionManager.Add(timelineItemGUI.item);
				}
			}
			else
			{
				SelectionManager.Add(timelineItemGUI.item);
			}
		}

		private static void HandleReselection(List<IBounds> elements)
		{
			foreach (IBounds current in elements)
			{
				if (current is TimelineClipGUI)
				{
					TimelineClipGUI timelineClipGUI = current as TimelineClipGUI;
					SelectionManager.Add(timelineClipGUI.item);
					break;
				}
			}
		}

		private static TimelineItemGUI PickItemGUI(List<IBounds> elements)
		{
			return (from x in elements.OfType<TimelineItemGUI>()
			orderby x.zOrder
			select x).LastOrDefault<TimelineItemGUI>();
		}

		private bool DrawSelection(object target, Event e, TimelineWindow.TimelineState state)
		{
			bool result;
			if (!this.m_IsCaptured)
			{
				result = false;
			}
			else
			{
				Rect rect = this.CurrentSelectionRect();
				Vector3 vector = rect.get_min();
				Vector3 vector2 = new Vector2(rect.get_xMax(), rect.get_yMin());
				Vector3 vector3 = rect.get_max();
				Vector3 vector4 = new Vector2(rect.get_xMin(), rect.get_yMax());
				Graphics.DrawDottedLine(vector, vector2, RectangleSelect.k_SegmentSize, DirectorStyles.Instance.customSkin.colorRectangleSelect);
				Graphics.DrawDottedLine(vector2, vector3, RectangleSelect.k_SegmentSize, DirectorStyles.Instance.customSkin.colorRectangleSelect);
				Graphics.DrawDottedLine(vector3, vector4, RectangleSelect.k_SegmentSize, DirectorStyles.Instance.customSkin.colorRectangleSelect);
				Graphics.DrawDottedLine(vector4, vector, RectangleSelect.k_SegmentSize, DirectorStyles.Instance.customSkin.colorRectangleSelect);
				result = true;
			}
			return result;
		}

		private Rect CurrentSelectionRect()
		{
			return Rect.MinMaxRect(Math.Min(this.m_Start.x, this.m_End.x), Math.Min(this.m_Start.y, this.m_End.y), Math.Max(this.m_Start.x, this.m_End.x), Math.Max(this.m_Start.y, this.m_End.y));
		}
	}
}
