using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[CanEditMultipleObjects, CustomEditor(typeof(EditorClip))]
	internal class ClipInspector : Editor
	{
		private static class Styles
		{
			public static readonly GUIContent StartName = EditorGUIUtility.TextContent("Start|The start time of the clip");

			public static readonly GUIContent DurationName = EditorGUIUtility.TextContent("Duration|The length of the clip");

			public static readonly GUIContent EndName = EditorGUIUtility.TextContent("End|The end time of the clip");

			public static readonly GUIContent EaseInDurationName = EditorGUIUtility.TextContent("Ease In Duration|The length of the blend in");

			public static readonly GUIContent EaseOutDurationName = EditorGUIUtility.TextContent("Ease Out Duration|The length of the blend out");

			public static readonly GUIContent ClipInName = EditorGUIUtility.TextContent("Clip In|Start the clip at this local time");

			public static readonly GUIContent TimeScaleName = EditorGUIUtility.TextContent("Speed Multiplier|Time scale of the playback speed");

			public static readonly GUIContent PreExtrapolateLabel = EditorGUIUtility.TextContent("Pre-Extrapolate|Extrapolation used prior to the first clip");

			public static readonly GUIContent PostExtrapolateLabel = EditorGUIUtility.TextContent("Post-Extrapolate|Extrapolation used after a clip ends");

			public static readonly GUIContent BlendInCurveName = EditorGUIUtility.TextContent("In|Blend In Curve");

			public static readonly GUIContent BlendOutCurveName = EditorGUIUtility.TextContent("Out|Blend Out Curve");

			public static readonly GUIContent PreviewTitle = EditorGUIUtility.TextContent("Curve Editor");

			public static readonly GUIContent ClipTimingTitle = EditorGUIUtility.TextContent("Clip Timing");

			public static readonly GUIContent AnimationExtrapolationTitle = EditorGUIUtility.TextContent("Animation Extrapolation");

			public static readonly GUIContent BlendCurvesTitle = EditorGUIUtility.TextContent("Blend Curves");

			public static readonly GUIContent GroupTimingTitle = EditorGUIUtility.TextContent("Multiple Clip Timing");

			public static readonly GUIContent MultipleClipsSelectedIncompatibleCapabilitiesWarning = EditorGUIUtility.TextContent("Multiple clips selected. Only common properties are shown.");

			public static readonly GUIContent MultipleSelectionTitle = EditorGUIUtility.TextContent("Timeline Clips");
		}

		private struct SelectionInfo
		{
			public bool supportsExtrapolation;

			public bool supportsClipIn;

			public bool supportsSpeedMultiplier;

			public bool supportsBlending;

			public bool hasBlendIn;

			public bool hasBlendOut;

			public bool selectedAssetTypesAreHomogeneous;

			public bool selectionContainsAtLeastTwoClipsOnTheSameTrack;
		}

		private class EditorClipSelection
		{
			public EditorClip editorClip
			{
				[CompilerGenerated]
				get
				{
					return this.<editorClip>k__BackingField;
				}
			}

			public TimelineClip clip
			{
				get
				{
					return (!(this.editorClip == null)) ? this.editorClip.clip : null;
				}
			}

			public SerializedObject playableAssetObject
			{
				[CompilerGenerated]
				get
				{
					return this.<playableAssetObject>k__BackingField;
				}
			}

			public int lastCurveVersion
			{
				get;
				set;
			}

			public double lastEvalTime
			{
				get;
				set;
			}

			public EditorClipSelection(EditorClip anEditorClip)
			{
				this.<editorClip>k__BackingField = anEditorClip;
				this.lastCurveVersion = -1;
				this.lastEvalTime = -1.0;
				SerializedObject serializedObject = new SerializedObject(this.editorClip);
				SerializedProperty serializedProperty = serializedObject.FindProperty("m_Item.m_Asset");
				if (serializedProperty != null)
				{
					PlayableAsset playableAsset = serializedProperty.get_objectReferenceValue() as PlayableAsset;
					if (playableAsset != null)
					{
						this.<playableAssetObject>k__BackingField = new SerializedObject(playableAsset);
					}
				}
			}
		}

		private SerializedProperty m_DisplayNameProperty;

		private SerializedProperty m_StartProperty;

		private SerializedProperty m_DurationProperty;

		private SerializedProperty m_BlendInDurationProperty;

		private SerializedProperty m_BlendOutDurationProperty;

		private SerializedProperty m_EaseInDurationProperty;

		private SerializedProperty m_EaseOutDurationProperty;

		private SerializedProperty m_ClipInProperty;

		private SerializedProperty m_TimeScaleProperty;

		private SerializedProperty m_PostExtrapolationModeProperty;

		private SerializedProperty m_PreExtrapolationModeProperty;

		private SerializedProperty m_PostExtrapolationTimeProperty;

		private SerializedProperty m_PreExtrapolationTimeProperty;

		private SerializedProperty m_MixInCurveProperty;

		private SerializedProperty m_MixOutCurveProperty;

		private SerializedProperty m_BlendInCurveModeProperty;

		private SerializedProperty m_BlendOutCurveModeProperty;

		private TimelineAsset m_TimelineAsset;

		private List<ClipInspector.EditorClipSelection> m_SelectionCache;

		private Editor m_SelectedPlayableAssetsInspector;

		private ClipInspectorCurveEditor m_ClipCurveEditor;

		private AnimationCurve[] m_PreviewCurves;

		private CurvePresetLibrary m_CurvePresets;

		private bool m_IsClipAssetInspectorExpanded = true;

		private GUIContent m_ClipAssetTitle = new GUIContent();

		private string m_MultiselectionHeaderTitle;

		private GUIContent m_IconCache;

		private Texture2D m_DefaultIcon;

		private ClipInspector.SelectionInfo m_SelectionInfo;

		private bool hasMultipleSelection
		{
			get
			{
				return base.get_targets().Length > 1;
			}
		}

		private float currentFrameRate
		{
			get
			{
				return (!this.m_TimelineAsset) ? TimelineAsset.EditorSettings.kDefaultFPS : this.m_TimelineAsset.editorSettings.fps;
			}
		}

		private bool selectionHasIncompatibleCapabilities
		{
			get
			{
				return !this.m_SelectionInfo.supportsBlending || !this.m_SelectionInfo.supportsClipIn || !this.m_SelectionInfo.supportsExtrapolation || !this.m_SelectionInfo.supportsSpeedMultiplier;
			}
		}

		private void InitializeProperties()
		{
			this.m_DisplayNameProperty = base.get_serializedObject().FindProperty("m_Item.m_DisplayName");
			this.m_StartProperty = base.get_serializedObject().FindProperty("m_Item.m_Start");
			this.m_DurationProperty = base.get_serializedObject().FindProperty("m_Item.m_Duration");
			this.m_BlendInDurationProperty = base.get_serializedObject().FindProperty("m_Item.m_BlendInDuration");
			this.m_BlendOutDurationProperty = base.get_serializedObject().FindProperty("m_Item.m_BlendOutDuration");
			this.m_EaseInDurationProperty = base.get_serializedObject().FindProperty("m_Item.m_EaseInDuration");
			this.m_EaseOutDurationProperty = base.get_serializedObject().FindProperty("m_Item.m_EaseOutDuration");
			this.m_ClipInProperty = base.get_serializedObject().FindProperty("m_Item.m_ClipIn");
			this.m_TimeScaleProperty = base.get_serializedObject().FindProperty("m_Item.m_TimeScale");
			this.m_PostExtrapolationModeProperty = base.get_serializedObject().FindProperty("m_Item.m_PostExtrapolationMode");
			this.m_PreExtrapolationModeProperty = base.get_serializedObject().FindProperty("m_Item.m_PreExtrapolationMode");
			this.m_PostExtrapolationTimeProperty = base.get_serializedObject().FindProperty("m_Item.m_PostExtrapolationTime");
			this.m_PreExtrapolationTimeProperty = base.get_serializedObject().FindProperty("m_Item.m_PreExtrapolationTime");
			this.m_MixInCurveProperty = base.get_serializedObject().FindProperty("m_Item.m_MixInCurve");
			this.m_MixOutCurveProperty = base.get_serializedObject().FindProperty("m_Item.m_MixOutCurve");
			this.m_BlendInCurveModeProperty = base.get_serializedObject().FindProperty("m_Item.m_BlendInCurveMode");
			this.m_BlendOutCurveModeProperty = base.get_serializedObject().FindProperty("m_Item.m_BlendOutCurveMode");
		}

		public override bool RequiresConstantRepaint()
		{
			return base.RequiresConstantRepaint() || (this.m_SelectedPlayableAssetsInspector != null && this.m_SelectedPlayableAssetsInspector.RequiresConstantRepaint());
		}

		internal override void OnHeaderTitleGUI(Rect titleRect, string header)
		{
			if (this.hasMultipleSelection)
			{
				base.OnHeaderTitleGUI(titleRect, this.m_MultiselectionHeaderTitle);
			}
			else if (this.m_DisplayNameProperty != null)
			{
				base.get_serializedObject().Update();
				EditorGUI.BeginChangeCheck();
				EditorGUI.DelayedTextField(titleRect, this.m_DisplayNameProperty, GUIContent.none);
				if (EditorGUI.EndChangeCheck())
				{
					this.ApplyModifiedProperties();
					TimelineWindow.RepaintIfEditingTimelineAsset(this.m_TimelineAsset);
				}
			}
		}

		internal override void DrawHeaderHelpAndSettingsGUI(Rect r)
		{
			Vector2 vector = EditorStyles.get_iconButton().CalcSize(EditorGUI.GUIContents.get_helpIcon());
			Object target = base.get_target();
			EditorGUI.HelpIconButton(new Rect(r.get_xMax() - vector.x, r.get_y() + 5f, vector.x, vector.y), target);
		}

		internal override void OnHeaderIconGUI(Rect iconRect)
		{
			if (this.hasMultipleSelection)
			{
				base.OnHeaderIconGUI(iconRect);
			}
			else
			{
				if (this.m_IconCache == null)
				{
					Texture2D texture2D = this.m_DefaultIcon;
					if (this.m_SelectionInfo.selectedAssetTypesAreHomogeneous)
					{
						TimelineClip clip = this.m_SelectionCache.First<ClipInspector.EditorClipSelection>().clip;
						texture2D = AssetPreview.GetMiniThumbnail(clip.underlyingAsset);
						if (texture2D == this.m_DefaultIcon)
						{
							TimelineTrackBaseGUI timelineTrackBaseGUI = TimelineWindow.instance.allTracks.Find((TimelineTrackBaseGUI uiTrack) => uiTrack.track == clip.parentTrack);
							if (timelineTrackBaseGUI != null && timelineTrackBaseGUI.drawer.GetIcon() != GUIContent.none)
							{
								this.m_IconCache = timelineTrackBaseGUI.drawer.GetIcon();
							}
						}
					}
					if (this.m_IconCache == null)
					{
						this.m_IconCache = new GUIContent(texture2D);
					}
				}
				GUI.Label(iconRect, this.m_IconCache);
			}
		}

		public void OnEnable()
		{
			this.m_ClipCurveEditor = new ClipInspectorCurveEditor();
			this.m_DefaultIcon = EditorGUIUtility.FindTexture("DefaultAsset Icon");
			this.m_SelectionCache = new List<ClipInspector.EditorClipSelection>();
			Object[] targets = base.get_targets();
			for (int i = 0; i < targets.Length; i++)
			{
				Object @object = targets[i];
				EditorClip editorClip = @object as EditorClip;
				if (editorClip != null)
				{
					if (!this.IsTimelineAssetValidForEditorClip(editorClip))
					{
						this.m_SelectionCache.Clear();
						return;
					}
					this.m_SelectionCache.Add(new ClipInspector.EditorClipSelection(editorClip));
				}
			}
			this.m_SelectionInfo = this.BuildSelectionInfo();
			if (this.m_SelectionInfo.selectedAssetTypesAreHomogeneous)
			{
				Object[] objects = (from e in this.m_SelectionCache
				select e.clip.asset).ToArray<Object>();
				this.m_SelectedPlayableAssetsInspector = TimelineInspectorUtility.GetInspectorForObjects(objects);
			}
			this.m_MultiselectionHeaderTitle = this.m_SelectionCache.Count + " " + ClipInspector.Styles.MultipleSelectionTitle.get_text();
			this.m_ClipAssetTitle.set_text(this.PlayableAssetSectionTitle());
			this.InitializeProperties();
		}

		private void DrawClipProperties()
		{
			IEnumerable<ClipInspector.EditorClipSelection> enumerable = from s in this.m_SelectionCache
			where s.editorClip.GetHashCode() != s.editorClip.lastHash
			select s;
			this.UnselectCurves();
			EditorGUI.BeginChangeCheck();
			if (this.hasMultipleSelection)
			{
				GUILayout.Label(ClipInspector.Styles.GroupTimingTitle, new GUILayoutOption[0]);
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
				this.DrawGroupSelectionProperties();
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
				EditorGUILayout.Space();
			}
			GUILayout.Label(ClipInspector.Styles.ClipTimingTitle, new GUILayoutOption[0]);
			if (this.hasMultipleSelection && this.selectionHasIncompatibleCapabilities)
			{
				GUILayout.Label(ClipInspector.Styles.MultipleClipsSelectedIncompatibleCapabilitiesWarning, EditorStyles.get_helpBox(), new GUILayoutOption[0]);
			}
			EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
			if (!this.m_SelectionInfo.selectionContainsAtLeastTwoClipsOnTheSameTrack)
			{
				this.DrawStartTimeProperty();
				this.DrawEndTimeProperty();
			}
			if (!this.hasMultipleSelection)
			{
				this.DrawDurationProperty();
			}
			if (this.m_SelectionInfo.supportsBlending)
			{
				EditorGUILayout.Space();
				this.DrawBlendingProperties();
			}
			if (this.m_SelectionInfo.supportsClipIn)
			{
				EditorGUILayout.Space();
				this.DrawClipInProperty();
			}
			if (!this.hasMultipleSelection && this.m_SelectionInfo.supportsSpeedMultiplier)
			{
				EditorGUILayout.Space();
				this.DrawTimeScaleProperty();
			}
			EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
			bool flag = false;
			foreach (ClipInspector.EditorClipSelection current in enumerable)
			{
				EditorUtility.SetDirty(current.editorClip);
				flag = true;
			}
			bool flag2 = false;
			if (EditorGUI.EndChangeCheck() || flag)
			{
				if (TimelineWindow.IsEditingTimelineAsset(this.m_TimelineAsset) && TimelineWindow.instance.state != null)
				{
					TimelineWindow.instance.state.Evaluate();
					TimelineWindow.instance.Repaint();
				}
				flag2 = true;
			}
			if (this.m_SelectionInfo.supportsExtrapolation)
			{
				EditorGUILayout.Space();
				GUILayout.Label(ClipInspector.Styles.AnimationExtrapolationTitle, new GUILayoutOption[0]);
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
				this.DrawExtrapolationOptions();
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
			}
			if (this.m_SelectionInfo.supportsBlending)
			{
				EditorGUILayout.Space();
				GUILayout.Label(ClipInspector.Styles.BlendCurvesTitle, new GUILayoutOption[0]);
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
				this.DrawBlendOptions();
				EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
			}
			EditorGUILayout.Space();
			if (this.CanShowPlayableAssetInspector())
			{
				this.DrawClipAssetGui();
			}
			if (flag2)
			{
				foreach (ClipInspector.EditorClipSelection current2 in this.m_SelectionCache)
				{
					current2.editorClip.lastHash = current2.editorClip.GetHashCode();
				}
			}
		}

		public override void OnInspectorGUI()
		{
			if (!(TimelineWindow.instance == null) && !(this.m_TimelineAsset == null))
			{
				base.get_serializedObject().Update();
				this.DrawClipProperties();
				this.ApplyModifiedProperties();
			}
		}

		private void DrawTimeScaleProperty()
		{
			double num = Math.Max(TimelineClip.kTimeScaleMin, Math.Min(TimelineClip.kTimeScaleMax, this.m_TimeScaleProperty.get_doubleValue()));
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(this.m_TimeScaleProperty, ClipInspector.Styles.TimeScaleName, new GUILayoutOption[0]);
			if (EditorGUI.EndChangeCheck())
			{
				this.m_TimeScaleProperty.set_doubleValue(Math.Max(TimelineClip.kTimeScaleMin, Math.Min(TimelineClip.kTimeScaleMax, this.m_TimeScaleProperty.get_doubleValue())));
				this.m_DurationProperty.set_doubleValue(this.m_DurationProperty.get_doubleValue() * num / this.m_TimeScaleProperty.get_doubleValue());
			}
		}

		private void DrawEndTimeProperty()
		{
			bool showMixed = this.m_StartProperty.get_hasMultipleDifferentValues() || this.m_DurationProperty.get_hasMultipleDifferentValues();
			EditorGUI.BeginChangeCheck();
			double time = this.m_StartProperty.get_doubleValue() + this.m_DurationProperty.get_doubleValue();
			double num = TimelineInspectorUtility.TimeField(ClipInspector.Styles.EndName, time, false, showMixed, (double)this.m_TimelineAsset.editorSettings.fps, -TimelineClip.kMaxTimeValue, TimelineClip.kMaxTimeValue * 2.0);
			if (EditorGUI.EndChangeCheck())
			{
				this.m_DurationProperty.set_doubleValue(Math.Max(1E-06, num - this.m_StartProperty.get_doubleValue()));
			}
		}

		protected void DrawClipAssetGui()
		{
			if (!(this.m_SelectedPlayableAssetsInspector == null))
			{
				this.m_IsClipAssetInspectorExpanded = EditorGUILayout.FoldoutTitlebar(this.m_IsClipAssetInspectorExpanded, this.m_ClipAssetTitle, false);
				if (this.m_IsClipAssetInspectorExpanded)
				{
					EditorGUILayout.Space();
					EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() + 1);
					this.ShowPlayableAssetInspector();
					EditorGUI.set_indentLevel(EditorGUI.get_indentLevel() - 1);
				}
			}
		}

		private void DrawExtrapolationOptions()
		{
			EditorGUI.BeginChangeCheck();
			double doubleValue = this.m_PreExtrapolationTimeProperty.get_doubleValue();
			bool flag = doubleValue > 0.0;
			if (flag)
			{
				EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
				EditorGUILayout.PropertyField(this.m_PreExtrapolationModeProperty, ClipInspector.Styles.PreExtrapolateLabel, new GUILayoutOption[0]);
				EditorGUI.set_showMixedValue(this.m_PreExtrapolationTimeProperty.get_hasMultipleDifferentValues());
				EditorGUILayout.DoubleField(doubleValue, EditorStyles.get_label(), new GUILayoutOption[0]);
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
			EditorGUILayout.PropertyField(this.m_PostExtrapolationModeProperty, ClipInspector.Styles.PostExtrapolateLabel, new GUILayoutOption[0]);
			EditorGUI.set_showMixedValue(this.m_PostExtrapolationTimeProperty.get_hasMultipleDifferentValues());
			EditorGUILayout.DoubleField(this.m_PostExtrapolationTimeProperty.get_doubleValue(), EditorStyles.get_label(), new GUILayoutOption[0]);
			EditorGUILayout.EndHorizontal();
			if (EditorGUI.EndChangeCheck())
			{
				if (TimelineWindow.IsEditingTimelineAsset(this.m_TimelineAsset) && TimelineWindow.instance.state != null)
				{
					TimelineWindow.instance.state.Refresh();
				}
			}
		}

		private void OnDestroy()
		{
			Object.DestroyImmediate(this.m_SelectedPlayableAssetsInspector);
		}

		public override GUIContent GetPreviewTitle()
		{
			return ClipInspector.Styles.PreviewTitle;
		}

		public override bool HasPreviewGUI()
		{
			return this.m_PreviewCurves != null;
		}

		public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
		{
			if (this.m_PreviewCurves != null && this.m_ClipCurveEditor != null)
			{
				this.SetCurveEditorTrackHead();
				this.m_ClipCurveEditor.OnGUI(r, this.m_CurvePresets);
			}
		}

		private void SetCurveEditorTrackHead()
		{
			if (!(TimelineWindow.instance == null) && TimelineWindow.instance.state != null)
			{
				if (!this.hasMultipleSelection)
				{
					EditorClip editorClip = base.get_target() as EditorClip;
					if (!(editorClip == null))
					{
						PlayableDirector currentDirector = TimelineWindow.instance.state.currentDirector;
						if (!(currentDirector == null))
						{
							this.m_ClipCurveEditor.trackTime = ClipInspectorCurveEditor.kDisableTrackTime;
						}
					}
				}
			}
		}

		private void UnselectCurves()
		{
			if (Event.get_current().get_type() == null)
			{
				this.m_PreviewCurves = null;
				if (this.m_ClipCurveEditor != null)
				{
					this.m_ClipCurveEditor.SetUpdateCurveCallback(null);
				}
			}
		}

		private void OnMixCurveSelected(string title, CurvePresetLibrary library, SerializedProperty curveSelected, bool easeIn)
		{
			this.m_CurvePresets = library;
			this.m_PreviewCurves = new AnimationCurve[]
			{
				curveSelected.get_animationCurveValue()
			};
			this.m_ClipCurveEditor.headerString = title;
			this.m_ClipCurveEditor.SetCurves(this.m_PreviewCurves, null);
			this.m_ClipCurveEditor.SetSelected(curveSelected.get_animationCurveValue());
			if (easeIn)
			{
				this.m_ClipCurveEditor.SetUpdateCurveCallback(new Action<AnimationCurve, EditorCurveBinding>(this.MixInCurveUpdated));
			}
			else
			{
				this.m_ClipCurveEditor.SetUpdateCurveCallback(new Action<AnimationCurve, EditorCurveBinding>(this.MixOutCurveUpdated));
			}
			base.Repaint();
		}

		private void MixInCurveUpdated(AnimationCurve curve, EditorCurveBinding binding)
		{
			curve.set_keys(CurveEditUtility.SanitizeCurveKeys(curve.get_keys(), true));
			this.m_MixInCurveProperty.set_animationCurveValue(curve);
			base.get_serializedObject().ApplyModifiedProperties();
			EditorClip editorClip = base.get_target() as EditorClip;
			if (editorClip != null)
			{
				editorClip.lastHash = editorClip.GetHashCode();
			}
			this.RefreshCurves();
		}

		private void MixOutCurveUpdated(AnimationCurve curve, EditorCurveBinding binding)
		{
			curve.set_keys(CurveEditUtility.SanitizeCurveKeys(curve.get_keys(), false));
			this.m_MixOutCurveProperty.set_animationCurveValue(curve);
			base.get_serializedObject().ApplyModifiedProperties();
			EditorClip editorClip = base.get_target() as EditorClip;
			if (editorClip != null)
			{
				editorClip.lastHash = editorClip.GetHashCode();
			}
			this.RefreshCurves();
		}

		private void RefreshCurves()
		{
			AnimationCurvePreviewCache.ClearCache();
			TimelineWindow.RepaintIfEditingTimelineAsset(this.m_TimelineAsset);
			base.Repaint();
		}

		private void DrawBlendCurve(GUIContent title, SerializedProperty modeProperty, SerializedProperty curveProperty, Action<SerializedProperty> onCurveClick)
		{
			EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
			EditorGUILayout.PropertyField(modeProperty, title, new GUILayoutOption[0]);
			if (this.hasMultipleSelection)
			{
				GUILayout.FlexibleSpace();
			}
			else
			{
				using (new EditorGUI.DisabledScope(modeProperty.get_intValue() != 1))
				{
					ClipInspectorCurveEditor.CurveField(GUIContent.none, curveProperty, onCurveClick);
				}
			}
			EditorGUILayout.EndHorizontal();
		}

		private void ShowPlayableAssetInspector()
		{
			if (this.m_SelectionInfo.selectedAssetTypesAreHomogeneous)
			{
				if (this.m_SelectedPlayableAssetsInspector != null)
				{
					foreach (ClipInspector.EditorClipSelection current in this.m_SelectionCache)
					{
						ClipInspector.PreparePlayableAsset(current);
					}
					EditorGUI.BeginChangeCheck();
					this.m_SelectedPlayableAssetsInspector.OnInspectorGUI();
					if (EditorGUI.EndChangeCheck())
					{
						if (TimelineWindow.IsEditingTimelineAsset(this.m_TimelineAsset) && TimelineWindow.instance.state != null)
						{
							TimelineWindow.instance.state.rebuildGraph = true;
							TimelineWindow.instance.Repaint();
						}
					}
				}
			}
		}

		private static void PreparePlayableAsset(ClipInspector.EditorClipSelection selectedItem)
		{
			if (Event.get_current().get_type() == 7)
			{
				if (selectedItem.playableAssetObject != null)
				{
					TimelineClip clip = selectedItem.clip;
					if (clip != null && !(clip.curves == null))
					{
						TimelineWindow instance = TimelineWindow.instance;
						if (!(instance == null) && instance.state != null)
						{
							if (!instance.state.previewMode)
							{
								selectedItem.lastEvalTime = -1.0;
							}
							else
							{
								double num = instance.state.time;
								num = clip.ToLocalTime(num);
								if (selectedItem.lastEvalTime == num)
								{
									int version = AnimationClipCurveCache.Instance.GetCurveInfo(clip.curves).version;
									if (version == selectedItem.lastCurveVersion)
									{
										return;
									}
									selectedItem.lastCurveVersion = version;
								}
								selectedItem.lastEvalTime = num;
								AnimationClipCurveInfo curveInfo = AnimationClipCurveCache.Instance.GetCurveInfo(clip.curves);
								if (curveInfo.bindings.Length != 0)
								{
									selectedItem.playableAssetObject.Update();
									SerializedProperty iterator = selectedItem.playableAssetObject.GetIterator();
									while (iterator.NextVisible(true))
									{
										if (clip.IsParameterAnimated(iterator.get_propertyPath()))
										{
											AnimationCurve animatedParameter = clip.GetAnimatedParameter(iterator.get_propertyPath());
											SerializedPropertyType propertyType = iterator.get_propertyType();
											switch (propertyType)
											{
											case 0:
												iterator.set_intValue(Mathf.FloorToInt(animatedParameter.Evaluate((float)num)));
												continue;
											case 1:
												iterator.set_boolValue(animatedParameter.Evaluate((float)num) > 0f);
												continue;
											case 2:
												iterator.set_floatValue(animatedParameter.Evaluate((float)num));
												continue;
											case 3:
											case 5:
											case 6:
											case 7:
												IL_18C:
												if (propertyType != 17)
												{
													continue;
												}
												goto IL_222;
											case 4:
												ClipInspector.SetAnimatedValue(clip, iterator, "r", num);
												ClipInspector.SetAnimatedValue(clip, iterator, "g", num);
												ClipInspector.SetAnimatedValue(clip, iterator, "b", num);
												ClipInspector.SetAnimatedValue(clip, iterator, "a", num);
												continue;
											case 8:
												goto IL_248;
											case 9:
												goto IL_235;
											case 10:
												goto IL_222;
											}
											goto IL_18C;
											continue;
											IL_248:
											ClipInspector.SetAnimatedValue(clip, iterator, "x", num);
											ClipInspector.SetAnimatedValue(clip, iterator, "y", num);
											continue;
											IL_235:
											ClipInspector.SetAnimatedValue(clip, iterator, "z", num);
											goto IL_248;
											IL_222:
											ClipInspector.SetAnimatedValue(clip, iterator, "w", num);
											goto IL_235;
										}
									}
									selectedItem.playableAssetObject.ApplyModifiedPropertiesWithoutUndo();
								}
							}
						}
					}
				}
			}
		}

		private static void SetAnimatedValue(TimelineClip clip, SerializedProperty property, string path, double localTime)
		{
			SerializedProperty serializedProperty = property.FindPropertyRelative(path);
			if (serializedProperty != null)
			{
				AnimationCurve animatedParameter = clip.GetAnimatedParameter(serializedProperty.get_propertyPath());
				if (animatedParameter != null)
				{
					serializedProperty.set_floatValue(animatedParameter.Evaluate((float)localTime));
				}
			}
		}

		private void ApplyModifiedProperties()
		{
			if (base.get_serializedObject().ApplyModifiedProperties())
			{
				Object[] targetObjects = base.get_serializedObject().get_targetObjects();
				for (int i = 0; i < targetObjects.Length; i++)
				{
					Object @object = targetObjects[i];
					EditorClip editorClip = @object as EditorClip;
					if (editorClip != null && editorClip.clip != null && editorClip.clip.parentTrack != null)
					{
						EditorUtility.SetDirty(editorClip.clip.parentTrack);
					}
				}
			}
		}

		private string PlayableAssetSectionTitle()
		{
			Object @object = (!this.m_SelectionCache.Any<ClipInspector.EditorClipSelection>()) ? null : this.m_SelectionCache.First<ClipInspector.EditorClipSelection>().clip.asset;
			return (!(@object != null)) ? string.Empty : ObjectNames.NicifyVariableName(@object.GetType().Name);
		}

		private bool IsTimelineAssetValidForEditorClip(EditorClip editorClip)
		{
			TimelineAsset timelineAsset = editorClip.clip.parentTrack.timelineAsset;
			bool result;
			if (this.m_TimelineAsset == null)
			{
				this.m_TimelineAsset = timelineAsset;
			}
			else if (timelineAsset != this.m_TimelineAsset)
			{
				this.m_TimelineAsset = null;
				result = false;
				return result;
			}
			result = true;
			return result;
		}

		private ClipInspector.SelectionInfo BuildSelectionInfo()
		{
			ClipInspector.SelectionInfo result = new ClipInspector.SelectionInfo
			{
				supportsBlending = true,
				supportsClipIn = true,
				supportsExtrapolation = true,
				supportsSpeedMultiplier = true,
				hasBlendIn = true,
				hasBlendOut = true,
				selectedAssetTypesAreHomogeneous = true
			};
			HashSet<TrackAsset> hashSet = new HashSet<TrackAsset>();
			Object @object = (!this.m_SelectionCache.Any<ClipInspector.EditorClipSelection>()) ? null : this.m_SelectionCache.First<ClipInspector.EditorClipSelection>().clip.asset;
			Type type = (!(@object != null)) ? null : @object.GetType();
			foreach (ClipInspector.EditorClipSelection current in this.m_SelectionCache)
			{
				TimelineClip clip = current.clip;
				result.supportsBlending &= clip.SupportsBlending();
				result.supportsClipIn &= clip.SupportsClipIn();
				result.supportsExtrapolation &= clip.SupportsExtrapolation();
				result.supportsSpeedMultiplier &= clip.SupportsSpeedMultiplier();
				result.hasBlendIn &= clip.hasBlendIn;
				result.hasBlendOut &= clip.hasBlendOut;
				result.selectedAssetTypesAreHomogeneous &= (clip.asset.GetType() == type);
				hashSet.Add(clip.parentTrack);
			}
			result.selectionContainsAtLeastTwoClipsOnTheSameTrack = (hashSet.Count != this.m_SelectionCache.Count<ClipInspector.EditorClipSelection>());
			return result;
		}

		private bool CanShowPlayableAssetInspector()
		{
			return !this.hasMultipleSelection || (this.m_SelectedPlayableAssetsInspector != null && this.m_SelectedPlayableAssetsInspector.get_canEditMultipleObjects() && this.m_SelectionInfo.selectedAssetTypesAreHomogeneous);
		}

		private void DrawStartTimeProperty()
		{
			TimelineInspectorUtility.TimeField(this.m_StartProperty, ClipInspector.Styles.StartName, false, (double)this.currentFrameRate, 0.0, TimelineClip.kMaxTimeValue);
		}

		private void DrawDurationProperty()
		{
			double minValue = 0.033333333333333333;
			if (this.currentFrameRate > 1.401298E-45f)
			{
				minValue = 1.0 / (double)this.currentFrameRate;
			}
			TimelineInspectorUtility.TimeField(this.m_DurationProperty, ClipInspector.Styles.DurationName, false, (double)this.currentFrameRate, minValue, TimelineClip.kMaxTimeValue);
		}

		private void DrawBlendingProperties()
		{
			bool hasBlendIn = this.m_SelectionInfo.hasBlendIn;
			double num = this.m_SelectionCache.Min((ClipInspector.EditorClipSelection e) => e.clip.duration);
			double maxValue = (!hasBlendIn) ? (num * 0.49) : TimelineClip.kMaxTimeValue;
			TimelineInspectorUtility.TimeField((!hasBlendIn) ? this.m_EaseInDurationProperty : this.m_BlendInDurationProperty, ClipInspector.Styles.EaseInDurationName, hasBlendIn, (double)this.currentFrameRate, 0.0, maxValue);
			bool hasBlendOut = this.m_SelectionInfo.hasBlendOut;
			maxValue = ((!hasBlendOut) ? (num * 0.49) : TimelineClip.kMaxTimeValue);
			TimelineInspectorUtility.TimeField((!hasBlendOut) ? this.m_EaseOutDurationProperty : this.m_BlendOutDurationProperty, ClipInspector.Styles.EaseOutDurationName, hasBlendOut, (double)this.currentFrameRate, 0.0, maxValue);
		}

		private void DrawClipInProperty()
		{
			TimelineInspectorUtility.TimeField(this.m_ClipInProperty, ClipInspector.Styles.ClipInName, false, (double)this.currentFrameRate, 0.0, TimelineClip.kMaxTimeValue);
		}

		private void DrawBlendOptions()
		{
			EditorGUI.BeginChangeCheck();
			this.DrawBlendCurve(ClipInspector.Styles.BlendInCurveName, this.m_BlendInCurveModeProperty, this.m_MixInCurveProperty, delegate(SerializedProperty x)
			{
				this.OnMixCurveSelected("Blend In", BuiltInPresets.blendInPresets, x, true);
			});
			this.DrawBlendCurve(ClipInspector.Styles.BlendOutCurveName, this.m_BlendOutCurveModeProperty, this.m_MixOutCurveProperty, delegate(SerializedProperty x)
			{
				this.OnMixCurveSelected("Blend Out", BuiltInPresets.blendOutPresets, x, false);
			});
			if (EditorGUI.EndChangeCheck())
			{
				TimelineWindow.RepaintIfEditingTimelineAsset(this.m_TimelineAsset);
			}
		}

		private void DrawGroupSelectionProperties()
		{
			EditorGUI.BeginChangeCheck();
			double num = this.m_SelectionCache.Min((ClipInspector.EditorClipSelection earliestEditorClip) => earliestEditorClip.clip.start);
			double num2 = TimelineInspectorUtility.TimeField(ClipInspector.Styles.StartName, num, false, false, (double)this.currentFrameRate, 0.0, TimelineClip.kMaxTimeValue);
			if (EditorGUI.EndChangeCheck())
			{
				double delta = num2 - num;
				this.ShiftSelectedClips(delta);
			}
			EditorGUI.BeginChangeCheck();
			double num3 = this.m_SelectionCache.Max((ClipInspector.EditorClipSelection lastEditorClip) => lastEditorClip.clip.end);
			double minValue = num3 - num;
			double num4 = TimelineInspectorUtility.TimeField(ClipInspector.Styles.EndName, num3, false, false, (double)this.currentFrameRate, minValue, TimelineClip.kMaxTimeValue);
			if (EditorGUI.EndChangeCheck())
			{
				double delta2 = num4 - num3;
				this.ShiftSelectedClips(delta2);
			}
		}

		private void ShiftSelectedClips(double delta)
		{
			foreach (ClipInspector.EditorClipSelection current in this.m_SelectionCache)
			{
				Undo.RegisterCompleteObjectUndo(current.editorClip, ClipInspector.Styles.GroupTimingTitle.get_text());
				current.clip.start += delta;
			}
		}
	}
}
