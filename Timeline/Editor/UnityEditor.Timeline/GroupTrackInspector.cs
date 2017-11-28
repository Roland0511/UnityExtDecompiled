using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[CanEditMultipleObjects, CustomEditor(typeof(GroupTrack))]
	internal class GroupTrackInspector : TrackAssetInspector
	{
		private static class Styles
		{
			public static readonly GUIContent GroupSubTrackHeaderName = EditorGUIUtility.TextContent("Name");

			public static readonly GUIContent GroupSubTrackHeaderType = EditorGUIUtility.TextContent("Type");

			public static readonly GUIContent GroupSubTrackHeaderDuration = EditorGUIUtility.TextContent("Duration");

			public static readonly GUIContent GroupSubTrackHeaderFrames = EditorGUIUtility.TextContent("Frames");
		}

		private ReorderableList m_SubTracks;

		[CompilerGenerated]
		private static ReorderableList.HeaderCallbackDelegate <>f__mg$cache0;

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();
			Object[] targets = base.get_targets();
			for (int i = 0; i < targets.Length; i++)
			{
				Object @object = targets[i];
				GroupTrack groupTrack = @object as GroupTrack;
				if (groupTrack == null)
				{
					break;
				}
				List<TrackAsset> subTracks = groupTrack.subTracks;
				string name = groupTrack.get_name();
				GUILayout.Label((subTracks.Count <= 0) ? name : string.Concat(new object[]
				{
					name,
					" (",
					subTracks.Count,
					")"
				}), EditorStyles.get_boldLabel(), new GUILayoutOption[0]);
				GUILayout.Space(3f);
				this.m_SubTracks.set_list(groupTrack.subTracks);
				this.m_SubTracks.DoLayoutList();
				this.m_SubTracks.set_index(-1);
			}
		}

		public override void OnEnable()
		{
			ReorderableList reorderableList = new ReorderableList(new string[0], typeof(string), false, true, false, false);
			reorderableList.drawElementCallback = new ReorderableList.ElementCallbackDelegate(this.OnDrawSubTrack);
			ReorderableList arg_4C_0 = reorderableList;
			if (GroupTrackInspector.<>f__mg$cache0 == null)
			{
				GroupTrackInspector.<>f__mg$cache0 = new ReorderableList.HeaderCallbackDelegate(GroupTrackInspector.OnDrawHeader);
			}
			arg_4C_0.drawHeaderCallback = GroupTrackInspector.<>f__mg$cache0;
			reorderableList.showDefaultBackground = true;
			reorderableList.set_index(0);
			reorderableList.elementHeight = 20f;
			this.m_SubTracks = reorderableList;
		}

		private static void OnDrawHeader(Rect rect)
		{
			int num = 4;
			float num2 = rect.get_width() / (float)num;
			rect.set_width(num2);
			GUI.Label(rect, GroupTrackInspector.Styles.GroupSubTrackHeaderName, EditorStyles.get_label());
			rect.set_x(rect.get_x() + num2);
			GUI.Label(rect, GroupTrackInspector.Styles.GroupSubTrackHeaderType, EditorStyles.get_label());
			rect.set_x(rect.get_x() + num2);
			GUI.Label(rect, GroupTrackInspector.Styles.GroupSubTrackHeaderDuration, EditorStyles.get_label());
			rect.set_x(rect.get_x() + num2);
			GUI.Label(rect, GroupTrackInspector.Styles.GroupSubTrackHeaderFrames, EditorStyles.get_label());
		}

		private void OnDrawSubTrack(Rect rect, int index, bool selected, bool focused)
		{
			TrackAsset trackAsset = this.m_SubTracks.get_list()[index] as TrackAsset;
			if (!(trackAsset == null))
			{
				int num = 4;
				float num2 = rect.get_width() / (float)num;
				rect.set_width(num2);
				GUI.Label(rect, trackAsset.get_name(), EditorStyles.get_label());
				rect.set_x(rect.get_x() + num2);
				GUI.Label(rect, trackAsset.GetType().Name, EditorStyles.get_label());
				rect.set_x(rect.get_x() + num2);
				GUI.Label(rect, trackAsset.get_duration().ToString(), EditorStyles.get_label());
				rect.set_x(rect.get_x() + num2);
				double num3 = TimeUtility.ToExactFrames(trackAsset.get_duration(), (double)TimelineWindow.instance.state.frameRate);
				GUI.Label(rect, num3.ToString(), EditorStyles.get_label());
			}
		}
	}
}
