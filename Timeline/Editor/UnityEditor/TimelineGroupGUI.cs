using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor
{
	internal class TimelineGroupGUI : TimelineTrackBaseGUI
	{
		protected DirectorStyles m_Styles;

		protected bool m_MustRecomputeUnions = true;

		protected int m_GroupDepth;

		protected Rect m_TrackRect = new Rect(0f, 0f, 0f, 0f);

		protected GUIContent m_ProblemIcon = null;

		private Rect m_HeaderRect;

		private readonly bool m_IsReferencedTrack;

		private readonly List<TimelineClipUnion> m_Unions = new List<TimelineClipUnion>();

		private Rect m_HeaderBounds = new Rect(0f, 0f, 1f, 1f);

		public override Rect boundingRect
		{
			get
			{
				return this.m_TrackRect;
			}
		}

		public override bool expandable
		{
			get
			{
				return !this.m_IsRoot;
			}
		}

		public override Rect headerBounds
		{
			get
			{
				this.m_HeaderBounds.set_x(Mathf.Max(0f, this.boundingRect.get_x() - TimelineWindow.instance.state.sequencerHeaderWidth));
				this.m_HeaderBounds.set_y(this.boundingRect.get_y());
				this.m_HeaderBounds.set_width(TimelineWindow.instance.state.sequencerHeaderWidth);
				this.m_HeaderBounds.set_height(this.boundingRect.get_height());
				return this.m_HeaderBounds;
			}
		}

		public virtual Rect indentedHeaderBounds
		{
			get
			{
				float num = (float)this.get_depth() * DirectorStyles.Instance.indentWidth + 2f;
				float num2 = (float)DirectorStyles.Instance.foldout.get_normal().get_background().get_width() + 4f;
				float num3 = num + num2;
				Rect headerRect = this.m_HeaderRect;
				headerRect.set_width(this.m_HeaderRect.get_width() - num3);
				headerRect.set_x(headerRect.get_x() + num3);
				return headerRect;
			}
		}

		public TimelineGroupGUI(TreeViewController treeview, TimelineTreeViewGUI treeviewGUI, int id, int depth, TreeViewItem parent, string displayName, TrackAsset trackAsset, bool isRoot) : base(id, depth, parent, displayName, trackAsset, treeview, treeviewGUI)
		{
			this.m_Styles = DirectorStyles.Instance;
			this.m_IsRoot = isRoot;
			string assetPath = AssetDatabase.GetAssetPath(trackAsset);
			string assetPath2 = AssetDatabase.GetAssetPath(treeviewGUI.TimelineWindow.timeline);
			if (assetPath != assetPath2)
			{
				this.m_IsReferencedTrack = true;
			}
			this.m_GroupDepth = TimelineGroupGUI.CalculateGroupDepth(parent);
		}

		public static void Create(TrackAsset parent, string title)
		{
			if (parent != null)
			{
				parent.SetCollapsed(false);
			}
			TimelineWindow.instance.AddTrack<GroupTrack>(parent, title);
		}

		public override bool IsMouseOver(Vector2 mousePosition)
		{
			return this.headerBounds.Contains(mousePosition);
		}

		public override float GetHeight(TimelineWindow.TimelineState state)
		{
			return state.trackHeight;
		}

		public override void SetHeight(float height)
		{
		}

		protected override void OnLockedChanged(bool value)
		{
		}

		public override void OnGraphRebuilt()
		{
		}

		private static int CalculateGroupDepth(TreeViewItem parent)
		{
			int num = 0;
			bool flag = false;
			do
			{
				TimelineGroupGUI timelineGroupGUI = parent as TimelineGroupGUI;
				if (timelineGroupGUI == null || timelineGroupGUI.track == null)
				{
					flag = true;
				}
				else
				{
					if (timelineGroupGUI.track.mediaType == TimelineAsset.MediaType.Group)
					{
						num++;
					}
					parent = parent.get_parent();
				}
			}
			while (!flag);
			return num;
		}

		protected override bool DetectProblems(TimelineWindow.TimelineState state)
		{
			return false;
		}

		public override bool CanBeSelected(Vector2 mousePosition)
		{
			return this.m_HeaderBounds.Contains(mousePosition);
		}

		public override void Draw(Rect headerRect, Rect trackRect, TimelineWindow.TimelineState state, float identWidth)
		{
			if (!(base.track == null))
			{
				if (!this.m_IsRoot)
				{
					if (this.get_depth() == 1)
					{
						EditorGUI.DrawRect(headerRect, DirectorStyles.Instance.customSkin.colorSequenceBackground);
					}
					Rect rect = headerRect;
					rect.set_x(headerRect.get_xMax() - 20f);
					rect.set_y(headerRect.get_y());
					rect.set_width(20f);
					trackRect.set_width(trackRect.get_width() + state.bindingAreaWidth);
					Rect rect2 = headerRect;
					if (base.isExpanded && this.get_children() != null && this.get_children().Count > 0)
					{
						rect2.set_height(rect2.get_height() + (base.GetChildrenHeight(this) + 1f));
					}
					rect2.set_xMin(rect2.get_xMin() + identWidth);
					Color color = DirectorStyles.Instance.customSkin.colorGroup;
					if (base.track.mediaType == TimelineAsset.MediaType.Animation)
					{
						color = DirectorStyles.Instance.customSkin.colorAnimation;
					}
					else if (base.track.mediaType == TimelineAsset.MediaType.Audio)
					{
						color = DirectorStyles.Instance.customSkin.colorAudio;
					}
					else if (base.track.mediaType == TimelineAsset.MediaType.Video)
					{
						color = DirectorStyles.Instance.customSkin.colorVideo;
					}
					else if (base.track.mediaType == TimelineAsset.MediaType.Script)
					{
						color = DirectorStyles.Instance.customSkin.colorScripting;
					}
					this.m_TrackRect = trackRect;
					this.m_HeaderRect = headerRect;
					Color color2 = color;
					bool flag = SelectionManager.Contains(base.track);
					if (flag)
					{
						color2 = DirectorStyles.Instance.customSkin.colorSelection;
					}
					else if (base.isDropTarget)
					{
						color2 = DirectorStyles.Instance.customSkin.colorDropTarget;
					}
					else if (this.m_GroupDepth % 2 == 1)
					{
						float num;
						float num2;
						float num3;
						Color.RGBToHSV(color2, ref num, ref num2, ref num3);
						num3 += 0.06f;
						color2 = Color.HSVToRGB(num, num2, num3);
					}
					using (new GUIColorOverride(color2))
					{
						GUI.Box(rect2, GUIContent.none, this.m_Styles.groupBackground);
					}
					Rect rect3 = headerRect;
					rect3.set_xMin(rect3.get_xMin() + (rect2.get_width() + identWidth));
					rect3.set_width(trackRect.get_width());
					rect3.set_height(rect2.get_height());
					color2 = ((!flag) ? this.m_Styles.customSkin.colorGroupTrackBackground : this.m_Styles.customSkin.colorTrackBackgroundSelected);
					EditorGUI.DrawRect(rect3, color2);
					bool flag2 = base.track.GetCollapsed() != !base.isExpanded;
					if (flag2)
					{
						base.track.SetCollapsed(!base.isExpanded);
					}
					if (this.m_MustRecomputeUnions || (flag2 && base.track.GetCollapsed()))
					{
						this.RecomputeRectUnions(state);
					}
					if (!base.isExpanded && this.get_children() != null && this.get_children().Count > 0)
					{
						Rect parentRect = trackRect;
						foreach (TimelineClipUnion current in this.m_Unions)
						{
							current.Draw(parentRect, state);
						}
					}
					if (base.track.locked)
					{
						GUI.Button(trackRect, "Locked");
					}
					Rect rect4 = headerRect;
					rect4.set_xMin(rect4.get_xMin() + (identWidth + 20f));
					string text = (!(base.track != null)) ? "missing" : base.track.get_name();
					rect4.set_width(this.m_Styles.groupFont.CalcSize(new GUIContent(text)).x);
					rect4.set_width(Math.Max(rect4.get_width(), 50f));
					if (base.track != null && base.track is GroupTrack)
					{
						Color newColor = this.m_Styles.groupFont.get_normal().get_textColor();
						if (flag)
						{
							newColor = Color.get_white();
						}
						EditorGUI.BeginChangeCheck();
						string text2;
						using (new StyleNormalColorOverride(this.m_Styles.groupFont, newColor))
						{
							text2 = EditorGUI.DelayedTextField(rect4, GUIContent.none, base.track.GetInstanceID(), base.track.get_name(), this.m_Styles.groupFont);
						}
						if (EditorGUI.EndChangeCheck())
						{
							base.track.set_name(text2);
							if (text2.Length == 0)
							{
								base.track.set_name(TimelineHelpers.GenerateUniqueActorName(state.timeline, "unnamed"));
							}
							this.set_displayName(base.track.get_name());
						}
					}
					using (new StyleNormalColorOverride(this.m_Styles.trackHeaderFont, Color.get_white()))
					{
						if (GUI.Button(rect, "+", this.m_Styles.trackHeaderFont))
						{
							this.OnAddTrackClicked();
						}
					}
					if (this.IsTrackRecording(state))
					{
						using (new GUIColorOverride(DirectorStyles.Instance.customSkin.colorTrackBackgroundRecording))
						{
							GUI.Label(rect2, GUIContent.none, this.m_Styles.displayBackground);
						}
					}
					if (Event.get_current().get_type() == 7)
					{
						base.isDropTarget = false;
					}
					if (this.m_IsReferencedTrack)
					{
						Rect rect5 = trackRect;
						rect5.set_x(state.timeAreaRect.get_xMax() - 20f);
						rect5.set_y(rect5.get_y() + 5f);
						rect5.set_width(30f);
						GUI.Label(rect5, DirectorStyles.referenceTrackLabel, EditorStyles.get_label());
					}
				}
			}
		}

		private void OnAddTrackClicked()
		{
			TimelineWindow.instance.ShowNewTracksContextMenu(base.track, this);
		}

		protected bool IsSubTrack()
		{
			return !(base.track == null) && !(base.track.parent == null) && base.track.parent is TrackAsset && ((TrackAsset)base.track.parent).mediaType != TimelineAsset.MediaType.Group;
		}

		protected TrackAsset ParentTrack()
		{
			TrackAsset result;
			if (this.IsSubTrack())
			{
				result = (base.track.parent as TrackAsset);
			}
			else
			{
				result = null;
			}
			return result;
		}

		private bool IsTrackRecording(TimelineWindow.TimelineState state)
		{
			return state.recording && base.track.mediaType == TimelineAsset.MediaType.Group && state.GetArmedTrack(base.track) != null;
		}

		private void RecomputeRectUnions(TimelineWindow.TimelineState state)
		{
			this.m_MustRecomputeUnions = false;
			this.m_Unions.Clear();
			if (this.get_children() != null)
			{
				foreach (TreeViewItem current in this.get_children())
				{
					TimelineTrackGUI timelineTrackGUI = current as TimelineTrackGUI;
					if (timelineTrackGUI != null)
					{
						timelineTrackGUI.RebuildGUICache(state);
						this.m_Unions.AddRange(TimelineClipUnion.Build(timelineTrackGUI.clips));
					}
				}
			}
		}

		public static void AddMenuItems(GenericMenu menu, GroupTrack track)
		{
			TimelineWindow.TimelineState state = TimelineWindow.instance.state;
			TrackType[] array = TimelineHelpers.GetMixableTypes();
			array = (from x in array
			orderby (!x.trackType.Assembly.FullName.Contains("UnityEngine.Sequence")) ? 1 : 0
			select x).ToArray<TrackType>();
			TrackType[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				TrackType trackType = array2[i];
				if (trackType.trackType != typeof(GroupTrack))
				{
					GenericMenu.MenuFunction2 menuFunction = delegate(object e)
					{
						track.SetCollapsed(false);
						state.GetWindow().AddTrack((e as TrackType).trackType, track, null);
						TimelineTrackBaseGUI timelineTrackBaseGUI = TimelineTrackBaseGUI.FindGUITrack(track);
						if (timelineTrackBaseGUI != null)
						{
							TimelineWindow.instance.treeView.data.SetExpanded(timelineTrackBaseGUI, true);
						}
					};
					object obj = trackType;
					string text = TimelineHelpers.GetTrackCategoryName(trackType);
					if (!string.IsNullOrEmpty(text))
					{
						text += "/";
					}
					menu.AddItem(new GUIContent("Add " + text + TimelineHelpers.GetTrackMenuName(trackType)), false, menuFunction, obj);
				}
			}
		}
	}
}
