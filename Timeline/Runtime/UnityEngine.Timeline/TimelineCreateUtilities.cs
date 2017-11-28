using System;

namespace UnityEngine.Timeline
{
	internal static class TimelineCreateUtilities
	{
		public static string GenerateUniqueActorName(TimelineAsset timeline, string prefix)
		{
			string result;
			if (!timeline.tracks.Exists((TrackAsset x) => x.get_name() == prefix))
			{
				result = prefix;
			}
			else
			{
				int num = 1;
				string newName = prefix + num;
				while (timeline.tracks.Exists((TrackAsset x) => x.get_name() == newName))
				{
					num++;
					newName = prefix + num;
				}
				result = newName;
			}
			return result;
		}

		public static void SaveAssetIntoObject(Object childAsset, Object masterAsset)
		{
			if ((masterAsset.get_hideFlags() & 52) != null)
			{
				childAsset.set_hideFlags(childAsset.get_hideFlags() | 52);
			}
			else
			{
				childAsset.set_hideFlags(childAsset.get_hideFlags() | 1);
			}
		}

		internal static bool ValidateParentTrack(TrackAsset parent, Type childType)
		{
			bool result;
			if (childType == null || !typeof(TrackAsset).IsAssignableFrom(childType))
			{
				result = false;
			}
			else if (parent == null)
			{
				result = true;
			}
			else
			{
				SupportsChildTracksAttribute supportsChildTracksAttribute = Attribute.GetCustomAttribute(parent.GetType(), typeof(SupportsChildTracksAttribute)) as SupportsChildTracksAttribute;
				if (supportsChildTracksAttribute == null)
				{
					result = false;
				}
				else if (supportsChildTracksAttribute.childType == null)
				{
					result = true;
				}
				else if (childType == supportsChildTracksAttribute.childType)
				{
					int num = 0;
					TrackAsset trackAsset = parent;
					while (trackAsset != null && trackAsset.isSubTrack)
					{
						num++;
						trackAsset = (trackAsset.parent as TrackAsset);
					}
					result = (num < supportsChildTracksAttribute.levels);
				}
				else
				{
					result = false;
				}
			}
			return result;
		}
	}
}
