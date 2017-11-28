using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class AnimationClipCurveCache
	{
		private static AnimationClipCurveCache s_Instance;

		private Dictionary<AnimationClip, AnimationClipCurveInfo> m_ClipCache = new Dictionary<AnimationClip, AnimationClipCurveInfo>();

		private bool m_IsEnabled;

		public static AnimationClipCurveCache Instance
		{
			get
			{
				if (AnimationClipCurveCache.s_Instance == null)
				{
					AnimationClipCurveCache.s_Instance = new AnimationClipCurveCache();
				}
				return AnimationClipCurveCache.s_Instance;
			}
		}

		public void OnEnable()
		{
			if (!this.m_IsEnabled)
			{
				AnimationUtility.onCurveWasModified = (AnimationUtility.OnCurveWasModified)Delegate.Combine(AnimationUtility.onCurveWasModified, new AnimationUtility.OnCurveWasModified(this.OnCurveWasModified));
				this.m_IsEnabled = true;
			}
		}

		public void OnDisable()
		{
			if (this.m_IsEnabled)
			{
				AnimationUtility.onCurveWasModified = (AnimationUtility.OnCurveWasModified)Delegate.Remove(AnimationUtility.onCurveWasModified, new AnimationUtility.OnCurveWasModified(this.OnCurveWasModified));
				this.m_IsEnabled = false;
			}
		}

		private void OnCurveWasModified(AnimationClip clip, EditorCurveBinding binding, AnimationUtility.CurveModifiedType modification)
		{
			AnimationClipCurveInfo animationClipCurveInfo;
			if (modification == null)
			{
				this.m_ClipCache.Remove(clip);
			}
			else if (this.m_ClipCache.TryGetValue(clip, out animationClipCurveInfo))
			{
				animationClipCurveInfo.dirty = true;
			}
		}

		public AnimationClipCurveInfo GetCurveInfo(AnimationClip clip)
		{
			AnimationClipCurveInfo result;
			if (clip == null)
			{
				result = null;
			}
			else
			{
				AnimationClipCurveInfo animationClipCurveInfo;
				if (!this.m_ClipCache.TryGetValue(clip, out animationClipCurveInfo))
				{
					animationClipCurveInfo = new AnimationClipCurveInfo();
					animationClipCurveInfo.dirty = true;
					this.m_ClipCache[clip] = animationClipCurveInfo;
				}
				if (animationClipCurveInfo.dirty)
				{
					animationClipCurveInfo.Update(clip);
				}
				result = animationClipCurveInfo;
			}
			return result;
		}
	}
}
