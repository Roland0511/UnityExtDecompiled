using System;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	internal class DirectorStyles
	{
		public static readonly GUIContent textContentEditWithAnimationEditor = EditorGUIUtility.TextContent("Edit With Animation Editor");

		public static readonly GUIContent recordContent = EditorGUIUtility.IconContent("Animation.Record");

		public static readonly GUIContent addIcon = EditorGUIUtility.IconContent("Toolbar Plus");

		public static readonly GUIContent addFCurve = new GUIContent(EditorGUIUtility.FindTexture("FilterByLabel"), "Add FCurve");

		public static readonly GUIContent soloContent = EditorGUIUtility.TextContent("Solo this track");

		public static readonly GUIContent muteContent = EditorGUIUtility.TextContent("Mute this track");

		public static readonly GUIContent referenceTrackLabel = EditorGUIUtility.TextContent("R|This track references an external asset");

		public static readonly GUIContent recordingLabel = EditorGUIUtility.TextContent("Recording...");

		public static readonly GUIContent sequenceSelectorIcon = EditorGUIUtility.IconContent("TimelineSelector");

		public static readonly GUIContent playContent = EditorGUIUtility.IconContent("Animation.Play", "|Play the timeline (Space).");

		public static readonly GUIContent gotoBeginingContent = EditorGUIUtility.IconContent("Animation.FirstKey", "|Go to the beginning of the timeline. (Shift+<)");

		public static readonly GUIContent gotoEndContent = EditorGUIUtility.IconContent("Animation.LastKey", "|Go to the end of the timeline. (Shift+>)");

		public static readonly GUIContent nextFrameContent = EditorGUIUtility.IconContent("Animation.NextKey", "|Go to the next frame.");

		public static readonly GUIContent previousFrameContent = EditorGUIUtility.IconContent("Animation.PrevKey", "|Go to the previous frame.");

		public static readonly GUIContent noTimelineAssetSelected = EditorGUIUtility.TextContent("To start creating a timeline, select a GameObject.");

		public static readonly GUIContent createTimelineOnSelection = EditorGUIUtility.TextContent("To begin a new timeline with {0}, create {1}.");

		public static readonly GUIContent emptyTimelineMessage = EditorGUIUtility.TextContent("There are no tracks in this timeline.");

		public static readonly GUIContent noTimelinesInScene = EditorGUIUtility.TextContent("No timeline found in the scene.");

		public static readonly GUIContent createNewTimelineText = EditorGUIUtility.TextContent("none");

		public static readonly GUIContent newContent = EditorGUIUtility.TextContent("Add|Add new tracks.");

		public static readonly GUIContent editTimelineAsAsset = EditorGUIUtility.TextContent("Edit Timeline Asset");

		public static readonly GUIContent timelineAssetEditModeTitle = EditorGUIUtility.TextContent("Timeline Playable Asset");

		public static readonly GUIContent previewContent = EditorGUIUtility.TextContent("Preview|Enable/disable scene preview mode.");

		public readonly GUIContent playrangeContent;

		public static readonly float kBaseIndent = 15f;

		public static readonly float kDurationGuiThickness = 5f;

		public static readonly float kDefaultTrackHeight = 30f;

		public GUIStyle handLeft = "MeTransitionHandleLeft";

		public GUIStyle handRight = "MeTransitionHandleRight";

		public GUIStyle groupBackground;

		public GUIStyle displayBackground;

		public GUIStyle fontClip;

		public GUIStyle trackHeaderFont;

		public GUIStyle groupFont;

		public GUIStyle timeCursor;

		public GUIStyle endmarker;

		public GUIStyle tinyFont;

		public GUIStyle foldout;

		public GUIStyle mute;

		public GUIStyle locked;

		public GUIStyle autoKey;

		public GUIStyle playTimeRangeStart;

		public GUIStyle playTimeRangeEnd;

		public GUIStyle options;

		public GUIStyle selectedStyle;

		public GUIStyle trackSwatchStyle;

		public GUIStyle connector;

		public GUIStyle keyframe;

		public GUIStyle warning;

		public GUIStyle extrapolationHold;

		public GUIStyle extrapolationLoop;

		public GUIStyle extrapolationPingPong;

		public GUIStyle extrapolationContinue;

		public GUIStyle eventTrakIcon;

		public GUIStyle eventIcon;

		public GUIStyle eventWhite;

		public GUIStyle outlineBorder;

		public GUIStyle timelineClip;

		public GUIStyle timelineClipSelected;

		public GUIStyle bottomShadow;

		public GUIStyle trackOptions;

		public GUIStyle infiniteTrack;

		public GUIStyle blendingIn;

		public GUIStyle blendingOut;

		public GUIStyle clipOut;

		public GUIStyle clipIn;

		public GUIStyle curves;

		public GUIStyle lockedBG;

		public GUIStyle activation;

		public GUIStyle playrange;

		public GUIStyle lockButton;

		public GUIStyle avatarMaskOn;

		public GUIStyle avatarMaskOff;

		private static DirectorStyles s_Instance;

		private readonly float k_Indent = 10f;

		private DirectorNamedColor m_DarkSkinColors;

		private DirectorNamedColor m_LightSkinColors;

		private DirectorNamedColor m_DefaultSkinColors;

		private static readonly string s_DarkSkinPath = "Editors/TimelineWindow/Timeline_DarkSkin.txt";

		private static readonly string s_LightSkinPath = "Editors/TimelineWindow/Timeline_LightSkin.txt";

		private GUIContent m_TempContent = new GUIContent();

		public static DirectorStyles Instance
		{
			get
			{
				if (DirectorStyles.s_Instance == null)
				{
					DirectorStyles.s_Instance = new DirectorStyles();
					DirectorStyles.s_Instance.Initialize();
				}
				return DirectorStyles.s_Instance;
			}
		}

		public DirectorNamedColor customSkin
		{
			get
			{
				return (!EditorGUIUtility.get_isProSkin()) ? this.m_LightSkinColors : this.m_DarkSkinColors;
			}
			internal set
			{
				if (EditorGUIUtility.get_isProSkin())
				{
					this.m_DarkSkinColors = value;
				}
				else
				{
					this.m_LightSkinColors = value;
				}
			}
		}

		public string cutomSkinContext
		{
			get
			{
				string result;
				if (this.customSkin == this.m_DarkSkinColors)
				{
					result = "Dark Skin";
				}
				else if (this.customSkin == this.m_LightSkinColors)
				{
					result = "Light Skin";
				}
				else
				{
					result = "Default";
				}
				return result;
			}
		}

		public float indentWidth
		{
			get
			{
				return this.k_Indent;
			}
		}

		private DirectorStyles()
		{
			this.handLeft = this.GetStyle("MeTransitionHandleLeft");
			this.handRight = this.GetStyle("MeTransitionHandleRight");
			this.groupBackground = this.GetStyle("groupBackground");
			this.displayBackground = this.GetStyle("sequenceClip");
			this.fontClip = this.GetStyle("Font.Clip");
			this.trackHeaderFont = this.GetStyle("sequenceTrackHeaderFont");
			this.groupFont = this.GetStyle("sequenceGroupFont");
			this.timeCursor = this.GetStyle("Icon.TimeCursor");
			this.endmarker = this.GetStyle("Icon.Endmarker");
			this.tinyFont = this.GetStyle("tinyFont");
			this.foldout = this.GetStyle("Icon.Foldout");
			this.mute = this.GetStyle("Icon.Mute");
			this.locked = this.GetStyle("Icon.Locked");
			this.autoKey = this.GetStyle("Icon.AutoKey");
			this.playTimeRangeStart = this.GetStyle("Icon.PlayAreaStart");
			this.playTimeRangeEnd = this.GetStyle("Icon.PlayAreaEnd");
			this.options = this.GetStyle("Icon.Options");
			this.selectedStyle = this.GetStyle("Color.Selected");
			this.trackSwatchStyle = this.GetStyle("Icon.TrackHeaderSwatch");
			this.connector = this.GetStyle("Icon.Connector");
			this.keyframe = this.GetStyle("Icon.Keyframe");
			this.warning = this.GetStyle("Icon.Warning");
			this.extrapolationHold = this.GetStyle("Icon.ExtrapolationHold");
			this.extrapolationLoop = this.GetStyle("Icon.ExtrapolationLoop");
			this.extrapolationPingPong = this.GetStyle("Icon.ExtrapolationPingPong");
			this.extrapolationContinue = this.GetStyle("Icon.ExtrapolationContinue");
			this.eventTrakIcon = this.GetStyle("Icon.EventTrack");
			this.eventIcon = this.GetStyle("Icon.Event");
			this.eventWhite = this.GetStyle("Icon.EventWhite");
			this.outlineBorder = this.GetStyle("Icon.OutlineBorder");
			this.timelineClip = this.GetStyle("Icon.Clip");
			this.timelineClipSelected = this.GetStyle("Icon.ClipSelected");
			this.bottomShadow = this.GetStyle("Icon.Shadow");
			this.trackOptions = this.GetStyle("Icon.TrackOptions");
			this.infiniteTrack = this.GetStyle("Icon.InfiniteTrack");
			this.blendingIn = this.GetStyle("Icon.BlendingIn");
			this.blendingOut = this.GetStyle("Icon.BlendingOut");
			this.clipOut = this.GetStyle("Icon.ClipOut");
			this.clipIn = this.GetStyle("Icon.ClipIn");
			this.curves = this.GetStyle("Icon.Curves");
			this.lockedBG = this.GetStyle("Icon.LockedBG");
			this.activation = this.GetStyle("Icon.Activation");
			this.playrange = this.GetStyle("Icon.Playrange");
			this.lockButton = this.GetStyle("IN LockButton");
			this.avatarMaskOn = this.GetStyle("Icon.AvatarMaskOn");
			this.avatarMaskOff = this.GetStyle("Icon.AvatarMaskOff");
			this.playrangeContent = new GUIContent(this.playrange.get_normal().get_background());
		}

		private DirectorNamedColor LoadColorSkin(string path)
		{
			TextAsset textAsset = EditorGUIUtility.LoadRequired(path) as TextAsset;
			DirectorNamedColor result;
			if (textAsset != null && !string.IsNullOrEmpty(textAsset.get_text()))
			{
				result = DirectorNamedColor.CreateAndLoadFromText(textAsset.get_text());
			}
			else
			{
				result = this.m_DefaultSkinColors;
			}
			return result;
		}

		private static DirectorNamedColor CreateDefaultSkin()
		{
			DirectorNamedColor directorNamedColor = ScriptableObject.CreateInstance<DirectorNamedColor>();
			directorNamedColor.SetDefault();
			return directorNamedColor;
		}

		public void ExportSkinToFile()
		{
			if (this.customSkin == this.m_DarkSkinColors)
			{
				this.customSkin.ToText(DirectorStyles.s_DarkSkinPath);
			}
			if (this.customSkin == this.m_LightSkinColors)
			{
				this.customSkin.ToText(DirectorStyles.s_LightSkinPath);
			}
		}

		public void ReloadSkin()
		{
			if (this.customSkin == this.m_DarkSkinColors)
			{
				this.m_DarkSkinColors = this.LoadColorSkin(DirectorStyles.s_DarkSkinPath);
			}
			else if (this.customSkin == this.m_LightSkinColors)
			{
				this.m_LightSkinColors = this.LoadColorSkin(DirectorStyles.s_LightSkinPath);
			}
		}

		public void Initialize()
		{
			this.m_DefaultSkinColors = DirectorStyles.CreateDefaultSkin();
			this.m_DarkSkinColors = this.LoadColorSkin(DirectorStyles.s_DarkSkinPath);
			this.m_LightSkinColors = this.LoadColorSkin(DirectorStyles.s_LightSkinPath);
		}

		public GUIStyle GetStyle(string s)
		{
			return new GUIStyle(s);
		}

		public string Elipsify(string label, Rect rect, GUIStyle style)
		{
			string result;
			if (label.Length == 0)
			{
				result = label;
			}
			else
			{
				this.m_TempContent.set_text(label);
				float num = style.CalcSize(this.m_TempContent).x - rect.get_width();
				if (num > 0f)
				{
					float num2 = style.CalcSize(this.m_TempContent).x / (float)label.Length;
					float num3 = rect.get_width() / num2;
					if (num3 - 3f > 0f)
					{
						label = label.Substring(0, (int)num3 - 3);
						label += "...";
					}
					else
					{
						int num4 = (int)Mathf.Floor(num3);
						if (num4 <= 0)
						{
							num4 = 1;
						}
						label = label.Substring(0, num4) + "...";
					}
				}
				result = label;
			}
			return result;
		}
	}
}
