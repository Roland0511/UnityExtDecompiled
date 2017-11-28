using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor
{
	internal class TimelineTreeViewGUI
	{
		private readonly TimelineAsset m_Timeline;

		private readonly TreeViewController m_TreeView;

		private readonly TimelineTreeView m_TimelineTreeView;

		private readonly TimelineWindow m_Window;

		private readonly TimelineDataSource m_DataSource;

		public TreeViewItem root
		{
			get
			{
				return this.m_DataSource.get_root();
			}
		}

		internal TimelineTrackBaseGUI[] visibleTrackGuis
		{
			get
			{
				List<TimelineTrackBaseGUI> list = new List<TimelineTrackBaseGUI>();
				int num;
				int num2;
				this.m_TreeView.get_gui().GetFirstAndLastRowVisible(ref num, ref num2);
				for (int i = num; i <= num2; i++)
				{
					TimelineTrackBaseGUI timelineTrackBaseGUI = this.m_TreeView.get_data().GetItem(i) as TimelineTrackBaseGUI;
					if (timelineTrackBaseGUI != null && timelineTrackBaseGUI != this.root)
					{
						TimelineTreeViewGUI.AddVisibleTrackRecursive(ref list, timelineTrackBaseGUI);
					}
				}
				return list.ToArray();
			}
		}

		public TrackAsset[] visibleTracks
		{
			get
			{
				return (from x in this.visibleTrackGuis
				select x.track).ToArray<TrackAsset>();
			}
		}

		public List<TimelineClipGUI> allClipGuis
		{
			get
			{
				TimelineDataSource timelineDataSource = this.m_TreeView.get_data() as TimelineDataSource;
				List<TimelineClipGUI> result;
				if (timelineDataSource != null && timelineDataSource.allTrackGuis != null)
				{
					result = timelineDataSource.allTrackGuis.OfType<TimelineTrackGUI>().SelectMany((TimelineTrackGUI x) => x.clips).ToList<TimelineClipGUI>();
				}
				else
				{
					result = null;
				}
				return result;
			}
		}

		public List<TimelineMarkerGUI> allEventGuis
		{
			get
			{
				TimelineDataSource timelineDataSource = this.m_TreeView.get_data() as TimelineDataSource;
				List<TimelineMarkerGUI> result;
				if (timelineDataSource != null && timelineDataSource.allTrackGuis != null)
				{
					result = timelineDataSource.allTrackGuis.OfType<TimelineTrackGUI>().SelectMany((TimelineTrackGUI x) => x.markers).ToList<TimelineMarkerGUI>();
				}
				else
				{
					result = null;
				}
				return result;
			}
		}

		public List<TimelineTrackBaseGUI> allTrackGuis
		{
			get
			{
				TimelineDataSource timelineDataSource = this.m_TreeView.get_data() as TimelineDataSource;
				List<TimelineTrackBaseGUI> result;
				if (timelineDataSource != null)
				{
					result = timelineDataSource.allTrackGuis;
				}
				else
				{
					result = null;
				}
				return result;
			}
		}

		public Vector2 contentSize
		{
			get
			{
				return this.m_TreeView.GetContentSize();
			}
		}

		public Vector2 scrollPosition
		{
			get
			{
				return this.m_TreeView.get_state().scrollPos;
			}
			set
			{
				Rect totalRect = this.m_TreeView.GetTotalRect();
				Vector2 contentSize = this.m_TreeView.GetContentSize();
				this.m_TreeView.get_state().scrollPos = new Vector2(value.x, Mathf.Min(new float[]
				{
					Mathf.Clamp(value.y, 0f, contentSize.y - totalRect.get_height())
				}));
			}
		}

		public ITreeViewGUI gui
		{
			get
			{
				return this.m_TimelineTreeView;
			}
		}

		public ITreeViewDataSource data
		{
			get
			{
				return (this.m_TreeView != null) ? this.m_TreeView.get_data() : null;
			}
		}

		public TimelineWindow TimelineWindow
		{
			get
			{
				return this.m_Window;
			}
		}

		public List<TrackAsset> selection
		{
			get
			{
				List<TrackAsset> list = new List<TrackAsset>();
				int[] selection = this.m_TreeView.GetSelection();
				TrackAsset[] array = this.m_Timeline.flattenedTracks.ToArray<TrackAsset>();
				int[] array2 = selection;
				for (int i = 0; i < array2.Length; i++)
				{
					int num = array2[i];
					TrackAsset[] array3 = array;
					for (int j = 0; j < array3.Length; j++)
					{
						TrackAsset trackAsset = array3[j];
						if (trackAsset.GetInstanceID() == num)
						{
							list.Add(trackAsset);
						}
					}
				}
				return list;
			}
		}

		public TimelineTreeViewGUI(TimelineWindow sequencerWindow, TimelineAsset timeline, Rect rect)
		{
			this.m_Timeline = timeline;
			this.m_Window = sequencerWindow;
			TreeViewState treeViewState = new TreeViewState();
			this.m_TreeView = new TreeViewController(sequencerWindow, treeViewState);
			this.m_TreeView.set_horizontalScrollbarStyle(GUIStyle.get_none());
			this.m_TimelineTreeView = new TimelineTreeView(sequencerWindow, this.m_TreeView);
			TimelineDragging timelineDragging = new TimelineDragging(this.m_TreeView, this.m_Window, this.m_Timeline);
			this.m_DataSource = new TimelineDataSource(this, this.m_TreeView, sequencerWindow);
			TimelineDataSource expr_7B = this.m_DataSource;
			expr_7B.onVisibleRowsChanged = (Action)Delegate.Combine(expr_7B.onVisibleRowsChanged, new Action(this.m_TimelineTreeView.CalculateRowRects));
			this.m_TreeView.Init(rect, this.m_DataSource, this.m_TimelineTreeView, timelineDragging);
			TreeViewController expr_C0 = this.m_TreeView;
			expr_C0.set_dragEndedCallback((Action<int[], bool>)Delegate.Combine(expr_C0.get_dragEndedCallback(), new Action<int[], bool>(delegate(int[] ids, bool value)
			{
				SelectionManager.Clear();
			})));
			this.m_DataSource.ExpandItems(this.m_DataSource.get_root());
		}

		public void CalculateRowRects()
		{
			this.m_TimelineTreeView.CalculateRowRects();
		}

		public void Reload()
		{
			this.m_TreeView.ReloadData();
			this.m_DataSource.ExpandItems(this.m_DataSource.get_root());
			this.m_TimelineTreeView.CalculateRowRects();
		}

		public void OnGUI(Rect rect)
		{
			int controlID = GUIUtility.GetControlID(1, rect);
			this.m_Window.state.keyboardControl = controlID;
			this.m_TreeView.OnGUI(rect, controlID);
		}

		public float GetRowHeightWithPadding(TreeViewItem i)
		{
			return this.m_TimelineTreeView.GetSizeOfRow(i).y;
		}

		public Rect GetRowRect(int row)
		{
			return this.m_TimelineTreeView.GetRowRect(row);
		}

		private static void AddVisibleTrackRecursive(ref List<TimelineTrackBaseGUI> list, TimelineTrackBaseGUI track)
		{
			if (track != null)
			{
				list.Add(track);
				if (track.isExpanded)
				{
					if (track.get_children() != null)
					{
						foreach (TreeViewItem current in track.get_children())
						{
							TimelineTreeViewGUI.AddVisibleTrackRecursive(ref list, current as TimelineTrackBaseGUI);
						}
					}
				}
			}
		}

		public void SetSelection(int[] selectedIDs, bool revealSelectionAndFrameLastSelected)
		{
			if (this.m_TreeView != null)
			{
				this.m_TreeView.SetSelection(selectedIDs, revealSelectionAndFrameLastSelected);
			}
		}
	}
}
