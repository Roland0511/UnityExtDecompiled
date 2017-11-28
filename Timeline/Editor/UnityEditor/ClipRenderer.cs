using System;
using UnityEditor.Timeline;
using UnityEngine;

namespace UnityEditor
{
	internal static class ClipRenderer
	{
		private static Mesh s_Quad;

		private static Material s_BlendMaterial;

		private static Material s_ClipMaterial;

		private static Vector3[] s_Vertices = new Vector3[4];

		private static Vector2[] s_UVs = new Vector2[4];

		private static void Initialize()
		{
			if (ClipRenderer.s_Quad == null)
			{
				ClipRenderer.s_Quad = new Mesh();
				Mesh expr_21 = ClipRenderer.s_Quad;
				expr_21.set_hideFlags(expr_21.get_hideFlags() | 52);
				ClipRenderer.s_Quad.set_name("TimelineQuadMesh");
				Vector3[] vertices = new Vector3[]
				{
					new Vector3(0f, 0f, 0f),
					new Vector3(1f, 0f, 0f),
					new Vector3(1f, 1f, 0f),
					new Vector3(0f, 1f, 0f)
				};
				Vector2[] uv = new Vector2[]
				{
					new Vector2(0f, 1f),
					new Vector2(1f, 1f),
					new Vector2(1f, 0f),
					new Vector2(0f, 0f)
				};
				int[] array = new int[]
				{
					0,
					1,
					2,
					0,
					2,
					3
				};
				Color32[] colors = new Color32[]
				{
					Color.get_white(),
					Color.get_white(),
					Color.get_white(),
					Color.get_white()
				};
				ClipRenderer.s_Quad.set_vertices(vertices);
				ClipRenderer.s_Quad.set_uv(uv);
				ClipRenderer.s_Quad.set_colors32(colors);
				ClipRenderer.s_Quad.SetIndices(array, 0, 0);
			}
			if (ClipRenderer.s_BlendMaterial == null)
			{
				Shader shader = (Shader)EditorGUIUtility.LoadRequired("Editors/TimelineWindow/DrawBlendShader.shader");
				ClipRenderer.s_BlendMaterial = new Material(shader);
			}
			if (ClipRenderer.s_ClipMaterial == null)
			{
				Shader shader2 = (Shader)EditorGUIUtility.LoadRequired("Editors/TimelineWindow/ClipShader.shader");
				ClipRenderer.s_ClipMaterial = new Material(shader2);
			}
		}

		public static void RenderTexture(Rect r, Texture mainTex, Texture mask, Color color, bool flipVertical)
		{
			ClipRenderer.Initialize();
			ClipRenderer.s_Vertices[0] = new Vector3(r.get_xMin(), r.get_yMin(), 0f);
			ClipRenderer.s_Vertices[1] = new Vector3(r.get_xMax(), r.get_yMin(), 0f);
			ClipRenderer.s_Vertices[2] = new Vector3(r.get_xMax(), r.get_yMax(), 0f);
			ClipRenderer.s_Vertices[3] = new Vector3(r.get_xMin(), r.get_yMax(), 0f);
			ClipRenderer.s_Quad.set_vertices(ClipRenderer.s_Vertices);
			float num = 0f;
			float num2 = 1f;
			if (flipVertical)
			{
				num = 1f;
				num2 = 0f;
			}
			ClipRenderer.s_UVs[0] = new Vector2(0f, num2);
			ClipRenderer.s_UVs[1] = new Vector2(1f, num2);
			ClipRenderer.s_UVs[2] = new Vector2(1f, num);
			ClipRenderer.s_UVs[3] = new Vector2(0f, num);
			ClipRenderer.s_Quad.set_uv(ClipRenderer.s_UVs);
			ClipRenderer.s_BlendMaterial.SetTexture("_MainTex", mainTex);
			ClipRenderer.s_BlendMaterial.SetTexture("_MaskTex", mask);
			ClipRenderer.s_BlendMaterial.SetColor("_Color", color);
			ClipRenderer.s_BlendMaterial.SetPass(0);
			Graphics.DrawMeshNow(ClipRenderer.s_Quad, Handles.get_matrix());
		}

		public static void RenderTexture(Rect r, GUIStyle style, Color color)
		{
			ClipRenderer.Initialize();
			ClipRenderer.s_Vertices[0] = new Vector3(r.get_xMin(), r.get_yMin(), 0f);
			ClipRenderer.s_Vertices[1] = new Vector3(r.get_xMax(), r.get_yMin(), 0f);
			ClipRenderer.s_Vertices[2] = new Vector3(r.get_xMax(), r.get_yMax(), 0f);
			ClipRenderer.s_Vertices[3] = new Vector3(r.get_xMin(), r.get_yMax(), 0f);
			ClipRenderer.s_Quad.set_vertices(ClipRenderer.s_Vertices);
			ClipRenderer.s_UVs[0] = new Vector2(0f, 1f);
			ClipRenderer.s_UVs[1] = new Vector2(1f, 1f);
			ClipRenderer.s_UVs[2] = new Vector2(1f, 0f);
			ClipRenderer.s_UVs[3] = new Vector2(0f, 0f);
			ClipRenderer.s_Quad.set_uv(ClipRenderer.s_UVs);
			ClipRenderer.s_ClipMaterial.SetTexture("_MainTex", style.get_normal().get_background());
			ClipRenderer.s_ClipMaterial.SetColor("_Color", color);
			ClipRenderer.s_ClipMaterial.SetPass(0);
			Graphics.DrawMeshNow(ClipRenderer.s_Quad, Handles.get_matrix());
		}

		public static void RenderClip(Rect r, Color color)
		{
			ClipRenderer.RenderTexture(r, DirectorStyles.Instance.timelineClip, color);
		}
	}
}
