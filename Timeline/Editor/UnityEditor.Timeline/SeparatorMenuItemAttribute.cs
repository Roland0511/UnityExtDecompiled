using System;

namespace UnityEditor.Timeline
{
	[AttributeUsage(AttributeTargets.All)]
	internal class SeparatorMenuItemAttribute : Attribute
	{
		public SeparatorMenuItemPosition position;

		public bool before
		{
			get
			{
				return (this.position & SeparatorMenuItemPosition.Before) == SeparatorMenuItemPosition.Before;
			}
		}

		public bool after
		{
			get
			{
				return (this.position & SeparatorMenuItemPosition.After) == SeparatorMenuItemPosition.After;
			}
		}

		public SeparatorMenuItemAttribute(SeparatorMenuItemPosition position)
		{
			this.position = position;
		}

		public SeparatorMenuItemAttribute()
		{
			this.position = SeparatorMenuItemPosition.None;
		}
	}
}
