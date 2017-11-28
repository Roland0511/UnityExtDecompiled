using System;
using System.Diagnostics;
using UnityEditor;

namespace UnityEngine.Timeline
{
	internal static class TimelineUndo
	{
		public static void PushDestroyUndo(TimelineAsset timeline, Object thingToDirty, Object objectToDestroy, string operation)
		{
			if (!(objectToDestroy == null))
			{
				EditorUtility.SetDirty(thingToDirty);
				if (timeline != null)
				{
					EditorUtility.SetDirty(timeline);
				}
				Undo.DestroyObjectImmediate(objectToDestroy);
			}
		}

		[Conditional("UNITY_EDITOR")]
		public static void PushUndo(Object thingToDirty, string operation)
		{
			EditorUtility.SetDirty(thingToDirty);
			Undo.RegisterCompleteObjectUndo(thingToDirty, "Timeline " + operation);
		}
	}
}
