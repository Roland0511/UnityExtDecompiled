using System;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal abstract class Manipulator
	{
		public static bool showEventConsumer
		{
			get;
			set;
		}

		protected Manipulator()
		{
			Manipulator.showEventConsumer = false;
		}

		public abstract void Init(IControl parent);

		public bool ConsumeEvent()
		{
			if (Manipulator.showEventConsumer)
			{
				Debug.Log(string.Concat(new object[]
				{
					"Event ",
					Event.get_current().get_type(),
					" consumed by ",
					base.GetType().Name
				}));
			}
			return true;
		}

		public bool IgnoreEvent()
		{
			return false;
		}

		public static List<IBounds> GetElementsAtPosition(QuadTree<IBounds> qtree, Vector2 point)
		{
			Rect r = new Rect(point.x, point.y, 1f, 1f);
			return Manipulator.GetElementsInRectangle(qtree, r);
		}

		public static List<IBounds> GetElementsInRectangle(QuadTree<IBounds> qtree, Rect r)
		{
			return qtree.ContainedBy(r);
		}
	}
}
