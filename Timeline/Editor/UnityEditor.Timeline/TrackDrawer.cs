using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class TrackDrawer : GUIDrawer
	{
		private static class Styles
		{
			public static readonly GUIContent newMarker = EditorGUIUtility.TextContent("Create New Marker");

			public static readonly GUIContent addMarker = EditorGUIUtility.TextContent("Add Existing Marker/");
		}

		public struct ClipDrawData
		{
			public TimelineClip clip;

			public Rect targetRect;

			public Rect unclippedRect;

			public Rect clippedRect;

			public Rect clipCenterSection;

			public string title;

			public bool selected;

			public bool inlineCurvesSelected;

			public GUIStyle style;

			public ITimelineState state;

			public GUIStyle selectedStyle;

			public double localVisibleStartTime;

			public double localVisibleEndTime;

			internal TimelineClipGUI uiClip;
		}

		public struct MarkerDrawData
		{
			public TimelineMarker timelineMarker;

			public Rect targetRect;

			public Rect unclippedRect;

			public bool selected;

			public bool runInEditor;

			public bool off;

			public GUIStyle style;

			public ITimelineState state;

			public GUIStyle selectedStyle;

			internal TimelineMarkerGUI uiMarker;
		}

		public class TrackMenuContext
		{
			public enum ClipTimeCreation
			{
				TimeCursor,
				Mouse
			}

			public TrackDrawer.TrackMenuContext.ClipTimeCreation clipTimeCreation = TrackDrawer.TrackMenuContext.ClipTimeCreation.TimeCursor;

			public Vector2 mousePosition = Vector2.get_zero();
		}

		protected static readonly GUIContent k_AddClipContent = EditorGUIUtility.TextContent("Add From ");

		protected static readonly GUIContent k_CreateCipContent = EditorGUIUtility.TextContent("Create Clip/");

		private static readonly string k_HoldText = LocalizationDatabase.GetLocalizedString("Hold");

		private static readonly float k_ClipColoredLineThickness = 3f;

		private static float k_MinClipWidth = 7f;

		private static GUIContent s_TitleContent = new GUIContent();

		private static Dictionary<int, string> s_LoopStringCache = new Dictionary<int, string>(100);

		public float DefaultTrackHeight = -1f;

		public TrackDrawer.TrackMenuContext trackMenuContext = new TrackDrawer.TrackMenuContext();

		private TrackColorAttribute m_ColorAttribute;

		private bool m_HasCheckForColorAttribute;

		private GUIContent m_IconGizmosContent;

		private bool m_HasCheckForIconInGizmosFolder;

		internal ITimelineState sequencerState
		{
			get;
			set;
		}

		public TrackAsset track
		{
			get;
			set;
		}

		public virtual Color trackColor
		{
			get
			{
				if (!this.m_HasCheckForColorAttribute)
				{
					object[] customAttributes = this.track.GetType().GetCustomAttributes(typeof(TrackColorAttribute), true);
					if (customAttributes.Length > 0)
					{
						this.m_ColorAttribute = (customAttributes[0] as TrackColorAttribute);
					}
					this.m_HasCheckForColorAttribute = true;
				}
				Color result;
				if (this.m_ColorAttribute != null)
				{
					result = this.m_ColorAttribute.color;
				}
				else
				{
					result = DirectorStyles.Instance.customSkin.colorDefaultTrackDrawer;
				}
				return result;
			}
		}

		public virtual bool canDrawExtrapolationIcon
		{
			get
			{
				return true;
			}
		}

		private DirectorStyles styles
		{
			get
			{
				return DirectorStyles.Instance;
			}
		}

		public static TrackDrawer CreateInstance(TrackAsset trackAsset)
		{
			TrackDrawer result;
			if (trackAsset == null)
			{
				result = Activator.CreateInstance<TrackDrawer>();
			}
			else
			{
				TrackDrawer trackDrawer = null;
				try
				{
					trackDrawer = (TrackDrawer)Activator.CreateInstance(TimelineHelpers.GetCustomDrawer(trackAsset.GetType()));
				}
				catch (Exception)
				{
					trackDrawer = Activator.CreateInstance<TrackDrawer>();
				}
				result = trackDrawer;
			}
			return result;
		}

		public virtual Color GetTrackBackgroundColor(TrackAsset trackAsset)
		{
			return DirectorStyles.Instance.customSkin.colorTrackBackground;
		}

		public virtual bool DrawTrackHeaderButton(Rect rect, TrackAsset track, ITimelineState state)
		{
			return false;
		}

		public virtual float GetHeight(TrackAsset t)
		{
			return this.DefaultTrackHeight;
		}

		public virtual GUIContent GetIcon()
		{
			if (!this.m_HasCheckForIconInGizmosFolder)
			{
				Texture2D texture2D = TrackDrawer.LoadIconInGizmosFolder(this.track.GetType().Name);
				if (texture2D != null)
				{
					this.m_IconGizmosContent = new GUIContent(texture2D);
				}
				this.m_HasCheckForIconInGizmosFolder = true;
			}
			GUIContent result;
			if (this.m_IconGizmosContent != null)
			{
				result = this.m_IconGizmosContent;
			}
			else
			{
				result = EditorGUIUtility.IconContent("ScriptableObject Icon");
			}
			return result;
		}

		private static Texture2D LoadIconInGizmosFolder(string filename)
		{
			string text = "Assets/Gizmos/" + filename + ".png";
			return AssetDatabase.LoadAssetAtPath<Texture2D>(text);
		}

		protected void AddCreateAssetMenuItem(GenericMenu menu, Type assetType, TrackAsset track, ITimelineState state)
		{
			if (!assetType.IsAbstract)
			{
				menu.AddItem(EditorGUIUtility.TextContent("Create " + ObjectNames.NicifyVariableName(assetType.Name) + " Clip"), false, delegate(object typeOfClip)
				{
					if (this.trackMenuContext.clipTimeCreation == TrackDrawer.TrackMenuContext.ClipTimeCreation.Mouse)
					{
						TimelineHelpers.CreateClipOnTrack(typeOfClip as Type, track, state, this.trackMenuContext.mousePosition);
					}
					else
					{
						TimelineHelpers.CreateClipOnTrack(typeOfClip as Type, track, state);
					}
					this.trackMenuContext.clipTimeCreation = TrackDrawer.TrackMenuContext.ClipTimeCreation.TimeCursor;
				}, assetType);
			}
		}

		protected void AddAddAssetMenuItem(GenericMenu menu, Type assetType, TrackAsset track, ITimelineState state)
		{
			if (!assetType.IsAbstract)
			{
				menu.AddItem(EditorGUIUtility.TextContent(TrackDrawer.k_AddClipContent.get_text() + " " + ObjectNames.NicifyVariableName(assetType.Name)), false, delegate(object typeOfClip)
				{
					TrackDrawer.AddAssetOnTrack(typeOfClip as Type, track, state);
				}, assetType);
			}
		}

		private static void AddAssetOnTrack(Type typeOfClip, TrackAsset track, ITimelineState state)
		{
			state.AddStartFrameDelegate(delegate(ITimelineState istate, Event currentEvent)
			{
				ObjectSelector.get_get().Show(null, typeOfClip, null, false);
				ObjectSelector.get_get().objectSelectorID = 0;
				ObjectSelector.get_get().set_searchFilter("");
				return true;
			});
			state.AddStartFrameDelegate(delegate(ITimelineState istate, Event currentEvent)
			{
				bool result;
				if (currentEvent.get_commandName() == "ObjectSelectorClosed")
				{
					AnimationTrack animationTrack = track as AnimationTrack;
					if (animationTrack && !animationTrack.inClipMode)
					{
						animationTrack.ConvertToClipMode();
					}
					TimelineClip timelineClip = TimelineHelpers.CreateClipOnTrack(EditorGUIUtility.GetObjectPickerObject(), track, istate, TimelineHelpers.sInvalidMousePosition);
					if (timelineClip != null && timelineClip.asset != null)
					{
						TimelineCreateUtilities.SaveAssetIntoObject(timelineClip.asset, track);
					}
					result = true;
				}
				else
				{
					result = false;
				}
				return result;
			});
		}

		internal static string GetDisplayName(Type t)
		{
			string text = "";
			string str = ObjectNames.NicifyVariableName(t.Name);
			object[] customAttributes = t.GetCustomAttributes(true);
			for (int i = 0; i < customAttributes.Length; i++)
			{
				object obj = customAttributes[i];
				if (obj is CategoryAttribute)
				{
					CategoryAttribute categoryAttribute = obj as CategoryAttribute;
					text = categoryAttribute.Category;
				}
				else if (obj is DisplayNameAttribute)
				{
					DisplayNameAttribute displayNameAttribute = obj as DisplayNameAttribute;
					str = displayNameAttribute.DisplayName;
				}
			}
			if (text.Length > 0 && text[text.Length - 1] != '/')
			{
				text += '/';
			}
			return text + str;
		}

		public virtual void OnBuildTrackContextMenu(GenericMenu menu, TrackAsset trackAsset, ITimelineState state)
		{
			bool flag = trackAsset is AnimationTrack || trackAsset is AudioTrack;
			if (flag)
			{
				List<Type> list = TimelineHelpers.GetTypesHandledByTrackType(TimelineHelpers.TrackTypeFromType(trackAsset.GetType())).ToList<Type>();
				for (int i = 0; i < list.Count; i++)
				{
					Type assetType = list[i];
					this.AddAddAssetMenuItem(menu, assetType, trackAsset, state);
				}
			}
			else if (TimelineHelpers.GetMediaTypeFromType(trackAsset.GetType()) == TimelineAsset.MediaType.Script)
			{
				Type customPlayableType = trackAsset.GetCustomPlayableType();
				if (customPlayableType != null)
				{
					string displayName = TrackDrawer.GetDisplayName(customPlayableType);
					GUIContent gUIContent = new GUIContent("Add " + displayName + " Clip");
					menu.AddItem(new GUIContent(gUIContent), false, delegate(object userData)
					{
						TimelineHelpers.CreateClipOnTrack(userData as Type, trackAsset, state);
					}, customPlayableType);
				}
			}
			ITimelineMarkerContainer markerContainer = trackAsset as ITimelineMarkerContainer;
			if (markerContainer != null)
			{
				menu.AddItem(TrackDrawer.Styles.newMarker, false, delegate
				{
					this.CreateNewMarker(markerContainer, state);
				});
				IEnumerable<string> enumerable = (from x in markerContainer.GetMarkers()
				select x.key).Distinct<string>();
				if (enumerable.Any<string>())
				{
					using (IEnumerator<string> enumerator = enumerable.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							string key = enumerator.Current;
							menu.AddItem(new GUIContent(TrackDrawer.Styles.addMarker.get_text() + key), false, delegate
							{
								this.CreateExistingMarker(key, markerContainer, state);
							});
						}
					}
				}
			}
		}

		private void CreateNewMarker(ITimelineMarkerContainer container, ITimelineState state)
		{
			string uniqueName = ObjectNames.GetUniqueName((from x in container.GetMarkers()
			select x.key).ToArray<string>(), "New Marker");
			this.CreateExistingMarker(uniqueName, container, state);
		}

		private void CreateExistingMarker(string key, ITimelineMarkerContainer container, ITimelineState state)
		{
			Vector3 mousePosition = (this.trackMenuContext.clipTimeCreation != TrackDrawer.TrackMenuContext.ClipTimeCreation.Mouse) ? TimelineHelpers.sInvalidMousePosition : this.trackMenuContext.mousePosition;
			double time = TrackDrawer.CalculateMarkerTimeForMousePosition(container as TrackAsset, state, mousePosition);
			TimelineUndo.PushUndo(container as Object, "Create Marker");
			TimelineMarker newEvent = container.CreateMarker(key, time);
			TrackDrawer.SelectMarkerInInspector(state, newEvent);
			state.Refresh();
		}

		private static double CalculateMarkerTimeForMousePosition(TrackAsset trackAsset, ITimelineState state, Vector3 mousePosition)
		{
			double time = 0.0;
			if (!float.IsPositiveInfinity(mousePosition.x) && !float.IsPositiveInfinity(mousePosition.y))
			{
				time = (double)state.ScreenSpacePixelToTimeAreaTime(mousePosition.x);
			}
			else if (trackAsset != null)
			{
				time = trackAsset.end + 0.5;
			}
			return Math.Max(0.0, state.SnapToFrameIfRequired(time));
		}

		private static void SelectMarkerInInspector(ITimelineState state, TimelineMarker newEvent)
		{
			EditorWindow.FocusWindowIfItsOpen<InspectorWindow>();
			SelectionManager.Clear();
			newEvent.selected = true;
			Object[] objects = new Object[]
			{
				EditorItemFactory.GetEditorMarker(newEvent)
			};
			Selection.set_objects(objects);
		}

		public virtual void OnBuildClipContextMenu(GenericMenu menu, TimelineClip[] clips, ITimelineState state)
		{
		}

		public virtual bool DrawTrack(Rect trackRect, TrackAsset trackAsset, Vector2 visibleTime, ITimelineState state)
		{
			return false;
		}

		protected void DrawClipErrorIcon(TrackDrawer.ClipDrawData clip, GUIContent content)
		{
			Rect targetRect = clip.targetRect;
			float num = Mathf.Min(targetRect.get_height() - 4f, (float)content.get_image().get_height());
			Rect rect = new Rect(targetRect.get_xMax() - 2f - (float)content.get_image().get_width(), targetRect.get_y() + (targetRect.get_height() - num) * 0.5f, (float)content.get_image().get_width(), num);
			GUI.Label(rect, content);
		}

		private bool HasErrors(TrackDrawer.ClipDrawData drawData)
		{
			return drawData.clip.asset == null;
		}

		private static void DrawLoops(TrackDrawer.ClipDrawData drawData, Rect containerRect)
		{
			if (drawData.selected || drawData.inlineCurvesSelected)
			{
				Color color = GUI.get_color();
				GUI.set_color(Color.get_white());
				int num = drawData.uiClip.minLoopIndex;
				for (int i = 0; i < drawData.uiClip.loopRects.Count; i++)
				{
					Rect rect = drawData.uiClip.loopRects[i];
					rect.set_x(rect.get_x() - drawData.unclippedRect.get_x());
					rect.set_x(rect.get_x() + 1f);
					rect.set_width(rect.get_width() - 2f);
					rect.set_y(5f);
					rect.set_height(rect.get_height() - 4f);
					rect.set_xMin(rect.get_xMin() - 4f);
					if (rect.get_width() >= 10f)
					{
						GUI.set_color(new Color(0f, 0f, 0f, 0.2f));
						GUI.Box(rect, GUIContent.none, DirectorStyles.Instance.displayBackground);
					}
					if (rect.get_width() > 30f)
					{
						GUI.set_color(Color.get_white());
						Graphics.ShadowLabel(rect, (!drawData.uiClip.supportsLooping) ? TrackDrawer.k_HoldText : TrackDrawer.GetLoopString(num), DirectorStyles.Instance.fontClip, Color.get_white(), Color.get_black());
					}
					num++;
					if (!drawData.uiClip.supportsLooping)
					{
						break;
					}
				}
				GUI.set_color(color);
			}
		}

		private void DrawClipBody(TrackDrawer.ClipDrawData drawData, bool drawCustomBody)
		{
			DirectorStyles instance = DirectorStyles.Instance;
			Color colorClipHighlight = instance.customSkin.colorClipHighlight;
			Color colorClipShadow = instance.customSkin.colorClipShadow;
			GUIStyle gUIStyle = instance.timelineClip;
			if (drawData.selected)
			{
				gUIStyle = instance.timelineClipSelected;
			}
			GUI.Box(drawData.clipCenterSection, GUIContent.none, gUIStyle);
			if (drawCustomBody)
			{
				Rect clippedRect = drawData.clippedRect;
				clippedRect.set_yMin(clippedRect.get_yMin() + 2f);
				clippedRect.set_yMax(clippedRect.get_yMax() - TrackDrawer.k_ClipColoredLineThickness);
				this.DrawCustomClipBody(drawData, clippedRect);
			}
			Rect targetRect = drawData.targetRect;
			targetRect.set_yMin(targetRect.get_yMax() - TrackDrawer.k_ClipColoredLineThickness);
			Color clipBaseColor = this.GetClipBaseColor(drawData.clip);
			EditorGUI.DrawRect(targetRect, clipBaseColor);
			EditorGUI.DrawRect(new Rect(drawData.targetRect.get_xMin(), drawData.targetRect.get_yMin(), drawData.targetRect.get_width() - 2f, 2f), colorClipHighlight);
			EditorGUI.DrawRect(new Rect(drawData.targetRect.get_xMin(), drawData.targetRect.get_yMin() + 2f, 2f, drawData.targetRect.get_height()), colorClipHighlight);
			EditorGUI.DrawRect(new Rect(drawData.targetRect.get_xMax() - 2f, drawData.targetRect.get_yMin(), 2f, drawData.targetRect.get_height()), colorClipShadow);
			EditorGUI.DrawRect(new Rect(drawData.targetRect.get_xMin(), drawData.targetRect.get_yMax() - 2f, drawData.targetRect.get_width(), 2f), colorClipShadow);
		}

		private static void DrawBorder(TrackDrawer.ClipDrawData drawData, Color color)
		{
			Rect clipCenterSection = drawData.clipCenterSection;
			EditorGUI.DrawRect(new Rect(clipCenterSection.get_xMin(), clipCenterSection.get_yMin(), clipCenterSection.get_width(), 2f), color);
			EditorGUI.DrawRect(new Rect(clipCenterSection.get_xMin(), clipCenterSection.get_yMax() - 2f, clipCenterSection.get_width(), 2f), color);
			if (drawData.uiClip.mixInRect.get_width() < 1f)
			{
				EditorGUI.DrawRect(new Rect(clipCenterSection.get_xMin(), clipCenterSection.get_yMin(), 2f, clipCenterSection.get_height()), color);
			}
			if (drawData.uiClip.mixOutRect.get_width() < 1f)
			{
				EditorGUI.DrawRect(new Rect(clipCenterSection.get_xMax() - 2f, clipCenterSection.get_yMin(), 2f, clipCenterSection.get_height()), color);
			}
		}

		private static void DrawClipRecorded(TrackDrawer.ClipDrawData drawData)
		{
			if (drawData.state.recording && drawData.clip.recordable && drawData.clip.parentTrack.IsRecordingToClip(drawData.clip))
			{
				TrackDrawer.DrawBorder(drawData, DirectorStyles.Instance.customSkin.colorRecordingClipOutline);
			}
		}

		private static void DrawClipSelected(TrackDrawer.ClipDrawData drawData)
		{
			if (SelectionManager.Contains(drawData.uiClip.clip))
			{
				Rect rect = drawData.clipCenterSection;
				TrackDrawer.DrawBorder(drawData, Color.get_white());
				if (drawData.uiClip.blendInKind == TimelineClipGUI.BlendKind.Ease)
				{
					rect = drawData.uiClip.mixInRect;
					rect.set_position(Vector2.get_zero());
					EditorGUI.DrawRect(new Rect(rect.get_xMin(), rect.get_yMax() - 2f, rect.get_width(), 2f), Color.get_white());
				}
				if (drawData.uiClip.blendInKind == TimelineClipGUI.BlendKind.Mix)
				{
					rect = drawData.uiClip.mixInRect;
					rect.set_position(Vector2.get_zero());
					EditorGUI.DrawRect(new Rect(rect.get_xMin(), rect.get_yMin(), rect.get_width(), 2f), Color.get_white());
					Graphics.DrawLineAA(4f, new Vector3(rect.get_xMin(), rect.get_yMin(), 0f), new Vector3(rect.get_xMax(), rect.get_yMax() - 1f, 0f), Color.get_white());
					if (drawData.uiClip.previousClip != null && SelectionManager.Contains(drawData.uiClip.previousClip.clip))
					{
						EditorGUI.DrawRect(new Rect(rect.get_xMin(), rect.get_yMax() - 2f, rect.get_width(), 2f), Color.get_white());
						EditorGUI.DrawRect(new Rect(rect.get_xMax() - 2f, rect.get_yMin(), 2f, rect.get_height()), Color.get_white());
						EditorGUI.DrawRect(new Rect(rect.get_xMin(), rect.get_yMin(), 2f, rect.get_height()), Color.get_white());
					}
				}
				if (drawData.uiClip.blendOutKind == TimelineClipGUI.BlendKind.Ease || drawData.uiClip.blendOutKind == TimelineClipGUI.BlendKind.Mix)
				{
					rect = drawData.uiClip.mixOutRect;
					rect.set_x(drawData.targetRect.get_xMax() - rect.get_width());
					rect.set_y(0f);
					EditorGUI.DrawRect(new Rect(rect.get_xMin(), rect.get_yMax() - 2f, rect.get_width(), 2f), Color.get_white());
					Graphics.DrawLineAA(4f, new Vector3(rect.get_xMin(), rect.get_yMin(), 0f), new Vector3(rect.get_xMax(), rect.get_yMax() - 1f, 0f), Color.get_white());
				}
			}
		}

		private static void DrawClipTimescale(TrackDrawer.ClipDrawData drawData)
		{
			if (drawData.clip.timeScale != 1.0)
			{
				float num = 4f;
				float num2 = 6f;
				float segmentsLength = (drawData.clip.timeScale <= 1.0) ? 15f : 5f;
				Vector3 vector = new Vector3(drawData.targetRect.get_min().x + num, drawData.targetRect.get_min().y + num2, 0f);
				Vector3 vector2 = new Vector3(drawData.targetRect.get_max().x - num, drawData.targetRect.get_min().y + num2, 0f);
				Graphics.DrawDottedLine(vector, vector2, segmentsLength, DirectorStyles.Instance.customSkin.colorClipFont);
				Graphics.DrawDottedLine(vector + new Vector3(0f, 1f, 0f), vector2 + new Vector3(0f, 1f, 0f), segmentsLength, DirectorStyles.Instance.customSkin.colorClipFont);
			}
		}

		private static void DrawClipInOut(TrackDrawer.ClipDrawData drawData)
		{
			if (drawData.clip.duration > drawData.clip.clipAssetDuration)
			{
				GUIStyle clipOut = DirectorStyles.Instance.clipOut;
				Rect targetRect = drawData.targetRect;
				targetRect.set_xMin(targetRect.get_xMax() - clipOut.get_fixedWidth() - 2f);
				targetRect.set_width(clipOut.get_fixedWidth());
				targetRect.set_yMin(targetRect.get_yMin() + (targetRect.get_height() - clipOut.get_fixedHeight()) / 2f);
				targetRect.set_height(clipOut.get_fixedHeight());
				GUI.Box(targetRect, GUIContent.none, clipOut);
			}
			if (drawData.clip.clipIn > 0.0)
			{
				GUIStyle clipIn = DirectorStyles.Instance.clipIn;
				Rect targetRect2 = drawData.targetRect;
				targetRect2.set_xMin(clipIn.get_fixedWidth());
				targetRect2.set_width(clipIn.get_fixedWidth());
				targetRect2.set_yMin(targetRect2.get_yMin() + (targetRect2.get_height() - clipIn.get_fixedHeight()) / 2f);
				targetRect2.set_height(clipIn.get_fixedHeight());
				GUI.Box(targetRect2, GUIContent.none, clipIn);
			}
		}

		private void DrawClipText(string text, Rect centerRect, TextAlignment alignment)
		{
			TrackDrawer.s_TitleContent.set_text(text);
			if (DirectorStyles.Instance.fontClip.CalcSize(TrackDrawer.s_TitleContent).x > centerRect.get_width())
			{
				TrackDrawer.s_TitleContent.set_text(DirectorStyles.Instance.Elipsify(TrackDrawer.s_TitleContent.get_text(), centerRect, DirectorStyles.Instance.fontClip));
			}
			TextAnchor alignment2 = DirectorStyles.Instance.fontClip.get_alignment();
			if (alignment != null)
			{
				if (alignment != 2)
				{
					if (alignment == 1)
					{
						DirectorStyles.Instance.fontClip.set_alignment(4);
					}
				}
				else
				{
					DirectorStyles.Instance.fontClip.set_alignment(5);
				}
			}
			else
			{
				DirectorStyles.Instance.fontClip.set_alignment(3);
			}
			Graphics.ShadowLabel(centerRect, TrackDrawer.s_TitleContent, DirectorStyles.Instance.fontClip, Color.get_white(), Color.get_black());
			DirectorStyles.Instance.fontClip.set_alignment(alignment2);
		}

		public static Color GetHighlightColor(Color clipColor)
		{
			float num;
			float num2;
			float num3;
			Color.RGBToHSV(clipColor, ref num, ref num2, ref num3);
			num3 *= 1.3f;
			return Color.HSVToRGB(num, num2, num3).get_gamma();
		}

		protected virtual void DrawCustomClipBody(TrackDrawer.ClipDrawData drawData, Rect centerRect)
		{
		}

		private void DrawDefaultClip(TrackDrawer.ClipDrawData drawData)
		{
			if (drawData.targetRect.get_width() < TrackDrawer.k_MinClipWidth)
			{
				drawData.targetRect.set_width(TrackDrawer.k_MinClipWidth);
				drawData.clipCenterSection.set_width(TrackDrawer.k_MinClipWidth);
				this.DrawClipBody(drawData, false);
				TrackDrawer.DrawClipSelected(drawData);
				this.DrawClipText(drawData.title, drawData.targetRect, 1);
			}
			else
			{
				this.DrawClipBody(drawData, true);
				TrackDrawer.DrawClipSelected(drawData);
				TrackDrawer.DrawClipRecorded(drawData);
				TrackDrawer.DrawClipTimescale(drawData);
				TrackDrawer.DrawClipInOut(drawData);
				Rect clipCenterSection = drawData.clipCenterSection;
				if (clipCenterSection.get_width() < 20f)
				{
					if (drawData.targetRect.get_width() > 20f)
					{
						this.DrawClipText(drawData.title, clipCenterSection, 1);
					}
				}
				else
				{
					TrackDrawer.DrawLoops(drawData, clipCenterSection);
					this.DrawClipText(drawData.title, clipCenterSection, 1);
				}
			}
		}

		private static void DrawDefaultEvent(TrackDrawer.MarkerDrawData drawData)
		{
			Color color = GUI.get_color();
			if (drawData.selected)
			{
				GUI.set_color(DirectorStyles.Instance.customSkin.colorEventSelected);
			}
			else if (drawData.runInEditor)
			{
				GUI.set_color(DirectorStyles.Instance.customSkin.colorEventRunInEditor);
			}
			else if (drawData.off)
			{
				GUI.set_color(DirectorStyles.Instance.customSkin.colorEventOff);
			}
			else
			{
				GUI.set_color(DirectorStyles.Instance.customSkin.colorEventNormal);
			}
			GUI.Box(drawData.targetRect, new GUIContent(string.Empty, drawData.timelineMarker.key), drawData.style);
			GUI.set_color(color);
		}

		public virtual Color GetClipBaseColor(TimelineClip clip)
		{
			return this.trackColor;
		}

		public Color GetClipSelectedColor(TimelineClip clip)
		{
			Color clipBaseColor = this.GetClipBaseColor(clip);
			float num;
			float num2;
			float num3;
			Color.RGBToHSV(clipBaseColor, ref num, ref num2, ref num3);
			num3 *= 1.3f;
			return Color.HSVToRGB(num, num2, num3);
		}

		public virtual void DrawClip(TrackDrawer.ClipDrawData drawData)
		{
			this.DrawDefaultClip(drawData);
		}

		public virtual void DrawEvent(TrackDrawer.MarkerDrawData drawData)
		{
			TrackDrawer.DrawDefaultEvent(drawData);
		}

		public virtual string GetCustomTitle(TrackAsset track)
		{
			return string.Empty;
		}

		internal virtual void ConfigureUIClip(TimelineClipGUI uiClip)
		{
			uiClip.AddManipulator(new MoveClip());
			uiClip.AddManipulator(new ClipContextMenu());
			uiClip.AddManipulator(new ClipActionsShortcutManipulator());
			uiClip.AddManipulator(new DrillIntoClip());
		}

		internal virtual void ConfigureUIEvent(TimelineMarkerGUI uiMarker)
		{
			uiMarker.AddManipulator(new MoveEvent());
			uiMarker.AddManipulator(new EventContextMenu());
		}

		internal virtual void ConfigureUITrack(TimelineTrackBaseGUI uiTrack)
		{
			uiTrack.ClearManipulators();
			uiTrack.AddManipulator(new SelectorTool());
			uiTrack.AddManipulator(new TrackContextMenuManipulator());
			uiTrack.AddManipulator(new TrackDoubleClick());
			uiTrack.AddManipulator(new TrackShortcutManipulator());
		}

		private static string GetLoopString(int loopIndex)
		{
			string text = null;
			if (!TrackDrawer.s_LoopStringCache.TryGetValue(loopIndex, out text))
			{
				text = "L" + loopIndex;
				TrackDrawer.s_LoopStringCache[loopIndex] = text;
			}
			return text;
		}
	}
}
