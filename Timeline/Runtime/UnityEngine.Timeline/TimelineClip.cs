using System;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace UnityEngine.Timeline
{
	[Serializable]
	public class TimelineClip : ITimelineItem, ISerializationCallbackReceiver
	{
		public enum ClipExtrapolation
		{
			None,
			Hold,
			Loop,
			PingPong,
			Continue
		}

		public enum BlendCurveMode
		{
			Auto,
			Manual
		}

		public static readonly ClipCaps kDefaultClipCaps = ClipCaps.Blending;

		public static readonly float kDefaultClipDurationInSeconds = 5f;

		public static readonly double kTimeScaleMin = 0.001;

		public static readonly double kTimeScaleMax = 1000.0;

		internal static readonly double kMinDuration = 0.016666666666666666;

		private const int kVersion = 1;

		internal static readonly double kMaxTimeValue = 1000000.0;

		[SerializeField]
		private double m_Start;

		[SerializeField]
		private double m_ClipIn;

		[SerializeField]
		private Object m_Asset;

		[SerializeField]
		private Object m_UnderlyingAsset;

		[FormerlySerializedAs("m_HackDuration"), SerializeField]
		private double m_Duration;

		[SerializeField]
		private double m_TimeScale = 1.0;

		[SerializeField]
		private TrackAsset m_ParentTrack;

		[SerializeField]
		private double m_EaseInDuration;

		[SerializeField]
		private double m_EaseOutDuration;

		[SerializeField]
		private double m_BlendInDuration = -1.0;

		[SerializeField]
		private double m_BlendOutDuration = -1.0;

		[SerializeField]
		private AnimationCurve m_MixInCurve;

		[SerializeField]
		private AnimationCurve m_MixOutCurve;

		[SerializeField]
		private TimelineClip.BlendCurveMode m_BlendInCurveMode = TimelineClip.BlendCurveMode.Auto;

		[SerializeField]
		private TimelineClip.BlendCurveMode m_BlendOutCurveMode = TimelineClip.BlendCurveMode.Auto;

		[SerializeField]
		private List<string> m_ExposedParameterNames;

		[SerializeField]
		private AnimationClip m_AnimationCurves;

		[SerializeField]
		private bool m_Recordable;

		[SerializeField]
		private TimelineClip.ClipExtrapolation m_PostExtrapolationMode;

		[SerializeField]
		private TimelineClip.ClipExtrapolation m_PreExtrapolationMode;

		[SerializeField]
		private double m_PostExtrapolationTime;

		[SerializeField]
		private double m_PreExtrapolationTime;

		[SerializeField]
		private string m_DisplayName;

		[HideInInspector, SerializeField]
		private int m_Version = 0;

		internal int dirtyHash
		{
			get;
			set;
		}

		public bool hasPreExtrapolation
		{
			get
			{
				return this.m_PreExtrapolationMode != TimelineClip.ClipExtrapolation.None && this.m_PreExtrapolationTime > 0.0;
			}
		}

		public bool hasPostExtrapolation
		{
			get
			{
				return this.m_PostExtrapolationMode != TimelineClip.ClipExtrapolation.None && this.m_PostExtrapolationTime > 0.0;
			}
		}

		public double timeScale
		{
			get
			{
				return (!this.clipCaps.HasAny(ClipCaps.SpeedMultiplier)) ? 1.0 : Math.Max(TimelineClip.kTimeScaleMin, Math.Min(this.m_TimeScale, TimelineClip.kTimeScaleMax));
			}
			set
			{
				this.m_TimeScale = ((!this.clipCaps.HasAny(ClipCaps.SpeedMultiplier)) ? 1.0 : Math.Max(TimelineClip.kTimeScaleMin, Math.Min(value, TimelineClip.kTimeScaleMax)));
			}
		}

		public double start
		{
			get
			{
				return this.m_Start;
			}
			set
			{
				this.m_Start = Math.Max(TimelineClip.SanitizeTimeValue(value, this.m_Start), 0.0);
			}
		}

		public double duration
		{
			get
			{
				return this.m_Duration;
			}
			set
			{
				this.m_Duration = Math.Max(TimelineClip.SanitizeTimeValue(value, this.m_Duration), 4.94065645841247E-324);
			}
		}

		public double end
		{
			get
			{
				return this.m_Start + this.m_Duration;
			}
		}

		public double clipIn
		{
			get
			{
				return (!this.clipCaps.HasAny(ClipCaps.ClipIn)) ? 0.0 : this.m_ClipIn;
			}
			set
			{
				this.m_ClipIn = ((!this.clipCaps.HasAny(ClipCaps.ClipIn)) ? 0.0 : Math.Max(Math.Min(TimelineClip.SanitizeTimeValue(value, this.m_ClipIn), TimelineClip.kMaxTimeValue), 0.0));
			}
		}

		public string displayName
		{
			get
			{
				return this.m_DisplayName;
			}
			set
			{
				this.m_DisplayName = value;
			}
		}

		public double clipAssetDuration
		{
			get
			{
				IPlayableAsset playableAsset = this.m_Asset as IPlayableAsset;
				return (playableAsset == null) ? 1.7976931348623157E+308 : playableAsset.get_duration();
			}
		}

		public AnimationClip curves
		{
			get
			{
				return this.m_AnimationCurves;
			}
		}

		public Object asset
		{
			get
			{
				return this.m_Asset;
			}
			set
			{
				this.m_Asset = value;
			}
		}

		public Object underlyingAsset
		{
			get
			{
				return (!(this.m_UnderlyingAsset != null)) ? this.m_Asset : this.m_UnderlyingAsset;
			}
			set
			{
				this.m_UnderlyingAsset = value;
			}
		}

		public TrackAsset parentTrack
		{
			get
			{
				return this.m_ParentTrack;
			}
			set
			{
				if (!(value == null))
				{
					if (!(this.m_ParentTrack == value))
					{
						if (this.m_ParentTrack != null)
						{
							this.m_ParentTrack.RemoveClip(this);
						}
						this.m_ParentTrack = value;
						this.m_ParentTrack.AddClip(this);
					}
				}
			}
		}

		public double easeInDuration
		{
			get
			{
				return (!this.clipCaps.HasAny(ClipCaps.Blending)) ? 0.0 : Math.Min(Math.Max(this.m_EaseInDuration, 0.0), this.duration * 0.49);
			}
			set
			{
				this.m_EaseInDuration = ((!this.clipCaps.HasAny(ClipCaps.Blending)) ? 0.0 : TimelineClip.SanitizeTimeValue(value, this.m_EaseInDuration));
			}
		}

		public double easeOutDuration
		{
			get
			{
				return (!this.clipCaps.HasAny(ClipCaps.Blending)) ? 0.0 : Math.Min(Math.Max(this.m_EaseOutDuration, 0.0), this.duration * 0.49);
			}
			set
			{
				this.m_EaseOutDuration = ((!this.clipCaps.HasAny(ClipCaps.Blending)) ? 0.0 : TimelineClip.SanitizeTimeValue(value, this.m_EaseOutDuration));
			}
		}

		public double eastOutTime
		{
			get
			{
				return this.duration - this.easeOutDuration + this.m_Start;
			}
		}

		public double blendInDuration
		{
			get
			{
				return (!this.clipCaps.HasAny(ClipCaps.Blending)) ? 0.0 : this.m_BlendInDuration;
			}
			set
			{
				this.m_BlendInDuration = ((!this.clipCaps.HasAny(ClipCaps.Blending)) ? 0.0 : TimelineClip.SanitizeTimeValue(value, this.m_BlendInDuration));
			}
		}

		public double blendOutDuration
		{
			get
			{
				return (!this.clipCaps.HasAny(ClipCaps.Blending)) ? 0.0 : this.m_BlendOutDuration;
			}
			set
			{
				this.m_BlendOutDuration = ((!this.clipCaps.HasAny(ClipCaps.Blending)) ? 0.0 : TimelineClip.SanitizeTimeValue(value, this.m_BlendOutDuration));
			}
		}

		public TimelineClip.BlendCurveMode blendInCurveMode
		{
			get
			{
				return this.m_BlendInCurveMode;
			}
			set
			{
				this.m_BlendInCurveMode = value;
			}
		}

		public TimelineClip.BlendCurveMode blendOutCurveMode
		{
			get
			{
				return this.m_BlendOutCurveMode;
			}
			set
			{
				this.m_BlendOutCurveMode = value;
			}
		}

		public bool hasBlendIn
		{
			get
			{
				return this.clipCaps.HasAny(ClipCaps.Blending) && this.m_BlendInDuration > 0.0;
			}
		}

		public bool hasBlendOut
		{
			get
			{
				return this.clipCaps.HasAny(ClipCaps.Blending) && this.m_BlendOutDuration > 0.0;
			}
		}

		public AnimationCurve mixInCurve
		{
			get
			{
				if (this.m_MixInCurve == null || this.m_MixInCurve.get_length() < 2)
				{
					this.m_MixInCurve = TimelineClip.GetDefaultMixInCurve();
				}
				return this.m_MixInCurve;
			}
			set
			{
				this.m_MixInCurve = value;
			}
		}

		public float mixInPercentage
		{
			get
			{
				return (float)(this.mixInDuration / this.duration);
			}
		}

		public double mixInDuration
		{
			get
			{
				return (!this.hasBlendIn) ? this.easeInDuration : this.blendInDuration;
			}
		}

		public AnimationCurve mixOutCurve
		{
			get
			{
				if (this.m_MixOutCurve == null || this.m_MixOutCurve.get_length() < 2)
				{
					this.m_MixOutCurve = TimelineClip.GetDefaultMixOutCurve();
				}
				return this.m_MixOutCurve;
			}
			set
			{
				this.m_MixOutCurve = value;
			}
		}

		public double mixOutTime
		{
			get
			{
				return this.duration - this.mixOutDuration + this.m_Start;
			}
		}

		public double mixOutDuration
		{
			get
			{
				return (!this.hasBlendOut) ? this.easeOutDuration : this.blendOutDuration;
			}
		}

		public float mixOutPercentage
		{
			get
			{
				return (float)(this.mixOutDuration / this.duration);
			}
		}

		public bool recordable
		{
			get
			{
				return this.m_Recordable;
			}
			internal set
			{
				this.m_Recordable = value;
			}
		}

		public List<string> exposedParameters
		{
			get
			{
				List<string> arg_1C_0;
				if ((arg_1C_0 = this.m_ExposedParameterNames) == null)
				{
					arg_1C_0 = (this.m_ExposedParameterNames = new List<string>());
				}
				return arg_1C_0;
			}
		}

		public ClipCaps clipCaps
		{
			get
			{
				ITimelineClipAsset timelineClipAsset = this.asset as ITimelineClipAsset;
				return (timelineClipAsset == null) ? TimelineClip.kDefaultClipCaps : timelineClipAsset.clipCaps;
			}
		}

		public AnimationClip animationClip
		{
			get
			{
				AnimationClip result;
				if (this.m_Asset == null)
				{
					result = null;
				}
				else
				{
					AnimationPlayableAsset animationPlayableAsset = this.m_Asset as AnimationPlayableAsset;
					result = ((!(animationPlayableAsset != null)) ? null : animationPlayableAsset.clip);
				}
				return result;
			}
		}

		public TimelineClip.ClipExtrapolation postExtrapolationMode
		{
			get
			{
				return (!this.clipCaps.HasAny(ClipCaps.Extrapolation)) ? TimelineClip.ClipExtrapolation.None : this.m_PostExtrapolationMode;
			}
			internal set
			{
				this.m_PostExtrapolationMode = ((!this.clipCaps.HasAny(ClipCaps.Extrapolation)) ? TimelineClip.ClipExtrapolation.None : value);
			}
		}

		public TimelineClip.ClipExtrapolation preExtrapolationMode
		{
			get
			{
				return (!this.clipCaps.HasAny(ClipCaps.Extrapolation)) ? TimelineClip.ClipExtrapolation.None : this.m_PreExtrapolationMode;
			}
			internal set
			{
				this.m_PreExtrapolationMode = ((!this.clipCaps.HasAny(ClipCaps.Extrapolation)) ? TimelineClip.ClipExtrapolation.None : value);
			}
		}

		public double extrapolatedStart
		{
			get
			{
				double result;
				if (this.m_PreExtrapolationMode != TimelineClip.ClipExtrapolation.None)
				{
					result = this.m_Start - this.m_PreExtrapolationTime;
				}
				else
				{
					result = this.m_Start;
				}
				return result;
			}
		}

		public double extrapolatedDuration
		{
			get
			{
				double num = this.m_Duration;
				if (this.m_PostExtrapolationMode != TimelineClip.ClipExtrapolation.None)
				{
					num += Math.Min(this.m_PostExtrapolationTime, TimelineClip.kMaxTimeValue);
				}
				if (this.m_PreExtrapolationMode != TimelineClip.ClipExtrapolation.None)
				{
					num += this.m_PreExtrapolationTime;
				}
				return num;
			}
		}

		internal TimelineClip(TrackAsset parent)
		{
			this.parentTrack = parent;
		}

		int ITimelineItem.Hash()
		{
			int arg_6D_0 = this.m_Start.GetHashCode();
			int arg_6D_1 = this.m_Duration.GetHashCode();
			int arg_6D_2 = this.m_TimeScale.GetHashCode();
			int arg_6D_3 = this.m_ClipIn.GetHashCode();
			int preExtrapolationMode = (int)this.m_PreExtrapolationMode;
			int arg_6D_4 = preExtrapolationMode.GetHashCode();
			int postExtrapolationMode = (int)this.m_PostExtrapolationMode;
			return HashUtility.CombineHash(arg_6D_0, arg_6D_1, arg_6D_2, arg_6D_3, arg_6D_4, postExtrapolationMode.GetHashCode());
		}

		public float EvaluateMixOut(double localTime)
		{
			float result;
			if (!this.clipCaps.HasAny(ClipCaps.Blending))
			{
				result = 1f;
			}
			else if (this.mixOutDuration > (double)Mathf.Epsilon)
			{
				float num = (float)(localTime - this.mixOutTime) / (float)this.mixOutDuration;
				num = Mathf.Clamp01(this.mixOutCurve.Evaluate(num));
				result = num;
			}
			else
			{
				result = 1f;
			}
			return result;
		}

		public float EvaluateMixIn(double localTime)
		{
			float result;
			if (!this.clipCaps.HasAny(ClipCaps.Blending))
			{
				result = 1f;
			}
			else if (this.mixInDuration > (double)Mathf.Epsilon)
			{
				float num = (float)(localTime - this.m_Start) / (float)this.mixInDuration;
				num = Mathf.Clamp01(this.mixInCurve.Evaluate(num));
				result = num;
			}
			else
			{
				result = 1f;
			}
			return result;
		}

		private static AnimationCurve GetDefaultMixInCurve()
		{
			return AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		}

		private static AnimationCurve GetDefaultMixOutCurve()
		{
			return AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
		}

		public double ToLocalTime(double time)
		{
			double result;
			if (time < 0.0)
			{
				result = time;
			}
			else
			{
				if (this.IsPreExtrapolatedTime(time))
				{
					time = TimelineClip.GetExtrapolatedTime(time - this.m_Start, this.m_PreExtrapolationMode, this.m_Duration);
				}
				else if (this.IsPostExtrapolatedTime(time))
				{
					time = TimelineClip.GetExtrapolatedTime(time - this.m_Start, this.m_PostExtrapolationMode, this.m_Duration);
				}
				else
				{
					time -= this.m_Start;
				}
				time *= this.timeScale;
				time += this.clipIn;
				result = time;
			}
			return result;
		}

		public double ToLocalTimeUnbound(double time)
		{
			return (time - this.m_Start) * this.timeScale + this.clipIn;
		}

		internal double FromLocalTimeUnbound(double time)
		{
			return (time - this.clipIn) / this.timeScale + this.m_Start;
		}

		private static double SanitizeTimeValue(double value, double defaultValue)
		{
			double result;
			if (double.IsInfinity(value) || double.IsNaN(value))
			{
				Debug.LogError("Invalid time value assigned");
				result = defaultValue;
			}
			else
			{
				result = Math.Max(-TimelineClip.kMaxTimeValue, Math.Min(TimelineClip.kMaxTimeValue, value));
			}
			return result;
		}

		internal void SetPostExtrapolationTime(double time)
		{
			this.m_PostExtrapolationTime = time;
		}

		internal void SetPreExtrapolationTime(double time)
		{
			this.m_PreExtrapolationTime = time;
		}

		public bool IsExtrapolatedTime(double sequenceTime)
		{
			return this.IsPreExtrapolatedTime(sequenceTime) || this.IsPostExtrapolatedTime(sequenceTime);
		}

		public bool IsPreExtrapolatedTime(double sequenceTime)
		{
			return this.preExtrapolationMode != TimelineClip.ClipExtrapolation.None && sequenceTime < this.m_Start && sequenceTime >= this.m_Start - this.m_PreExtrapolationTime;
		}

		public bool IsPostExtrapolatedTime(double sequenceTime)
		{
			return this.postExtrapolationMode != TimelineClip.ClipExtrapolation.None && sequenceTime > this.end && sequenceTime - this.end < this.m_PostExtrapolationTime;
		}

		private static double GetExtrapolatedTime(double time, TimelineClip.ClipExtrapolation mode, double duration)
		{
			double result;
			if (duration == 0.0)
			{
				result = 0.0;
			}
			else
			{
				switch (mode)
				{
				case TimelineClip.ClipExtrapolation.Hold:
					if (time < 0.0)
					{
						result = 0.0;
						return result;
					}
					if (time > duration)
					{
						result = duration;
						return result;
					}
					break;
				case TimelineClip.ClipExtrapolation.Loop:
					if (time < 0.0)
					{
						time = duration - -time % duration;
					}
					else if (time > duration)
					{
						time %= duration;
					}
					break;
				case TimelineClip.ClipExtrapolation.PingPong:
					if (time < 0.0)
					{
						time = duration * 2.0 - -time % (duration * 2.0);
						time = duration - Math.Abs(time - duration);
					}
					else
					{
						time %= duration * 2.0;
						time = duration - Math.Abs(time - duration);
					}
					break;
				}
				result = time;
			}
			return result;
		}

		internal void AllocateAnimatedParameterCurves()
		{
			if (this.m_AnimationCurves == null)
			{
				AnimationClip animationClip = new AnimationClip();
				animationClip.set_legacy(true);
				this.m_AnimationCurves = animationClip;
			}
		}

		internal void ClearAnimatedParameterCurves()
		{
			this.m_AnimationCurves = null;
		}

		void ISerializationCallbackReceiver.OnBeforeSerialize()
		{
			this.m_Version = 1;
		}

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (this.m_Version < 1)
			{
				if (this.m_ClipIn > 0.0 && this.m_TimeScale > 1.4012984643248171E-45)
				{
					this.m_ClipIn *= this.m_TimeScale;
				}
			}
		}
	}
}
