using System;
using System.Collections.Generic;

namespace UnityEditor.Timeline
{
	internal interface ISnappable
	{
		IEnumerable<Edge> SnappableEdgesFor(IAttractable attractable);
	}
}
