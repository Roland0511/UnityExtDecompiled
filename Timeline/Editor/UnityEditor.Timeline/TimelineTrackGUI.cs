using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class TimelineTrackGUI : TimelineGroupGUI, IClipCurveEditorOwner
	{
		private List<TimelineClipGUI> m_ClipGUICache = new List<TimelineClipGUI>();

		private List<TimelineMarkerGUI> m_MarkerGuiCache = new List<TimelineMarkerGUI>();

		private static GUIContent s_ArmForRecordContentOn;

		private static GUIContent s_ArmForRecordContentOff;

		private bool m_HadProblems;

		private bool m_InitHadProblems;

		private int m_TrackHash = -1;

		private int m_BlendHash = -1;

		private readonly PlayableBinding[] m_Bindings;

		private bool? m_TrackAllowsRecording = null;

		private readonly InfiniteTrackDrawer m_InfiniteTrackDrawer = null;

		private int m_ClipParityID = 0;

		private static readonly GUIContent s_LockMuteOverlay = new GUIContent();

		private const int k_SelectedClipZOrder = 1000;

		private const float k_ButtonSize = 16f;

		private const float k_ButtonPadding = 3f;

		private const float k_LockTextPadding = 40f;

		public bool resortClips
		{
			get;
			set;
		}

		public bool resortEvents
		{
			get;
			set;
		}

		public override Rect boundingRect
		{
			get
			{
				Rect boundingRect = base.boundingRect;
				boundingRect.set_height(boundingRect.get_height() + this.InlineAnimationCurveHeight());
				return boundingRect;
			}
		}

		public override bool expandable
		{
			get
			{
				return this.get_hasChildren();
			}
		}

		public List<TimelineClipGUI> clips
		{
			get
			{
				return this.m_ClipGUICache;
			}
		}

		public List<TimelineMarkerGUI> markers
		{
			get
			{
				return this.m_MarkerGuiCache;
			}
		}

		public List<TimelineItemGUI> items
		{
			get
			{
				return this.m_ClipGUICache.Cast<TimelineItemGUI>().Concat(this.m_MarkerGuiCache.Cast<TimelineItemGUI>()).ToList<TimelineItemGUI>();
			}
		}

		public InlineCurveEditor inlineCurveEditor
		{
			get;
			private set;
		}

		public ClipCurveEditor clipCurveEditor
		{
			get;
			private set;
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

		public bool supportsLooping
		{
			get
			{
				return false;
			}
		}

		public override Rect indentedHeaderBounds
		{
			get
			{
				float num = (float)this.get_depth() * DirectorStyles.Instance.indentWidth + 2f;
				Rect headerBounds = this.headerBounds;
				headerBounds.set_width(this.headerBounds.get_width() - num);
				headerBounds.set_x(headerBounds.get_x() + num);
				return headerBounds;
			}
		}

		private bool trackAllowsRecording
		{
			get
			{
				if (!this.m_TrackAllowsRecording.HasValue)
				{
					this.m_TrackAllowsRecording = new bool?(this.DoesTrackAllowRecording());
				}
				return this.m_TrackAllowsRecording.Value;
			}
		}

		private bool showSceneReference
		{
			get
			{
				bool result;
				if (base.track == null || base.IsSubTrack() || this.m_Bindings.Length == 0)
				{
					result = false;
				}
				else
				{
					PlayableBinding playableBinding = this.m_Bindings[0];
					result = (playableBinding.get_sourceObject() != null && (playableBinding.get_streamType() == null || (playableBinding.get_streamType() == 3 && playableBinding.get_sourceBindingType() != null && typeof(Object).IsAssignableFrom(playableBinding.get_sourceBindingType())) || playableBinding.get_streamType() == 1));
				}
				return result;
			}
		}

		public GUIContent headerIcon
		{
			get
			{
				return base.drawer.GetIcon();
			}
		}

		public TimelineTrackGUI(TreeViewController tv, TimelineTreeViewGUI w, int id, int depth, TreeViewItem parent, string displayName, TrackAsset sequenceActor) : base(tv, w, id, depth, parent, displayName, sequenceActor, false)
		{
			AnimationTrack animationTrack = sequenceActor as AnimationTrack;
			if (animationTrack != null)
			{
				this.m_InfiniteTrackDrawer = new InfiniteTrackDrawer(new AnimationTrackKeyDataSource(animationTrack));
				this.UpdateInfiniteClipEditor(animationTrack, w.TimelineWindow);
				if (animationTrack.ShouldShowInfiniteClipEditor())
				{
					this.clipCurveEditor = new ClipCurveEditor(new InfiniteClipCurveDataSource(this), w.TimelineWindow);
				}
			}
			this.m_HadProblems = false;
			this.m_InitHadProblems = false;
			this.m_Bindings = base.track.get_outputs().ToArray<PlayableBinding>();
			base.AddManipulator(new TrackVerticalResize());
		}

		public override float GetVerticalSpacingBetweenTracks()
		{
			float result;
			if (base.track != null && base.track.isSubTrack)
			{
				result = 1f;
			}
			else
			{
				result = base.GetVerticalSpacingBetweenTracks();
			}
			return result;
		}

		private void UpdateInfiniteClipEditor(AnimationTrack animationTrack, TimelineWindow window)
		{
			if (animationTrack != null && this.clipCurveEditor == null && animationTrack.ShouldShowInfiniteClipEditor())
			{
				this.clipCurveEditor = new ClipCurveEditor(new InfiniteClipCurveDataSource(this), window);
			}
		}

		protected override void OnLockedChanged(bool value)
		{
			TimelineWindow.TimelineState state = TimelineWindow.instance.state;
			if (!base.locked)
			{
				foreach (TimelineItemGUI current in this.items)
				{
					SelectionManager.Remove(current.item);
				}
			}
			if (base.locked)
			{
				state.UnarmForRecord(base.track);
			}
		}

		public override bool CanBeSelected(Vector2 mousePosition)
		{
			return this.headerBounds.Contains(mousePosition);
		}

		public void RangeSelectItems(TimelineItemGUI lastItemToSelect, TimelineWindow.TimelineState state)
		{
			List<TimelineItemGUI> source = SelectionManager.SelectedItemGUI().ToList<TimelineItemGUI>();
			TimelineItemGUI timelineItemGUI = source.FirstOrDefault<TimelineItemGUI>();
			if (timelineItemGUI == null)
			{
				SelectionManager.Add(lastItemToSelect.item);
			}
			else
			{
				List<TimelineTrackBaseGUI> allTracks = TimelineWindow.instance.allTracks;
				List<TimelineItemGUI> list = allTracks.OfType<TimelineTrackGUI>().SelectMany((TimelineTrackGUI ttGui) => from x in ttGui.items
				orderby x.item.start
				select x).ToList<TimelineItemGUI>();
				int num = list.IndexOf(timelineItemGUI);
				TimelineItemGUI item = source.LastOrDefault<TimelineItemGUI>();
				int num2 = list.IndexOf(item);
				int num3 = list.IndexOf(lastItemToSelect);
				if (num < num3)
				{
					for (int i = num; i <= num3; i++)
					{
						SelectionManager.Add(list[i].item);
					}
				}
				else
				{
					for (int j = num; j >= num3; j--)
					{
						SelectionManager.Add(list[j].item);
					}
				}
				if (Math.Min(num, num2) < num3 && num3 < Math.Max(num, num2))
				{
					for (int k = Math.Min(num2, num3); k <= Math.Max(num2, num3); k++)
					{
						SelectionManager.Remove(list[k].item);
					}
				}
				SelectionManager.Add(lastItemToSelect.item);
			}
		}

		private bool IsMuted(TimelineWindow.TimelineState state)
		{
			return !(base.track == null) && base.track.mediaType != TimelineAsset.MediaType.Group && (state.soloTracks.Count > 0 || base.track.muted);
		}

		public override void Draw(Rect headerRect, Rect trackRect, TimelineWindow.TimelineState state, float identWidth)
		{
			if (base.track.GetShowInlineCurves() && this.inlineCurveEditor == null)
			{
				this.inlineCurveEditor = new InlineCurveEditor(this);
			}
			this.UpdateInfiniteClipEditor(base.track as AnimationTrack, state.GetWindow());
			Rect trackRect2 = trackRect;
			float num = this.InlineAnimationCurveHeight();
			trackRect.set_height(trackRect.get_height() - num);
			if (Event.get_current().get_type() == 7)
			{
				this.m_TrackRect = trackRect;
				state.quadTree.Insert(this);
				int num2 = this.BlendHash();
				if (this.m_BlendHash != num2)
				{
					this.UpdateClipOverlaps();
					this.m_BlendHash = num2;
				}
				base.isDropTarget = false;
			}
			if (TimelineTrackGUI.s_ArmForRecordContentOn == null)
			{
				TimelineTrackGUI.s_ArmForRecordContentOn = new GUIContent(TimelineWindow.styles.autoKey.get_active().get_background());
			}
			if (TimelineTrackGUI.s_ArmForRecordContentOff == null)
			{
				TimelineTrackGUI.s_ArmForRecordContentOff = new GUIContent(TimelineWindow.styles.autoKey.get_normal().get_background());
			}
			base.track.SetCollapsed(!base.isExpanded);
			headerRect.set_width(headerRect.get_width() - 2f);
			if (this.m_TrackHash != base.track.Hash())
			{
				this.RebuildGUICache(state);
			}
			bool flag = false;
			Vector2 timeAreaShownRange = state.timeAreaShownRange;
			if (base.drawer != null)
			{
				flag = base.drawer.DrawTrack(trackRect, base.track, timeAreaShownRange, state);
			}
			if (!flag)
			{
				using (new GUIViewportScope(trackRect))
				{
					this.DrawBackground(trackRect, state);
					if (this.resortClips)
					{
						int num3 = 0;
						this.SortClipsByStartTime();
						this.ResetClipParityID();
						foreach (TimelineClipGUI current in this.m_ClipGUICache)
						{
							current.parityID = this.GetNextClipParityID();
							current.zOrder = num3++;
							if (SelectionManager.Contains(current.clip))
							{
								current.zOrder += 1000;
							}
						}
						IEnumerable<TimelineClipGUI> selectedClips = SelectionManager.SelectedClipGUI();
						this.m_ClipGUICache = (from x in this.m_ClipGUICache
						orderby selectedClips.Contains(x), x.clip.start
						select x).ToList<TimelineClipGUI>();
						this.resortClips = false;
					}
					if (this.resortEvents)
					{
						int num4 = 0;
						this.SortEventsByStartTime();
						foreach (TimelineMarkerGUI current2 in this.m_MarkerGuiCache)
						{
							current2.zOrder = num4++;
							if (SelectionManager.Contains(current2.timelineMarker))
							{
								current2.zOrder += 1000;
							}
						}
						IEnumerable<TimelineMarkerGUI> selectedMarkers = SelectionManager.SelectedMarkerGUI();
						this.m_MarkerGuiCache = (from x in this.m_MarkerGuiCache
						orderby selectedMarkers.Contains(x), x.timelineMarker.time
						select x).ToList<TimelineMarkerGUI>();
						this.resortEvents = false;
					}
					this.DrawClips(trackRect, state);
					this.DrawEvents(trackRect, state);
					this.DrawClipConnectors();
				}
				if (this.m_InfiniteTrackDrawer != null)
				{
					this.m_InfiniteTrackDrawer.DrawTrack(trackRect, base.track, timeAreaShownRange, state);
				}
			}
			this.DrawTrackHeader(headerRect, state, identWidth, num);
			this.DrawInlineCurves(headerRect, trackRect2, state, identWidth, num);
			this.DrawMuteState(trackRect, state);
			this.DrawLockState(trackRect, state);
		}

		public override bool OnEvent(Event evt, TimelineWindow.TimelineState state, bool isCaptureSession)
		{
			bool result;
			if (base.track.GetShowInlineCurves() && this.inlineCurveEditor != null)
			{
				if (this.inlineCurveEditor.OnEvent(evt, state, isCaptureSession))
				{
					result = true;
					return result;
				}
			}
			result = base.OnEvent(evt, state, isCaptureSession);
			return result;
		}

		private void DrawInlineCurves(Rect headerRect, Rect trackRect, TimelineWindow.TimelineState state, float identWidth, float inlineCurveHeight)
		{
			if (base.track.GetShowInlineCurves() && this.inlineCurveEditor != null && inlineCurveHeight != 0f)
			{
				float num = trackRect.get_height() - inlineCurveHeight;
				trackRect.set_y(trackRect.get_y() + num);
				trackRect.set_height(inlineCurveHeight);
				headerRect.set_x(headerRect.get_x() + DirectorStyles.kBaseIndent);
				headerRect.set_width(headerRect.get_width() - DirectorStyles.kBaseIndent);
				headerRect.set_y(headerRect.get_y() + num);
				headerRect.set_height(inlineCurveHeight);
				if (this.inlineCurveEditor != null)
				{
					this.inlineCurveEditor.Draw(headerRect, trackRect, state, identWidth);
				}
			}
		}

		private void DrawLockTrackBG(Rect trackRect)
		{
			int num = (int)Mathf.Ceil(trackRect.get_width() / (float)this.m_Styles.lockedBG.get_normal().get_background().get_width());
			Rect rect = trackRect;
			rect.set_width((float)this.m_Styles.lockedBG.get_normal().get_background().get_width());
			for (int num2 = 0; num2 != num; num2++)
			{
				GUI.Box(rect, GUIContent.none, this.m_Styles.lockedBG);
				rect.set_x(rect.get_x() + ((float)this.m_Styles.lockedBG.get_normal().get_background().get_width() - 1f));
			}
		}

		private void DrawTrackStateBox(Rect trackRect)
		{
			if (base.track.locked)
			{
				TimelineTrackGUI.s_LockMuteOverlay.set_text("Locked");
				if (base.track.muted)
				{
					GUIContent expr_36 = TimelineTrackGUI.s_LockMuteOverlay;
					expr_36.set_text(expr_36.get_text() + " / Muted");
				}
			}
			else if (base.track.muted)
			{
				TimelineTrackGUI.s_LockMuteOverlay.set_text("Muted");
				if (base.track.locked)
				{
					TimelineTrackGUI.s_LockMuteOverlay.set_text("Locked / Muted");
				}
			}
			Rect rect = trackRect;
			rect.set_width(this.m_Styles.fontClip.CalcSize(TimelineTrackGUI.s_LockMuteOverlay).x + 40f);
			rect.set_x(rect.get_x() + (trackRect.get_width() - rect.get_width()) / 2f);
			rect.set_height(rect.get_height() - 4f);
			rect.set_y(rect.get_y() + 2f);
			using (new GUIColorOverride(this.m_Styles.customSkin.colorLockTextBG))
			{
				GUI.Box(rect, GUIContent.none, this.m_Styles.displayBackground);
			}
			Graphics.ShadowLabel(rect, TimelineTrackGUI.s_LockMuteOverlay, this.m_Styles.fontClip, Color.get_white(), Color.get_black());
		}

		public void DrawLockState(Rect trackRect, TimelineWindow.TimelineState state)
		{
			if (base.track.locked)
			{
				this.DrawLockTrackBG(trackRect);
				this.DrawTrackStateBox(trackRect);
			}
		}

		private void DrawErrorIcon(Rect position, TimelineWindow.TimelineState state)
		{
			Rect rect = position;
			rect.set_x(position.get_xMax() + 3f);
			rect.set_width(state.bindingAreaWidth);
			EditorGUI.LabelField(position, this.m_ProblemIcon);
		}

		private TrackBindingValidationResult GetTrackBindingValidationResult(TimelineWindow.TimelineState state)
		{
			TrackAsset track = (!base.IsSubTrack()) ? base.track : base.ParentTrack();
			return state.ValidateBindingForTrack(track);
		}

		protected override bool DetectProblems(TimelineWindow.TimelineState state)
		{
			return !this.GetTrackBindingValidationResult(state).IsValid() && state != null && state.currentDirector != null;
		}

		protected void DrawBackground(Rect trackRect, TimelineWindow.TimelineState state)
		{
			bool flag = this.IsRecording(state) && (this.m_InfiniteTrackDrawer == null || this.m_InfiniteTrackDrawer.CanDraw(base.track, state));
			if (flag)
			{
				this.DrawRecordingTrackBackground(trackRect);
			}
			else
			{
				Color color = (!SelectionManager.Contains(base.track)) ? DirectorStyles.Instance.customSkin.colorTrackBackground : DirectorStyles.Instance.customSkin.colorTrackBackgroundSelected;
				EditorGUI.DrawRect(trackRect, color);
			}
		}

		public float InlineAnimationCurveHeight()
		{
			float result;
			if (!base.track.GetShowInlineCurves())
			{
				result = 0f;
			}
			else if (!TimelineUtility.TrackHasAnimationCurves(base.track))
			{
				result = 0f;
			}
			else
			{
				result = TimelineWindowViewPrefs.GetInlineCurveHeight(base.track);
			}
			return result;
		}

		public override float GetHeight(TimelineWindow.TimelineState state)
		{
			float num = base.drawer.GetHeight(base.track);
			if (num < 0f)
			{
				num = state.trackHeight;
			}
			return num * state.trackScale + this.InlineAnimationCurveHeight();
		}

		private float GetExpandedHeight(TimelineWindow.TimelineState state)
		{
			float num = this.GetHeight(state);
			if (base.isExpanded && this.get_children() != null)
			{
				foreach (TreeViewItem current in this.get_children())
				{
					TimelineTrackGUI timelineTrackGUI = current as TimelineTrackGUI;
					if (timelineTrackGUI != null)
					{
						num += timelineTrackGUI.GetVerticalSpacingBetweenTracks();
						num += timelineTrackGUI.GetHeight(state);
					}
				}
			}
			return num;
		}

		private static bool CanDrawIcon(GUIContent icon)
		{
			return icon != null && icon != GUIContent.none && icon.get_image() != null;
		}

		private static string GetTrackDisplayName(TrackAsset track, TimelineWindow.TimelineState state)
		{
			string result;
			if (track == null)
			{
				result = "";
			}
			else
			{
				string name = track.get_name();
				if (track.get_name().StartsWith(track.GetType().Name))
				{
					if (state.currentDirector != null)
					{
						GameObject sceneGameObject = TimelineUtility.GetSceneGameObject(state.currentDirector, track);
						if (sceneGameObject != null)
						{
							name = sceneGameObject.get_name();
						}
					}
				}
				result = name;
			}
			return result;
		}

		private void DrawTrackHeader(Rect headerRect, TimelineWindow.TimelineState state, float indentWidth, float inlineCurveHeight)
		{
			using (new GUIViewportScope(headerRect))
			{
				headerRect.set_x(headerRect.get_x() + indentWidth);
				headerRect.set_width(headerRect.get_width() - indentWidth);
				if (Event.get_current().get_type() == 7)
				{
					bool hasProblems = this.DetectProblems(state);
					this.RefreshStateIfBindingProblemIsFound(state, hasProblems);
					this.UpdateBindingProblemValues(hasProblems);
				}
				Rect rect = headerRect;
				rect.set_height(rect.get_height() - inlineCurveHeight);
				this.DrawHeaderBackground(headerRect);
				rect.set_x(rect.get_x() + this.m_Styles.trackSwatchStyle.get_fixedWidth());
				Rect rect2 = new Rect(headerRect.get_xMax() - 16f - 3f, rect.get_y() + (rect.get_height() - 16f) / 2f, 16f, 16f);
				rect.set_x(rect.get_x() + this.DrawTrackIconKind(rect, state));
				this.DrawTrackBinding(rect, headerRect, state);
				if (base.track.mediaType == TimelineAsset.MediaType.Group)
				{
					return;
				}
				rect2.set_x(rect2.get_x() - this.DrawTrackDropDownMenu(rect2, state));
				rect2.set_x(rect2.get_x() - this.DrawInlineCurveButton(rect2, state));
				rect2.set_x(rect2.get_x() - this.DrawMuteButton(rect2, state));
				rect2.set_x(rect2.get_x() - this.DrawLockButton(rect2));
				rect2.set_x(rect2.get_x() - this.DrawRecordButton(rect2, state));
				rect2.set_x(rect2.get_x() - this.DrawCustomTrackButton(rect2, state));
			}
			this.DrawTrackColorKind(headerRect, state);
		}

		private void RefreshStateIfBindingProblemIsFound(TimelineWindow.TimelineState state, bool hasProblems)
		{
			if (this.m_InitHadProblems && this.m_HadProblems != hasProblems)
			{
				TrackBindingValidationResult trackBindingValidationResult = this.GetTrackBindingValidationResult(state);
				bool flag = !trackBindingValidationResult.IsValid() && trackBindingValidationResult.bindingState != TimelineTrackBindingState.BoundGameObjectIsDisabled && trackBindingValidationResult.bindingState != TimelineTrackBindingState.RequiredComponentOnBoundGameObjectIsDisabled;
				if (flag)
				{
					state.Refresh();
				}
			}
		}

		private void UpdateBindingProblemValues(bool hasProblems)
		{
			this.m_HadProblems = hasProblems;
			this.m_InitHadProblems = true;
		}

		private void DrawHeaderBackground(Rect headerRect)
		{
			Color color = (!SelectionManager.Contains(base.track)) ? DirectorStyles.Instance.customSkin.colorTrackHeaderBackground : DirectorStyles.Instance.customSkin.colorSelection;
			Rect rect = headerRect;
			rect.set_x(rect.get_x() + this.m_Styles.trackSwatchStyle.get_fixedWidth());
			rect.set_width(rect.get_width() - this.m_Styles.trackSwatchStyle.get_fixedWidth());
			EditorGUI.DrawRect(rect, color);
		}

		private float DrawTrackColorKind(Rect rect, TimelineWindow.TimelineState state)
		{
			float result;
			if (base.track != null && base.track.isSubTrack)
			{
				result = 0f;
			}
			else
			{
				using (new GUIColorOverride(base.drawer.trackColor))
				{
					rect.set_height(this.GetExpandedHeight(state));
					rect.set_width(this.m_Styles.trackSwatchStyle.get_fixedWidth());
					GUI.Box(rect, GUIContent.none, this.m_Styles.trackSwatchStyle);
				}
				result = this.m_Styles.trackSwatchStyle.get_fixedWidth();
			}
			return result;
		}

		private float DrawTrackIconKind(Rect rect, TimelineWindow.TimelineState state)
		{
			float result;
			if (base.track != null && base.track.isSubTrack)
			{
				result = 0f;
			}
			else
			{
				rect.set_yMin(rect.get_yMin() + (rect.get_height() - 16f) / 2f);
				rect.set_width(16f);
				rect.set_height(16f);
				if (this.m_HadProblems)
				{
					this.GenerateIconForBindingValidationResult(this.m_Styles, this.GetTrackBindingValidationResult(state));
					if (TimelineTrackGUI.CanDrawIcon(this.m_ProblemIcon))
					{
						this.DrawErrorIcon(rect, state);
					}
				}
				else if (TimelineTrackGUI.CanDrawIcon(this.headerIcon))
				{
					GUI.Box(rect, this.headerIcon, GUIStyle.get_none());
				}
				result = rect.get_width();
			}
			return result;
		}

		private void DrawMuteState(Rect trackRect, TimelineWindow.TimelineState state)
		{
			if (this.IsMuted(state))
			{
				Rect rect = trackRect;
				rect.set_x(rect.get_x() + this.m_Styles.trackSwatchStyle.get_fixedWidth());
				rect.set_width(rect.get_width() - this.m_Styles.trackSwatchStyle.get_fixedWidth());
				EditorGUI.DrawRect(rect, DirectorStyles.Instance.customSkin.colorTrackDarken);
				this.DrawTrackStateBox(trackRect);
			}
		}

		private void DrawTrackBinding(Rect rect, Rect headerRect, TimelineWindow.TimelineState state)
		{
			if (this.showSceneReference)
			{
				if (state.currentDirector != null)
				{
					this.DoTrackBindingGUI(rect, headerRect, state);
					return;
				}
			}
			GUIStyle trackHeaderFont = this.m_Styles.trackHeaderFont;
			trackHeaderFont.get_normal().set_textColor((!SelectionManager.Contains(base.track)) ? this.m_Styles.customSkin.colorTrackFont : Color.get_white());
			bool flag = false;
			string text = base.drawer.GetCustomTitle(base.track);
			if (string.IsNullOrEmpty(text))
			{
				flag = true;
				text = TimelineTrackGUI.GetTrackDisplayName(base.track, state);
			}
			rect.set_width(this.m_Styles.trackHeaderFont.CalcSize(new GUIContent(text)).x);
			if (flag)
			{
				if (GUIUtility.get_keyboardControl() == base.track.GetInstanceID())
				{
					Rect rect2 = rect;
					rect2.set_width(headerRect.get_xMax() - rect.get_xMin() - 80f);
					base.track.set_name(EditorGUI.DelayedTextField(rect2, GUIContent.none, base.track.GetInstanceID(), base.track.get_name(), trackHeaderFont));
				}
				else
				{
					EditorGUI.DelayedTextField(rect, GUIContent.none, base.track.GetInstanceID(), text, trackHeaderFont);
				}
			}
			else
			{
				EditorGUI.LabelField(rect, text, trackHeaderFont);
			}
		}

		protected float DrawTrackDropDownMenu(Rect rect, TimelineWindow.TimelineState state)
		{
			rect.set_y(rect.get_y() + 2f);
			if (GUI.Button(rect, GUIContent.none, this.m_Styles.trackOptions))
			{
				SelectionManager.Clear();
				SelectionManager.Add(base.track);
				base.DisplayTrackMenu(state);
			}
			return 16f;
		}

		private float DrawMuteButton(Rect rect, TimelineWindow.TimelineState state)
		{
			float result;
			if (!base.IsSubTrack() && base.track.muted)
			{
				if (GUI.Button(rect, GUIContent.none, TimelineWindow.styles.mute))
				{
					base.track.muted = false;
					state.Refresh();
				}
				result = 16f;
			}
			else
			{
				result = 0f;
			}
			return result;
		}

		private float DrawLockButton(Rect rect)
		{
			float result;
			if (base.track.locked)
			{
				if (GUI.Button(rect, GUIContent.none, TimelineWindow.styles.locked))
				{
					base.locked = false;
				}
				result = 16f;
			}
			else
			{
				result = 0f;
			}
			return result;
		}

		private bool CanDrawInlineCurve()
		{
			return TimelineUtility.TrackHasAnimationCurves(base.track);
		}

		private float DrawInlineCurveButton(Rect rect, TimelineWindow.TimelineState state)
		{
			float result;
			if (!this.CanDrawInlineCurve())
			{
				result = 0f;
			}
			else
			{
				bool flag = GUI.Toggle(rect, base.track.GetShowInlineCurves(), GUIContent.none, DirectorStyles.Instance.curves);
				if (flag != base.track.GetShowInlineCurves())
				{
					TimelineUndo.PushUndo(base.track, (!flag) ? "Hide Inline Curves" : "Show Inline Curves");
					base.track.SetShowInlineCurves(flag);
					state.GetWindow().treeView.CalculateRowRects();
				}
				result = 16f;
			}
			return result;
		}

		private float DrawRecordButton(Rect rect, TimelineWindow.TimelineState state)
		{
			float result;
			if (this.trackAllowsRecording)
			{
				TrackAsset track = (!base.IsSubTrack()) ? base.track : base.ParentTrack();
				using (new EditorGUI.DisabledScope(base.track.locked || !state.ValidateBindingForTrack(track).IsValid()))
				{
					if (this.IsRecording(state))
					{
						state.editorWindow.Repaint();
						float num = Time.get_realtimeSinceStartup() % 1f;
						GUIContent none = TimelineTrackGUI.s_ArmForRecordContentOn;
						if (num < 0.22f)
						{
							none = GUIContent.none;
						}
						if (GUI.Button(rect, none, GUIStyle.get_none()))
						{
							state.UnarmForRecord(base.track);
						}
					}
					else if (GUI.Button(rect, TimelineTrackGUI.s_ArmForRecordContentOff, GUIStyle.get_none()))
					{
						state.ArmForRecord(base.track);
					}
					result = 16f;
					return result;
				}
			}
			result = 0f;
			return result;
		}

		private float DrawCustomTrackButton(Rect rect, TimelineWindow.TimelineState state)
		{
			float result;
			if (base.drawer.DrawTrackHeaderButton(rect, base.track, state))
			{
				result = 16f;
			}
			else
			{
				result = 0f;
			}
			return result;
		}

		private void GenerateIconForBindingValidationResult(DirectorStyles styles, TrackBindingValidationResult validationResult)
		{
			if (!validationResult.IsValid())
			{
				if (this.m_ProblemIcon == null)
				{
					this.m_ProblemIcon = new GUIContent();
				}
				switch (validationResult.bindingState)
				{
				case TimelineTrackBindingState.NoGameObjectBound:
					this.m_ProblemIcon.set_image(styles.warning.get_normal().get_background());
					this.m_ProblemIcon.set_tooltip("This actor or track is not bound to any GameObject in the scene.");
					break;
				case TimelineTrackBindingState.BoundGameObjectIsDisabled:
					this.m_ProblemIcon.set_image(styles.warning.get_normal().get_background());
					this.m_ProblemIcon.set_tooltip("The bound GameObject (" + validationResult.bindingName + ") is disabled");
					break;
				case TimelineTrackBindingState.NoValidComponentOnBoundGameObject:
					this.m_ProblemIcon.set_image(styles.warning.get_normal().get_background());
					this.m_ProblemIcon.set_tooltip("Could not find an Animator on" + validationResult.bindingName);
					break;
				case TimelineTrackBindingState.RequiredComponentOnBoundGameObjectIsDisabled:
					this.m_ProblemIcon.set_image(styles.warning.get_normal().get_background());
					this.m_ProblemIcon.set_tooltip("Animator is disabled");
					break;
				}
			}
		}

		private void DoTrackBindingGUI(Rect rect, Rect headerRect, ITimelineState state)
		{
			float num = 130f;
			rect.set_y(rect.get_y() + (rect.get_height() - 16f) / 2f);
			rect.set_height(16f);
			rect.set_width(headerRect.get_xMax() - num - rect.get_xMin());
			Object genericBinding = state.currentDirector.GetGenericBinding(base.track);
			if (rect.Contains(Event.get_current().get_mousePosition()) && TimelineTrackGUI.IsDraggingEvent() && DragAndDrop.get_objectReferences().Length == 1)
			{
				this.HandleDragAndDrop(state, TimelineTrackGUI.GetRequiredBindingType(this.m_Bindings[0]));
			}
			else
			{
				TrackAsset track = base.track;
				switch (this.m_Bindings[0].get_streamType())
				{
				case 0:
				{
					EditorGUI.BeginChangeCheck();
					Animator animator = EditorGUI.ObjectField(rect, genericBinding, typeof(Animator), true) as Animator;
					if (EditorGUI.EndChangeCheck())
					{
						TimelineTrackGUI.SetTrackBinding(state, track, (!(animator == null)) ? animator.get_gameObject() : null);
					}
					goto IL_1E8;
				}
				case 1:
				{
					EditorGUI.BeginChangeCheck();
					AudioSource objectToBind = EditorGUI.ObjectField(rect, genericBinding, typeof(AudioSource), true) as AudioSource;
					if (EditorGUI.EndChangeCheck())
					{
						TimelineTrackGUI.SetTrackBinding(state, track, objectToBind);
					}
					goto IL_1E8;
				}
				case 3:
					if (this.m_Bindings[0].get_sourceBindingType() != null && typeof(Object).IsAssignableFrom(this.m_Bindings[0].get_sourceBindingType()))
					{
						EditorGUI.BeginChangeCheck();
						Object objectToBind2 = EditorGUI.ObjectField(rect, genericBinding, this.m_Bindings[0].get_sourceBindingType(), true);
						if (EditorGUI.EndChangeCheck())
						{
							TimelineTrackGUI.SetTrackBinding(state, track, objectToBind2);
						}
					}
					goto IL_1E8;
				}
				throw new NotImplementedException("");
				IL_1E8:;
			}
		}

		private static Type GetRequiredBindingType(PlayableBinding binding)
		{
			Type result = binding.get_sourceBindingType();
			if (binding.get_streamType() == null)
			{
				result = typeof(Animator);
			}
			else if (binding.get_streamType() == 1)
			{
				result = typeof(AudioSource);
			}
			return result;
		}

		private void HandleDragAndDrop(ITimelineState state, Type requiredComponent)
		{
			DragAndDropVisualMode dragAndDropVisualMode = 32;
			if (requiredComponent != null && requiredComponent.IsInstanceOfType(DragAndDrop.get_objectReferences()[0]))
			{
				dragAndDropVisualMode = 2;
				if (Event.get_current().get_type() == 10)
				{
					TimelineTrackGUI.SetTrackBinding(state, base.track, DragAndDrop.get_objectReferences()[0]);
				}
			}
			else if (typeof(Component).IsAssignableFrom(requiredComponent))
			{
				GameObject gameObjectBeingDragged = DragAndDrop.get_objectReferences()[0] as GameObject;
				if (gameObjectBeingDragged != null)
				{
					dragAndDropVisualMode = 2;
					if (Event.get_current().get_type() == 10)
					{
						Component component = gameObjectBeingDragged.GetComponent(requiredComponent);
						if (component == null)
						{
							string str = requiredComponent.ToString().Split(".".ToCharArray()).Last<string>();
							GenericMenu genericMenu = new GenericMenu();
							genericMenu.AddItem(EditorGUIUtility.TextContent("Create " + str + " on " + gameObjectBeingDragged.get_name()), false, delegate(object nullParam)
							{
								Undo.AddComponent(gameObjectBeingDragged, requiredComponent);
								TimelineTrackGUI.SetTrackBinding(state, this.track, gameObjectBeingDragged);
							}, null);
							genericMenu.AddSeparator("");
							genericMenu.AddItem(EditorGUIUtility.TextContent("Cancel"), false, delegate(object userData)
							{
							}, null);
							genericMenu.ShowAsContext();
						}
						else
						{
							TimelineTrackGUI.SetTrackBinding(state, base.track, gameObjectBeingDragged);
						}
					}
				}
			}
			DragAndDrop.set_visualMode(dragAndDropVisualMode);
			if (dragAndDropVisualMode == 2)
			{
				DragAndDrop.AcceptDrag();
			}
		}

		private static void SetTrackBinding(ITimelineState state, TrackAsset track, Object objectToBind)
		{
			if (state != null)
			{
				state.previewMode = false;
				TimelineUtility.SetBindingInDirector(state.currentDirector, track, objectToBind);
				state.rebuildGraph = true;
			}
		}

		private static void SetTrackBinding(ITimelineState state, TrackAsset track, GameObject gameObjectToBind)
		{
			if (state != null)
			{
				state.previewMode = false;
				TimelineUtility.SetSceneGameObject(state.currentDirector, track, gameObjectToBind);
				state.rebuildGraph = true;
			}
		}

		private static bool IsDraggingEvent()
		{
			return Event.get_current().get_type() == 9 || Event.get_current().get_type() == 15 || Event.get_current().get_type() == 10;
		}

		private bool IsRecording(TimelineWindow.TimelineState state)
		{
			return state.recording && state.IsArmedForRecord(base.track);
		}

		private void DrawClips(Rect trackRect, TimelineWindow.TimelineState state)
		{
			Vector2 timeAreaShownRange = state.timeAreaShownRange;
			for (int num = 0; num != this.m_ClipGUICache.Count; num++)
			{
				TimelineClipGUI timelineClipGUI = this.m_ClipGUICache[num];
				timelineClipGUI.visible = (timelineClipGUI.clip.end >= (double)timeAreaShownRange.x && timelineClipGUI.clip.start <= (double)timeAreaShownRange.y);
				if (timelineClipGUI.visible)
				{
					if (num + 1 < this.m_ClipGUICache.Count)
					{
						timelineClipGUI.nextClip = this.m_ClipGUICache[num + 1];
					}
					if (num > 0)
					{
						timelineClipGUI.previousClip = this.m_ClipGUICache[num - 1];
					}
					timelineClipGUI.DrawBlendingCurves(state);
					timelineClipGUI.Draw(trackRect, state, base.drawer);
				}
			}
		}

		private void DrawEvents(Rect trackRect, TimelineWindow.TimelineState state)
		{
			for (int num = 0; num != this.m_MarkerGuiCache.Count; num++)
			{
				TimelineMarkerGUI timelineMarkerGUI = this.m_MarkerGuiCache[num];
				timelineMarkerGUI.Draw(trackRect, state, base.drawer);
			}
		}

		public void DrawRecordingTrackBackground(Rect trackRect)
		{
			EditorGUI.DrawRect(trackRect, DirectorStyles.Instance.customSkin.colorTrackBackgroundRecording);
			Graphics.ShadowLabel(trackRect, this.m_Styles.Elipsify(DirectorStyles.recordingLabel.get_text(), trackRect, this.m_Styles.fontClip), this.m_Styles.fontClip, Color.get_white(), Color.get_black());
		}

		private void DrawClipConnectors()
		{
			double num = -1.7976931348623157E+308;
			GUIStyle connector = this.m_Styles.connector;
			foreach (TimelineClipGUI current in from c in this.m_ClipGUICache
			where c.visible
			orderby c.clip.start
			select c)
			{
				if (current.UnClippedRect.get_width() > 14f)
				{
					double num2 = Math.Abs(current.clip.start - num);
					if (num2 < 1E-09)
					{
						Rect unClippedRect = current.UnClippedRect;
						unClippedRect.set_x(unClippedRect.get_x() - connector.get_fixedWidth() / 2f);
						unClippedRect.set_width(connector.get_fixedWidth());
						unClippedRect.set_height(connector.get_fixedHeight());
						GUI.Box(unClippedRect, GUIContent.none, connector);
					}
				}
				num = current.clip.start + current.clip.duration;
			}
		}

		private void UpdateClipOverlaps()
		{
			TrackExtensions.ComputeBlendsFromOverlaps((from c in this.m_ClipGUICache
			select c.clip).ToArray<TimelineClip>());
		}

		public void RebuildGUICache(TimelineWindow.TimelineState state)
		{
			TimelineItemGUI[] array = (from x in state.captured.OfType<TimelineItemGUI>()
			where x.selectableObject != null && x.parentTrackGUI.track == base.track
			select x).ToArray<TimelineItemGUI>();
			TimelineItemGUI[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				TimelineItemGUI item = array2[i];
				state.captured.Remove(item);
			}
			this.m_ChildrenControls = (from x in this.m_ChildrenControls
			where x.GetType() != typeof(TimelineItemGUI)
			select x).ToList<Control>();
			this.RebuildClipsGUICache();
			this.RebuildEventsGUICache(state);
			this.SortItemsByStartTime();
			this.m_TrackHash = base.track.Hash();
		}

		private void RebuildClipsGUICache()
		{
			this.m_ClipGUICache = new List<TimelineClipGUI>();
			TimelineClip[] clips = base.track.clips;
			for (int i = 0; i < clips.Length; i++)
			{
				TimelineClip clip = clips[i];
				TimelineClipGUI item = new TimelineClipGUI(clip, this);
				this.m_ClipGUICache.Add(item);
				this.m_ChildrenControls.Add(item);
			}
		}

		private void RebuildEventsGUICache(TimelineWindow.TimelineState state)
		{
			this.m_MarkerGuiCache = new List<TimelineMarkerGUI>();
			ITimelineMarkerContainer timelineMarkerContainer = base.track as ITimelineMarkerContainer;
			if (timelineMarkerContainer != null)
			{
				TimelineMarker[] markers = timelineMarkerContainer.GetMarkers();
				for (int i = 0; i < markers.Length; i++)
				{
					TimelineMarker theMarker = markers[i];
					TimelineMarkerGUI item = new TimelineMarkerGUI(theMarker, state.timeline, this);
					this.m_MarkerGuiCache.Add(item);
					this.m_ChildrenControls.Add(item);
				}
			}
		}

		public void SortItemsByStartTime()
		{
			this.SortClipsByStartTime();
			this.SortEventsByStartTime();
		}

		public void SortClipsByStartTime()
		{
			this.m_ClipGUICache = (from x in this.m_ClipGUICache
			orderby x.clip.start
			select x).ToList<TimelineClipGUI>();
			this.resortClips = true;
		}

		public void SortEventsByStartTime()
		{
			this.m_MarkerGuiCache = (from x in this.m_MarkerGuiCache
			orderby x.timelineMarker.time
			select x).ToList<TimelineMarkerGUI>();
			this.resortEvents = true;
		}

		public int BlendHash()
		{
			int num = 0;
			for (int i = 0; i < this.m_ClipGUICache.Count; i++)
			{
				TimelineClip clip = this.m_ClipGUICache[i].clip;
				num = HashUtility.CombineHash(num, (clip.duration - clip.start).GetHashCode(), clip.blendInCurveMode.GetHashCode(), clip.blendOutCurveMode.GetHashCode());
			}
			return num;
		}

		public override void OnGraphRebuilt()
		{
			this.RefreshCurveEditor();
		}

		public void RefreshCurveEditor()
		{
			AnimationTrack animationTrack = base.track as AnimationTrack;
			TimelineWindow instance = TimelineWindow.instance;
			if (animationTrack != null && instance != null && instance.state != null)
			{
				bool flag = this.clipCurveEditor != null;
				bool flag2 = animationTrack.ShouldShowInfiniteClipEditor();
				if (flag != flag2)
				{
					instance.state.AddEndFrameDelegate(delegate(ITimelineState x, Event currentEvent)
					{
						x.Refresh();
						return true;
					});
				}
			}
		}

		private bool DoesTrackAllowRecording()
		{
			bool result;
			if (base.track is AnimationTrack)
			{
				result = true;
			}
			else
			{
				result = base.track.clips.Any((TimelineClip c) => c.HasAnyAnimatableParameters());
			}
			return result;
		}

		public void ResetClipParityID()
		{
			this.m_ClipParityID = 0;
		}

		public int GetNextClipParityID()
		{
			int clipParityID = this.m_ClipParityID;
			this.m_ClipParityID++;
			this.m_ClipParityID %= 2;
			return clipParityID;
		}
	}
}
