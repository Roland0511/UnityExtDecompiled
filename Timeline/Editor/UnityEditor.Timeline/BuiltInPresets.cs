using System;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal static class BuiltInPresets
	{
		private static CurvePresetLibrary s_BlendInPresets;

		private static CurvePresetLibrary s_BlendOutPresets;

		internal static CurvePresetLibrary blendInPresets
		{
			get
			{
				if (BuiltInPresets.s_BlendInPresets == null)
				{
					BuiltInPresets.s_BlendInPresets = ScriptableObject.CreateInstance<CurvePresetLibrary>();
					BuiltInPresets.s_BlendInPresets.Add(new AnimationCurve(CurveEditorWindow.GetConstantKeys(1f)), "None");
					BuiltInPresets.s_BlendInPresets.Add(new AnimationCurve(CurveEditorWindow.GetLinearKeys()), "Linear");
					BuiltInPresets.s_BlendInPresets.Add(new AnimationCurve(CurveEditorWindow.GetEaseInKeys()), "EaseIn");
					BuiltInPresets.s_BlendInPresets.Add(new AnimationCurve(CurveEditorWindow.GetEaseOutKeys()), "EaseOut");
					BuiltInPresets.s_BlendInPresets.Add(new AnimationCurve(CurveEditorWindow.GetEaseInOutKeys()), "EaseInOut");
				}
				return BuiltInPresets.s_BlendInPresets;
			}
		}

		internal static CurvePresetLibrary blendOutPresets
		{
			get
			{
				if (BuiltInPresets.s_BlendOutPresets == null)
				{
					BuiltInPresets.s_BlendOutPresets = ScriptableObject.CreateInstance<CurvePresetLibrary>();
					BuiltInPresets.s_BlendOutPresets.Add(new AnimationCurve(CurveEditorWindow.GetConstantKeys(1f)), "None");
					BuiltInPresets.s_BlendOutPresets.Add(BuiltInPresets.ReverseCurve(new AnimationCurve(CurveEditorWindow.GetLinearKeys())), "Linear");
					BuiltInPresets.s_BlendOutPresets.Add(BuiltInPresets.ReverseCurve(new AnimationCurve(CurveEditorWindow.GetEaseInKeys())), "EaseIn");
					BuiltInPresets.s_BlendOutPresets.Add(BuiltInPresets.ReverseCurve(new AnimationCurve(CurveEditorWindow.GetEaseOutKeys())), "EaseOut");
					BuiltInPresets.s_BlendOutPresets.Add(BuiltInPresets.ReverseCurve(new AnimationCurve(CurveEditorWindow.GetEaseInOutKeys())), "EaseInOut");
				}
				return BuiltInPresets.s_BlendOutPresets;
			}
		}

		private static AnimationCurve ReverseCurve(AnimationCurve curve)
		{
			Keyframe[] keys = curve.get_keys();
			for (int i = 0; i < keys.Length; i++)
			{
				keys[i].set_value(1f - keys[i].get_value());
				Keyframe[] expr_35_cp_0 = keys;
				int expr_35_cp_1 = i;
				expr_35_cp_0[expr_35_cp_1].set_inTangent(expr_35_cp_0[expr_35_cp_1].get_inTangent() * -1f);
				Keyframe[] expr_4D_cp_0 = keys;
				int expr_4D_cp_1 = i;
				expr_4D_cp_0[expr_4D_cp_1].set_outTangent(expr_4D_cp_0[expr_4D_cp_1].get_outTangent() * -1f);
			}
			curve.set_keys(keys);
			return curve;
		}
	}
}
