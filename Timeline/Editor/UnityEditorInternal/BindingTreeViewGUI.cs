using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace UnityEditorInternal
{
	internal class BindingTreeViewGUI : TreeViewGUI
	{
		private static readonly float s_RowRightOffset = 10f;

		private static readonly float s_ColorIndicatorTopMargin = 3f;

		private static readonly Color s_KeyColorForNonCurves = new Color(0.7f, 0.7f, 0.7f, 0.5f);

		private static readonly Color s_ChildrenCurveLabelColor = new Color(1f, 1f, 1f, 0.7f);

		public BindingTreeViewGUI(TreeViewController treeView) : base(treeView, true)
		{
			this.k_IconWidth = 13f;
		}

		public override void OnRowGUI(Rect rowRect, TreeViewItem node, int row, bool selected, bool focused)
		{
			Color color = GUI.get_color();
			GUI.set_color((node.get_parent().get_id() != -1) ? BindingTreeViewGUI.s_ChildrenCurveLabelColor : Color.get_white());
			base.OnRowGUI(rowRect, node, row, selected, focused);
			GUI.set_color(color);
			this.DoCurveColorIndicator(rowRect, node as CurveTreeViewNode);
		}

		protected override bool IsRenaming(int id)
		{
			return false;
		}

		public override bool BeginRename(TreeViewItem item, float delay)
		{
			return false;
		}

		private void DoCurveColorIndicator(Rect rect, CurveTreeViewNode node)
		{
			if (node != null)
			{
				if (Event.get_current().get_type() == 7)
				{
					Color color = GUI.get_color();
					if (node.bindings.Length == 1 && !node.bindings[0].get_isPPtrCurve())
					{
						GUI.set_color(CurveUtility.GetPropertyColor(node.bindings[0].propertyName));
					}
					else
					{
						GUI.set_color(BindingTreeViewGUI.s_KeyColorForNonCurves);
					}
					Texture iconCurve = CurveUtility.GetIconCurve();
					rect = new Rect(rect.get_xMax() - BindingTreeViewGUI.s_RowRightOffset - (float)iconCurve.get_width() * 0.5f - 5f, rect.get_yMin() + BindingTreeViewGUI.s_ColorIndicatorTopMargin, (float)iconCurve.get_width(), (float)iconCurve.get_height());
					GUI.DrawTexture(rect, iconCurve, 2, true, 1f);
					GUI.set_color(color);
				}
			}
		}

		protected override Texture GetIconForItem(TreeViewItem item)
		{
			CurveTreeViewNode curveTreeViewNode = item as CurveTreeViewNode;
			Texture result;
			if (curveTreeViewNode == null)
			{
				result = null;
			}
			else if (curveTreeViewNode.bindings == null || curveTreeViewNode.bindings.Length == 0)
			{
				result = null;
			}
			else
			{
				result = AssetPreview.GetMiniTypeThumbnail(curveTreeViewNode.bindings[0].get_type());
			}
			return result;
		}
	}
}
