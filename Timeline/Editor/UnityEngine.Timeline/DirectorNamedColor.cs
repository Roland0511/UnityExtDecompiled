using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor.Timeline;

namespace UnityEngine.Timeline
{
	[Serializable]
	internal class DirectorNamedColor : ScriptableObject
	{
		[SerializeField]
		public Color colorPlayhead;

		[SerializeField]
		public Color colorSelection;

		[SerializeField]
		public Color colorEndmarker;

		[SerializeField]
		public Color colorTimelineItem;

		[SerializeField]
		public Color colorGroup;

		[SerializeField]
		public Color colorGroupTrackBackground;

		[SerializeField]
		public Color colorAnimation;

		[SerializeField]
		public Color colorAnimationRecorded;

		[SerializeField]
		public Color colorAudio;

		[SerializeField]
		public Color colorAudioWaveform;

		[SerializeField]
		public Color colorScripting;

		[SerializeField]
		public Color colorVideo;

		[SerializeField]
		public Color colorEvent;

		[SerializeField]
		public Color colorActivation;

		[SerializeField]
		public Color colorDropTarget;

		[SerializeField]
		public Color colorClipFont;

		[SerializeField]
		public Color colorClipBackground;

		[SerializeField]
		public Color colorClipTrimLine;

		[SerializeField]
		public Color colorTrackBackground;

		[SerializeField]
		public Color colorTrackHeaderBackground;

		[SerializeField]
		public Color colorTrackDarken;

		[SerializeField]
		public Color colorTrackBackgroundRecording;

		[SerializeField]
		public Color colorInfiniteTrackBackgroundRecording;

		[SerializeField]
		public Color colorTrackBackgroundSelected;

		[SerializeField]
		public Color colorTrackFont;

		[SerializeField]
		public Color colorCurveSelected;

		[SerializeField]
		public Color colorCurveModeSelection;

		[SerializeField]
		public Color colorClipUnion;

		[SerializeField]
		public Color colorTopOutline1;

		[SerializeField]
		public Color colorTopOutline2;

		[SerializeField]
		public Color colorTopOutline3;

		[SerializeField]
		public Color colorTimecodeBackground;

		[SerializeField]
		public Color colorDurationLine;

		[SerializeField]
		public Color colorRange;

		[SerializeField]
		public Color colorSequenceBackground;

		[SerializeField]
		public Color colorTooltipBackground;

		[SerializeField]
		public Color colorBindingSelectorItemBackground;

		[SerializeField]
		public Color colorInfiniteClipLine;

		[SerializeField]
		public Color colorRectangleSelect;

		[SerializeField]
		public Color colorSnapLine;

		[SerializeField]
		public Color colorDefaultTrackDrawer;

		[SerializeField]
		public Color colorValidDropTarget = new Color(1f, 0.921568632f, 0.0156862754f, 1f);

		[SerializeField]
		public Color colorInvalidDropTarget = new Color(1f, 0.921568632f, 0.0156862754f, 0.3f);

		[SerializeField]
		public Color colorBreadCrumb = new Color(0.294117659f, 0.4392157f, 0.698039234f);

		[SerializeField]
		public Color colorBreadCrumbInactive = new Color(1f, 1f, 0f);

		[SerializeField]
		public Color colorDuration = new Color(0.66f, 0.66f, 0.66f, 1f);

		[SerializeField]
		public Color colorRecordingClipOutline = new Color(1f, 0f, 0f, 0.9f);

		[SerializeField]
		public Color colorAnimEditorBinding = new Color(0.211764708f, 0.211764708f, 0.211764708f);

		[SerializeField]
		public Color colorInifiniteTrack = new Color(0.0392156877f, 0.0392156877f, 0.0392156877f);

		[SerializeField]
		public Color colorTimelineBackground = new Color(0.2f, 0.2f, 0.2f, 1f);

		[SerializeField]
		public Color colorKeyFrame = Color.get_white();

		[SerializeField]
		public Color colorLockTextBG = Color.get_red();

