using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace UnityEditorInternal
{
	internal class CurveTreeViewNode : TreeViewItem
	{
		private EditorCurveBinding[] m_Bindings;

		public EditorCurveBinding[] bindings
		{
			get
			{
				return this.m_Bindings;
			}
		}

		public CurveTreeViewNode(int id, TreeViewItem parent, string displayName, EditorCurveBinding[] bindings) : base(id, (parent == null) ? -1 : (parent.get_depth() + 1), parent, displayName)
		{
			this.m_Bindings = bindings;
		}
	}
}
