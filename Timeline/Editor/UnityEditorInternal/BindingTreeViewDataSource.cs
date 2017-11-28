using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UnityEditorInternal
{
	internal class BindingTreeViewDataSource : TreeViewDataSource
	{
		private AnimationClip m_Clip;

		public BindingTreeViewDataSource(TreeViewController treeView, AnimationClip clip) : base(treeView)
		{
			this.m_Clip = clip;
			base.set_showRootItem(false);
			base.set_rootIsCollapsable(false);
		}

		private void SetupRootNodeSettings()
		{
			base.set_showRootItem(false);
			this.SetExpanded(base.get_root(), true);
		}

		private static string GroupName(EditorCurveBinding binding)
		{
			string text = AnimationWindowUtility.GetNicePropertyGroupDisplayName(binding.get_type(), binding.propertyName);
			if (!string.IsNullOrEmpty(binding.path))
			{
				text = binding.path + " : " + text;
			}
			return text;
		}

		private static string PropertyName(EditorCurveBinding binding)
		{
			return AnimationWindowUtility.GetPropertyDisplayName(binding.propertyName);
		}

		public override void FetchData()
		{
			if (!(this.m_Clip == null))
			{
				List<EditorCurveBinding> source = AnimationUtility.GetCurveBindings(this.m_Clip).ToList<EditorCurveBinding>();
				var enumerable = source.GroupBy((EditorCurveBinding p) => BindingTreeViewDataSource.GroupName(p), (EditorCurveBinding p) => p, (string key, IEnumerable<EditorCurveBinding> g) => new
				{
					parent = key,
					bindings = g.ToList<EditorCurveBinding>()
				});
				this.m_RootItem = new CurveTreeViewNode(-1, null, "root", null);
				this.m_RootItem.set_children(new List<TreeViewItem>());
				int hashCode = Guid.NewGuid().GetHashCode();
				foreach (var current in enumerable)
				{
					CurveTreeViewNode curveTreeViewNode = new CurveTreeViewNode(hashCode++, this.m_RootItem, current.parent, current.bindings.ToArray());
					this.m_RootItem.get_children().Add(curveTreeViewNode);
					if (current.bindings.Count > 1)
					{
						for (int i = 0; i < current.bindings.Count; i++)
						{
							if (curveTreeViewNode.get_children() == null)
							{
								curveTreeViewNode.set_children(new List<TreeViewItem>());
							}
							CurveTreeViewNode item = new CurveTreeViewNode(hashCode++, curveTreeViewNode, BindingTreeViewDataSource.PropertyName(current.bindings[i]), new EditorCurveBinding[]
							{
								current.bindings[i]
							});
							curveTreeViewNode.get_children().Add(item);
						}
					}
				}
				this.SetupRootNodeSettings();
				this.m_NeedRefreshRows = true;
			}
		}

		public void UpdateData()
		{
			this.m_TreeView.ReloadData();
		}
	}
}
