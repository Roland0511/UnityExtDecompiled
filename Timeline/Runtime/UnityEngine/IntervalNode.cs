using System;
using System.Collections.Generic;

namespace UnityEngine
{
	internal class IntervalNode<T> where T : class, IInterval
	{
		private long m_Center;

		private List<T> m_Children;

		private IntervalNode<T> m_LeftNode;

		private IntervalNode<T> m_RightNode;

		public IntervalNode(ICollection<T> items)
		{
			if (items.Count != 0)
			{
				long num = 9223372036854775807L;
				long num2 = -9223372036854775808L;
				foreach (T current in items)
				{
					num = Math.Min(num, current.intervalStart);
					num2 = Math.Max(num2, current.intervalEnd);
				}
				this.m_Center = (num2 + num) / 2L;
				this.m_Children = new List<T>();
				List<T> list = new List<T>();
				List<T> list2 = new List<T>();
				foreach (T current2 in items)
				{
					if (current2.intervalEnd < this.m_Center)
					{
						list.Add(current2);
					}
					else if (current2.intervalStart > this.m_Center)
					{
						list2.Add(current2);
					}
					else
					{
						this.m_Children.Add(current2);
					}
				}
				if (this.m_Children.Count == 0)
				{
					this.m_Children = null;
				}
				if (list.Count > 0)
				{
					this.m_LeftNode = new IntervalNode<T>(list);
				}
				if (list2.Count > 0)
				{
					this.m_RightNode = new IntervalNode<T>(list2);
				}
			}
		}

		public void Query(long time, int bitflag, ref List<T> results)
		{
			if (this.m_Children != null)
			{
				foreach (T current in this.m_Children)
				{
					if (time >= current.intervalStart && time < current.intervalEnd)
					{
						current.intervalBit = bitflag;
						results.Add(current);
					}
				}
			}
			if (time < this.m_Center && this.m_LeftNode != null)
			{
				this.m_LeftNode.Query(time, bitflag, ref results);
			}
			else if (time > this.m_Center && this.m_RightNode != null)
			{
				this.m_RightNode.Query(time, bitflag, ref results);
			}
		}
	}
}
