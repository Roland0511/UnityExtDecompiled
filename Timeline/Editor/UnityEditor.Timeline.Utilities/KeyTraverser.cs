using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline.Utilities
{
	internal class KeyTraverser
	{
		private float[] m_KeyCache;

		private int m_DirtyStamp = -1;

		private int m_LastHash = -1;

		private readonly TimelineAsset m_Asset;

		private readonly float m_Epsilon;

		private int m_LastIndex = -1;

		public int lastIndex
		{
			get
			{
				return this.m_LastIndex;
			}
		}

		public KeyTraverser(TimelineAsset timeline, float epsilon)
		{
			this.m_Asset = timeline;
			this.m_Epsilon = epsilon;
		}

		public static IEnumerable<float> GetClipKeyTimes(TimelineClip clip)
		{
			IEnumerable<float> result;
			if (clip == null || clip.animationClip == null || clip.animationClip.get_empty())
			{
				result = new float[0];
			}
			else
			{
				result = from k in AnimationClipCurveCache.Instance.GetCurveInfo(clip.animationClip).keyTimes
				select (float)clip.FromLocalTimeUnbound((double)k) into k
				where (double)k >= clip.start && (double)k <= clip.end
				select k;
			}
			return result;
		}

		public static IEnumerable<float> GetTrackKeyTimes(AnimationTrack track)
		{
			IEnumerable<float> result;
			if (track != null)
			{
				if (track.inClipMode)
				{
					result = (from c in track.clips
					where c.recordable
					select c).SelectMany((TimelineClip x) => KeyTraverser.GetClipKeyTimes(x));
					return result;
				}
				if (track.animClip != null && !track.animClip.get_empty())
				{
					result = AnimationClipCurveCache.Instance.GetCurveInfo(track.animClip).keyTimes;
					return result;
				}
			}
			result = new float[0];
			return result;
		}

		private static int CalcAnimClipHash(TrackAsset asset)
		{
			int num = 0;
			if (asset != null)
			{
				AnimationTrack animationTrack = asset as AnimationTrack;
				if (animationTrack != null)
				{
					for (int num2 = 0; num2 != animationTrack.clips.Length; num2++)
					{
						num ^= ((ITimelineItem)animationTrack.clips[num2]).Hash();
					}
				}
				for (int num3 = 0; num3 != asset.subTracks.Count; num3++)
				{
					num ^= KeyTraverser.CalcAnimClipHash(asset.subTracks[num3]);
				}
			}
			return num;
		}

		internal static int CalcAnimClipHash(TimelineAsset asset)
		{
			int num = 0;
			foreach (TrackAsset current in asset.tracks)
			{
				num ^= KeyTraverser.CalcAnimClipHash(current);
			}
			return num;
		}

		private void RebuildKeyCache()
		{
			this.m_KeyCache = (from x in (from x in this.m_Asset.flattenedTracks
			where x as AnimationTrack != null
			select x).Cast<AnimationTrack>().SelectMany((AnimationTrack t) => KeyTraverser.GetTrackKeyTimes(t))
			orderby x
			select x).ToArray<float>();
			if (this.m_KeyCache.Length > 0)
			{
				float[] array = new float[this.m_KeyCache.Length];
				array[0] = this.m_KeyCache[0];
				int num = 0;
				for (int i = 1; i < this.m_KeyCache.Length; i++)
				{
					if (this.m_KeyCache[i] - array[num] > this.m_Epsilon)
					{
						num++;
						array[num] = this.m_KeyCache[i];
					}
				}
				this.m_KeyCache = array;
				Array.Resize<float>(ref this.m_KeyCache, num + 1);
			}
		}

		private void CheckCache(int dirtyStamp)
		{
			int num = KeyTraverser.CalcAnimClipHash(this.m_Asset);
			if (dirtyStamp != this.m_DirtyStamp || num != this.m_LastHash)
			{
				this.RebuildKeyCache();
				this.m_DirtyStamp = dirtyStamp;
				this.m_LastHash = num;
			}
		}

		public float GetNextKey(float key, int dirtyStamp)
		{
			this.CheckCache(dirtyStamp);
			float result;
			if (this.m_KeyCache.Length > 0)
			{
				if (key < this.m_KeyCache.Last<float>() - this.m_Epsilon)
				{
					if (key > this.m_KeyCache[0] - this.m_Epsilon)
					{
						float num = key + this.m_Epsilon;
						int num2 = this.m_KeyCache.Length - 1;
						int num3 = 0;
						while (num2 - num3 > 1)
						{
							int num4 = (num3 + num2) / 2;
							if (num > this.m_KeyCache[num4])
							{
								num3 = num4;
							}
							else
							{
								num2 = num4;
							}
						}
						this.m_LastIndex = num2;
						result = this.m_KeyCache[num2];
						return result;
					}
					this.m_LastIndex = 0;
					result = this.m_KeyCache[0];
					return result;
				}
				else if (key < this.m_KeyCache.Last<float>() + this.m_Epsilon)
				{
					this.m_LastIndex = this.m_KeyCache.Length - 1;
					result = Mathf.Max(key, this.m_KeyCache.Last<float>());
					return result;
				}
			}
			this.m_LastIndex = -1;
			result = key;
			return result;
		}

		public float GetPrevKey(float key, int dirtyStamp)
		{
			this.CheckCache(dirtyStamp);
			float result;
			if (this.m_KeyCache.Length > 0)
			{
				if (key > this.m_KeyCache[0] + this.m_Epsilon)
				{
					if (key < this.m_KeyCache.Last<float>() + this.m_Epsilon)
					{
						float num = key - this.m_Epsilon;
						int num2 = this.m_KeyCache.Length - 1;
						int num3 = 0;
						while (num2 - num3 > 1)
						{
							int num4 = (num3 + num2) / 2;
							if (num < this.m_KeyCache[num4])
							{
								num2 = num4;
							}
							else
							{
								num3 = num4;
							}
						}
						this.m_LastIndex = num3;
						result = this.m_KeyCache[num3];
						return result;
					}
					this.m_LastIndex = this.m_KeyCache.Length - 1;
					result = this.m_KeyCache.Last<float>();
					return result;
				}
				else if (key >= this.m_KeyCache[0] - this.m_Epsilon)
				{
					this.m_LastIndex = 0;
					result = Mathf.Min(key, this.m_KeyCache[0]);
					return result;
				}
			}
			this.m_LastIndex = -1;
			result = key;
			return result;
		}

		public int GetKeyCount(int dirtyStamp)
		{
			this.CheckCache(dirtyStamp);
			return this.m_KeyCache.Length;
		}
	}
}
