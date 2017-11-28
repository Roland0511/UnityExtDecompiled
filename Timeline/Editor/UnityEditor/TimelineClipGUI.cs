using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor
{
	internal class TimelineClipGUI : TimelineItemGUI, IClipCurveEditorOwner, ISnappable, IAttractable
	{
		internal enum BlendKind
		{
			None,
			Ease,
			Mix
		}

		private Rect m_ClipCenterSection;

		private readonly List<Rect> m_LoopRects = new List<Rect>();

		private int m_ProjectedClipHash;

		private readonly TimelineClipHandle m_LeftHandle;

		private readonly TimelineClipHandle m_RightHandle;

		private readonly TimelineBlendHandle m_BlendInHandle;

		private readonly TimelineBlendHandle m_BlendOutHandle;

		private TrackDrawer.ClipDrawData m_ClipDrawData;

		private Rect m_MixOutRect = default(Rect);

		private Rect m_MixInRect = default(Rect);

		private int m_MinLoopIndex = 1;

		private static readonly float k_MinMixWidth = 2f;

		private static readonly float k_HandleWidth = 10f;

		public bool supportResize
		{
			get;
			set;
		}

		public ClipCurveEditor clipCurveEditor
		{
			get;
			private set;
		}

		public bool hideName
		{
			get;
			set;
		}

		public TimelineClipGUI previousClip
		{
			get;
			set;
		}

		public TimelineClipGUI nextClip
		{
			get;
			set;
		}

		public float blendingStopsAt
		{
			get
			{
				return (float)(this.clip.start + Math.Max(0.0, this.clip.blendInDuration));
			}
		}

		public ReadOnlyCollection<Rect> loopRects
		{
			get
			{
				return this.m_LoopRects.AsReadOnly();
			}
		}

		public bool overlaps
		{
			get
			{
				return this.clip.hasBlendIn;
			}
		}

		public bool isOverlapped
		{
			get
			{
				return this.clip.hasBlendOut;
			}
		}

		public string name
		{
			get
			{
				string result;
				if (this.hideName)
				{
					result = string.Empty;
				}
				else if (this.clip.displayName == null)
				{
					result = "(Empty)";
				}
				else
				{
					result = this.clip.displayName;
				}
				return result;
			}
		}

		public bool inlineCurvesSelected
		{
			get
			{
				return SelectionManager.IsCurveEditorFocused(this);
			}
			set
			{
				if (!value && SelectionManager.IsCurveEditorFocused(this))
				{
					SelectionManager.SelectInlineCurveEditor(null);
				}
				else
				{
					SelectionManager.SelectInlineCurveEditor(this);
				}
			}
		}

		public Rect mixOutRect
		{
			get
			{
				float mixOutPercentage = this.clip.mixOutPercentage;
				this.m_MixOutRect.Set(base.UnClippedRect.get_min().x + base.UnClippedRect.get_width() * (1f - mixOutPercentage), base.UnClippedRect.get_min().y, base.UnClippedRect.get_width() * mixOutPercentage, base.UnClippedRect.get_height());
				return this.m_MixOutRect;
			}
		}

		public Rect mixInRect
		{
			get
			{
				this.m_MixInRect.Set(base.UnClippedRect.get_xMin(), base.UnClippedRect.get_yMin(), base.UnClippedRect.get_width() * this.clip.mixInPercentage, base.UnClippedRect.get_height());
				return this.m_MixInRect;
			}
		}

		internal TimelineClipGUI.BlendKind blendInKind
		{
			get
			{
				TimelineClipGUI.BlendKind result;
				if (this.mixInRect.get_width() > TimelineClipGUI.k_MinMixWidth && this.overlaps)
				{
					result = TimelineClipGUI.BlendKind.Mix;
				}
				else if (this.mixInRect.get_width() > TimelineClipGUI.k_MinMixWidth)
				{
					result = TimelineClipGUI.BlendKind.Ease;
				}
				else
				{
					result = TimelineClipGUI.BlendKind.None;
				}
				return result;
			}
		}

		internal TimelineClipGUI.BlendKind blendOutKind
		{
			get
			{
				TimelineClipGUI.BlendKind result;
				if (this.mixOutRect.get_width() > TimelineClipGUI.k_MinMixWidth && this.isOverlapped)
				{
					result = TimelineClipGUI.BlendKind.Mix;
				}
				else if (this.mixOutRect.get_width() > TimelineClipGUI.k_MinMixWidth)
				{
					result = TimelineClipGUI.BlendKind.Ease;
				}
				else
				{
					result = TimelineClipGUI.BlendKind.None;
				}
				return result;
			}
		}

		public double start
		{
			get
			{
				return this.clip.start;
			}
			set
			{
				this.clip.start = value;
			}
		}

		public double end
		{
			get
			{
				return this.clip.end;
			}
		}

		public double duration
		{
			get
			{
				return this.clip.duration;
			}
		}

		public bool supportsLooping
		{
			get
			{
				return this.clip.SupportsLooping();
			}
		}

		public int minLoopIndex
		{
			get
			{
				return this.m_MinLoopIndex;
			}
		}

		public int parityID
		{
			get;
			set;
		}

		public TimelineClip clip
		{
			get
			{
				return (TimelineClip)this.m_EditorItem.item;
			}
		}

		public TimelineClipGUI(TimelineClip clip, TimelineTrackGUI parent) : base(parent)
		{
			this.m_EditorItem = EditorItemFactory.GetEditorClip(clip);
			clip.dirtyHash = 0;
			this.supportResize = true;
			if (parent.drawer != null)
			{
				parent.drawer.ConfigureUIClip(this);
			}
			DragClipHandle clipHandleManipulator = (!clip.SupportsClipIn()) ? new SimpleDragClipHandle() : new DragClipHandle();
			this.m_LeftHandle = new TimelineClipHandle(this, TimelineClipHandle.DragDirection.Left, clipHandleManipulator);
			this.m_RightHandle = new TimelineClipHandle(this, TimelineClipHandle.DragDirection.Right, clipHandleManipulator);
			this.m_BlendInHandle = new TimelineBlendHandle(this, TimelineBlendHandle.DragDirection.Left);
			this.m_BlendOutHandle = new TimelineBlendHandle(this, TimelineBlendHandle.DragDirection.Right);
			base.AddChild(this.m_LeftHandle);
			base.AddChild(this.m_RightHandle);
			base.AddChild(this.m_BlendInHandle);
			base.AddChild(this.m_BlendOutHandle);
			TimelineItemGUI.s_ItemToItemGUI[clip] = this;
		}

		private void CreateInlineCurveEditor(TimelineWindow.TimelineState state)
		{
			if (Event.get_current().get_type() == 8)
			{
				if (this.clipCurveEditor == null)
				{
					AnimationClip animationClip = this.clip.animationClip;
					if (animationClip != null && animationClip.get_empty())
					{
						animationClip = null;
					}
					if (animationClip != null && !this.clip.recordable)
					{
						animationClip = null;
					}
					if (this.clip.curves != null || animationClip != null)
					{
						state.AddEndFrameDelegate(delegate(ITimelineState istate, Event currentEvent)
						{
							this.clipCurveEditor = new ClipCurveEditor(new TimelineClipCurveDataSource(this), this.m_ParentTrack.TimelineWindow);
							return true;
						});
					}
				}
			}
		}

		public override bool OnEvent(Event evt, TimelineWindow.TimelineState state, bool isCaptureSession)
		{
			return (base.parentTrackGUI == null || !base.parentTrackGUI.track.locked) && base.OnEvent(evt, state, isCaptureSession);
		}

		public void DrawDragPreview(Rect rect, Color color)
		{
			EditorGUI.DrawRect(rect, color);
			Graphics.ShadowLabel(rect, this.name, this.m_Styles.fontClip, Color.get_white(), Color.get_black());
		}

		private int ComputeDirtyHash()
		{
			return HashUtility.CombineHash(this.clip.clipAssetDuration.GetHashCode(), this.clip.duration.GetHashCode(), this.clip.timeScale.GetHashCode(), this.clip.start.GetHashCode());
		}

		private void DrawClipByDrawer(Rect drawRect, string title, GUIStyle style, TimelineWindow.TimelineState state, float rectXOffset)
		{
			this.m_ClipDrawData.uiClip = this;
			this.m_ClipDrawData.clip = this.clip;
			this.m_ClipDrawData.targetRect = drawRect;
			this.m_ClipDrawData.clipCenterSection = this.m_ClipCenterSection;
			this.m_ClipDrawData.unclippedRect = base.UnClippedRect;
			this.m_ClipDrawData.title = title;
			this.m_ClipDrawData.selected = SelectionManager.Contains(this.clip);
			this.m_ClipDrawData.inlineCurvesSelected = this.inlineCurvesSelected;
			this.m_ClipDrawData.style = style;
			this.m_ClipDrawData.state = state;
			this.m_ClipDrawData.selectedStyle = this.m_Styles.selectedStyle;
			Vector3 vector = state.timeAreaShownRange;
			this.m_ClipDrawData.localVisibleStartTime = this.clip.ToLocalTimeUnbound(Math.Max(this.clip.start, (double)vector.x));
			this.m_ClipDrawData.localVisibleEndTime = this.clip.ToLocalTimeUnbound(Math.Min(this.clip.end, (double)vector.y));
			this.m_ClipDrawData.clippedRect = new Rect(this.m_ClippedRect.get_x() - rectXOffset, 0f, this.m_ClippedRect.get_width(), this.m_ClippedRect.get_height());
			base.parentTrackGUI.drawer.DrawClip(this.m_ClipDrawData);
		}

		public void DrawInto(Rect drawRect, GUIStyle style, TimelineWindow.TimelineState state)
		{
			this.CreateInlineCurveEditor(state);
			GUI.BeginClip(drawRect);
			Rect rect = drawRect;
			rect.set_x(0f);
			rect.set_y(0f);
			string str = "";
			if (SelectionManager.Contains(this.clip) && !object.Equals(1.0, this.clip.timeScale))
			{
				str = " " + this.clip.timeScale.ToString("F2") + "x";
			}
			string title = this.m_Styles.Elipsify(this.name, rect, this.m_Styles.fontClip) + str;
			this.DrawClipByDrawer(rect, title, style, state, drawRect.get_x());
			GUI.EndClip();
			if (SelectionManager.Contains(this.clip) && this.supportResize)
			{
				Rect bounds = this.bounds;
				bounds.set_xMin(bounds.get_xMin() + this.m_LeftHandle.bounds.get_width());
				bounds.set_xMax(bounds.get_xMax() - this.m_RightHandle.bounds.get_width());
				EditorGUIUtility.AddCursorRect(bounds, 8);
			}
			if (this.supportResize)
			{
				if (drawRect.get_width() > 3f * TimelineClipGUI.k_HandleWidth)
				{
					this.m_LeftHandle.Draw(drawRect);
				}
				this.m_RightHandle.Draw(drawRect);
				state.quadTree.Insert(this.m_LeftHandle);
				state.quadTree.Insert(this.m_RightHandle);
			}
		}

		private void CalculateClipRectangle(TrackAsset parentTrack, Rect trackRect, TimelineWindow.TimelineState state, int projectedClipHash)
		{
			if (this.m_ProjectedClipHash == projectedClipHash)
			{
				if (Event.get_current().get_type() == 7 && !parentTrack.locked)
				{
					state.quadTree.Insert(this);
				}
			}
			else
			{
				this.m_ProjectedClipHash = projectedClipHash;
				Rect rect = this.RectToTimeline(trackRect, state);
				if (rect.get_width() < DirectorStyles.Instance.eventWhite.get_fixedWidth() + 1f)
				{
					rect.set_width(DirectorStyles.Instance.eventWhite.get_fixedWidth() + 1f);
					rect.set_x(rect.get_x() - DirectorStyles.Instance.eventWhite.get_fixedWidth() / 2f);
				}
				base.rect = rect;
				this.m_UnclippedRect = rect;
				if (Event.get_current().get_type() == 7 && !parentTrack.locked)
				{
					state.quadTree.Insert(this);
				}
				rect.set_xMin(Mathf.Max(rect.get_xMin(), trackRect.get_xMin()));
				rect.set_xMax(Mathf.Min(rect.get_xMax(), trackRect.get_xMax()));
				base.clippedRect = rect;
				if (rect.get_width() <= 0f)
				{
					rect.set_width(0f);
				}
				else if (base.clippedRect.get_width() < 2f)
				{
					this.m_ClippedRect.set_width(5f);
				}
			}
		}

		private void CalculateBlendRect()
		{
			this.m_ClipCenterSection = base.rect;
			this.m_ClipCenterSection.set_x(0f);
			this.m_ClipCenterSection.set_y(0f);
			this.m_ClipCenterSection.set_xMin(base.UnClippedRect.get_width() * this.clip.mixInPercentage);
			this.m_ClipCenterSection.set_width(base.rect.get_width());
			this.m_ClipCenterSection.set_xMax(this.m_ClipCenterSection.get_xMax() - this.mixOutRect.get_width());
			this.m_ClipCenterSection.set_xMax(this.m_ClipCenterSection.get_xMax() - base.UnClippedRect.get_width() * this.clip.mixInPercentage);
		}

		public virtual void Draw(Rect trackRect, TimelineWindow.TimelineState state, TrackDrawer drawer)
		{
			if (SelectionManager.Contains(this.clip))
			{
				this.clip.dirtyHash = 0;
			}
			int num = this.ComputeDirtyHash();
			int h = HashUtility.CombineHash(state.timeAreaTranslation.GetHashCode(), state.timeAreaScale.GetHashCode(), trackRect.GetHashCode());
			this.CalculateClipRectangle(base.parentTrackGUI.track, trackRect, state, num.CombineHash(h));
			this.CalculateBlendRect();
			this.CalculateLoopRects(trackRect, state, num);
			this.clip.dirtyHash = num;
			if (drawer.canDrawExtrapolationIcon)
			{
				this.DrawExtrapolation(trackRect, base.UnClippedRect);
			}
			this.DrawInto(base.rect, this.m_Styles.displayBackground, state);
		}

		public void DrawBlendingCurves(TimelineWindow.TimelineState state)
		{
			if (Event.get_current().get_type() == 7)
			{
				Color color = (!SelectionManager.Contains(this.clip)) ? Color.get_white() : TrackDrawer.GetHighlightColor(Color.get_white());
				Color colorTrackBackground = DirectorStyles.Instance.customSkin.colorTrackBackground;
				Color color2 = (!SelectionManager.Contains(this.clip)) ? DirectorStyles.Instance.customSkin.colorTrackBackground : Color.get_white();
				if (this.blendInKind == TimelineClipGUI.BlendKind.Ease)
				{
					ClipRenderer.RenderTexture(this.mixInRect, DirectorStyles.Instance.timelineClip.get_normal().get_background(), DirectorStyles.Instance.blendingIn.get_normal().get_background(), color, false);
					EditorGUI.DrawRect(new Rect(this.mixInRect.get_xMax() - 2f, this.mixInRect.get_yMin(), 2f, this.mixInRect.get_height()), colorTrackBackground);
					Graphics.DrawAAPolyLine(4f, new Vector3[]
					{
						new Vector3(this.mixInRect.get_xMin() + 1f, this.mixInRect.get_yMax() - 1f, 0f),
						new Vector3(this.mixInRect.get_xMax(), this.mixInRect.get_yMin() - 1.5f, 0f)
					}, color2);
				}
				if (this.blendOutKind == TimelineClipGUI.BlendKind.Ease || this.blendOutKind == TimelineClipGUI.BlendKind.Mix)
				{
					ClipRenderer.RenderTexture(this.mixOutRect, DirectorStyles.Instance.timelineClip.get_normal().get_background(), DirectorStyles.Instance.blendingOut.get_normal().get_background(), color, false);
					EditorGUI.DrawRect(new Rect(this.mixOutRect.get_xMin(), this.mixOutRect.get_yMin(), 2f, this.mixOutRect.get_height()), colorTrackBackground);
					Graphics.DrawLineAA(4f, new Vector3(this.mixOutRect.get_xMin() + 1.5f, this.mixOutRect.get_yMin() + 1.5f, 0f), new Vector3(this.mixOutRect.get_xMax(), this.mixOutRect.get_yMax() - 1f, 0f), color2);
				}
				if (this.blendInKind == TimelineClipGUI.BlendKind.Mix)
				{
					ClipRenderer.RenderTexture(this.mixInRect, DirectorStyles.Instance.timelineClip.get_normal().get_background(), DirectorStyles.Instance.blendingOut.get_normal().get_background(), color, false);
					EditorGUI.DrawRect(new Rect(this.mixInRect.get_xMax(), this.mixInRect.get_yMin(), 2f, this.mixOutRect.get_height()), colorTrackBackground);
					Graphics.DrawAAPolyLine(4f, new Vector3[]
					{
						new Vector3(this.mixInRect.get_xMin(), this.mixInRect.get_yMin(), 0f),
						new Vector3(this.mixInRect.get_xMax(), this.mixInRect.get_yMax() - 1f, 0f)
					}, color2);
				}
			}
		}

		private GUIStyle GetExtrapolationIcon(TimelineClip.ClipExtrapolation mode)
		{
			GUIStyle gUIStyle = null;
			GUIStyle result;
			switch (mode)
			{
			case TimelineClip.ClipExtrapolation.None:
				result = null;
				return result;
			case TimelineClip.ClipExtrapolation.Hold:
				gUIStyle = this.m_Styles.extrapolationHold;
				break;
			case TimelineClip.ClipExtrapolation.Loop:
				gUIStyle = this.m_Styles.extrapolationLoop;
				break;
			case TimelineClip.ClipExtrapolation.PingPong:
				gUIStyle = this.m_Styles.extrapolationPingPong;
				break;
			case TimelineClip.ClipExtrapolation.Continue:
				gUIStyle = this.m_Styles.extrapolationContinue;
				break;
			}
			result = gUIStyle;
			return result;
		}

		private Rect GetPreExtrapolationBounds(Rect trackRect, Rect clipRect, GUIStyle icon)
		{
			float num = clipRect.get_xMin() - (icon.get_fixedWidth() + 10f);
			float num2 = trackRect.get_yMin() + (trackRect.get_height() - icon.get_fixedHeight()) / 2f;
			Rect result;
			if (this.previousClip != null)
			{
				float num3 = Mathf.Abs(base.UnClippedRect.get_xMin() - this.previousClip.UnClippedRect.get_xMax());
				if (num3 < icon.get_fixedWidth())
				{
					result = new Rect(0f, 0f, 0f, 0f);
					return result;
				}
				if (num3 < icon.get_fixedWidth() + 20f)
				{
					float num4 = (num3 - icon.get_fixedWidth()) / 2f;
					num = clipRect.get_xMin() - (icon.get_fixedWidth() + num4);
				}
			}
			result = new Rect(num, num2, icon.get_fixedWidth(), icon.get_fixedHeight());
			return result;
		}

		private Rect GetPostExtrapolationBounds(Rect trackRect, Rect clipRect, GUIStyle icon)
		{
			float num = clipRect.get_xMax() + 10f;
			float num2 = trackRect.get_yMin() + (trackRect.get_height() - icon.get_fixedHeight()) / 2f;
			Rect result;
			if (this.nextClip != null)
			{
				float num3 = Mathf.Abs(this.nextClip.UnClippedRect.get_xMin() - base.UnClippedRect.get_xMax());
				if (num3 < icon.get_fixedWidth())
				{
					result = new Rect(0f, 0f, 0f, 0f);
					return result;
				}
				if (num3 < icon.get_fixedWidth() + 20f)
				{
					float num4 = (num3 - icon.get_fixedWidth()) / 2f;
					num = clipRect.get_xMax() + num4;
				}
			}
			result = new Rect(num, num2, icon.get_fixedWidth(), icon.get_fixedHeight());
			return result;
		}

		private static void DrawExtrapolationIcon(Rect rect, GUIStyle icon)
		{
			GUI.Label(rect, GUIContent.none, icon);
		}

		private void DrawExtrapolation(Rect trackRect, Rect clipRect)
		{
			if (this.clip.hasPreExtrapolation)
			{
				GUIStyle extrapolationIcon = this.GetExtrapolationIcon(this.clip.preExtrapolationMode);
				if (extrapolationIcon != null)
				{
					Rect preExtrapolationBounds = this.GetPreExtrapolationBounds(trackRect, clipRect, extrapolationIcon);
					if (preExtrapolationBounds.get_width() > 1f && preExtrapolationBounds.get_height() > 1f)
					{
						TimelineClipGUI.DrawExtrapolationIcon(preExtrapolationBounds, extrapolationIcon);
					}
				}
			}
			if (this.clip.hasPostExtrapolation)
			{
				GUIStyle extrapolationIcon2 = this.GetExtrapolationIcon(this.clip.postExtrapolationMode);
				if (extrapolationIcon2 != null)
				{
					Rect postExtrapolationBounds = this.GetPostExtrapolationBounds(trackRect, clipRect, extrapolationIcon2);
					if (postExtrapolationBounds.get_width() > 1f && postExtrapolationBounds.get_height() > 1f)
					{
						TimelineClipGUI.DrawExtrapolationIcon(postExtrapolationBounds, extrapolationIcon2);
					}
				}
			}
		}

		private static Rect ProjectRectOnTimeline(Rect rect, Rect trackRect, TimelineWindow.TimelineState state)
		{
			Rect result = rect;
			result.set_x(result.get_x() * state.timeAreaScale.x);
			result.set_width(result.get_width() * state.timeAreaScale.x);
			result.set_x(result.get_x() + (state.timeAreaTranslation.x + trackRect.get_xMin()));
			result.set_y(trackRect.get_y() + 2f);
			result.set_height(trackRect.get_height() - 4f);
			return result;
		}

		public void CalculateLoopRects(Rect trackRect, TimelineWindow.TimelineState state, int currentClipHash)
		{
			if (this.clip.duration >= TimelineWindow.TimelineState.kTimeEpsilon)
			{
				if (this.clip.dirtyHash != currentClipHash)
				{
					float num = 0f;
					this.m_LoopRects.Clear();
					double[] loopTimes = TimelineHelpers.GetLoopTimes(this.clip);
					double loopDuration = TimelineHelpers.GetLoopDuration(this.clip);
					this.m_MinLoopIndex = 0;
					if (!this.supportsLooping)
					{
						if (loopTimes.Length > 1)
						{
							double num2 = loopTimes[1];
							float num3 = (float)(this.clip.duration - num2);
							this.m_LoopRects.Add(TimelineClipGUI.ProjectRectOnTimeline(new Rect((float)(num2 + this.clip.start), 0f, num3, 0f), trackRect, state));
						}
					}
					else
					{
						int num4 = Array.BinarySearch<double>(loopTimes, (double)state.PixelToTime(trackRect.get_xMin()));
						int num5 = Array.BinarySearch<double>(loopTimes, (double)state.PixelToTime(trackRect.get_xMax()));
						num4 = ((num4 < 0) ? Math.Max(1, ~num4 - 2) : num4);
						num5 = ((num5 < 0) ? Math.Max(1, ~num5) : num5);
						this.m_MinLoopIndex = num4;
						int num6 = num5 - num4;
						if ((float)num6 * 4f < trackRect.get_width())
						{
							for (int i = num4; i < num5; i++)
							{
								double num7 = loopTimes[i];
								float num8 = Mathf.Min((float)(this.clip.duration - num7), (float)loopDuration);
								Rect item = TimelineClipGUI.ProjectRectOnTimeline(new Rect((float)(num7 + this.clip.start), 0f, num8, 0f), trackRect, state);
								if (item.get_xMin() > trackRect.get_xMax())
								{
									break;
								}
								if (item.get_xMax() >= trackRect.get_xMin())
								{
									this.m_LoopRects.Add(item);
									num += item.get_width();
								}
								else
								{
									this.m_MinLoopIndex++;
								}
							}
							if ((float)this.m_LoopRects.Count * 4f >= trackRect.get_width())
							{
								this.m_LoopRects.Clear();
							}
							if (num < 2f)
							{
								this.m_LoopRects.Clear();
							}
						}
					}
				}
			}
		}

		public Rect RectToTimeline(Rect trackRect, TimelineWindow.TimelineState state)
		{
			Rect result = new Rect((float)this.clip.start * state.timeAreaScale.x, 0f, (float)this.clip.duration * state.timeAreaScale.x, 0f);
			result.set_xMin(result.get_xMin() + (state.timeAreaTranslation.x + trackRect.get_xMin()));
			result.set_xMax(result.get_xMax() + (state.timeAreaTranslation.x + trackRect.get_xMin()));
			result.set_y(trackRect.get_y() + 2f);
			result.set_height(trackRect.get_height() - 4f);
			result.set_y(trackRect.get_y());
			result.set_height(trackRect.get_height());
			return result;
		}

		public IEnumerable<Edge> SnappableEdgesFor(IAttractable attractable)
		{
			List<Edge> list = new List<Edge>();
			if (attractable != this)
			{
				bool flag = !base.parentTrackGUI.get_hasChildren() || base.parentTrackGUI.isExpanded;
				if (flag)
				{
					list.Add(new Edge(this.clip.start));
					list.Add(new Edge(this.clip.end));
				}
			}
			return list;
		}
	}
}
