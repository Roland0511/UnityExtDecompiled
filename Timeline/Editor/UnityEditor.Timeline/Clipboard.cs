using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal sealed class Clipboard
	{
		private static readonly int kListInitialSize = 10;

		private static List<SerializedObject> m_Data = new List<SerializedObject>(Clipboard.kListInitialSize);

		public static void AddData(Object data)
		{
			Clipboard.AddDataInternal(data);
		}

		public static void AddDataCollection(IEnumerable<Object> data)
		{
			foreach (Object current in data)
			{
				Clipboard.AddDataInternal(current);
			}
		}

		public static IEnumerable<T> GetData<T>() where T : class
		{
			IEnumerable<T> result;
			try
			{
				result = (from x in Clipboard.m_Data
				where typeof(T).IsAssignableFrom(x.get_targetObject().GetType())
				select x into y
				select y.get_targetObject() as T).ToList<T>();
			}
			catch (NullReferenceException)
			{
				result = Enumerable.Empty<T>();
			}
			return result;
		}

		public static void Clear()
		{
			Clipboard.m_Data.Clear();
		}

		private static void AddDataInternal(Object data)
		{
			SerializedObject item = new SerializedObject(data);
			Clipboard.m_Data.Add(item);
		}
	}
}
