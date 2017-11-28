using System;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal struct CurveBindingPair
	{
		public EditorCurveBinding binding;

		public AnimationCurve curve;

		public ObjectReferenceKeyframe[] objectCurve;
	}
}
