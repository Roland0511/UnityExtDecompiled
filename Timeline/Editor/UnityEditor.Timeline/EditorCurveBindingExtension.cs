using System;
using UnityEditorInternal;

namespace UnityEditor.Timeline
{
	internal static class EditorCurveBindingExtension
	{
		public static string GetGroupID(this EditorCurveBinding binding)
		{
			return binding.get_type() + AnimationWindowUtility.GetPropertyGroupName(binding.propertyName);
		}
	}
}
