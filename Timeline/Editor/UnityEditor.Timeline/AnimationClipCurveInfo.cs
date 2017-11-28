using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class AnimationClipCurveInfo
	{
		private bool m_CurveDirty = true;

		private bool m_KeysDirty = true;

		public AnimationCurve[] curves;

		public EditorCurveBinding[] bindings;

		public EditorCurveBinding[] objectBindings;

		public List<ObjectReferenceKeyframe[]> objectCurves;

		private Dictionary<string, CurveBindingGroup> m_groupings;

		private float[] m_KeyTimes;

		private Dictionary<EditorCurveBinding, float[]> m_individualBindinsKey;

		public bool dirty
		{
			get
			{
				return this.m_CurveDirty;
			}
			set
			{
				this.m_CurveDirty = value;
				if (this.m_CurveDirty)
				{
					this.m_KeysDirty = true;
					if (this.m_groupings != null)
					{
						this.m_groupings.Clear();
					}
				}
			}
		}

		public int version
		{
			get;
			private set;
		}

		public float[] keyTimes
		{
			get
			{
				if (this.m_KeysDirty || this.m_KeyTimes == null)
				{
					this.RebuildKeyCache();
				}
				return this.m_KeyTimes;
			}
		}

		public float[] GetCurveTimes(EditorCurveBinding curve)
		{
			return this.GetCurveTimes(new EditorCurveBinding[]
			{
				curve
			});
		}

		public float[] GetCurveTimes(EditorCurveBinding[] curves)
		{
			if (this.m_KeysDirty || this.m_KeyTimes == null)
			{
				this.RebuildKeyCache();
			}
			List<float> list = new List<float>();
			for (int i = 0; i < curves.Length; i++)
			{
				EditorCurveBinding key = curves[i];
				if (this.m_individualBindinsKey.ContainsKey(key))
				{
					list.AddRange(this.m_individualBindinsKey[key]);
				}
			}
			return list.ToArray();
		}

		private void RebuildKeyCache()
		{
			this.m_individualBindinsKey = new Dictionary<EditorCurveBinding, float[]>();
			List<float> list = (from z in this.curves.SelectMany((AnimationCurve y) => y.get_keys())
			select z.get_time()).ToList<float>();
			for (int i = 0; i < this.objectCurves.Count; i++)
			{
				ObjectReferenceKeyframe[] source = this.objectCurves[i];
				list.AddRange(from x in source
				select x.time);
			}
			for (int j = 0; j < this.bindings.Count<EditorCurveBinding>(); j++)
			{
				this.m_individualBindinsKey.Add(this.bindings[j], (from k in this.curves[j].get_keys()
				select k.get_time()).Distinct<float>().ToArray<float>());
			}
			this.m_KeyTimes = (from x in list
			orderby x
			select x).Distinct<float>().ToArray<float>();
			this.m_KeysDirty = false;
		}

		public void Update(AnimationClip clip)
		{
			List<EditorCurveBinding> list = new List<EditorCurveBinding>();
			EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);
			for (int i = 0; i < curveBindings.Length; i++)
			{
				EditorCurveBinding editorCurveBinding = curveBindings[i];
				if (!editorCurveBinding.propertyName.Contains("LocalRotation.w"))
				{
					list.Add(RotationCurveInterpolation.RemapAnimationBindingForRotationCurves(editorCurveBinding, clip));
				}
			}
			this.bindings = list.ToArray();
			this.curves = new AnimationCurve[this.bindings.Length];
			for (int j = 0; j < this.bindings.Length; j++)
			{
				this.curves[j] = AnimationUtility.GetEditorCurve(clip, this.bindings[j]);
			}
			this.objectBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
			this.objectCurves = new List<ObjectReferenceKeyframe[]>(this.objectBindings.Length);
			for (int k = 0; k < this.objectBindings.Length; k++)
			{
				this.objectCurves.Add(AnimationUtility.GetObjectReferenceCurve(clip, this.objectBindings[k]));
			}
			this.m_CurveDirty = false;
			this.m_KeysDirty = true;
			this.version++;
		}

		public bool GetBindingForCurve(AnimationCurve curve, ref EditorCurveBinding binding)
		{
			bool result;
			for (int i = 0; i < this.curves.Length; i++)
			{
				if (curve == this.curves[i])
				{
					binding = this.bindings[i];
					result = true;
					return result;
				}
			}
			result = false;
			return result;
		}

		public AnimationCurve GetCurveForBinding(EditorCurveBinding binding)
		{
			AnimationCurve result;
			for (int i = 0; i < this.curves.Length; i++)
			{
				if (binding.Equals(this.bindings[i]))
				{
					result = this.curves[i];
					return result;
				}
			}
			result = null;
			return result;
		}

		public ObjectReferenceKeyframe[] GetObjectCurveForBinding(EditorCurveBinding binding)
		{
			ObjectReferenceKeyframe[] result;
			if (this.objectCurves == null)
			{
				result = null;
			}
			else
			{
				for (int i = 0; i < this.objectCurves.Count; i++)
				{
					if (binding.Equals(this.objectBindings[i]))
					{
						result = this.objectCurves[i];
						return result;
					}
				}
				result = null;
			}
			return result;
		}

		public CurveBindingGroup GetGroupBinding(string groupID)
		{
			if (this.m_groupings == null)
			{
				this.m_groupings = new Dictionary<string, CurveBindingGroup>();
			}
			CurveBindingGroup curveBindingGroup = null;
			if (!this.m_groupings.TryGetValue(groupID, out curveBindingGroup))
			{
				curveBindingGroup = new CurveBindingGroup();
				curveBindingGroup.timeRange = new Vector2(3.40282347E+38f, -3.40282347E+38f);
				curveBindingGroup.valueRange = new Vector2(3.40282347E+38f, -3.40282347E+38f);
				List<CurveBindingPair> list = new List<CurveBindingPair>();
				for (int i = 0; i < this.bindings.Length; i++)
				{
					if (this.bindings[i].GetGroupID() == groupID)
					{
						list.Add(new CurveBindingPair
						{
							binding = this.bindings[i],
							curve = this.curves[i]
						});
						for (int j = 0; j < this.curves[i].get_keys().Length; j++)
						{
							Keyframe keyframe = this.curves[i].get_keys()[j];
							curveBindingGroup.timeRange = new Vector2(Mathf.Min(keyframe.get_time(), curveBindingGroup.timeRange.x), Mathf.Max(keyframe.get_time(), curveBindingGroup.timeRange.y));
							curveBindingGroup.valueRange = new Vector2(Mathf.Min(keyframe.get_value(), curveBindingGroup.valueRange.x), Mathf.Max(keyframe.get_value(), curveBindingGroup.valueRange.y));
						}
					}
				}
				for (int k = 0; k < this.objectBindings.Length; k++)
				{
					if (this.objectBindings[k].GetGroupID() == groupID)
					{
						list.Add(new CurveBindingPair
						{
							binding = this.objectBindings[k],
							objectCurve = this.objectCurves[k]
						});
						for (int l = 0; l < this.objectCurves[k].Length; l++)
						{
							ObjectReferenceKeyframe objectReferenceKeyframe = this.objectCurves[k][l];
							curveBindingGroup.timeRange = new Vector2(Mathf.Min(objectReferenceKeyframe.time, curveBindingGroup.timeRange.x), Mathf.Max(objectReferenceKeyframe.time, curveBindingGroup.timeRange.y));
						}
					}
				}
				curveBindingGroup.curveBindingPairs = (from x in list
				orderby AnimationWindowUtility.GetComponentIndex(x.binding.propertyName)
				select x).ToArray<CurveBindingPair>();
				this.m_groupings.Add(groupID, curveBindingGroup);
			}
			return curveBindingGroup;
		}
	}
}