		[SerializeField]
		public Color colorInlineCurveVerticalLines = new Color(1f, 1f, 1f, 0.2f);

		[SerializeField]
		public Color colorInlineCurveOutOfRangeOverlay = new Color(0f, 0f, 0f, 0.5f);

		[SerializeField]
		public Color colorClipHighlight = new Color(1f, 1f, 1f, 0.2f);

		[SerializeField]
		public Color colorClipShadow = new Color(0f, 0f, 0f, 0.2f);

		[SerializeField]
		public Color colorEventNormal = new Color(0.75f, 0.75f, 0.15f, 1f);

		[SerializeField]
		public Color colorEventSelected = new Color(1f, 1f, 1f, 1f);

		[SerializeField]
		public Color colorEventRunInEditor = new Color(0.8f, 0.25f, 0.25f, 1f);

		[SerializeField]
		public Color colorEventOff = new Color(0.5f, 0.5f, 0.15f, 0.75f);

		public void SetDefault()
		{
			this.colorPlayhead = DirectorStyles.Instance.timeCursor.get_normal().get_textColor();
			this.colorSelection = DirectorStyles.Instance.selectedStyle.get_normal().get_textColor();
			this.colorEndmarker = DirectorStyles.Instance.endmarker.get_normal().get_textColor();
			this.colorGroup = new Color(0.094f, 0.357f, 0.384f, 0.31f);
			this.colorGroupTrackBackground = new Color(0f, 0f, 0f, 1f);
			this.colorAnimation = new Color(0.3f, 0.39f, 0.46f, 1f);
			this.colorAnimationRecorded = new Color(this.colorAnimation.r * 0.75f, this.colorAnimation.g * 0.75f, this.colorAnimation.b * 0.75f, 1f);
			this.colorAudio = new Color(1f, 0.635f, 0f);
			this.colorAudioWaveform = new Color(0.129f, 0.164f, 0.254f);
			this.colorScripting = new Color(0.655f, 0.655f, 0.655f);
			this.colorVideo = new Color(0.255f, 0.411f, 0.388f);
			this.colorEvent = DirectorStyles.Instance.eventIcon.get_normal().get_textColor();
			this.colorActivation = Color.get_green();
			this.colorDropTarget = new Color(0.514f, 0.627f, 0.827f);
			this.colorClipFont = DirectorStyles.Instance.fontClip.get_normal().get_textColor();
			this.colorTrackBackground = new Color(0.2f, 0.2f, 0.2f, 1f);
			this.colorTrackBackgroundSelected = new Color(1f, 1f, 1f, 0.33f);
			this.colorTrackFont = DirectorStyles.Instance.trackHeaderFont.get_normal().get_textColor();
			this.colorCurveSelected = new Color(1f, 1f, 1f, 0.6f);
			this.colorCurveModeSelection = new Color(0.447f, 0.447f, 0.447f, 1f);
			this.colorClipUnion = new Color(0.72f, 0.72f, 0.72f, 0.8f);
			this.colorCurveSelected = new Color(1f, 1f, 1f, 0.6f);
			this.colorCurveModeSelection = new Color(0.447f, 0.447f, 0.447f, 1f);
			this.colorClipUnion = new Color(0.72f, 0.72f, 0.72f, 0.8f);
			this.colorTopOutline1 = new Color(0.152f, 0.152f, 0.152f, 1f);
			this.colorTopOutline2 = new Color(0.184f, 0.184f, 0.184f, 1f);
			this.colorTopOutline3 = new Color(0.274f, 0.274f, 0.274f, 1f);
			this.colorTimecodeBackground = new Color(0.219f, 0.219f, 0.219f, 1f);
			this.colorDurationLine = new Color(0.129411772f, 0.427450985f, 0.470588237f);
			this.colorRange = new Color(0.733f, 0.733f, 0.733f, 0.7f);
			this.colorSequenceBackground = new Color(0.16f, 0.16f, 0.16f, 1f);
			this.colorTooltipBackground = new Color(0.113725491f, 0.1254902f, 0.129411772f);
			this.colorBindingSelectorItemBackground = new Color(0.2f, 0.5f, 1f, 0.5f);
			this.colorInfiniteClipLine = new Color(0.282352954f, 0.305882365f, 0.321568638f);
			this.colorRectangleSelect = new Color(1f, 0.6f, 0f, 0.8f);
			this.colorTrackBackgroundRecording = new Color(1f, 0f, 0f, 0.1f);
			this.colorClipTrimLine = new Color(1f, 1f, 1f, 0.5f);
			this.colorClipBackground = new Color(0.1f, 0.1f, 0.2f, 0.2f);
			this.colorTrackDarken = new Color(0f, 0f, 0f, 0.4f);
			this.colorTrackHeaderBackground = new Color(0.2f, 0.2f, 0.2f, 1f);
			this.colorSnapLine = new Color(1f, 0f, 0f, 0.4f);
			this.colorDefaultTrackDrawer = new Color(0.854901969f, 0.8627451f, 0.870588243f);
			this.colorValidDropTarget = Color.get_yellow();
			this.colorInvalidDropTarget = Color.get_yellow();
			this.colorInvalidDropTarget.a = 0.3f;
			this.colorRecordingClipOutline = new Color(1f, 0f, 0f, 0.9f);
			this.colorInlineCurveVerticalLines = new Color(1f, 1f, 1f, 0.2f);
			this.colorInlineCurveOutOfRangeOverlay = new Color(0f, 0f, 0f, 0.5f);
			this.colorEventNormal = new Color(0.8f, 0.25f, 0.25f, 1f);
			this.colorEventSelected = new Color(1f, 1f, 1f, 1f);
			this.colorEventRunInEditor = new Color(0.8f, 0.25f, 0.25f, 1f);
			this.colorEventOff = new Color(0.5f, 0.5f, 0.15f, 0.75f);
		}

