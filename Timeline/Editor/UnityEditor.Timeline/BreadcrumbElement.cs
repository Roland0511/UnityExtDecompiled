using System;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[Serializable]
	internal class BreadcrumbElement
	{
		public PlayableAsset asset;

		public TimelineClip clip;

		public BreadcrumbElement()
		{
			this.asset = null;
			this.clip = null;
		}

		public override int GetHashCode()
		{
			return this.asset.GetHashCode() ^ this.clip.GetHashCode();
		}

		public override string ToString()
		{
			string result;
			if (this.asset != null)
			{
				result = this.asset.get_name();
			}
			else
			{
				result = "";
			}
			return result;
		}
	}
}
