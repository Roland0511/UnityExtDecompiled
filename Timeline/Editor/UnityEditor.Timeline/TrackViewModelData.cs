using System;

namespace UnityEditor.Timeline
{
	[Serializable]
	internal class TrackViewModelData
	{
		public static readonly float k_DefaultinlineAnimationCurveHeight = 100f;

		public bool collapsed = true;

		public bool showInlineCurves = false;

		public float inlineAnimationCurveHeight = TrackViewModelData.k_DefaultinlineAnimationCurveHeight;
	}
}
