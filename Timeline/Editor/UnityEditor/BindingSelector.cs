using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Timeline;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor
{
	internal class BindingSelector
	{
		private TreeViewController m_TreeView;

		private TreeViewState m_TreeViewState;

		private BindingTreeViewDataSource m_TreeViewDataSource;

		private CurveDataSource m_ClipDataSource;

		private TimelineWindow m_Window;

		private CurveEditor m_CurveEditor;

		private ReorderableList m_DopeLines;

		private string[] m_StringList = new string[0];

		public static float kBottomPadding = 5f;

		private int[] m_Selection;

		private bool m_PartOfSelection;

		public bool selectable
		{
			get
			{
				return true;
			}
		}

		public object selectableObject
		{
			get
			{
				return this;
			}
		}

		public bool selected
		{
			get
			{
				return this.m_PartOfSelection;
			}
			set
			{
				this.m_PartOfSelection = value;
				if (!this.m_PartOfSelection)
				{
					this.m_DopeLines.set_index(-1);
				}
			}
		}

		public BindingSelector(EditorWindow window, CurveEditor curveEditor)
		{
			this.m_Window = (window as TimelineWindow);
			this.m_CurveEditor = curveEditor;
			this.m_DopeLines = new ReorderableList(this.m_StringList, typeof(string), false, false, false, false);
			this.m_DopeLines.drawElementBackgroundCallback = null;
			this.m_DopeLines.showDefaultBackground = false;
			this.m_DopeLines.set_index(0);
			this.m_DopeLines.headerHeight = 0f;
			this.m_DopeLines.elementHeight = 20f;
			this.m_DopeLines.set_draggable(false);
		}

		public virtual void Delete(ITimelineState state)
		{
			if (this.m_DopeLines.get_index() >= 1)
			{
				if (this.m_ClipDataSource != null)
				{
					AnimationClip animationClip = this.m_ClipDataSource.animationClip;
					if (!(animationClip == null))
					{
						int num = this.m_DopeLines.get_index() - 1;
						EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(animationClip);
						if (num < curveBindings.Length)
						{
							TimelineUndo.PushUndo(animationClip, "Delete Curve");
							AnimationUtility.SetEditorCurve(animationClip, curveBindings[this.m_DopeLines.get_index() - 1], null);
							state.rebuildGraph = true;
						}
					}
				}
			}
		}

		public void OnGUI(Rect targetRect)
		{
			if (this.m_TreeView != null)
			{
				this.m_TreeView.OnEvent();
				this.m_TreeView.OnGUI(targetRect, GUIUtility.GetControlID(1));
			}
		}

		public void InitIfNeeded(Rect rect, CurveDataSource dataSource)
		{
			if (Event.get_current().get_type() == 8)
			{
				this.m_ClipDataSource = dataSource;
				AnimationClip animationClip = dataSource.animationClip;
				int num = (this.m_DopeLines.get_list() == null) ? 0 : this.m_DopeLines.get_list().Count;
				List<EditorCurveBinding> list = new List<EditorCurveBinding>();
				list.Add(new EditorCurveBinding
				{
					propertyName = "Summary"
				});
				if (animationClip != null)
				{
					list.AddRange(AnimationUtility.GetCurveBindings(animationClip));
				}
				this.m_DopeLines.set_list(list.ToArray());
				if (num != this.m_DopeLines.get_list().Count)
				{
					this.UpdateRowHeight();
				}
				if (this.m_TreeViewState == null)
				{
					this.m_TreeViewState = new TreeViewState();
					TreeViewController treeViewController = new TreeViewController(this.m_Window, this.m_TreeViewState);
					treeViewController.set_useExpansionAnimation(false);
					treeViewController.set_deselectOnUnhandledMouseDown(true);
					this.m_TreeView = treeViewController;
					TreeViewController expr_FD = this.m_TreeView;
					expr_FD.set_selectionChangedCallback((Action<int[]>)Delegate.Combine(expr_FD.get_selectionChangedCallback(), new Action<int[]>(this.OnItemSelectionChanged)));
					this.m_TreeViewDataSource = new BindingTreeViewDataSource(this.m_TreeView, animationClip);
					this.m_TreeView.Init(rect, this.m_TreeViewDataSource, new BindingTreeViewGUI(this.m_TreeView), null);
					this.m_TreeViewDataSource.UpdateData();
					this.OnItemSelectionChanged(null);
				}
			}
		}

		private void UpdateRowHeight()
		{
			this.m_ClipDataSource.SetHeight((float)this.m_DopeLines.get_list().Count * this.m_DopeLines.elementHeight);
		}

		private void OnItemSelectionChanged(int[] selection)
		{
			if (selection == null || selection.Length == 0)
			{
				if (this.m_TreeViewDataSource.GetRows().Count > 0)
				{
					this.m_Selection = (from r in this.m_TreeViewDataSource.GetRows()
					select r.get_id()).ToArray<int>();
				}
			}
			else
			{
				this.m_Selection = selection.ToArray<int>();
			}
			this.RefreshCurves();
		}

		public void RefreshCurves()
		{
			if (this.m_ClipDataSource != null && this.m_Selection != null)
			{
				List<EditorCurveBinding> list = new List<EditorCurveBinding>();
				int[] selection = this.m_Selection;
				for (int i = 0; i < selection.Length; i++)
				{
					int num = selection[i];
					CurveTreeViewNode curveTreeViewNode = (CurveTreeViewNode)this.m_TreeView.FindItem(num);
					if (curveTreeViewNode != null && curveTreeViewNode.bindings != null)
					{
						list.AddRange(curveTreeViewNode.bindings);
					}
				}
				AnimationClip animationClip = this.m_ClipDataSource.animationClip;
				List<CurveWrapper> list2 = new List<CurveWrapper>();
				int num2 = 0;
				foreach (EditorCurveBinding current in list)
				{
					CurveWrapper curveWrapper = new CurveWrapper();
					curveWrapper.id = num2++;
					curveWrapper.binding = current;
					curveWrapper.groupId = -1;
					curveWrapper.color = CurveUtility.GetPropertyColor(current.propertyName);
					curveWrapper.hidden = false;
					curveWrapper.readOnly = false;
					curveWrapper.set_renderer(new NormalCurveRenderer(AnimationUtility.GetEditorCurve(animationClip, current)));
					curveWrapper.getAxisUiScalarsCallback = new CurveWrapper.GetAxisScalarsCallback(this.GetAxisScalars);
					CurveWrapper curveWrapper2 = curveWrapper;
					curveWrapper2.get_renderer().SetCustomRange(0f, animationClip.get_length());
					list2.Add(curveWrapper2);
				}
				this.m_CurveEditor.set_animationCurves(list2.ToArray());
			}
		}

		public void RefreshTree()
		{
			if (this.m_TreeViewDataSource != null)
			{
				if (this.m_Selection == null)
				{
					this.m_Selection = new int[0];
				}
				string[] selected = (from x in this.m_Selection
				select this.m_TreeViewDataSource.FindItem(x) into t
				where t != null
				select t into c
				select c.get_displayName()).ToArray<string>();
				this.m_TreeViewDataSource.UpdateData();
				int[] array = (from x in this.m_TreeViewDataSource.GetRows()
				where selected.Contains(x.get_displayName())
				select x.get_id()).ToArray<int>();
				if (!array.Any<int>())
				{
					if (this.m_TreeViewDataSource.GetRows().Count > 0)
					{
						array = new int[]
						{
							this.m_TreeViewDataSource.GetItem(0).get_id()
						};
					}
				}
				this.OnItemSelectionChanged(array);
			}
		}

		private Vector2 GetAxisScalars()
		{
			return new Vector2(1f, 1f);
		}

		internal virtual bool IsRenamingNodeAllowed(TreeViewItem node)
		{
			return false;
		}
	}
}
