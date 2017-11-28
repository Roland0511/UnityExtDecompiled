using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Timeline;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEditor
{
	internal class TimelineDragging : TreeViewDragging
	{
		private class TimelineDragData
		{
			public readonly List<TreeViewItem> draggedItems;

			public TimelineDragData(List<TreeViewItem> draggedItems)
			{
				this.draggedItems = draggedItems;
			}
		}

		private const string k_GenericDragId = "TimelineDragging";

		private readonly int kDragSensitivity = 2;

		private readonly TimelineAsset m_Timeline;

		private readonly TimelineWindow m_Window;

		public TimelineDragging(TreeViewController treeView, TimelineWindow window, TimelineAsset data) : base(treeView)
		{
			this.m_Timeline = data;
			this.m_Window = window;
		}

		internal static int GetItemControlID(TreeViewItem item)
		{
			return ((item == null) ? 0 : item.get_id()) + 10000000;
		}

		public override bool CanStartDrag(TreeViewItem targetItem, List<int> draggedItemIDs, Vector2 mouseDownPosition)
		{
			bool result;
			if (Event.get_current().get_modifiers() != null)
			{
				result = false;
			}
			else if (this.m_Window.state.isDragging)
			{
				result = false;
			}
			else
			{
				TimelineTrackBaseGUI timelineTrackBaseGUI = targetItem as TimelineTrackBaseGUI;
				result = (timelineTrackBaseGUI != null && !(timelineTrackBaseGUI.track == null) && !timelineTrackBaseGUI.track.locked && (Event.get_current().get_type() != 3 || Mathf.Abs(Event.get_current().get_delta().y) >= (float)this.kDragSensitivity));
			}
			return result;
		}

		public override void StartDrag(TreeViewItem draggedNode, List<int> draggedItemIDs)
		{
			DragAndDrop.PrepareStartDrag();
			List<TreeViewItem> draggedItems = SelectionManager.SelectedTrackGUI().Cast<TreeViewItem>().ToList<TreeViewItem>();
			DragAndDrop.SetGenericData("TimelineDragging", new TimelineDragging.TimelineDragData(draggedItems));
			DragAndDrop.set_objectReferences(new Object[0]);
			string text = draggedItemIDs.Count + ((draggedItemIDs.Count <= 1) ? "" : "s");
			TimelineGroupGUI timelineGroupGUI = draggedNode as TimelineGroupGUI;
			if (timelineGroupGUI != null)
			{
				text = timelineGroupGUI.get_displayName();
			}
			DragAndDrop.StartDrag(text);
		}

		public TrackType ResolveTypeAmbiguity(TrackType[] types)
		{
			TrackType returnedType = null;
			GenericMenu genericMenu = new GenericMenu();
			for (int i = 0; i < types.Length; i++)
			{
				TrackType trackType = types[i];
				genericMenu.AddItem(new GUIContent(trackType.trackType.Name), false, delegate(object s)
				{
					returnedType = (TrackType)s;
				}, trackType);
			}
			genericMenu.ShowAsContext();
			return returnedType;
		}

		public override bool DragElement(TreeViewItem targetItem, Rect targetItemRect, int row)
		{
			TimelineTrackGUI timelineTrackGUI = targetItem as TimelineTrackGUI;
			bool flag = !(DragAndDrop.GetGenericData("TimelineDragging") is TimelineDragging.TimelineDragData) && (DragAndDrop.get_objectReferences().Any<Object>() || DragAndDrop.get_paths().Any<string>());
			return (timelineTrackGUI == null || !flag || targetItemRect.Contains(Event.get_current().get_mousePosition())) && base.DragElement(targetItem, targetItemRect, row);
		}

		public override DragAndDropVisualMode DoDrag(TreeViewItem parentItem, TreeViewItem targetItem, bool perform, TreeViewDragging.DropPosition dropPos)
		{
			this.m_Window.isDragging = false;
			DragAndDropVisualMode dragAndDropVisualMode = 0;
			TimelineDragging.TimelineDragData timelineDragData = DragAndDrop.GetGenericData("TimelineDragging") as TimelineDragging.TimelineDragData;
			if (timelineDragData != null)
			{
				dragAndDropVisualMode = this.HandleTrackDrop(parentItem, targetItem, perform, dropPos);
			}
			else if (DragAndDrop.get_objectReferences().Any<Object>() || DragAndDrop.get_paths().Any<string>())
			{
				dragAndDropVisualMode = this.HandleGameObjectDrop(parentItem, targetItem, perform, dropPos);
				if (dragAndDropVisualMode == null)
				{
					dragAndDropVisualMode = this.HandleAudioSourceDrop(parentItem, targetItem, perform, dropPos);
				}
				if (dragAndDropVisualMode == null)
				{
					dragAndDropVisualMode = this.HandleObjectDrop(parentItem, targetItem, perform, dropPos);
				}
			}
			this.m_Window.isDragging = false;
			if (dragAndDropVisualMode == 1 && targetItem != null)
			{
				TimelineGroupGUI timelineGroupGUI = targetItem as TimelineGroupGUI;
				if (timelineGroupGUI != null)
				{
					timelineGroupGUI.isDropTarget = true;
				}
			}
			return dragAndDropVisualMode;
		}

		public DragAndDropVisualMode HandleAudioSourceDrop(TreeViewItem parentItem, TreeViewItem targetItem, bool perform, TreeViewDragging.DropPosition dropPos)
		{
			DragAndDropVisualMode result;
			if (parentItem != null || targetItem != null)
			{
				result = 0;
			}
			else
			{
				IEnumerable<AudioSource> enumerable = DragAndDrop.get_objectReferences().OfType<AudioSource>();
				if (!enumerable.Any<AudioSource>())
				{
					result = 0;
				}
				else if (this.m_Window.state.currentDirector == null)
				{
					result = 32;
				}
				else
				{
					if (perform)
					{
						foreach (AudioSource current in enumerable)
						{
							AudioTrack audioTrack = this.m_Window.AddTrack<AudioTrack>(null, string.Empty);
							this.m_Window.state.currentDirector.SetGenericBinding(audioTrack, current);
						}
						this.m_Window.state.Refresh();
					}
					result = 1;
				}
			}
			return result;
		}

		public DragAndDropVisualMode HandleGameObjectDrop(TreeViewItem parentItem, TreeViewItem targetItem, bool perform, TreeViewDragging.DropPosition dropPos)
		{
			DragAndDropVisualMode result;
			if (parentItem != null || targetItem != null)
			{
				result = 0;
			}
			else if (!DragAndDrop.get_objectReferences().Any((Object x) => x is GameObject))
			{
				result = 0;
			}
			else if (this.m_Window.state.currentDirector == null)
			{
				result = 32;
			}
			else
			{
				if (perform)
				{
					Object[] objectReferences = DragAndDrop.get_objectReferences();
					for (int i = 0; i < objectReferences.Length; i++)
					{
						Object @object = objectReferences[i];
						GameObject go = @object as GameObject;
						if (!(go == null))
						{
							PrefabType prefabType = PrefabUtility.GetPrefabType(go);
							if (prefabType != 1 && prefabType != 2)
							{
								IEnumerable<TrackType> enumerable = from x in TimelineHelpers.GetMixableTypes()
								where x.requiresGameObjectBinding
								select x;
								GenericMenu genericMenu = new GenericMenu();
								foreach (TrackType current in enumerable)
								{
									genericMenu.AddItem(new GUIContent(TimelineHelpers.GetTrackMenuName(current)), false, delegate(object e)
									{
										TrackAsset trackAsset = this.m_Window.AddTrack(((TrackType)e).trackType, null, string.Empty);
										if (trackAsset.GetType() == typeof(ActivationTrack))
										{
											TimelineClip timelineClip = trackAsset.CreateClip(0.0);
											timelineClip.displayName = ActivationTrackDrawer.Styles.ClipText.get_text();
										}
										this.m_Window.state.previewMode = false;
										TimelineUtility.SetSceneGameObject(this.m_Window.state.currentDirector, trackAsset, go);
									}, current);
								}
								genericMenu.ShowAsContext();
							}
							this.m_Window.state.Refresh();
						}
					}
				}
				result = 1;
			}
			return result;
		}

		private static bool ValidateObjectDrop(Object obj)
		{
			AnimationClip animationClip = obj as AnimationClip;
			return animationClip == null || !animationClip.get_legacy();
		}

		public DragAndDropVisualMode HandleObjectDrop(TreeViewItem parentItem, TreeViewItem targetItem, bool perform, TreeViewDragging.DropPosition dropPos)
		{
			DragAndDropVisualMode result;
			if (!perform)
			{
				List<Object> list = new List<Object>();
				if (DragAndDrop.get_objectReferences().Any<Object>())
				{
					list.AddRange(DragAndDrop.get_objectReferences());
					using (List<Object>.Enumerator enumerator = list.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Object o = enumerator.Current;
							TrackType[] trackTypeHandle = TimelineHelpers.GetTrackTypeHandle(o.GetType());
							if (!trackTypeHandle.Any<TrackType>() || trackTypeHandle.Any((TrackType t) => !TimelineDragging.ValidateObjectDrop(o)))
							{
								result = 0;
								return result;
							}
						}
					}
					result = 1;
				}
				else if (DragAndDrop.get_paths().Any<string>())
				{
					TimelineGroupGUI timelineGroupGUI = targetItem as TimelineGroupGUI;
					if (timelineGroupGUI == null)
					{
						result = 1;
					}
					else if (timelineGroupGUI.track == null)
					{
						result = 0;
					}
					else
					{
						TrackType tt = TimelineHelpers.TrackTypeFromType(timelineGroupGUI.track.GetType());
						string[] paths = DragAndDrop.get_paths();
						for (int i = 0; i < paths.Length; i++)
						{
							string text = paths[i];
							list.AddRange(AssetDatabase.LoadAllAssetsAtPath(text));
						}
						bool flag = (from o in list
						where o != null
						select o).Any((Object o) => TimelineHelpers.IsTypeSupportedByTrack(tt, o.GetType()));
						result = ((!flag) ? 0 : 1);
					}
				}
				else
				{
					result = 0;
				}
			}
			else
			{
				List<Object> list2 = new List<Object>();
				list2.AddRange(DragAndDrop.get_objectReferences());
				TrackAsset trackAsset = null;
				TimelineGroupGUI timelineGroupGUI2 = (TimelineGroupGUI)targetItem;
				if (targetItem != null && timelineGroupGUI2.track != null)
				{
					trackAsset = timelineGroupGUI2.track;
				}
				Vector2 mousePosition = Event.get_current().get_mousePosition();
				foreach (Object current in list2)
				{
					if (!(current == null))
					{
						if (current is TimelineAsset)
						{
							if (TimelineHelpers.IsCircularRef(this.m_Timeline, current as TimelineAsset))
							{
								Debug.LogError("Cannot add " + current.get_name() + " to the sequence because it would cause a circular reference");
								continue;
							}
							TimelineAsset timelineAsset = current as TimelineAsset;
							foreach (TrackAsset current2 in timelineAsset.tracks)
							{
								if (current2.mediaType == TimelineAsset.MediaType.Group)
								{
									this.m_Timeline.AddTrackInternal(current2);
								}
							}
							this.m_Window.state.Refresh();
							EditorUtility.SetDirty(this.m_Timeline);
						}
						else if (current is TrackAsset)
						{
							this.m_Timeline.AddTrackInternal((TrackAsset)current);
						}
						else
						{
							TrackType trackType = null;
							TrackAsset trackAsset2 = null;
							TrackType[] trackTypeHandle2 = TimelineHelpers.GetTrackTypeHandle(current.GetType());
							if (trackAsset != null)
							{
								TrackType[] array = trackTypeHandle2;
								for (int j = 0; j < array.Length; j++)
								{
									TrackType trackType2 = array[j];
									if (trackAsset.GetType() == trackType2.trackType)
									{
										trackType = trackType2;
										break;
									}
								}
							}
							if (trackType == null)
							{
								if (trackTypeHandle2.Length == 1)
								{
									trackType = trackTypeHandle2[0];
								}
								else if (trackTypeHandle2.Length > 1)
								{
									trackType = this.ResolveTypeAmbiguity(trackTypeHandle2);
								}
							}
							if (trackType == null)
							{
								continue;
							}
							if (!TimelineDragging.ValidateObjectDrop(current))
							{
								continue;
							}
							if (trackAsset != null && trackAsset.GetType() == trackType.trackType)
							{
								trackAsset2 = trackAsset;
							}
							if (trackAsset2 == null)
							{
								trackAsset2 = this.m_Window.AddTrack(trackType.trackType, null, string.Empty);
							}
							if (trackAsset2 == null)
							{
								result = 0;
								return result;
							}
							Object @object = TimelineDragging.TransformObjectBeingDroppedAccordingToTrackRules(trackAsset2, current);
							if (@object == null)
							{
								continue;
							}
							TimelineUndo.PushUndo(trackAsset2, "Create Clip");
							AnimationTrack animationTrack = trackAsset2 as AnimationTrack;
							if (animationTrack != null)
							{
								animationTrack.ConvertToClipMode();
							}
							TimelineClip timelineClip = TimelineHelpers.CreateClipOnTrack(@object, trackAsset2, this.m_Window.state, mousePosition);
							if (timelineClip != null)
							{
								float num = this.m_Window.state.TimeToPixel(1.0) - this.m_Window.state.TimeToPixel(0.0);
								mousePosition.x += (float)timelineClip.duration * num;
								if (timelineClip.asset is ScriptableObject)
								{
									string assetPath = AssetDatabase.GetAssetPath(timelineClip.asset);
									if (assetPath.Length == 0)
									{
										TimelineCreateUtilities.SaveAssetIntoObject(timelineClip.asset, trackAsset2);
									}
								}
								TimelineDragging.FrameClips(this.m_Window.state);
								trackAsset2.SetCollapsed(false);
							}
						}
						this.m_Window.state.Refresh();
						EditorUtility.SetDirty(this.m_Timeline);
					}
				}
				result = 1;
			}
			return result;
		}

		private static Object TransformObjectBeingDroppedAccordingToTrackRules(TrackAsset trackToReceiveClip, Object obj)
		{
			Object result;
			if (trackToReceiveClip is PlayableTrack && obj is MonoScript)
			{
				MonoScript monoScript = (MonoScript)obj;
				if (!typeof(IPlayableAsset).IsAssignableFrom(monoScript.GetClass()) || !typeof(Object).IsAssignableFrom(monoScript.GetClass()))
				{
					Debug.LogError("The MonoScript " + monoScript.get_name() + " is not a valid PlayableAsset");
					result = null;
					return result;
				}
				int num = InternalEditorUtility.CreateScriptableObjectUnchecked(monoScript);
				AssetDatabase.AddInstanceIDToAssetWithRandomFileId(num, trackToReceiveClip, true);
				obj = EditorUtility.InstanceIDToObject(num);
				if (obj == null)
				{
					Debug.LogError("Unable to create PlayableAsset from MonoScript " + monoScript.get_name());
				}
			}
			result = obj;
			return result;
		}

		public DragAndDropVisualMode HandleTrackDrop(TreeViewItem parentItem, TreeViewItem targetItem, bool perform, TreeViewDragging.DropPosition dropPos)
		{
			((TimelineTreeView)this.m_Window.treeView.gui).showInsertionMarker = false;
			TimelineDragging.TimelineDragData timelineDragData = (TimelineDragging.TimelineDragData)DragAndDrop.GetGenericData("TimelineDragging");
			DragAndDropVisualMode result;
			if (!TimelineDragging.ValidDrag(targetItem, timelineDragData.draggedItems))
			{
				result = 0;
			}
			else
			{
				TimelineGroupGUI timelineGroupGUI = targetItem as TimelineGroupGUI;
				if (timelineGroupGUI != null && timelineGroupGUI.track != null)
				{
					((TimelineTreeView)this.m_Window.treeView.gui).showInsertionMarker = true;
				}
				if (dropPos == null)
				{
					TimelineGroupGUI timelineGroupGUI2 = targetItem as TimelineGroupGUI;
					if (timelineGroupGUI2 != null)
					{
						timelineGroupGUI2.isDropTarget = true;
					}
				}
				if (perform)
				{
					List<TrackAsset> draggedActors = (from x in timelineDragData.draggedItems.OfType<TimelineGroupGUI>()
					select x.track).ToList<TrackAsset>();
					if (draggedActors.Count == 0)
					{
						result = 0;
						return result;
					}
					PlayableAsset playableAsset = this.m_Timeline;
					TimelineGroupGUI timelineGroupGUI3 = parentItem as TimelineGroupGUI;
					if (timelineGroupGUI3 != null && timelineGroupGUI3.track != null)
					{
						playableAsset = timelineGroupGUI3.track;
					}
					TrackAsset trackAsset = (timelineGroupGUI == null) ? null : timelineGroupGUI.track;
					if (playableAsset == this.m_Timeline && dropPos == 1 && trackAsset == null)
					{
						trackAsset = this.m_Timeline.tracks.LastOrDefault((TrackAsset x) => !draggedActors.Contains(x));
					}
					if (TrackExtensions.ReparentTracks(draggedActors, playableAsset, trackAsset, dropPos == 2))
					{
						this.m_Window.state.Refresh(true);
					}
				}
				result = 16;
			}
			return result;
		}

		private static void FrameClips(TimelineWindow.TimelineState state)
		{
			int num = 0;
			float num2 = 3.40282347E+38f;
			float num3 = -3.40282347E+38f;
			foreach (TrackAsset current in state.timeline.tracks)
			{
				num += current.clips.Length;
				if (num > 1)
				{
					return;
				}
				TimelineClip[] clips = current.clips;
				for (int i = 0; i < clips.Length; i++)
				{
					TimelineClip timelineClip = clips[i];
					num2 = Mathf.Min(num2, (float)timelineClip.start);
					num3 = Mathf.Max(num3, (float)timelineClip.start + (float)timelineClip.duration);
				}
			}
			if (num == 1)
			{
				float num4 = num3 - num2;
				if (num4 > 0f)
				{
					state.SetTimeAreaShownRange(Mathf.Max(0f, num2 - num4), num3 + num4);
				}
				else
				{
					state.SetTimeAreaShownRange(0f, 100f);
				}
			}
		}

		private static bool ValidDrag(TreeViewItem target, List<TreeViewItem> draggedItems)
		{
			bool result;
			for (TreeViewItem treeViewItem = target; treeViewItem != null; treeViewItem = treeViewItem.get_parent())
			{
				if (draggedItems.Contains(treeViewItem))
				{
					result = false;
					return result;
				}
			}
			result = true;
			return result;
		}
	}
}
