using System;

namespace UnityEditor
{
	internal interface IClipCurveEditorOwner
	{
		ClipCurveEditor clipCurveEditor
		{
			get;
		}

		bool inlineCurvesSelected
		{
			get;
			set;
		}

		bool supportsLooping
		{
			get;
		}
	}
}
