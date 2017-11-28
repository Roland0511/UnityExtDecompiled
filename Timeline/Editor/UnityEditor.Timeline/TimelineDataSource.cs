using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class TimelineDataSource : TreeViewDataSource
	{
		private readonly TimelineWindow m_TimelineWindow;

		private readonly TimelineTreeViewGUI m_ParentGUI;

		public List<TimelineTrackBaseGUI> allTrackGuis
		{
			get;
			private set;
		}

		public TreeViewItem treeroot
		{
			get
			{
				return this.m_RootItem;
			}
		}

		public int itemCounter
		{
			get;
			private set;
		}

		public TimelineDataSource(TimelineTreeViewGUI parentGUI, TreeViewController treeView, TimelineWindow sequencerWindow) : base(treeView)
		{
			this.m_TreeView.set_useExpansionAnimation(false);
			this.m_TimelineWindow = sequencerWindow;
			this.m_ParentGUI = parentGUI;
			this.FetchData();
		}

		public override bool IsExpanded(TreeViewItem item)
		{
			return !this.IsExpandable(item) || this.IsExpanded(item.get_id());
		}

		public override bool IsExpandable(TreeViewItem item)
		{
			bool flag = false;
			TimelineTrackBaseGUI timelineTrackBaseGUI = item as TimelineTrackBaseGUI;
			if (timelineTrackBaseGUI != null)
			{
				flag = timelineTrackBaseGUI.expandable;
			}
			return flag && item.get_hasChildren();
		}

		public sealed override void FetchData()
		{
			this.itemCounter = 1;
			this.m_RootItem = new TimelineGroupGUI(this.m_TreeView, this.m_ParentGUI, 1, 0, null, "root", null, true);
			Dictionary<TrackAsset, TimelineGroupGUI> dictionary = new Dictionary<TrackAsset, TimelineGroupGUI>();
			List<TrackAsset> tracks = this.m_TimelineWindow.timeline.tracks;
			this.allTrackGuis = new List<TimelineTrackBaseGUI>(this.m_TimelineWindow.timeline.tracks.Count);
			foreach (TrackAsset current in tracks)
			{
				this.CreateItem(current, ref dictionary, tracks, this.m_RootItem);
			}
			this.m_NeedRefreshRows = true;
			this.SetExpanded(this.m_RootItem, true);
		}

		private TimelineGroupGUI CreateItem(TrackAsset a, ref Dictionary<TrackAsset, TimelineGroupGUI> tree, List<TrackAsset> selectedRows, TreeViewItem parentTreeViewItem)
		{
			TimelineGroupGUI result;
			if (a == null)
			{
				result = null;
			}
			else
			{
				if (tree == null)
				{
					throw new ArgumentNullException("tree");
				}
				if (selectedRows == null)
				{
					throw new ArgumentNullException("selectedRows");
				}
				if (tree.ContainsKey(a))
				{
					result = tree[a];
				}
				else
				{
					TimelineTrackBaseGUI timelineTrackBaseGUI = parentTreeViewItem as TimelineTrackBaseGUI;
					if (selectedRows.Contains(a.parent as TrackAsset))
					{
						timelineTrackBaseGUI = this.CreateItem(a.parent as TrackAsset, ref tree, selectedRows, parentTreeViewItem);
					}
					int num = -1;
					if (timelineTrackBaseGUI != null)
					{
						num = timelineTrackBaseGUI.get_depth();
					}
					num++;
					TimelineGroupGUI timelineGroupGUI;
					if (a.GetType() != TimelineHelpers.GroupTrackType.trackType)
					{
						timelineGroupGUI = new TimelineTrackGUI(this.m_TreeView, this.m_ParentGUI, a.GetInstanceID(), num, timelineTrackBaseGUI, a.get_name(), a);
					}
					else
					{
						timelineGroupGUI = new TimelineGroupGUI(this.m_TreeView, this.m_ParentGUI, a.GetInstanceID(), num, timelineTrackBaseGUI, a.get_name(), a, false);
					}
					this.allTrackGuis.Add(timelineGroupGUI);
					if (timelineTrackBaseGUI != null)
					{
						if (timelineTrackBaseGUI.get_children() == null)
						{
							timelineTrackBaseGUI.set_children(new List<TreeViewItem>());
						}
						timelineTrackBaseGUI.get_children().Add(timelineGroupGUI);
					}
					else
					{
						this.m_RootItem = timelineGroupGUI;
						this.SetExpanded(this.m_RootItem, true);
					}
					tree[a] = timelineGroupGUI;
					AnimationTrack animationTrack = timelineGroupGUI.track as AnimationTrack;
					bool flag = animationTrack != null && animationTrack.ShouldShowInfiniteClipEditor();
					if (flag)
					{
						if (timelineGroupGUI.get_children() == null)
						{
							timelineGroupGUI.set_children(new List<TreeViewItem>());
						}
					}
					else
					{
						bool flag2 = false;
						for (int num2 = 0; num2 != timelineGroupGUI.track.clips.Length; num2++)
						{
							AnimationClip animationClip = timelineGroupGUI.track.clips[num2].curves;
							AnimationClip animationClip2 = timelineGroupGUI.track.clips[num2].animationClip;
							if (animationClip != null && animationClip.get_empty())
							{
								animationClip = null;
							}
							if (animationClip2 != null && animationClip2.get_empty())
							{
								animationClip2 = null;
							}
							if (animationClip2 != null && (animationClip2.get_hideFlags() & 8) != null)
							{
								animationClip2 = null;
							}
							if (!timelineGroupGUI.track.clips[num2].recordable)
							{
								animationClip2 = null;
							}
							flag2 = (animationClip != null || animationClip2 != null);
							if (flag2)
							{
								break;
							}
						}
						if (flag2)
						{
							if (timelineGroupGUI.get_children() == null)
							{
								timelineGroupGUI.set_children(new List<TreeViewItem>());
							}
						}
					}
					if (a.subTracks != null)
					{
						for (int i = 0; i < a.subTracks.Count; i++)
						{
							this.CreateItem(a.subTracks[i], ref tree, selectedRows, timelineGroupGUI);
						}
					}
					result = timelineGroupGUI;
				}
			}
			return result;
		}

		public override bool CanBeParent(TreeViewItem item)
		{
			TimelineTrackGUI timelineTrackGUI = item as TimelineTrackGUI;
			return timelineTrackGUI == null;
		}

		public void ExpandItems(TreeViewItem item)
		{
			if (this.treeroot == item)
			{
				this.SetExpanded(this.treeroot, true);
			}
			TimelineGroupGUI timelineGroupGUI = item as TimelineGroupGUI;
			if (timelineGroupGUI != null && timelineGroupGUI.track != null)
			{
				this.SetExpanded(item, !timelineGroupGUI.track.GetCollapsed());
			}
			if (item.get_children() != null)
			{
				for (int i = 0; i < item.get_children().Count; i++)
				{
					this.ExpandItems(item.get_children()[i]);
				}
			}
		}
	}
}
