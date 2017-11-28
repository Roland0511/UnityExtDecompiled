using System;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal static class CurveBindingGroupExtensions
	{
		public static bool IsEnableGroup(this CurveBindingGroup curves)
		{
			return curves.isFloatCurve && curves.count == 1 && curves.curveBindingPairs[0].binding.propertyName == "m_Enabled";
		}

		public static bool IsVectorGroup(this CurveBindingGroup curves)
		{
			bool result;
			if (!curves.isFloatCurve)
			{
				result = false;
			}
			else if (curves.count <= 1 || curves.count > 4)
			{
				result = false;
			}
			else
			{
				char c = curves.curveBindingPairs[0].binding.propertyName.Last<char>();
				result = (c == 'x' || c == 'y' || c == 'z' || c == 'w');
			}
			return result;
		}

		public static bool IsColorGroup(this CurveBindingGroup curves)
		{
			bool result;
			if (!curves.isFloatCurve)
			{
				result = false;
			}
			else if (curves.count != 3 && curves.count != 4)
			{
				result = false;
			}
			else
			{
				char c = curves.curveBindingPairs[0].binding.propertyName.Last<char>();
				result = (c == 'r' || c == 'g' || c == 'b' || c == 'a');
			}
			return result;
		}

		public static string GetDescription(this CurveBindingGroup group, float t)
		{
			string text = string.Empty;
			if (group.isFloatCurve)
			{
				if (group.count > 1)
				{
					text = text + "(" + group.curveBindingPairs[0].curve.Evaluate(t).ToString("0.##");
					for (int i = 1; i < group.curveBindingPairs.Length; i++)
					{
						text = text + "," + group.curveBindingPairs[i].curve.Evaluate(t).ToString("0.##");
					}
					text += ")";
				}
				else
				{
					text = group.curveBindingPairs[0].curve.Evaluate(t).ToString("0.##");
				}
			}
			else if (group.isObjectCurve)
			{
				Object @object = null;
				if (group.curveBindingPairs[0].objectCurve.Length > 0)
				{
					@object = CurveEditUtility.Evaluate(group.curveBindingPairs[0].objectCurve, t);
				}
				text = ((!(@object == null)) ? @object.get_name() : "None");
			}
			return text;
		}
	}
}
