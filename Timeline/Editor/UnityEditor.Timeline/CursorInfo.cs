using System;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class CursorInfo
	{
		public Rect bounds;

		public MouseCursor cursor;

		private ulong m_Id;

		private static ulong ms_Counter;

		public ulong ID
		{
			get
			{
				return this.m_Id;
			}
		}

		public CursorInfo()
		{
			this.bounds = default(Rect);
			this.cursor = 0;
			ulong expr_23 = CursorInfo.ms_Counter;
			CursorInfo.ms_Counter = expr_23 + 1uL;
			this.m_Id = expr_23;
		}
	}
}
