using System;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal interface IEditorItem
	{
		ITimelineItem item
		{
			get;
		}

		bool locked
		{
			get;
		}

		string timelineName
		{
			get;
			set;
		}
	}
}
