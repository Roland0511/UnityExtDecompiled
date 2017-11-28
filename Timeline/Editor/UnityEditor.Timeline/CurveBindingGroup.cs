using System;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class CurveBindingGroup
	{
		public CurveBindingPair[] curveBindingPairs
		{
			get;
			set;
		}

		public Vector2 timeRange
		{
			get;
			set;
		}

		public Vector2 valueRange
		{
			get;
			set;
		}

		public bool isFloatCurve
		{
			get
			{
				return this.curveBindingPairs != null && this.curveBindingPairs.Length > 0 && this.curveBindingPairs[0].curve != null;
			}
		}

		public bool isObjectCurve
		{
			get
			{
				return this.curveBindingPairs != null && this.curveBindingPairs.Length > 0 && this.curveBindingPairs[0].objectCurve != null;
			}
		}

		public int count
		{
			get
			{
				int result;
				if (this.curveBindingPairs == null)
				{
					result = 0;
				}
				else
				{
					result = this.curveBindingPairs.Length;
				}
				return result;
			}
		}
	}
}
