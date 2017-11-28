using System;

namespace UnityEditor.Timeline
{
	internal interface IAttractionHandler
	{
		void OnAttractedEdge(IAttractable attractable, AttractedEdge edge, double time, double duration);
	}
}
