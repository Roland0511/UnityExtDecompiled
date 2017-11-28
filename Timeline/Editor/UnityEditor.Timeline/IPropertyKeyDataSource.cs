using System;
using System.Collections.Generic;

namespace UnityEditor.Timeline
{
	internal interface IPropertyKeyDataSource
	{
		float[] GetKeys();

		Dictionary<float, string> GetDescriptions();
	}
}