		public void ToText(string path)
		{
			StringBuilder stringBuilder = new StringBuilder();
			FieldInfo[] fields = base.GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
			FieldInfo[] array = fields;
			for (int i = 0; i < array.Length; i++)
			{
				FieldInfo fieldInfo = array[i];
				if (fieldInfo.FieldType == typeof(Color))
				{
					Color color = (Color)fieldInfo.GetValue(this);
					stringBuilder.AppendLine(fieldInfo.Name + "," + color);
				}
			}
			string path2 = Application.get_dataPath() + "/Editor Default Resources/" + path;
			File.WriteAllText(path2, stringBuilder.ToString());
		}

		public void FromText(string text)
		{
			string[] array = text.Split(new char[]
			{
				'\n',
				'\r'
			}, StringSplitOptions.RemoveEmptyEntries);
			Dictionary<string, Color> dictionary = new Dictionary<string, Color>();
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				string text2 = array2[i];
				string[] array3 = text2.Replace("RGBA(", "").Replace(")", "").Split(new char[]
				{
					','
				});
				if (array3.Length == 5)
				{
					string key = array3[0].Trim();
					Color black = Color.get_black();
					bool flag = float.TryParse(array3[1], out black.r) && float.TryParse(array3[2], out black.g) && float.TryParse(array3[3], out black.b) && float.TryParse(array3[4], out black.a);
					if (flag)
					{
						dictionary[key] = black;
					}
				}
			}
			FieldInfo[] fields = typeof(DirectorNamedColor).GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
			FieldInfo[] array4 = fields;
			for (int j = 0; j < array4.Length; j++)
			{
				FieldInfo fieldInfo = array4[j];
				if (fieldInfo.FieldType == typeof(Color))
				{
					Color black2 = Color.get_black();
					if (dictionary.TryGetValue(fieldInfo.Name, out black2))
					{
						fieldInfo.SetValue(this, black2);
					}
				}
			}
		}

		public static DirectorNamedColor CreateAndLoadFromText(string text)
		{
			DirectorNamedColor directorNamedColor = ScriptableObject.CreateInstance<DirectorNamedColor>();
			directorNamedColor.FromText(text);
			return directorNamedColor;
		}
	}
}
