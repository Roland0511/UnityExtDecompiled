using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class TimelineTreeView : ITreeViewGUI
	{
		protected float m_FoldoutWidth;

		protected Rect m_DraggingInsertionMarkerRect;

		protected readonly TreeViewController m_TreeView;

		private List<Rect> m_RowRects = new List<Rect>();

		private float m_MaxWidthOfRows;

		private readonly TimelineWindow.TimelineState m_State;

		private static readonly float kMinTrackHeight = 25f;

		private static readonly float kFoldOutOffset = 14f;

		private static DirectorStyles m_Styles;

		public bool showInsertionMarker
		{
			get;
			set;
		}

		public virtual float topRowMargin
		{
			get;
			private set;
		}

		public virtual float bottomRowMargin
		{
			get;
			private set;
		}

		public virtual float halfDropBetweenHeight
		{
			get
			{
				return 8f;
			}
		}

		public TimelineTreeView(TimelineWindow sequencerWindow, TreeViewController treeView)
		{
			this.m_TreeView = treeView;
			this.m_TreeView.set_useExpansionAnimation(true);
			TimelineTreeView.m_Styles = DirectorStyles.Instance;
			this.m_State = sequencerWindow.state;
			this.m_FoldoutWidth = DirectorStyles.Instance.foldout.get_fixedWidth();
		}

		public void OnInitialize()
		{
		}

		public Rect GetRectForFraming(int row)
		{
			return this.GetRowRect(row, 1f);
		}

		public virtual Vector2 GetSizeOfRow(TreeViewItem item)
		{
			Vector2 result;
			if (item.get_displayName() == "root")
			{
				result = new Vector2(this.m_TreeView.GetTotalRect().get_width(), 0f);
			}
			else
			{
				TimelineGroupGUI timelineGroupGUI = item as TimelineGroupGUI;
				if (timelineGroupGUI != null)
				{
					result = new Vector2(this.m_TreeView.GetTotalRect().get_width(), timelineGroupGUI.GetHeight(this.m_State));
				}
				else
				{
					float num = this.m_State.trackHeight;
					if (item.get_hasChildren() && this.m_TreeView.get_data().IsExpanded(item))
					{
						num = Mathf.Min(this.m_State.trackHeight, TimelineTreeView.kMinTrackHeight);
					}
					result = new Vector2(this.m_TreeView.GetTotalRect().get_width(), num);
				}
			}
			return result;
		}

		public virtual void BeginRowGUI()
		{
			if (this.m_TreeView.GetTotalRect().get_width() != this.GetRowRect(0).get_width())
			{
				this.CalculateRowRects();
			}
			this.m_DraggingInsertionMarkerRect.set_x(-1f);
		}

		public virtual void EndRowGUI()
		{
			if (this.m_DraggingInsertionMarkerRect.get_x() >= 0f && Event.get_current().get_type() == 7)
			{
				Rect draggingInsertionMarkerRect = this.m_DraggingInsertionMarkerRect;
				draggingInsertionMarkerRect.set_height(1f);
				if (this.m_TreeView.get_dragging().get_drawRowMarkerAbove())
				{
					draggingInsertionMarkerRect.set_y(draggingInsertionMarkerRect.get_y() - 2.5f);
				}
				else
				{
					draggingInsertionMarkerRect.set_y(draggingInsertionMarkerRect.get_y() + (this.m_DraggingInsertionMarkerRect.get_height() - 0.5f + 1f));
				}
				EditorGUI.DrawRect(draggingInsertionMarkerRect, Color.get_white());
			}
		}

		public virtual void OnRowGUI(Rect rowRect, TreeViewItem item, int row, bool selected, bool focused)
		{
			Rect headerRect = rowRect;
			Rect trackRect = rowRect;
			headerRect.set_width(this.m_State.sequencerHeaderWidth);
			trackRect.set_xMin(trackRect.get_xMin() + this.m_State.sequencerHeaderWidth);
			trackRect.set_width(rowRect.get_width() - this.m_State.sequencerHeaderWidth - 1f);
			float foldoutIndent = this.GetFoldoutIndent(item);
			Rect rect = rowRect;
			TimelineTrackBaseGUI timelineTrackBaseGUI = (TimelineTrackBaseGUI)item;
			timelineTrackBaseGUI.isExpanded = this.m_TreeView.get_data().IsExpanded(item);
			timelineTrackBaseGUI.Draw(headerRect, trackRect, this.m_State, foldoutIndent);
			if (trackRect.Contains(Event.get_current().get_mousePosition()) || Event.get_current().get_isKey())
			{
				timelineTrackBaseGUI.OnEvent(Event.get_current(), this.m_State, false);
			}
			if (Event.get_current().get_type() == 7)
			{
				if (this.showInsertionMarker)
				{
					if (this.m_TreeView.get_dragging() != null && this.m_TreeView.get_dragging().GetRowMarkerControlID() == TreeViewController.GetItemControlID(item))
					{
						this.m_DraggingInsertionMarkerRect = new Rect(rowRect.get_x() + foldoutIndent, rowRect.get_y(), rowRect.get_width() - foldoutIndent, rowRect.get_height());
					}
				}
			}
			bool flag = this.m_TreeView.get_data().IsExpandable(item);
			if (flag)
			{
				rect.set_x(foldoutIndent - TimelineTreeView.kFoldOutOffset);
				rect.set_width(this.m_FoldoutWidth);
				EditorGUI.BeginChangeCheck();
				float num = (float)DirectorStyles.Instance.foldout.get_normal().get_background().get_height();
				rect.set_y(rect.get_y() + num / 2f);
				rect.set_height(num);
				bool flag2 = GUI.Toggle(rect, this.m_TreeView.get_data().IsExpanded(item), GUIContent.none, TimelineTreeView.m_Styles.foldout);
				if (EditorGUI.EndChangeCheck())
				{
					if (Event.get_current().get_alt())
					{
						this.m_TreeView.get_data().SetExpandedWithChildren(item, flag2);
					}
					else
					{
						this.m_TreeView.get_data().SetExpanded(item, flag2);
					}
				}
			}
			if (headerRect.Contains(Event.get_current().get_mousePosition()) || Event.get_current().get_isKey())
			{
				timelineTrackBaseGUI.OnEvent(Event.get_current(), this.m_State, false);
			}
		}

		public Rect GetRenameRect(Rect rowRect, int row, TreeViewItem item)
		{
			return rowRect;
		}

		public void BeginPingItem(TreeViewItem item, float topPixelOfRow, float availableWidth)
		{
		}

		public void EndPingItem()
		{
		}

		public Rect GetRowRect(int row, float rowWidth)
		{
			return this.GetRowRect(row);
		}

		public Rect GetRowRect(int row)
		{
			Rect result;
			if (this.m_RowRects.Count == 0)
			{
				result = default(Rect);
			}
			else if (row >= this.m_RowRects.Count)
			{
				result = default(Rect);
			}
			else
			{
				result = this.m_RowRects[row];
			}
			return result;
		}

		private static float GetSpacing(TreeViewItem item)
		{
			TimelineTrackBaseGUI timelineTrackBaseGUI = item as TimelineTrackBaseGUI;
			float result;
			if (timelineTrackBaseGUI != null)
			{
				result = timelineTrackBaseGUI.GetVerticalSpacingBetweenTracks();
			}
			else
			{
				result = 3f;
			}
			return result;
		}

		public void CalculateRowRects()
		{
			if (!this.m_TreeView.get_isSearching())
			{
				IList<TreeViewItem> rows = this.m_TreeView.get_data().GetRows();
				this.m_RowRects = new List<Rect>(rows.Count);
				float num = 6f;
				this.m_MaxWidthOfRows = 1f;
				for (int i = 0; i < rows.Count; i++)
				{
					TreeViewItem item = rows[i];
					if (i != 0)
					{
						num += TimelineTreeView.GetSpacing(item);
					}
					Vector2 sizeOfRow = this.GetSizeOfRow(item);
					this.m_RowRects.Add(new Rect(0f, num, sizeOfRow.x, sizeOfRow.y));
					num += sizeOfRow.y;
					if (sizeOfRow.x > this.m_MaxWidthOfRows)
					{
						this.m_MaxWidthOfRows = sizeOfRow.x;
					}
				}
			}
		}

		public virtual void BeginPingNode(TreeViewItem item, float topPixelOfRow, float availableWidth)
		{
		}

		public virtual void EndPingNode()
		{
		}

		public virtual bool BeginRename(TreeViewItem item, float delay)
		{
			return false;
		}

		public virtual void EndRename()
		{
		}

		public virtual float GetFoldoutIndent(TreeViewItem item)
		{
			float result;
			if (item.get_depth() <= 1 || this.m_TreeView.get_isSearching())
			{
				result = DirectorStyles.kBaseIndent;
			}
			else
			{
				int num = item.get_depth();
				TimelineTrackGUI timelineTrackGUI = item as TimelineTrackGUI;
				if (timelineTrackGUI != null && timelineTrackGUI.track != null && timelineTrackGUI.track.isSubTrack)
				{
					num--;
				}
				result = (float)num * DirectorStyles.kBaseIndent;
			}
			return result;
		}

		public virtual float GetContentIndent(TreeViewItem item)
		{
			return this.GetFoldoutIndent(item);
		}

		public int GetNumRowsOnPageUpDown(TreeViewItem fromItem, bool pageUp, float heightOfTreeView)
		{
			return (int)Mathf.Floor(heightOfTreeView / 30f);
		}

		public void GetFirstAndLastRowVisible(out int firstRowVisible, out int lastRowVisible)
		{
			int rowCount = this.m_TreeView.get_data().get_rowCount();
			if (rowCount == 0)
			{
				firstRowVisible = (lastRowVisible = -1);
			}
			else
			{
				if (rowCount != this.m_RowRects.Count)
				{
					Debug.LogError("Mismatch in state: rows vs cached rects. Did you remember to hook up: dataSource.onVisibleRowsChanged += gui.CalculateRowRects ?");
					this.CalculateRowRects();
				}
				float y = this.m_TreeView.get_state().scrollPos.y;
				float height = this.m_TreeView.GetTotalRect().get_height();
				int num = -1;
				int num2 = -1;
				for (int i = 0; i < this.m_RowRects.Count; i++)
				{
					bool flag = (this.m_RowRects[i].get_y() > y && this.m_RowRects[i].get_y() < y + height) || (this.m_RowRects[i].get_yMax() > y && this.m_RowRects[i].get_yMax() < y + height);
					if (flag)
					{
						if (num == -1)
						{
							num = i;
						}
						num2 = i;
					}
				}
				if (num != -1 && num2 != -1)
				{
					firstRowVisible = num;
					lastRowVisible = num2;
				}
				else
				{
					firstRowVisible = 0;
					lastRowVisible = rowCount - 1;
				}
			}
		}

		public Vector2 GetTotalSize()
		{
			Vector2 result;
			if (this.m_RowRects.Count == 0)
			{
				result = new Vector2(0f, 0f);
			}
			else
			{
				result = new Vector2(this.m_MaxWidthOfRows, this.m_RowRects[this.m_RowRects.Count - 1].get_yMax());
			}
			return result;
		}
	}
}
