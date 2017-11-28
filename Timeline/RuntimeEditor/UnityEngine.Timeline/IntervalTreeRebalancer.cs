using System;

namespace UnityEngine.Timeline
{
	internal class IntervalTreeRebalancer
	{
		private IntervalTree<RuntimeElement> m_Tree;

		private int m_Hash;

		public IntervalTreeRebalancer(IntervalTree<RuntimeElement> tree)
		{
			this.m_Tree = tree;
			this.m_Hash = this.m_Tree.GetHash();
		}

		public bool Rebalance()
		{
			int hash = this.m_Tree.GetHash();
			bool result;
			if (this.m_Hash != hash)
			{
				this.m_Tree.dirty = true;
				this.m_Hash = hash;
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}
	}
}
