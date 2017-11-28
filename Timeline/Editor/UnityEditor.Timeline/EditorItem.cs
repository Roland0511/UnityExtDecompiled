using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal abstract class EditorItem<T> : ScriptableObject, IEditorItem where T : ITimelineItem
	{
		[SerializeField]
		protected T m_Item;

		public ITimelineItem item
		{
			get
			{
				return this.m_Item;
			}
		}

		public Object asset
		{
			get;
			set;
		}

		public string timelineName
		{
			get;
			set;
		}

		public int lastHash
		{
			get;
			set;
		}

		public bool locked
		{
			get
			{
				return this.m_Item.parentTrack != null && this.m_Item.parentTrack.locked;
			}
		}

		public override int GetHashCode()
		{
			return this.m_Item.Hash();
		}

		public void SetItem(T newItem)
		{
			this.m_Item = newItem;
		}
	}
}
