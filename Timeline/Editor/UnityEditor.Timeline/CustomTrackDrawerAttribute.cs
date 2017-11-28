using System;

namespace UnityEditor.Timeline
{
	internal sealed class CustomTrackDrawerAttribute : Attribute
	{
		public Type assetType;

		public CustomTrackDrawerAttribute(Type type)
		{
			this.assetType = type;
		}
	}
}
