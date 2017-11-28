using System;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class BreadcrumbDrawer
	{
		private static readonly GUIStyle k_BreadCrumbLeft = "GUIEditor.BreadcrumbLeft";

		private static readonly string k_Elipsis = "...";

		private static string FitTextInArea(float areaWidth, string text, GUIStyle style)
		{
			int num = style.get_border().get_left() + style.get_border().get_right();
			float x = style.CalcSize(EditorGUIUtility.TextContent(text)).x;
			string result;
			if ((float)num + x < areaWidth)
			{
				result = text;
			}
			else
			{
				float num2 = areaWidth - (float)num;
				float num3 = x / (float)text.Length;
				int num4 = (int)Mathf.Floor(num2 / num3);
				num4 -= 3;
				if (num4 < 0)
				{
					result = BreadcrumbDrawer.k_Elipsis;
				}
				else if (num4 <= text.Length)
				{
					result = BreadcrumbDrawer.k_Elipsis + " " + text.Substring(text.Length - num4);
				}
				else
				{
					result = BreadcrumbDrawer.k_Elipsis;
				}
			}
			return result;
		}

		public static void Draw(float breadcrumbAreaWidth, string timelineAssetName, string directorName)
		{
			GUILayout.BeginHorizontal(new GUILayoutOption[]
			{
				GUILayout.Width(breadcrumbAreaWidth)
			});
			if (!string.IsNullOrEmpty(directorName))
			{
				timelineAssetName = timelineAssetName + " (" + directorName + ")";
			}
			GUILayout.Box(BreadcrumbDrawer.FitTextInArea(breadcrumbAreaWidth, timelineAssetName, BreadcrumbDrawer.k_BreadCrumbLeft), BreadcrumbDrawer.k_BreadCrumbLeft, new GUILayoutOption[0]);
			GUILayout.Space(10f);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
		}
	}
}
