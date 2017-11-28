using System;

namespace UnityEngine.Timeline
{
	internal class TrackType
	{
		public readonly Type trackType;

		public readonly TimelineAsset.MediaType mediaType;

		public bool requiresGameObjectBinding
		{
			get
			{
				bool result;
				if (this.mediaType == TimelineAsset.MediaType.Animation || this.mediaType == TimelineAsset.MediaType.Audio)
				{
					result = true;
				}
				else if (!this.trackType.IsDefined(typeof(TrackBindingTypeAttribute), true))
				{
					result = false;
				}
				else
				{
					TrackBindingTypeAttribute trackBindingTypeAttribute = Attribute.GetCustomAttribute(this.trackType, typeof(TrackBindingTypeAttribute), true) as TrackBindingTypeAttribute;
					result = (typeof(GameObject).IsAssignableFrom(trackBindingTypeAttribute.type) || typeof(Component).IsAssignableFrom(trackBindingTypeAttribute.type));
				}
				return result;
			}
		}

		public TrackType(Type trackType, TimelineAsset.MediaType mediaType)
		{
			this.trackType = trackType;
			this.mediaType = mediaType;
		}

		public TrackType(Type trackType)
		{
			this.trackType = trackType;
			this.mediaType = TimelineAsset.MediaType.Animation;
			object[] customAttributes = trackType.GetCustomAttributes(typeof(TrackMediaType), true);
			if (customAttributes.Length > 0)
			{
				this.mediaType = ((TrackMediaType)customAttributes[0]).m_MediaType;
			}
		}

		public override bool Equals(object obj)
		{
			bool result;
			if (obj == null)
			{
				result = false;
			}
			else
			{
				TrackType trackType = obj as TrackType;
				result = (trackType != null && this.trackType == trackType.trackType);
			}
			return result;
		}

		public bool Equals(TrackType p)
		{
			return p != null && this.trackType == p.trackType;
		}

		public override int GetHashCode()
		{
			return this.trackType.GetHashCode() ^ this.mediaType.GetHashCode();
		}
	}
}
