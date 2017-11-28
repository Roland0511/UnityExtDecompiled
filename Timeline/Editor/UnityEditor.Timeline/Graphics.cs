using System;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal static class Graphics
	{
		public static bool ShadowedButton(Rect rect, string text, GUIStyle style, Color shadowColor)
		{
			Rect rect2 = rect;
			rect2.set_xMin(rect2.get_xMin() + 2f);
			rect2.set_yMin(rect2.get_yMin() + 2f);
			style.get_normal().set_textColor(shadowColor);
			GUI.Label(rect2, text, style);
			return GUI.Button(rect, text, style);
		}

		public static void ShadowLabel(Rect rect, string text, GUIStyle style, Color textColor, Color shadowColor)
		{
			GUIContent content = new GUIContent(text);
			Graphics.ShadowLabel(rect, content, style, textColor, shadowColor);
		}

		public static void ShadowLabel(Rect rect, GUIContent content, GUIStyle style, Color textColor, Color shadowColor)
		{
			Rect rect2 = rect;
			rect2.set_xMin(rect2.get_xMin() + 2f);
			rect2.set_yMin(rect2.get_yMin() + 2f);
			style.get_normal().set_textColor(Color.get_black());
			GUI.Label(rect2, content, style);
			style.get_normal().set_textColor(textColor);
			GUI.Label(rect, content, style);
		}

		public static void DrawOutlineRect(Rect rect, Color color)
		{
			Graphics.DrawLine(new Vector3(rect.get_x(), rect.get_y(), 0f), new Vector3(rect.get_x() + rect.get_width(), rect.get_y(), 0f), color);
			Graphics.DrawLine(new Vector3(rect.get_x() + rect.get_width(), rect.get_y(), 0f), new Vector3(rect.get_x() + rect.get_width(), rect.get_y() + rect.get_height(), 0f), color);
			Graphics.DrawLine(new Vector3(rect.get_x(), rect.get_y(), 0f), new Vector3(rect.get_x(), rect.get_y() + rect.get_height(), 0f), color);
			Graphics.DrawLine(new Vector3(rect.get_x(), rect.get_y() + rect.get_height(), 0f), new Vector3(rect.get_x() + rect.get_width(), rect.get_y() + rect.get_height(), 0f), color);
		}

		public static void DrawLine(Vector3 p1, Vector3 p2, Color color)
		{
			Color color2 = Handles.get_color();
			Handles.set_color(color);
			Handles.DrawLine(p1, p2);
			Handles.set_color(color2);
		}

		public static void DrawLineAA(Vector3 p1, Vector3 p2, Color col)
		{
			Color color = Handles.get_color();
			Handles.set_color(col);
			Handles.DrawAAPolyLine(1f, new Vector3[]
			{
				p1,
				p2
			});
			Handles.set_color(color);
		}

		public static void DrawLineAA(float width, Vector3 p1, Vector3 p2, Color color)
		{
			Graphics.DrawAAPolyLine(width, new Vector3[]
			{
				p1,
				p2
			}, color);
		}

		public static void DrawAAPolyLine(float width, Vector3[] points, Color color)
		{
			Color color2 = Handles.get_color();
			Handles.set_color(color);
			Handles.DrawAAPolyLine(width, points);
			Handles.set_color(color2);
		}

		public static void DrawDottedLine(Vector3 p1, Vector3 p2, float segmentsLength, Color col)
		{
			HandleUtility.ApplyWireMaterial();
			GL.Begin(1);
			GL.Color(col);
			float num = Vector3.Distance(p1, p2);
			int num2 = Mathf.CeilToInt(num / segmentsLength);
			for (int i = 0; i < num2; i += 2)
			{
				GL.Vertex(Vector3.Lerp(p1, p2, (float)i * segmentsLength / num));
				GL.Vertex(Vector3.Lerp(p1, p2, (float)(i + 1) * segmentsLength / num));
			}
			GL.End();
		}
	}
}
