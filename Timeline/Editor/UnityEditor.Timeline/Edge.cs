using System;
using System.Runtime.InteropServices;

namespace UnityEditor.Timeline
{
	[StructLayout(LayoutKind.Sequential, Size = 1)]
	internal struct Edge
	{
		public double time
		{
			get;
			set;
		}

		public Edge(double edgeTime)
		{
			this.time = edgeTime;
		}
	}
}
