using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEditor.Callbacks;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
	[EditorWindowTitle(title = "Timeline", useTypeNameAsIconName = true)]
	internal class TimelineWindow : EditorWindow, IControl
	{
		public class TimelineState : ITimelineState
		{
			private TimelineWindow m_Window;

			private bool m_Recording;

			private bool m_Playing;

			private float m_PlaybackSpeed;

			private double m_Time;

			private readonly QuadTree<IBounds> m_QuadTree = new QuadTree<IBounds>();

			private readonly List<IControl> m_CaptureSession = new List<IControl>();

			private int m_DirtyStamp;

			private float m_SequencerHeaderWidth = 307f;

			private float m_BindingAreaWidth = 40f;

			private int m_ActiveView;

			private PlayableDirector m_CurrentDirector;

			private bool m_MustRebuildGraph;

			private List<TrackAsset> m_SoloTracks = new List<TrackAsset>();

			private static AnimationModeDriver s_PreviewDriver;

			public static readonly int kTimeCodeTextFieldId = 3790;

			private static readonly double k_MaxTimelineDurationInSeconds = 9000000.0;

			private static readonly float k_MinSequencerHeaderWidth = 307f;

			private static readonly float k_MaxSequencerHeaderWidth = 650f;

			private static readonly float k_MaxTimeAreaScaling = 90000f;

			private Dictionary<TrackAsset, TrackAsset> m_ArmedTracks = new Dictionary<TrackAsset, TrackAsset>();

			private int m_TreeViewKeyboardControlId;

			private TimelineWindow.TimelineWindowPreferences m_Preferences;

			private List<PendingUpdateDelegate> m_OnStartFrameUpdates;

			private List<PendingUpdateDelegate> m_OnEndFrameUpdates;

			public static double kTimeEpsilon
			{
				get
				{
					return TimeUtility.kTimeEpsilon;
				}
			}

			public static AnimationModeDriver previewDriver
			{
				get
				{
					if (TimelineWindow.TimelineState.s_PreviewDriver == null)
					{
						TimelineWindow.TimelineState.s_PreviewDriver = ScriptableObject.CreateInstance<AnimationModeDriver>();
					}
					return TimelineWindow.TimelineState.s_PreviewDriver;
				}
			}

			public EditorWindow editorWindow
			{
				get
				{
					return this.m_Window;
				}
			}

			public bool rebuildGraph
			{
				get
				{
					return this.m_MustRebuildGraph;
				}
				set
				{
					this.SyncNotifyValue<bool>(ref this.m_MustRebuildGraph, value, "rebuildGraph");
				}
			}

			public double duration
			{
				get
				{
					double result;
					if (this.timeline == null)
					{
						result = 0.0;
					}
					else
					{
						result = ((this.timeline.durationMode != TimelineAsset.DurationMode.FixedLength) ? this.timeline.get_duration() : this.timeline.fixedDuration);
					}
					return result;
				}
			}

			public float mouseDragLag
			{
				get;
				set;
			}

			public int keyboardControl
			{
				get
				{
					return this.m_TreeViewKeyboardControlId;
				}
				set
				{
					this.m_TreeViewKeyboardControlId = value;
				}
			}

			public QuadTree<IBounds> quadTree
			{
				get
				{
					return this.m_QuadTree;
				}
			}

			public List<IControl> captured
			{
				get
				{
					return this.m_CaptureSession;
				}
			}

			public PlayableDirector currentDirector
			{
				get
				{
					return this.m_CurrentDirector;
				}
				set
				{
					if (value != this.m_CurrentDirector)
					{
						this.OnCurrentDirectorWillChange(value);
					}
					this.SyncNotifyValue<PlayableDirector>(ref this.m_CurrentDirector, value, "currentDirector");
				}
			}

			public int activeView
			{
				get
				{
					return this.m_ActiveView;
				}
				set
				{
					this.SyncNotifyValue<int>(ref this.m_ActiveView, value, "activeView");
				}
			}

			public List<TrackAsset> soloTracks
			{
				get
				{
					return this.m_SoloTracks;
				}
				set
				{
					this.m_SoloTracks = value;
				}
			}

			public TimelineAsset timeline
			{
				get
				{
					return this.m_Window.timeline;
				}
			}

			public TrackAsset rootTrack
			{
				get;
				private set;
			}

			public bool isJogging
			{
				get;
				set;
			}

			public bool isDragging
			{
				get;
				set;
			}

			public float bindingAreaWidth
			{
				get
				{
					return this.m_BindingAreaWidth;
				}
				set
				{
					this.m_BindingAreaWidth = value;
				}
			}

			public float sequencerHeaderWidth
			{
				get
				{
					return this.m_SequencerHeaderWidth;
				}
				set
				{
					this.m_SequencerHeaderWidth = Mathf.Clamp(value, TimelineWindow.TimelineState.k_MinSequencerHeaderWidth, TimelineWindow.TimelineState.k_MaxSequencerHeaderWidth);
				}
			}

			public float mainAreaWidth
			{
				get;
				set;
			}

			public float trackHeight
			{
				get
				{
					return TimelineWindowViewPrefs.GetTimelineAssetViewData(this.timeline).trackHeight;
				}
				set
				{
					TimelineWindowViewPrefs.GetTimelineAssetViewData(this.timeline).trackHeight = value;
					this.m_Window.treeView.CalculateRowRects();
				}
			}

			public float trackScale
			{
				get
				{
					return TimelineWindowViewPrefs.GetTimelineAssetViewData(this.timeline).trackScale;
				}
				set
				{
					TimelineWindowViewPrefs.GetTimelineAssetViewData(this.timeline).trackScale = value;
					this.m_Window.treeView.CalculateRowRects();
				}
			}

			public int dirtyStamp
			{
				get
				{
					return this.m_DirtyStamp;
				}
				set
				{
					this.SyncNotifyValue<int>(ref this.m_DirtyStamp, value, "dirtyStamp");
				}
			}

			public bool showQuadTree
			{
				get;
				set;
			}

			public bool canRecord
			{
				get
				{
					return AnimationMode.InAnimationMode(TimelineWindow.TimelineState.previewDriver) || !AnimationMode.InAnimationMode();
				}
			}

			public bool recording
			{
				get
				{
					if (!this.previewMode)
					{
						this.m_Recording = false;
					}
					return this.m_Recording;
				}
				set
				{
					if (value)
					{
						this.previewMode = true;
					}
					bool flag = value;
					if (!this.previewMode)
					{
						flag = false;
					}
					if (flag && this.m_ArmedTracks.Count == 0)
					{
						Debug.LogError("Cannot enable recording without an armed track");
						flag = false;
					}
					if (!flag)
					{
						this.m_ArmedTracks.Clear();
					}
					if (flag != this.m_Recording)
					{
						if (flag)
						{
							AnimationMode.StartAnimationRecording();
						}
						else
						{
							AnimationMode.StopAnimationRecording();
						}
						InspectorWindow.RepaintAllInspectors();
					}
					this.SyncNotifyValue<bool>(ref this.m_Recording, flag, "recording");
				}
			}

			public bool previewMode
			{
				get
				{
					return AnimationMode.InAnimationMode(TimelineWindow.TimelineState.previewDriver);
				}
				set
				{
					bool flag = AnimationMode.InAnimationMode(TimelineWindow.TimelineState.previewDriver);
					if (!value)
					{
						if (flag)
						{
							AnimationMode.StopAnimationMode(TimelineWindow.TimelineState.previewDriver);
							AnimationPropertyContextualMenu.Instance.SetResponder(null);
							if (!Application.get_isPlaying())
							{
								this.Stop();
							}
						}
					}
					else if (!flag)
					{
						this.EvaluateImmediate();
					}
				}
			}

			public bool playing
			{
				get
				{
					bool result;
					if (Application.get_isPlaying())
					{
						result = (this.currentDirector != null && this.currentDirector.get_state() == 1);
					}
					else
					{
						result = this.m_Playing;
					}
					return result;
				}
				set
				{
					if (!Application.get_isPlaying())
					{
						this.SyncNotifyValue<bool>(ref this.m_Playing, value, "playing");
					}
				}
			}

			public float playbackSpeed
			{
				get
				{
					return this.m_PlaybackSpeed;
				}
				set
				{
					this.SyncNotifyValue<float>(ref this.m_PlaybackSpeed, value, "playbackSpeed");
				}
			}

			public bool timeInFrames
			{
				get
				{
					return TimelineWindowViewPrefs.GetTimelineAssetViewData(this.timeline).timeInFrames;
				}
				set
				{
					TimelineWindowViewPrefs.GetTimelineAssetViewData(this.timeline).timeInFrames = value;
				}
			}

			public bool showAudioWaveform
			{
				get
				{
					return TimelineWindowViewPrefs.GetTimelineAssetViewData(this.timeline).showAudioWaveform;
				}
				set
				{
					TimelineWindowViewPrefs.GetTimelineAssetViewData(this.timeline).showAudioWaveform = value;
				}
			}

			public bool frameSnap
			{
				get
				{
					return this.m_Preferences.frameSnap;
				}
				set
				{
					this.SyncNotifyValue<bool>(ref this.m_Preferences.frameSnap, value, "frameSnap");
				}
			}

			public bool edgeSnaps
			{
				get
				{
					return this.m_Preferences.edgeSnaps;
				}
				set
				{
					this.m_Preferences.edgeSnaps = value;
				}
			}

			public bool playRangeLoopMode
			{
				get
				{
					return this.m_Window.m_Preferences.playRangeLoopMode;
				}
				set
				{
					this.m_Window.m_Preferences.playRangeLoopMode = value;
				}
			}

			public double time
			{
				get
				{
					if (this.currentDirector != null)
					{
						this.m_Time = this.currentDirector.get_time();
					}
					return this.m_Time;
				}
				set
				{
					if (value > TimelineWindow.TimelineState.k_MaxTimelineDurationInSeconds)
					{
						value = TimelineWindow.TimelineState.k_MaxTimelineDurationInSeconds;
					}
					if (this.currentDirector != null)
					{
						this.currentDirector.set_time(value);
					}
					this.SyncNotifyValue<double>(ref this.m_Time, value, "time");
				}
			}

			public bool isClipSnapping
			{
				get;
				set;
			}

			public int frame
			{
				get
				{
					return TimeUtility.ToFrames(this.time, (double)this.frameRate);
				}
				set
				{
					this.time = TimeUtility.FromFrames(Mathf.Max(0, value), (double)this.frameRate);
				}
			}

			public float frameRate
			{
				get
				{
					float result;
					if (this.m_Window.timeline != null)
					{
						result = this.m_Window.timeline.editorSettings.fps;
					}
					else
					{
						result = TimelineAsset.EditorSettings.kDefaultFPS;
					}
					return result;
				}
				set
				{
					TimelineAsset.EditorSettings editorSettings = this.timeline.editorSettings;
					if (editorSettings.fps != value)
					{
						editorSettings.fps = Mathf.Max(value, (float)TimeUtility.kFrameRateEpsilon);
						EditorUtility.SetDirty(this.timeline);
					}
				}
			}

			public Vector2 timeAreaShownRange
			{
				get
				{
					Vector2 result;
					if (this.m_Window.timeline != null)
					{
						result = TimelineWindowViewPrefs.GetTimelineAssetViewData(this.m_Window.timeline).timeAreaShownRange;
					}
					else
					{
						result = TimelineAssetViewModel.kTimeAreaDefaultRange;
					}
					return result;
				}
			}

			public Vector2 timeAreaTranslation
			{
				get
				{
					return this.m_Window.m_TimeArea.get_translation();
				}
			}

			public Vector2 timeAreaScale
			{
				get
				{
					return this.m_Window.m_TimeArea.get_scale();
				}
			}

			public Rect timeAreaRect
			{
				get
				{
					return this.m_Window.timeAreaBounds;
				}
			}

			public float windowHeight
			{
				get
				{
					return this.m_Window.get_position().get_height();
				}
			}

			public bool playRangeEnabled
			{
				get
				{
					return !EditorApplication.get_isPlaying() && TimelineWindowViewPrefs.GetTimelineAssetViewData(this.timeline).playRangeEnabled;
				}
				set
				{
					if (!EditorApplication.get_isPlaying())
					{
						TimelineWindowViewPrefs.GetTimelineAssetViewData(this.timeline).playRangeEnabled = value;
					}
				}
			}

			public Vector2 playRangeTime
			{
				get
				{
					return TimelineWindowViewPrefs.GetTimelineAssetViewData(this.timeline).timeAreaPlayRange;
				}
				set
				{
					TimelineWindowViewPrefs.GetTimelineAssetViewData(this.timeline).timeAreaPlayRange = this.ValidatePlayRange(value);
				}
			}

			public TimelineState(TimelineWindow w)
			{
				this.m_Window = w;
				this.m_Playing = false;
				this.m_Preferences = w.m_Preferences;
			}

			public void OnDestroy()
			{
				if (!Application.get_isPlaying())
				{
					this.Stop();
				}
				if (this.m_OnStartFrameUpdates != null)
				{
					this.m_OnStartFrameUpdates.Clear();
				}
				if (this.m_OnEndFrameUpdates != null)
				{
					this.m_OnEndFrameUpdates.Clear();
				}
			}

			public void SetCurrentSequence(TimelineAsset asset)
			{
				Selection.set_activeObject(asset);
				this.GetWindow().OnSelectionChange();
				this.GetWindow().SetCurrentTimeline(asset, null);
			}

			public double SnapToFrameIfRequired(double currentTime)
			{
				double result;
				if (this.frameSnap)
				{
					result = TimeUtility.FromFrames(TimeUtility.ToFrames(currentTime, (double)this.frameRate), (double)this.frameRate);
				}
				else
				{
					result = currentTime;
				}
				return result;
			}

			public void Reset()
			{
				this.recording = false;
				this.currentDirector = null;
				this.Stop();
				this.Refresh();
				this.playing = false;
			}

			public double GetSnappedTimeAtMousePosition(Vector2 mousePos)
			{
				return this.SnapToFrameIfRequired((double)this.ScreenSpacePixelToTimeAreaTime(mousePos.x));
			}

			private void SyncNotifyValue<T>(ref T oldValue, T newValue, string propertyChanged)
			{
				bool flag = false;
				if (oldValue == null)
				{
					oldValue = newValue;
					flag = true;
				}
				else if (!oldValue.Equals(newValue))
				{
					oldValue = newValue;
					flag = true;
				}
				if (flag && this.m_Window != null && this.m_Window.OnStateChange != null)
				{
					TimelineWindow.StateEventArgs stateEventArgs = new TimelineWindow.StateEventArgs();
					stateEventArgs.state = this;
					stateEventArgs.propertyChanged = propertyChanged;
					this.m_Window.OnStateChange(this.m_Window, stateEventArgs);
				}
			}

			public string TimeAsString(double timeValue, string format = "F2")
			{
				string result;
				if (this.timeInFrames)
				{
					result = TimeUtility.TimeAsFrames(timeValue, (double)this.frameRate, format);
				}
				else
				{
					result = TimeUtility.TimeAsTimeCode(timeValue, (double)this.frameRate, format);
				}
				return result;
			}

			public void SetTimeAreaTransform(Vector2 newTranslation, Vector2 newScale)
			{
				this.m_Window.m_TimeArea.SetTransform(newTranslation, newScale);
				this.TimeAreaChanged();
			}

			public void SetTimeAreaShownRange(float min, float max)
			{
				this.m_Window.m_TimeArea.SetShownHRange(min, max);
				this.TimeAreaChanged();
			}

			internal void TimeAreaChanged()
			{
				if (this.m_Window.m_TimeArea.get_scale().x > TimelineWindow.TimelineState.k_MaxTimeAreaScaling)
				{
					Vector2 vector = new Vector2(TimelineWindow.TimelineState.k_MaxTimeAreaScaling, this.m_Window.m_TimeArea.get_scale().y);
					this.m_Window.m_TimeArea.SetTransform(this.m_Window.m_TimeArea.get_translation(), vector);
				}
				if (this.timeline != null)
				{
					TimelineAssetViewModel timelineAssetViewData = TimelineWindowViewPrefs.GetTimelineAssetViewData(this.timeline);
					Vector2 vector2 = new Vector2(this.m_Window.m_TimeArea.get_shownArea().get_x(), this.m_Window.m_TimeArea.get_shownArea().get_xMax());
					if (timelineAssetViewData.timeAreaShownRange != vector2)
					{
						timelineAssetViewData.timeAreaShownRange = vector2;
						EditorUtility.SetDirty(this.timeline);
					}
				}
			}

			public bool TimeIsInRange(float value)
			{
				Rect shownArea = this.m_Window.m_TimeArea.get_shownArea();
				return value >= shownArea.get_x() && value <= shownArea.get_xMax();
			}

			public void EnsurePlayHeadIsVisible()
			{
				double num = (double)this.PixelToTime(this.timeAreaRect.get_xMin());
				double num2 = (double)this.PixelToTime(this.timeAreaRect.get_xMax());
				double time = this.time;
				if (time < num || time > num2)
				{
					float num3 = (float)(num2 - num);
					float min = (float)time - num3 / 2f;
					float max = (float)time + num3 / 2f;
					this.SetTimeAreaShownRange(min, max);
				}
			}

			private Vector2 ValidatePlayRange(Vector2 playRange)
			{
				Vector2 result;
				if (playRange == TimelineAssetViewModel.kNoPlayRangeSet)
				{
					result = playRange;
				}
				else
				{
					float num = 0.01f / Mathf.Max(1f, this.frameRate);
					Vector2 vector = playRange;
					if (vector.y - vector.x < num)
					{
						vector.x = vector.y - num;
					}
					if (vector.x < 0f)
					{
						vector.x = 0f;
					}
					if ((double)vector.y > this.duration)
					{
						vector.y = (float)this.duration;
					}
					if (vector.y - vector.x < num)
					{
						vector.y = Mathf.Min(vector.x + num, (float)this.duration);
					}
					result = vector;
				}
				return result;
			}

			public TimelineWindow GetWindow()
			{
				return this.m_Window;
			}

			public void Stop()
			{
				if (this.currentDirector != null)
				{
					this.currentDirector.StopImmediately();
				}
			}

			public void Play()
			{
				if (!(this.currentDirector == null))
				{
					this.currentDirector.Evaluate();
				}
			}

			public void BreadcrumbSetRoot(PlayableAsset asset)
			{
				if (asset == null)
				{
					this.currentDirector = null;
				}
				List<BreadcrumbElement> breadcrumbPath = new List<BreadcrumbElement>();
				this.m_Window.breadcrumbPath = breadcrumbPath;
				this.BreadcrumbGoto(asset, null);
			}

			public void BreadcrumbGoto(PlayableAsset asset, TimelineClip instance)
			{
				if (!(asset == null))
				{
					BreadcrumbElement breadcrumbElement = new BreadcrumbElement();
					List<BreadcrumbElement> breadcrumbPath = this.m_Window.breadcrumbPath;
					if (breadcrumbPath.Count == 0)
					{
						breadcrumbElement.asset = asset;
						breadcrumbElement.clip = instance;
						breadcrumbPath.Add(breadcrumbElement);
						this.m_Window.breadcrumbPath = breadcrumbPath;
					}
					else
					{
						breadcrumbElement.asset = asset;
						breadcrumbElement.clip = instance;
						int num = breadcrumbPath.IndexOf(breadcrumbElement);
						if (num < 0)
						{
							Debug.LogWarning("Attempting to jump into a clip that was not part of the breadcrumb list");
						}
						else
						{
							try
							{
								if (num == 0)
								{
									TimelineAsset timelineAsset = Selection.get_activeObject() as TimelineAsset;
									if (timelineAsset)
									{
										this.m_Window.SetCurrentTimeline(timelineAsset, this.currentDirector);
									}
									this.rootTrack = null;
								}
								this.m_Window.breadcrumbPath = breadcrumbPath.Take(num + 1).ToList<BreadcrumbElement>();
							}
							catch (Exception ex)
							{
								Debug.LogError("Exception in BreadcrumbGoto: " + ex.Message);
							}
						}
					}
				}
			}

			public void BreadcrumbDrillInto(PlayableAsset asset, TimelineClip instance)
			{
				if (asset == null)
				{
					Debug.LogWarning("Attempting to drill into a clip that is not nested or is null");
				}
				else
				{
					if (this.rootTrack == null)
					{
						this.rootTrack = instance.parentTrack;
					}
					SelectionManager.Clear();
					List<BreadcrumbElement> breadcrumbPath = this.m_Window.breadcrumbPath;
					breadcrumbPath.Add(new BreadcrumbElement
					{
						asset = asset,
						clip = instance
					});
					this.m_Window.breadcrumbPath = breadcrumbPath;
				}
			}

			public void Evaluate()
			{
				if (!(this.currentDirector == null))
				{
					if (!EditorApplication.get_isPlaying() && !this.previewMode)
					{
						this.GatherProperties(this.currentDirector);
					}
					this.ForceTimeOnDirector();
					this.currentDirector.DeferredEvaluate();
					if (!EditorApplication.get_isPlaying())
					{
						GameView.RepaintAll();
						SceneView.RepaintAll();
					}
				}
			}

			public void EvaluateImmediate()
			{
				if (!(this.currentDirector == null))
				{
					if (!EditorApplication.get_isPlaying() && !this.previewMode)
					{
						this.GatherProperties(this.currentDirector);
					}
					if (this.previewMode)
					{
						this.ForceTimeOnDirector();
						this.currentDirector.Evaluate();
					}
				}
			}

			private void ForceTimeOnDirector()
			{
				double time = this.currentDirector.get_time();
				this.currentDirector.set_time(time);
			}

			public void Refresh()
			{
				this.Refresh(true);
			}

			public void Refresh(bool dirtyAsset)
			{
				this.captured.Clear();
				this.CheckRecordingState();
				this.dirtyStamp++;
				this.rebuildGraph = true;
			}

			public bool IsEditingASubItem()
			{
				return this.IsCurrentEditingASequencerTextField() || !SelectionManager.IsCurveEditorFocused(null);
			}

			public bool IsCurrentEditingASequencerTextField()
			{
				bool result;
				if (this.timeline == null)
				{
					result = false;
				}
				else if (TimelineWindow.TimelineState.kTimeCodeTextFieldId == GUIUtility.get_keyboardControl())
				{
					result = true;
				}
				else
				{
					result = (this.timeline.flattenedTracks.Count((TrackAsset t) => t.GetInstanceID() == GUIUtility.get_keyboardControl()) != 0);
				}
				return result;
			}

			public float TimeToTimeAreaPixel(double time)
			{
				float num = (float)time;
				num *= this.timeAreaScale.x;
				return num + (this.timeAreaTranslation.x + this.sequencerHeaderWidth);
			}

			public float TimeToScreenSpacePixel(double time)
			{
				float num = (float)time;
				num *= this.timeAreaScale.x;
				return num + this.timeAreaTranslation.x;
			}

			public float TimeToPixel(double time)
			{
				return this.m_Window.m_TimeArea.TimeToPixel((float)time, this.m_Window.timeAreaBounds);
			}

			public float PixelToTime(float pixel)
			{
				return this.m_Window.m_TimeArea.PixelToTime(pixel, this.m_Window.timeAreaBounds);
			}

			public float TimeAreaPixelToTime(float pixel)
			{
				return this.PixelToTime(pixel);
			}

			public float ScreenSpacePixelToTimeAreaTime(float p)
			{
				p -= this.m_Window.timeAreaBounds.get_x();
				return this.TrackSpacePixelToTimeAreaTime(p);
			}

			public float TrackSpacePixelToTimeAreaTime(float p)
			{
				p -= this.timeAreaTranslation.x;
				float result;
				if (this.timeAreaScale.x > 0f)
				{
					result = p / this.timeAreaScale.x;
				}
				else
				{
					result = p;
				}
				return result;
			}

			public void OffsetTimeArea(int pixels)
			{
				Vector3 vector = this.timeAreaTranslation;
				vector.x += (float)pixels;
				this.SetTimeAreaTransform(vector, this.timeAreaScale);
			}

			public Component GetBindingForTrack(TrackAsset trackAsset)
			{
				Component result;
				if (this.currentDirector == null)
				{
					result = null;
				}
				else if (trackAsset.mediaType != TimelineAsset.MediaType.Animation)
				{
					result = null;
				}
				else
				{
					GameObject sceneGameObject = TimelineUtility.GetSceneGameObject(this.currentDirector, trackAsset);
					if (sceneGameObject == null)
					{
						result = null;
					}
					else
					{
						result = sceneGameObject.GetComponent<Animator>();
					}
				}
				return result;
			}

			public GameObject GetSceneReference(TrackAsset asset)
			{
				GameObject result;
				if (this.currentDirector == null)
				{
					result = null;
				}
				else
				{
					result = TimelineUtility.GetSceneGameObject(this.currentDirector, asset);
				}
				return result;
			}

			public void CalculateRowRects()
			{
				if (this.m_Window != null && this.m_Window.treeView != null)
				{
					this.m_Window.treeView.CalculateRowRects();
				}
			}

			public void ArmForRecord(TrackAsset track)
			{
				this.m_ArmedTracks[TimelineUtility.GetSceneReferenceTrack(track)] = track;
				if (track != null && !this.recording)
				{
					this.recording = true;
				}
				if (this.recording)
				{
					track.OnRecordingArmed(this.currentDirector);
					this.CalculateRowRects();
				}
			}

			public void UnarmForRecord(TrackAsset track)
			{
				this.m_ArmedTracks.Remove(TimelineUtility.GetSceneReferenceTrack(track));
				if (this.m_ArmedTracks.Count == 0)
				{
					this.recording = false;
				}
				track.OnRecordingUnarmed(this.currentDirector);
			}

			public void UpdateRecordingState()
			{
				if (this.recording)
				{
					foreach (TrackAsset current in this.m_ArmedTracks.Values)
					{
						if (current != null)
						{
							current.OnRecordingTimeChanged(this.currentDirector);
						}
					}
				}
			}

			public bool IsArmedForRecord(TrackAsset track)
			{
				return track == this.GetArmedTrack(track);
			}

			public TrackAsset GetArmedTrack(TrackAsset track)
			{
				TrackAsset result = null;
				this.m_ArmedTracks.TryGetValue(TimelineUtility.GetSceneReferenceTrack(track), out result);
				return result;
			}

			private void CheckRecordingState()
			{
				if (this.m_ArmedTracks.Any((KeyValuePair<TrackAsset, TrackAsset> t) => t.Value == null))
				{
					this.m_ArmedTracks = (from t in this.m_ArmedTracks
					where t.Value != null
					select t).ToDictionary((KeyValuePair<TrackAsset, TrackAsset> t) => t.Key, (KeyValuePair<TrackAsset, TrackAsset> t) => t.Value);
					if (this.m_ArmedTracks.Count == 0)
					{
						this.recording = false;
					}
				}
			}

			private void OnCurrentDirectorWillChange(PlayableDirector value)
			{
				if (!Application.get_isPlaying())
				{
					this.Stop();
				}
				this.previewMode = false;
				this.rebuildGraph = true;
				this.GatherProperties(value);
			}

			public void GatherProperties(PlayableDirector director)
			{
				if (!(director == null))
				{
					if (!this.previewMode)
					{
						AnimationMode.StartAnimationMode(TimelineWindow.TimelineState.previewDriver);
						AnimationPropertyContextualMenu.Instance.SetResponder(new TimelineRecordingContextualResponder(this));
						if (!this.previewMode)
						{
							return;
						}
					}
					TimelineAsset timelineAsset = director.get_playableAsset() as TimelineAsset;
					if (timelineAsset != null)
					{
						PropertyCollector propertyCollector = new PropertyCollector();
						propertyCollector.PushActiveGameObject(null);
						timelineAsset.GatherProperties(director, propertyCollector);
					}
				}
			}

			public void RebindAnimators()
			{
				if (this.currentDirector != null && this.timeline != null)
				{
					IEnumerable<AnimationTrack> enumerable = this.timeline.GetOutputTracks().OfType<AnimationTrack>();
					foreach (AnimationTrack current in enumerable)
					{
						GameObject sceneGameObject = TimelineUtility.GetSceneGameObject(this.currentDirector, current);
						if (sceneGameObject != null)
						{
							Animator component = sceneGameObject.GetComponent<Animator>();
							if (component != null)
							{
								component.Rebind();
							}
						}
					}
				}
			}

			public void RebindAnimators(List<GameObject> objects)
			{
				if (objects != null && objects.Count != 0)
				{
					foreach (GameObject current in objects)
					{
						Animator component = current.GetComponent<Animator>();
						if (component != null)
						{
							component.Rebind();
						}
					}
				}
			}

			internal void ProcessStartFramePendingUpdates()
			{
				if (this.m_OnStartFrameUpdates != null)
				{
					this.m_OnStartFrameUpdates.RemoveAll((PendingUpdateDelegate callback) => callback(this, Event.get_current()));
				}
			}

			internal void ProcessEndFramePendingUpdates()
			{
				if (this.m_OnEndFrameUpdates != null)
				{
					this.m_OnEndFrameUpdates.RemoveAll((PendingUpdateDelegate callback) => callback(this, Event.get_current()));
				}
			}

			public void AddStartFrameDelegate(PendingUpdateDelegate updateDelegate)
			{
				if (this.m_OnStartFrameUpdates == null)
				{
					this.m_OnStartFrameUpdates = new List<PendingUpdateDelegate>();
				}
				if (!this.m_OnStartFrameUpdates.Contains(updateDelegate))
				{
					this.m_OnStartFrameUpdates.Add(updateDelegate);
				}
			}

			public void AddEndFrameDelegate(PendingUpdateDelegate updateDelegate)
			{
				if (this.m_OnEndFrameUpdates == null)
				{
					this.m_OnEndFrameUpdates = new List<PendingUpdateDelegate>();
				}
				if (!this.m_OnEndFrameUpdates.Contains(updateDelegate))
				{
					this.m_OnEndFrameUpdates.Add(updateDelegate);
				}
			}

			internal TrackBindingValidationResult ValidateBindingForTrack(TrackAsset track)
			{
				TrackBindingValidationResult result;
				if (this.currentDirector == null)
				{
					result = new TrackBindingValidationResult(TimelineTrackBindingState.NoGameObjectBound, null);
				}
				else
				{
					IEnumerable<PlayableBinding> enumerable = (!(track != null)) ? null : track.get_outputs();
					if (enumerable == null || enumerable.Count<PlayableBinding>() == 0)
					{
						result = new TrackBindingValidationResult(TimelineTrackBindingState.Valid, null);
					}
					else
					{
						Object genericBinding = this.currentDirector.GetGenericBinding(enumerable.First<PlayableBinding>().get_sourceObject());
						if (enumerable.First<PlayableBinding>().get_streamType() == null)
						{
							GameObject gameObject = genericBinding as GameObject;
							if (gameObject == null)
							{
								result = new TrackBindingValidationResult(TimelineTrackBindingState.NoGameObjectBound, null);
								return result;
							}
							Animator component = gameObject.GetComponent<Animator>();
							if (component == null)
							{
								result = new TrackBindingValidationResult(TimelineTrackBindingState.NoValidComponentOnBoundGameObject, genericBinding.get_name());
								return result;
							}
							if (!gameObject.get_activeInHierarchy())
							{
								result = new TrackBindingValidationResult(TimelineTrackBindingState.BoundGameObjectIsDisabled, genericBinding.get_name());
								return result;
							}
							if (!component.get_enabled())
							{
								result = new TrackBindingValidationResult(TimelineTrackBindingState.RequiredComponentOnBoundGameObjectIsDisabled, null);
								return result;
							}
						}
						result = new TrackBindingValidationResult(TimelineTrackBindingState.Valid, null);
					}
				}
				return result;
			}

			public void UpdateRootPlayableDuration(double duration)
			{
				if (this.currentDirector != null)
				{
					if (this.currentDirector.get_playableGraph().IsValid())
					{
						if (this.currentDirector.get_playableGraph().GetRootPlayableCount() > 0)
						{
							Playable rootPlayable = this.currentDirector.get_playableGraph().GetRootPlayable(0);
							if (PlayableExtensions.IsValid<Playable>(rootPlayable))
							{
								PlayableExtensions.SetDuration<Playable>(rootPlayable, duration);
							}
						}
					}
				}
			}
		}

		[Serializable]
		public class TimelineWindowPreferences
		{
			public bool frameSnap = true;

			public bool edgeSnaps = true;

			public bool playRangeLoopMode = true;
		}

		private class SequnenceMenuNameFormater
		{
			private Dictionary<int, int> m_UniqueItem = new Dictionary<int, int>();

			public string Format(string text)
			{
				int hashCode = text.GetHashCode();
				int num = 0;
				string result;
				if (this.m_UniqueItem.ContainsKey(hashCode))
				{
					num = this.m_UniqueItem[hashCode];
					num++;
					this.m_UniqueItem[hashCode] = num;
					result = string.Format("{0}{1}", text, num);
				}
				else
				{
					this.m_UniqueItem.Add(hashCode, num);
					result = text;
				}
				return result;
			}
		}

		internal enum PlayModeState
		{
			Paused,
			Stopped,
			Playing
		}

		private enum TimelineItemArea
		{
			Header,
			Lines
		}

		internal delegate void TimelineViewType(Rect clientRect, TimelineWindow.TimelineState state, TimelineModeGUIState trackState);

		internal class TimelineView
		{
			public TimelineWindow.TimelineViewType m_Callback;

			public string m_Name;

			public TimelineView(string name, TimelineWindow.TimelineViewType callback)
			{
				this.m_Name = name;
				this.m_Callback = callback;
			}
		}

		public enum PreviewPlayMode
		{
			Hold,
			Once,
			Loop,
			None
		}

		internal class StateEventArgs : EventArgs
		{
			public string propertyChanged;

			public TimelineWindow.TimelineState state;
		}

		[SerializeField]
		private TimelineWindow.TimelineWindowPreferences m_Preferences = new TimelineWindow.TimelineWindowPreferences();

		[SerializeField]
		private bool m_Locked;

		private readonly PreviewResizer m_PreviewResizer = new PreviewResizer();

		private bool m_LastFrameHadSequence;

		private int m_CurrentSceneHashCode = -1;

		[NonSerialized]
		private bool m_HasBeenInitialized;

		[SerializeField]
		private List<BreadcrumbElement> m_BreadcrumbPath = new List<BreadcrumbElement>();

		public static readonly float kBreadcrumbHeight = 28f;

		public static readonly float kBreadCrumbDelta = 270f;

		private TimeAreaItem m_TimelineDuration;

		private static TimelineMode m_ActiveMode;

		private static TimelineMode m_InactiveMode;

		private static TimelineMode m_EditAssetMode;

		private Vector2 m_HierachySplitterMinMax = new Vector2(0.15f, 10.5f);

		[SerializeField]
		private float m_HierarchySplitterPerc = 0.2f;

		private int m_SplitterCaptured;

		private Rect m_SplitterLineRect = Rect.get_zero();

		private List<TimelineWindow.TimelineView> m_Views;

		[SerializeField]
		private SequencerModeType m_CurrentMode;

		internal const int kTimeCodeFieldWidth = 70;

		internal const float kScrollBarHeight = 10f;

		private static readonly float kScrollbarSize = 25f;

		private List<Manipulator> m_Manipulators = new List<Manipulator>();

		private Control.MouseDownState m_MouseDownState = Control.MouseDownState.None;

		private float m_MouseDownTime;

		private TimeAreaItem m_PlayRangeEnd;

		private TimeAreaItem m_PlayRangeStart;

		private double m_PreviousTime;

		[SerializeField]
		private int m_LastSelectedObjectID;

		[NonSerialized]
		private TimeArea m_TimeArea;

		private static readonly float kTimeAreaYPosition = 17f;

		private static readonly float kTimeAreaHeight = 25f;

		private static readonly float kTimeAreaMinWidth = 50f;

		private float m_LastFrameRate;

		private TimeAreaItem m_PlayHead;

		private const float k_TracksYPosition = 42f;

		private const float k_CreateButtonWidth = 70f;

		private const float k_TrackHeaderEndZoneWidth = 50f;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cache0;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cache1;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cache2;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cache3;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cache4;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cache5;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cache6;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cache7;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cache8;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cache9;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cacheA;

		[CompilerGenerated]
		private static TimelineUIEvent <>f__mg$cacheB;

		public event TimelineUIEvent MouseDown
		{
			add
			{
				TimelineUIEvent timelineUIEvent = this.MouseDown;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.MouseDown, (TimelineUIEvent)Delegate.Combine(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
			remove
			{
				TimelineUIEvent timelineUIEvent = this.MouseDown;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.MouseDown, (TimelineUIEvent)Delegate.Remove(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
		}

		public event TimelineUIEvent MouseDrag
		{
			add
			{
				TimelineUIEvent timelineUIEvent = this.MouseDrag;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.MouseDrag, (TimelineUIEvent)Delegate.Combine(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
			remove
			{
				TimelineUIEvent timelineUIEvent = this.MouseDrag;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.MouseDrag, (TimelineUIEvent)Delegate.Remove(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
		}

		public event TimelineUIEvent MouseWheel
		{
			add
			{
				TimelineUIEvent timelineUIEvent = this.MouseWheel;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.MouseWheel, (TimelineUIEvent)Delegate.Combine(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
			remove
			{
				TimelineUIEvent timelineUIEvent = this.MouseWheel;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.MouseWheel, (TimelineUIEvent)Delegate.Remove(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
		}

		public event TimelineUIEvent MouseMove
		{
			add
			{
				TimelineUIEvent timelineUIEvent = this.MouseMove;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.MouseMove, (TimelineUIEvent)Delegate.Combine(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
			remove
			{
				TimelineUIEvent timelineUIEvent = this.MouseMove;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.MouseMove, (TimelineUIEvent)Delegate.Remove(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
		}

		public event TimelineUIEvent MouseUp
		{
			add
			{
				TimelineUIEvent timelineUIEvent = this.MouseUp;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.MouseUp, (TimelineUIEvent)Delegate.Combine(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
			remove
			{
				TimelineUIEvent timelineUIEvent = this.MouseUp;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.MouseUp, (TimelineUIEvent)Delegate.Remove(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
		}

		public event TimelineUIEvent DoubleClick
		{
			add
			{
				TimelineUIEvent timelineUIEvent = this.DoubleClick;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.DoubleClick, (TimelineUIEvent)Delegate.Combine(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
			remove
			{
				TimelineUIEvent timelineUIEvent = this.DoubleClick;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.DoubleClick, (TimelineUIEvent)Delegate.Remove(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
		}

		public event TimelineUIEvent KeyDown
		{
			add
			{
				TimelineUIEvent timelineUIEvent = this.KeyDown;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.KeyDown, (TimelineUIEvent)Delegate.Combine(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
			remove
			{
				TimelineUIEvent timelineUIEvent = this.KeyDown;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.KeyDown, (TimelineUIEvent)Delegate.Remove(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
		}

		public event TimelineUIEvent KeyUp
		{
			add
			{
				TimelineUIEvent timelineUIEvent = this.KeyUp;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.KeyUp, (TimelineUIEvent)Delegate.Combine(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
			remove
			{
				TimelineUIEvent timelineUIEvent = this.KeyUp;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.KeyUp, (TimelineUIEvent)Delegate.Remove(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
		}

		public event TimelineUIEvent DragPerform
		{
			add
			{
				TimelineUIEvent timelineUIEvent = this.DragPerform;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.DragPerform, (TimelineUIEvent)Delegate.Combine(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
			remove
			{
				TimelineUIEvent timelineUIEvent = this.DragPerform;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.DragPerform, (TimelineUIEvent)Delegate.Remove(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
		}

		public event TimelineUIEvent DragExited
		{
			add
			{
				TimelineUIEvent timelineUIEvent = this.DragExited;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.DragExited, (TimelineUIEvent)Delegate.Combine(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
			remove
			{
				TimelineUIEvent timelineUIEvent = this.DragExited;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.DragExited, (TimelineUIEvent)Delegate.Remove(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
		}

		public event TimelineUIEvent DragUpdated
		{
			add
			{
				TimelineUIEvent timelineUIEvent = this.DragUpdated;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.DragUpdated, (TimelineUIEvent)Delegate.Combine(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
			remove
			{
				TimelineUIEvent timelineUIEvent = this.DragUpdated;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.DragUpdated, (TimelineUIEvent)Delegate.Remove(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
		}

		public event TimelineUIEvent Overlay
		{
			add
			{
				TimelineUIEvent timelineUIEvent = this.Overlay;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.Overlay, (TimelineUIEvent)Delegate.Combine(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
			remove
			{
				TimelineUIEvent timelineUIEvent = this.Overlay;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.Overlay, (TimelineUIEvent)Delegate.Remove(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
		}

		public event TimelineUIEvent ContextClick
		{
			add
			{
				TimelineUIEvent timelineUIEvent = this.ContextClick;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.ContextClick, (TimelineUIEvent)Delegate.Combine(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
			remove
			{
				TimelineUIEvent timelineUIEvent = this.ContextClick;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.ContextClick, (TimelineUIEvent)Delegate.Remove(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
		}

		public event TimelineUIEvent ValidateCommand
		{
			add
			{
				TimelineUIEvent timelineUIEvent = this.ValidateCommand;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.ValidateCommand, (TimelineUIEvent)Delegate.Combine(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
			remove
			{
				TimelineUIEvent timelineUIEvent = this.ValidateCommand;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.ValidateCommand, (TimelineUIEvent)Delegate.Remove(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
		}

		public event TimelineUIEvent ExecuteCommand
		{
			add
			{
				TimelineUIEvent timelineUIEvent = this.ExecuteCommand;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.ExecuteCommand, (TimelineUIEvent)Delegate.Combine(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
			remove
			{
				TimelineUIEvent timelineUIEvent = this.ExecuteCommand;
				TimelineUIEvent timelineUIEvent2;
				do
				{
					timelineUIEvent2 = timelineUIEvent;
					timelineUIEvent = Interlocked.CompareExchange<TimelineUIEvent>(ref this.ExecuteCommand, (TimelineUIEvent)Delegate.Remove(timelineUIEvent2, value), timelineUIEvent);
				}
				while (timelineUIEvent != timelineUIEvent2);
			}
		}

		public event EventHandler<TimelineWindow.StateEventArgs> OnStateChange
		{
			add
			{
				EventHandler<TimelineWindow.StateEventArgs> eventHandler = this.OnStateChange;
				EventHandler<TimelineWindow.StateEventArgs> eventHandler2;
				do
				{
					eventHandler2 = eventHandler;
					eventHandler = Interlocked.CompareExchange<EventHandler<TimelineWindow.StateEventArgs>>(ref this.OnStateChange, (EventHandler<TimelineWindow.StateEventArgs>)Delegate.Combine(eventHandler2, value), eventHandler);
				}
				while (eventHandler != eventHandler2);
			}
			remove
			{
				EventHandler<TimelineWindow.StateEventArgs> eventHandler = this.OnStateChange;
				EventHandler<TimelineWindow.StateEventArgs> eventHandler2;
				do
				{
					eventHandler2 = eventHandler;
					eventHandler = Interlocked.CompareExchange<EventHandler<TimelineWindow.StateEventArgs>>(ref this.OnStateChange, (EventHandler<TimelineWindow.StateEventArgs>)Delegate.Remove(eventHandler2, value), eventHandler);
				}
				while (eventHandler != eventHandler2);
			}
		}

		public static TimelineWindow instance
		{
			get;
			private set;
		}

		public Rect clientArea
		{
			get;
			set;
		}

		public bool isDragging
		{
			get;
			set;
		}

		public static DirectorStyles styles
		{
			get
			{
				return DirectorStyles.Instance;
			}
		}

		public List<TimelineTrackBaseGUI> allTracks
		{
			get
			{
				return (this.treeView == null) ? new List<TimelineTrackBaseGUI>() : this.treeView.allTrackGuis;
			}
		}

		public TimelineWindow.TimelineState state
		{
			get;
			private set;
		}

		public bool locked
		{
			get
			{
				return !(this.timeline == null) && this.m_Locked;
			}
			set
			{
				this.m_Locked = value;
			}
		}

		public TimelineAsset timeline
		{
			get;
			set;
		}

		private List<BreadcrumbElement> breadcrumbPath
		{
			get
			{
				return this.m_BreadcrumbPath;
			}
			set
			{
				this.m_BreadcrumbPath = new List<BreadcrumbElement>(value);
			}
		}

		private float breadCrumbAreaWidth
		{
			get
			{
				return this.timeAreaBounds.get_width() - TimelineWindow.kBreadCrumbDelta;
			}
		}

		private TimelineMode currentMode
		{
			get
			{
				TimelineMode result;
				switch (this.m_CurrentMode)
				{
				case SequencerModeType.Active:
					if (TimelineWindow.m_ActiveMode == null)
					{
						TimelineWindow.m_ActiveMode = new TimelineActiveMode();
					}
					result = TimelineWindow.m_ActiveMode;
					return result;
				case SequencerModeType.EditAsset:
					if (TimelineWindow.m_EditAssetMode == null)
					{
						TimelineWindow.m_EditAssetMode = new TimelineAssetEditionMode();
					}
					result = TimelineWindow.m_EditAssetMode;
					return result;
				}
				if (TimelineWindow.m_InactiveMode == null)
				{
					TimelineWindow.m_InactiveMode = new TimelineInactiveMode();
				}
				result = TimelineWindow.m_InactiveMode;
				return result;
			}
		}

		public Rect sequenceHeaderBounds
		{
			get
			{
				return Rect.MinMaxRect(0f, this.timeAreaBounds.get_yMax(), this.state.sequencerHeaderWidth, base.get_position().get_yMax() - 10f);
			}
		}

		public Rect clipArea
		{
			get
			{
				return new Rect(this.state.sequencerHeaderWidth, this.timeAreaBounds.get_yMax(), this.clientArea.get_width() - this.state.sequencerHeaderWidth - TimelineWindow.kScrollbarSize, this.clientArea.get_height() - this.timeAreaBounds.get_height() - TimelineWindow.kScrollbarSize);
			}
		}

		public Rect timeAreaBounds
		{
			get
			{
				return new Rect(this.state.sequencerHeaderWidth, TimelineWindow.kTimeAreaYPosition, Mathf.Max(base.get_position().get_width() - this.state.sequencerHeaderWidth, TimelineWindow.kTimeAreaMinWidth), TimelineWindow.kTimeAreaHeight);
			}
		}

		public Rect tracksBounds
		{
			get
			{
				Rect timeAreaBounds = this.timeAreaBounds;
				return Rect.MinMaxRect(timeAreaBounds.get_xMin(), timeAreaBounds.get_yMax(), timeAreaBounds.get_xMax() - 1f, timeAreaBounds.get_yMax() + base.get_position().get_height());
			}
		}

		public Rect treeviewBounds
		{
			get
			{
				return new Rect(0f, 42f, base.get_position().get_width(), this.clientArea.get_height() - 42f);
			}
		}

		public TimelineTreeViewGUI treeView
		{
			get;
			private set;
		}

		public TimelineWindow()
		{
			this.InitializeIControl();
			this.InitializeManipulators();
		}

		private void OnEnable()
		{
			base.set_titleContent(base.GetLocalizedTitleContent());
			this.m_PreviewResizer.Init("TimelineWindow");
			if (TimelineWindow.instance == null)
			{
				TimelineWindow.instance = this;
			}
			AnimationClipCurveCache.Instance.OnEnable();
			if (this.currentMode == null)
			{
				this.EnableInactiveMode();
			}
			if (this.state == null)
			{
				this.state = new TimelineWindow.TimelineState(this);
				this.OnSelectionChange();
				if (this.m_BreadcrumbPath.Count > 0)
				{
					BreadcrumbElement breadcrumbElement = this.m_BreadcrumbPath[0];
					breadcrumbElement.clip = null;
					this.m_BreadcrumbPath[0] = breadcrumbElement;
				}
			}
			TimelineWindow.InitializeShortcuts();
		}

		private void OnDisable()
		{
			if (TimelineWindow.instance == this)
			{
				TimelineWindow.instance = null;
			}
			if (this.state != null)
			{
				this.state.Reset();
			}
			if (TimelineWindow.instance == null)
			{
				SelectionManager.RemoveTimelineSelection();
			}
			AnimationClipCurveCache.Instance.OnDisable();
		}

		private void OnDestroy()
		{
			if (this.state != null)
			{
				this.state.OnDestroy();
			}
			this.m_HasBeenInitialized = false;
			this.RemoveEditorCallbacks();
		}

		private void OnLostFocus()
		{
			this.isDragging = false;
			if (this.state != null)
			{
				this.state.captured.Clear();
			}
			base.Repaint();
		}

		private void OnFocus()
		{
			this.OnSelectionChange();
		}

		private void OnGUI()
		{
			this.DetectActiveSceneChanges();
			this.DetectStateChanges();
			if (this.state != null)
			{
				this.state.ProcessStartFramePendingUpdates();
			}
			if (Event.get_current().get_type() == 3 && this.state != null && this.state.mouseDragLag > 0f)
			{
				this.state.mouseDragLag -= Time.get_deltaTime();
			}
			else if (!TimelineWindow.PerformUndo())
			{
				if (EditorApplication.get_isPlaying())
				{
					if (this.state != null)
					{
						if (this.state.recording)
						{
							this.state.recording = false;
						}
					}
					base.Repaint();
				}
				this.clientArea = base.get_position();
				this.DoLayout();
				if (this.state.captured.Count > 0)
				{
					foreach (IControl current in this.state.captured)
					{
						current.DrawOverlays(Event.get_current(), this.state);
					}
					base.Repaint();
				}
				if (this.state.showQuadTree)
				{
					this.state.quadTree.DebugDraw();
				}
				if (Event.get_current().get_type() == 7)
				{
					this.RebuildGraphIfNecessary(true);
					if (this.state != null)
					{
						this.state.ProcessEndFramePendingUpdates();
					}
					Control.DrawCursors();
				}
			}
		}

		private void DetectActiveSceneChanges()
		{
			if (this.m_CurrentSceneHashCode == -1)
			{
				this.m_CurrentSceneHashCode = SceneManager.GetActiveScene().GetHashCode();
			}
			if (this.m_CurrentSceneHashCode != SceneManager.GetActiveScene().GetHashCode())
			{
				bool flag = false;
				for (int i = 0; i < SceneManager.get_sceneCount(); i++)
				{
					Scene sceneAt = SceneManager.GetSceneAt(i);
					if (sceneAt.GetHashCode() == this.m_CurrentSceneHashCode && sceneAt.get_isLoaded())
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					this.SetCurrentTimeline(null, null);
					this.m_CurrentSceneHashCode = SceneManager.GetActiveScene().GetHashCode();
				}
			}
		}

		private void DetectStateChanges()
		{
			if (this.m_LastFrameHadSequence && this.timeline == null)
			{
				this.SetCurrentTimeline(null, null);
			}
			this.m_LastFrameHadSequence = (this.timeline != null);
			if (this.state != null)
			{
				if (this.state.currentDirector == null)
				{
					this.state.recording = false;
					if (!this.locked)
					{
						GameObject gameObject = (!(Selection.get_activeObject() != null)) ? null : (Selection.get_activeObject() as GameObject);
						PlayableDirector playableDirector = (!(gameObject != null)) ? null : gameObject.GetComponent<PlayableDirector>();
						if (playableDirector != null)
						{
							this.SetCurrentTimeline(playableDirector.get_playableAsset() as TimelineAsset, playableDirector);
						}
					}
				}
				else if (this.state.timeline != this.state.currentDirector.get_playableAsset())
				{
					if (!this.locked)
					{
						this.SetCurrentTimeline(this.state.currentDirector.get_playableAsset() as TimelineAsset, this.state.currentDirector);
					}
					else
					{
						this.SetCurrentTimeline(this.state.timeline, null);
					}
				}
			}
		}

		private void Initialize()
		{
			bool flag = false;
			if (!this.m_HasBeenInitialized)
			{
				this.InitializeStateChange();
				this.InitializeEditorCallbacks();
				this.m_HasBeenInitialized = true;
			}
			this.InitializeViews();
			this.InitializeTimeArea();
			if (this.treeView == null && this.timeline != null)
			{
				flag = true;
				this.treeView = new TimelineTreeViewGUI(this, this.timeline, base.get_position());
				this.state.Refresh(false);
			}
			if (flag)
			{
				TimelineWindow.StateEventArgs e = new TimelineWindow.StateEventArgs
				{
					state = this.state,
					propertyChanged = ""
				};
				if (this.OnStateChange != null)
				{
					this.OnStateChange(this, e);
				}
			}
		}

		private static bool PerformUndo()
		{
			return Event.get_current().get_isKey() && Event.get_current().get_keyCode() == 122 && EditorGUI.get_actionKey();
		}

		public void RebuildGraphIfNecessary(bool evaluate = true)
		{
			if (this.state != null && !(this.state.currentDirector == null) && !(this.timeline == null))
			{
				if (!EditorApplication.get_isPlaying())
				{
					if (this.state.rebuildGraph)
					{
						double time = this.state.time;
						this.state.GatherProperties(this.state.currentDirector);
						this.state.Stop();
						this.state.Play();
						this.state.time = time;
						if (evaluate)
						{
							this.state.EvaluateImmediate();
						}
						base.Repaint();
					}
					this.state.rebuildGraph = false;
				}
			}
		}

		public TrackAsset AddTrack(Type type, TrackAsset parent = null, string name = null)
		{
			TrackAsset result;
			if (this.state.timeline == null)
			{
				result = null;
			}
			else
			{
				TrackAsset trackAsset = this.state.timeline.CreateTrack(type, parent, name);
				if (trackAsset != null)
				{
					this.state.Refresh();
				}
				result = trackAsset;
			}
			return result;
		}

		public T AddTrack<T>(TrackAsset parent = null, string name = null) where T : TrackAsset, new()
		{
			return (T)((object)this.AddTrack(typeof(T), parent, name));
		}

		internal static bool IsEditingTimelineAsset(TimelineAsset timelineAsset)
		{
			return TimelineWindow.instance != null && TimelineWindow.instance.state != null && TimelineWindow.instance.state.timeline == timelineAsset;
		}

		internal static void RepaintIfEditingTimelineAsset(TimelineAsset timelineAsset)
		{
			if (TimelineWindow.IsEditingTimelineAsset(timelineAsset))
			{
				TimelineWindow.instance.Repaint();
			}
		}

		[MenuItem("Assets/Create/Timeline", false, 450)]
		public static void CreateNewTimeline()
		{
			TimelineUtility.CreateAsset<TimelineAsset>("New Timeline.playable");
		}

		[MenuItem("Window/Timeline", false, 2016)]
		public static void ShowWindow()
		{
			EditorWindow.GetWindow<TimelineWindow>(new Type[]
			{
				typeof(SceneView)
			});
			TimelineWindow.instance.Focus();
		}

		[OnOpenAsset(1)]
		public static bool OnDoubleClick(int instanceID, int line)
		{
			TimelineAsset timelineAsset = EditorUtility.InstanceIDToObject(instanceID) as TimelineAsset;
			bool result;
			if (timelineAsset == null)
			{
				result = false;
			}
			else
			{
				TimelineWindow.ShowWindow();
				TimelineWindow.instance.SetCurrentTimeline(timelineAsset, null);
				result = true;
			}
			return result;
		}

		protected virtual void ShowButton(Rect r)
		{
			bool flag = this.state == null || this.state.timeline == null;
			using (new EditorGUI.DisabledScope(flag))
			{
				EditorGUI.BeginChangeCheck();
				bool locked = GUI.Toggle(r, this.locked, GUIContent.none, DirectorStyles.Instance.lockButton);
				if (EditorGUI.EndChangeCheck())
				{
					this.locked = locked;
					if (!this.locked)
					{
						this.OnSelectionChange();
					}
				}
			}
		}

		private static void InitializeShortcuts()
		{
			Shortcuts.AddAction("Play", 32, 0);
			Shortcuts.AddAction("FrameAll", 97, 0);
			Shortcuts.AddAction("FrameSelection", 102, 0);
			Shortcuts.AddAction("PrevFrame", 44, 0);
			Shortcuts.AddAction("NextFrame", 46, 0);
			Shortcuts.AddAction("PrevKey", 44, 2);
			Shortcuts.AddAction("NextKey", 46, 2);
			Shortcuts.AddAction("GotoStart", 44, 1);
			Shortcuts.AddAction("GotoEnd", 46, 1);
			Shortcuts.AddAction("NudgeLeft", 49, 0);
			Shortcuts.AddAction("NudgeLeft", 257, 0);
			Shortcuts.AddAction("NudgeRight", 50, 0);
			Shortcuts.AddAction("NudgeRight", 258, 0);
			Shortcuts.AddAction("Split", 115, 0);
			Shortcuts.AddAction("TrimStart", 105, 0);
			Shortcuts.AddAction("TrimEnd", 111, 0);
			Shortcuts.AddAction("ToggleLock", 108, 0);
			Shortcuts.AddAction("ToggleMute", 109, 0);
			Shortcuts.AddAction("Delete", new KeyCode[]
			{
				127,
				8
			}, 64);
			string arg_123_0 = "ZoomIn";
			KeyCode[] expr_117 = new KeyCode[3];
			RuntimeHelpers.InitializeArray(expr_117, fieldof(<PrivateImplementationDetails>.$field-08C53541A11F0F3BB982CFDF7AFC9EC3B7DAEDB7).FieldHandle);
			Shortcuts.AddAction(arg_123_0, expr_117, 0);
			string arg_13F_0 = "ZoomOut";
			KeyCode[] expr_133 = new KeyCode[3];
			RuntimeHelpers.InitializeArray(expr_133, fieldof(<PrivateImplementationDetails>.$field-36FAC96FADAC0736A9418803146C6A077C38348E).FieldHandle);
			Shortcuts.AddAction(arg_13F_0, expr_133, 0);
			Shortcuts.AddAction("Copy", 99, 10);
			Shortcuts.AddAction("Paste", 118, 10);
			Shortcuts.AddAction("Duplicate", 100, 10);
		}

		private void InitWithSequence(TimelineAsset seq)
		{
			this.m_LastFrameHadSequence = (seq != null);
			this.state.Reset();
			this.treeView = null;
			this.timeline = seq;
			this.ResetBreadCrumbs();
			this.EnableEditAssetMode();
		}

		public void SetCurrentTimeline(TimelineAsset seq, PlayableDirector instanceOfDirector)
		{
			if (this.state != null)
			{
				bool flag = seq == null || instanceOfDirector != null || this.timeline != seq || this.state.currentDirector != null;
				if (flag)
				{
					this.InitWithSequence(seq);
				}
				if (seq != null && instanceOfDirector != null)
				{
					this.BindToDirector(instanceOfDirector);
					this.EnableActiveMode();
				}
				this.Upgrade(seq);
				base.Repaint();
				if (instanceOfDirector != null)
				{
					this.m_LastSelectedObjectID = instanceOfDirector.get_gameObject().GetInstanceID();
				}
				else if (seq != null)
				{
					this.m_LastSelectedObjectID = seq.GetInstanceID();
				}
				else
				{
					this.m_LastSelectedObjectID = 0;
				}
				this.m_LastFrameHadSequence = (seq != null);
				TimelineWindowViewPrefs.Save();
			}
		}

		private void BindToDirector(PlayableDirector obj)
		{
			if (this.state != null && !(obj == null))
			{
				if (this.state.currentDirector != obj)
				{
					this.state.Stop();
					this.state.currentDirector = obj;
				}
				this.state.rebuildGraph = true;
				base.Repaint();
			}
		}

		private void ResetBreadCrumbs()
		{
			this.state.BreadcrumbSetRoot(this.timeline);
			base.Repaint();
		}

		private void DoBreadcrumbGUI()
		{
			using (new EditorGUI.DisabledScope(this.currentMode.headerState.breadCrumb == TimelineModeGUIState.Disabled))
			{
				BreadcrumbDrawer.Draw(this.breadCrumbAreaWidth, (this.state == null || !(this.state.timeline != null)) ? string.Empty : this.state.timeline.get_name(), (this.state == null || !(this.state.currentDirector != null)) ? string.Empty : this.state.currentDirector.get_name());
			}
		}

		private void DoSequenceSelectorGUI()
		{
			using (new EditorGUI.DisabledScope(this.currentMode.headerState.sequenceSelector == TimelineModeGUIState.Disabled))
			{
				if (GUILayout.Button(DirectorStyles.sequenceSelectorIcon, EditorStyles.get_toolbarPopup(), new GUILayoutOption[]
				{
					GUILayout.Width(32f)
				}))
				{
					PlayableDirector[] directorsInSceneUsingAsset = TimelineUtility.GetDirectorsInSceneUsingAsset(null);
					GenericMenu genericMenu = new GenericMenu();
					TimelineWindow.SequnenceMenuNameFormater sequnenceMenuNameFormater = new TimelineWindow.SequnenceMenuNameFormater();
					PlayableDirector[] array = directorsInSceneUsingAsset;
					for (int i = 0; i < array.Length; i++)
					{
						PlayableDirector playableDirector = array[i];
						if (playableDirector.get_playableAsset() is TimelineAsset)
						{
							string text = sequnenceMenuNameFormater.Format(playableDirector.get_playableAsset().get_name() + " (" + playableDirector.get_name() + ")");
							bool flag = this.state.currentDirector == playableDirector;
							genericMenu.AddItem(new GUIContent(text), flag, delegate(object arg)
							{
								PlayableDirector playableDirector2 = (PlayableDirector)arg;
								if (playableDirector2)
								{
									this.SetCurrentTimeline(playableDirector2.get_playableAsset() as TimelineAsset, playableDirector2);
								}
							}, playableDirector);
						}
					}
					if (directorsInSceneUsingAsset.Length == 0)
					{
						genericMenu.AddDisabledItem(DirectorStyles.noTimelinesInScene);
					}
					genericMenu.ShowAsContext();
				}
				GUILayout.Space(10f);
			}
		}

		private void DurationGUI(TimelineWindow.TimelineItemArea area)
		{
			if (this.currentMode.ShouldShowTimeArea(this.state))
			{
				if (this.state.timeline.durationMode != TimelineAsset.DurationMode.BasedOnClips || this.state.duration > 0.0)
				{
					if (this.m_TimelineDuration == null)
					{
						this.m_TimelineDuration = new TimeAreaItem(TimelineWindow.styles.endmarker, new Action<double, bool>(this.OnTrackDurationDrag))
						{
							tooltip = "End of sequence marker",
							boundOffset = new Vector2(0f, -DirectorStyles.kDurationGuiThickness)
						};
					}
					bool flag = area == TimelineWindow.TimelineItemArea.Header;
					this.DrawDuration(flag, !flag);
				}
			}
		}

		private void DrawDuration(bool drawhead, bool drawline)
		{
			double duration = this.state.duration;
			if (this.state.TimeIsInRange((float)duration))
			{
				Color colorEndmarker = DirectorStyles.Instance.customSkin.colorEndmarker;
				Color headColor = Color.get_white();
				bool flag = !EditorApplication.get_isPlaying() && this.state.timeline.durationMode == TimelineAsset.DurationMode.FixedLength;
				if (flag)
				{
					if (this.m_TimelineDuration.bounds.Contains(Event.get_current().get_mousePosition()))
					{
						if (this.m_PlayHead != null && this.m_PlayHead.bounds.Contains(Event.get_current().get_mousePosition()))
						{
							flag = false;
						}
						else if (this.m_TimelineDuration.OnEvent(Event.get_current(), this.state, false))
						{
							Event.get_current().Use();
						}
					}
				}
				else
				{
					colorEndmarker.a *= 0.66f;
					headColor = DirectorStyles.Instance.customSkin.colorDuration;
				}
				this.m_TimelineDuration.lineColor = colorEndmarker;
				this.m_TimelineDuration.headColor = headColor;
				this.m_TimelineDuration.drawHead = drawhead;
				this.m_TimelineDuration.drawLine = drawline;
				this.m_TimelineDuration.canMoveHead = flag;
				Rect timeAreaBounds = this.timeAreaBounds;
				timeAreaBounds.set_height(this.clientArea.get_height());
				this.m_TimelineDuration.Draw(timeAreaBounds, this.state, duration);
			}
			if (this.timeline != null && drawhead)
			{
				float num = this.state.TimeToPixel(duration);
				if (num > this.state.timeAreaRect.get_xMin())
				{
					Color colorDurationLine = DirectorStyles.Instance.customSkin.colorDurationLine;
					Rect rect = Rect.MinMaxRect(this.state.timeAreaRect.get_xMin(), this.timeAreaBounds.get_y() - DirectorStyles.kDurationGuiThickness + this.timeAreaBounds.get_height(), num, this.timeAreaBounds.get_y() + this.timeAreaBounds.get_height());
					EditorGUI.DrawRect(rect, colorDurationLine);
				}
			}
		}

		private void OnTrackDurationDrag(double newTime, bool initialFrame)
		{
			if (this.state.timeline.durationMode == TimelineAsset.DurationMode.FixedLength)
			{
				this.state.timeline.fixedDuration = newTime;
				this.state.UpdateRootPlayableDuration(newTime);
			}
			this.m_TimelineDuration.showTooltip = true;
		}

		private void InitializeEditorCallbacks()
		{
			Undo.postprocessModifications = (Undo.PostprocessModifications)Delegate.Combine(Undo.postprocessModifications, new Undo.PostprocessModifications(this.PostprocessAnimationRecordingModifications));
			Undo.postprocessModifications = (Undo.PostprocessModifications)Delegate.Combine(Undo.postprocessModifications, new Undo.PostprocessModifications(this.ProcessAssetModifications));
			Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Combine(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(this.OnUndoRedo));
			EditorApplication.add_playModeStateChanged(new Action<PlayModeStateChange>(this.OnPlayModeStateChanged));
			AnimationUtility.onCurveWasModified = (AnimationUtility.OnCurveWasModified)Delegate.Combine(AnimationUtility.onCurveWasModified, new AnimationUtility.OnCurveWasModified(this.OnCurveModified));
			EditorApplication.editorApplicationQuit = (UnityAction)Delegate.Combine(EditorApplication.editorApplicationQuit, new UnityAction(this.OnEditorQuit));
		}

		private void OnEditorQuit()
		{
			TimelineWindowViewPrefs.Save();
		}

		private void RemoveEditorCallbacks()
		{
			EditorApplication.remove_playModeStateChanged(new Action<PlayModeStateChange>(this.OnPlayModeStateChanged));
			Undo.undoRedoPerformed = (Undo.UndoRedoCallback)Delegate.Remove(Undo.undoRedoPerformed, new Undo.UndoRedoCallback(this.OnUndoRedo));
			Undo.postprocessModifications = (Undo.PostprocessModifications)Delegate.Remove(Undo.postprocessModifications, new Undo.PostprocessModifications(this.PostprocessAnimationRecordingModifications));
			Undo.postprocessModifications = (Undo.PostprocessModifications)Delegate.Remove(Undo.postprocessModifications, new Undo.PostprocessModifications(this.ProcessAssetModifications));
			AnimationUtility.onCurveWasModified = (AnimationUtility.OnCurveWasModified)Delegate.Remove(AnimationUtility.onCurveWasModified, new AnimationUtility.OnCurveWasModified(this.OnCurveModified));
			EditorApplication.editorApplicationQuit = (UnityAction)Delegate.Remove(EditorApplication.editorApplicationQuit, new UnityAction(this.OnEditorQuit));
		}

		private void OnCurveModified(AnimationClip clip, EditorCurveBinding binding, AnimationUtility.CurveModifiedType type)
		{
			InspectorWindow.RepaintAllInspectors();
			if (type != 2)
			{
				this.state.Evaluate();
			}
			if (this.state != null && type != 1)
			{
				this.state.rebuildGraph = true;
			}
		}

		private void OnPlayModeStateChanged(PlayModeStateChange playModeState)
		{
			if (playModeState == 1 || playModeState == 3)
			{
				TimelineWindowViewPrefs.Save();
			}
			bool flag = playModeState == 1 || playModeState == 3;
			if (flag && this.state != null)
			{
				this.state.Stop();
			}
		}

		private UndoPropertyModification[] PostprocessAnimationRecordingModifications(UndoPropertyModification[] modifications)
		{
			UndoPropertyModification[] result;
			if (!this.state.recording)
			{
				result = modifications;
			}
			else
			{
				UndoPropertyModification[] array = TimelineRecording.ProcessUndoModification(modifications, this.state);
				if (array != modifications)
				{
					bool flag = EditorWindow.get_focusedWindow() == null || EditorWindow.get_focusedWindow() is InspectorWindow || EditorWindow.get_focusedWindow() is TimelineWindow;
					if (flag)
					{
						base.Repaint();
					}
				}
				result = array;
			}
			return result;
		}

		private UndoPropertyModification[] ProcessAssetModifications(UndoPropertyModification[] modifications)
		{
			bool flag = false;
			int num = 0;
			while (num < modifications.Length && !flag)
			{
				UndoPropertyModification mod = modifications[num];
				if (mod.previousValue != null && mod.previousValue.target is AvatarMask)
				{
					flag = (this.state.timeline != null && this.state.timeline.GetOutputTracks().OfType<AnimationTrack>().Any((AnimationTrack x) => mod.previousValue.target == x.avatarMask));
				}
				num++;
			}
			if (flag)
			{
				this.state.rebuildGraph = true;
				base.Repaint();
			}
			return modifications;
		}

		private void OnUndoRedo()
		{
			List<string> list = new List<string>();
			List<string> list2 = new List<string>();
			Undo.GetRecords(list, list2);
			bool flag = list2.Any((string x) => x.IndexOf("Timeline", StringComparison.OrdinalIgnoreCase) >= 0);
			if (!flag)
			{
				flag = list.Any((string x) => x.IndexOf("Timeline", StringComparison.OrdinalIgnoreCase) >= 0);
			}
			if (flag)
			{
				if (this.state != null)
				{
					this.state.RebindAnimators();
					this.state.Refresh();
				}
				base.Repaint();
			}
		}

		private void InitializeViews()
		{
			if (this.m_Views == null)
			{
				this.m_Views = new List<TimelineWindow.TimelineView>();
				this.m_Views.Add(new TimelineWindow.TimelineView("Timeline Editor", new TimelineWindow.TimelineViewType(this.TracksGUI)));
				this.m_Views.Add(new TimelineWindow.TimelineView("Curves", new TimelineWindow.TimelineViewType(this.EmptyViewGUI)));
				this.state.activeView = 0;
			}
		}

		private void EnableActiveMode()
		{
			this.m_CurrentMode = SequencerModeType.Active;
		}

		private void EnableInactiveMode()
		{
			this.m_CurrentMode = SequencerModeType.Inactive;
		}

		private void EnableEditAssetMode()
		{
			this.m_CurrentMode = SequencerModeType.EditAsset;
		}

		private void DoLayout()
		{
			this.Initialize();
			this.HandleSplitterResize();
			this.RunCaptureSession();
			this.ProcessManipulators();
			this.SequencerGUI();
		}

		private void TimelineSectionGUI()
		{
			GUILayout.BeginVertical(new GUILayoutOption[0]);
			GUILayout.BeginHorizontal(EditorStyles.get_toolbarButton(), new GUILayoutOption[]
			{
				GUILayout.Width(this.timeAreaBounds.get_width())
			});
			this.DoSequenceSelectorGUI();
			this.DoBreadcrumbGUI();
			this.OptionsGUI();
			GUILayout.EndHorizontal();
			this.TimelineGUI();
			GUILayout.EndVertical();
		}

		private void SplitterGUI()
		{
			if (this.timeline != null && this.timeline.tracks.Count > 0)
			{
				this.m_SplitterLineRect.Set(this.state.sequencerHeaderWidth - 1f, 0f, 1f, this.clientArea.get_height());
				EditorGUI.DrawRect(this.m_SplitterLineRect, DirectorStyles.Instance.customSkin.colorTopOutline3);
				this.m_SplitterLineRect.Set(this.state.sequencerHeaderWidth, 0f, 1f, this.clientArea.get_height());
				EditorGUI.DrawRect(this.m_SplitterLineRect, DirectorStyles.Instance.customSkin.colorTopOutline3);
			}
		}

		private void DrawTimelineBackgroundColor()
		{
			EditorGUI.DrawRect(this.timeAreaBounds, DirectorStyles.Instance.customSkin.colorTimelineBackground);
		}

		private void TrackViewsGUI()
		{
			this.m_Views[this.state.activeView].m_Callback(this.treeviewBounds, this.state, this.currentMode.TrackState(this.state));
		}

		private void SequencerGUI()
		{
			GUILayout.BeginVertical(new GUILayoutOption[0]);
			this.DrawTimelineBackgroundColor();
			this.DurationGUI(TimelineWindow.TimelineItemArea.Header);
			this.PlayRangeGUI(TimelineWindow.TimelineItemArea.Header);
			this.TimeCursorGUI(TimelineWindow.TimelineItemArea.Header);
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			this.SequencerHeaderGUI();
			this.TimelineSectionGUI();
			GUILayout.EndHorizontal();
			this.TrackViewsGUI();
			this.DurationGUI(TimelineWindow.TimelineItemArea.Lines);
			this.PlayRangeGUI(TimelineWindow.TimelineItemArea.Lines);
			this.TimeCursorGUI(TimelineWindow.TimelineItemArea.Lines);
			GUILayout.EndVertical();
			this.SplitterGUI();
		}

		private void RunCaptureSession()
		{
			if (Event.get_current().get_isKey() && Event.get_current().get_keyCode() == 27)
			{
				this.state.captured.Clear();
			}
			bool flag = false;
			if (this.state.captured.Count > 0)
			{
				List<IControl> list = new List<IControl>();
				foreach (IControl current in this.state.captured)
				{
					if (!list.Contains(current))
					{
						list.Add(current);
					}
				}
				for (int num = 0; num != list.Count; num++)
				{
					IControl control = list[num];
					bool flag2 = control.OnEvent(Event.get_current(), this.state, true);
					if (flag2)
					{
						flag = true;
					}
				}
			}
			if (flag)
			{
				if (Event.get_current().get_type() == 1)
				{
					GUIUtility.set_hotControl(0);
				}
				Event.get_current().Use();
			}
		}

		private void HandleSplitterResize()
		{
			this.state.mainAreaWidth = base.get_position().get_width();
			if (!(this.state.timeline == null))
			{
				Rect rect = new Rect(this.state.sequencerHeaderWidth - 3f, 0f, 6f, this.clientArea.get_height());
				EditorGUIUtility.AddCursorRect(rect, 19);
				if (Event.get_current().get_type() == null)
				{
					if (rect.Contains(Event.get_current().get_mousePosition()))
					{
						this.m_SplitterCaptured = 1;
					}
				}
				if (this.m_SplitterCaptured > 0)
				{
					if (Event.get_current().get_type() == 1)
					{
						this.m_SplitterCaptured = 0;
						Event.get_current().Use();
					}
					if (Event.get_current().get_type() == 3)
					{
						if (this.m_SplitterCaptured == 1)
						{
							float num = Event.get_current().get_delta().x / base.get_position().get_width();
							this.m_HierarchySplitterPerc = Mathf.Clamp(this.m_HierarchySplitterPerc + num, this.m_HierachySplitterMinMax.x, this.m_HierachySplitterMinMax.y);
							this.state.sequencerHeaderWidth += Event.get_current().get_delta().x;
						}
						Event.get_current().Use();
					}
				}
			}
		}

		public void ShowNewTracksContextMenu(TrackAsset parentTrack)
		{
			this.ShowNewTracksContextMenu(parentTrack, null);
		}

		private void AddMenuItem(GenericMenu menu, TrackAsset parentTrack, TimelineGroupGUI parentGroup, TrackType type)
		{
			GenericMenu.MenuFunction2 menuFunction = delegate(object arg)
			{
				SelectionManager.Clear();
				if (parentTrack is GroupTrack)
				{
					parentTrack.SetCollapsed(false);
				}
				TrackAsset trackAsset = this.state.GetWindow().AddTrack(((TrackType)arg).trackType, (parentGroup != null) ? parentGroup.track : null, null);
				if (parentGroup != null)
				{
					this.treeView.data.SetExpanded(parentGroup, true);
				}
				if (trackAsset.GetType() == typeof(ActivationTrack))
				{
					TimelineClip timelineClip = trackAsset.CreateClip(0.0);
					timelineClip.displayName = ActivationTrackDrawer.Styles.ClipText.get_text();
					this.state.Refresh();
				}
			};
			string text = TimelineHelpers.GetTrackCategoryName(type);
			if (!string.IsNullOrEmpty(text))
			{
				text += "/";
			}
			menu.AddItem(new GUIContent(text + TimelineHelpers.GetTrackMenuName(type)), false, menuFunction, type);
		}

		public void ShowNewTracksContextMenu(TrackAsset parentTrack, TimelineGroupGUI parentGroup)
		{
			GenericMenu genericMenu = new GenericMenu();
			string title = (!(parentTrack == null)) ? "Track Sub-Group" : "Track Group";
			genericMenu.AddItem(new GUIContent(title), false, delegate(object f)
			{
				SelectionManager.Clear();
				TimelineGroupGUI.Create(parentTrack, title);
				this.state.Refresh();
			}, null);
			genericMenu.AddSeparator("");
			IEnumerable<TrackType> enumerable = from x in TimelineHelpers.GetMixableTypes()
			where x.trackType != typeof(GroupTrack)
			select x;
			IEnumerable<TrackType> enumerable2 = from x in enumerable
			where x.trackType.FullName.Contains("UnityEngine.Timeline")
			select x;
			IEnumerable<TrackType> enumerable3 = enumerable.Except(enumerable2);
			foreach (TrackType current in enumerable2)
			{
				this.AddMenuItem(genericMenu, parentTrack, parentGroup, current);
			}
			genericMenu.AddSeparator("");
			foreach (TrackType current2 in enumerable3)
			{
				this.AddMenuItem(genericMenu, parentTrack, parentGroup, current2);
			}
			genericMenu.ShowAsContext();
		}

		private void OptionsGUI()
		{
			if (this.currentMode.headerState.options != TimelineModeGUIState.Hidden && !(this.timeline == null))
			{
				using (new EditorGUI.DisabledScope(this.currentMode.headerState.options == TimelineModeGUIState.Disabled))
				{
					GUILayout.FlexibleSpace();
					if (GUILayout.Button(TimelineWindow.styles.options.get_normal().get_background(), EditorStyles.get_toolbarButton(), new GUILayoutOption[0]))
					{
						GenericMenu genericMenu = new GenericMenu();
						genericMenu.AddItem(EditorGUIUtility.TextContent("Seconds"), !this.state.timeInFrames, new GenericMenu.MenuFunction2(this.ChangeTimeCode), "seconds");
						genericMenu.AddItem(EditorGUIUtility.TextContent("Frames"), this.state.timeInFrames, new GenericMenu.MenuFunction2(this.ChangeTimeCode), "frames");
						genericMenu.AddSeparator("");
						IEnumerator enumerator = Enum.GetValues(typeof(TimelineAsset.DurationMode)).GetEnumerator();
						try
						{
							while (enumerator.MoveNext())
							{
								object current = enumerator.Current;
								TimelineAsset.DurationMode mode = (TimelineAsset.DurationMode)current;
								GUIContent gUIContent = EditorGUIUtility.TextContent("Duration Mode/" + ObjectNames.NicifyVariableName(mode.ToString()));
								if (this.state.recording || this.state.timeline == null)
								{
									genericMenu.AddDisabledItem(gUIContent);
								}
								else
								{
									genericMenu.AddItem(gUIContent, this.state.timeline.durationMode == mode, delegate
									{
										TimelineWindow.SelectDurationCallback(this.state, mode);
									});
								}
							}
						}
						finally
						{
							IDisposable disposable;
							if ((disposable = (enumerator as IDisposable)) != null)
							{
								disposable.Dispose();
							}
						}
						genericMenu.AddSeparator("");
						bool flag = false;
						flag |= this.AddStandardFrameRateMenu(genericMenu, "Frame Rate/Film (24)", 24f);
						flag |= this.AddStandardFrameRateMenu(genericMenu, "Frame Rate/PAL (25)", 25f);
						flag |= this.AddStandardFrameRateMenu(genericMenu, "Frame Rate/NTSC (29.97)", 29.97f);
						flag |= this.AddStandardFrameRateMenu(genericMenu, "Frame Rate/30", 30f);
						flag |= this.AddStandardFrameRateMenu(genericMenu, "Frame Rate/50", 50f);
						flag |= this.AddStandardFrameRateMenu(genericMenu, "Frame Rate/60", 60f);
						if (flag)
						{
							genericMenu.AddDisabledItem(EditorGUIUtility.TextContent("Frame Rate/Custom"));
						}
						else
						{
							genericMenu.AddItem(EditorGUIUtility.TextContent("Frame Rate/Custom (" + this.state.frameRate + ")"), true, delegate
							{
							});
						}
						genericMenu.AddSeparator("");
						if (this.state.playRangeEnabled)
						{
							genericMenu.AddItem(EditorGUIUtility.TextContent("Play Range Mode/Loop"), this.state.playRangeLoopMode, delegate
							{
								this.state.playRangeLoopMode = true;
							});
							genericMenu.AddItem(EditorGUIUtility.TextContent("Play Range Mode/Once"), !this.state.playRangeLoopMode, delegate
							{
								this.state.playRangeLoopMode = false;
							});
						}
						else
						{
							genericMenu.AddDisabledItem(EditorGUIUtility.TextContent("Play Range Mode"));
						}
						genericMenu.AddSeparator("");
						genericMenu.AddItem(EditorGUIUtility.TextContent("Show Audio Waveforms"), this.state.showAudioWaveform, delegate
						{
							this.state.showAudioWaveform = !this.state.showAudioWaveform;
						});
						genericMenu.AddSeparator("");
						genericMenu.AddItem(EditorGUIUtility.TextContent("Snap to Frame"), this.state.frameSnap, delegate
						{
							this.state.frameSnap = !this.state.frameSnap;
						});
						genericMenu.AddItem(EditorGUIUtility.TextContent("Edge Snap"), this.state.edgeSnaps, delegate
						{
							this.state.edgeSnaps = !this.state.edgeSnaps;
						});
						if (Unsupported.IsDeveloperBuild())
						{
							genericMenu.AddItem(new GUIContent("Show Snapping Debug"), MagnetEngine.displayDebugLayout, delegate
							{
								MagnetEngine.displayDebugLayout = !MagnetEngine.displayDebugLayout;
							});
							genericMenu.AddItem(EditorGUIUtility.TextContent("Debug TimeArea"), false, delegate
							{
								Debug.LogFormat("translation: {0}   scale: {1}   rect: {2}   shownRange: {3}", new object[]
								{
									this.m_TimeArea.get_translation(),
									this.m_TimeArea.get_scale(),
									this.m_TimeArea.get_rect(),
									this.m_TimeArea.get_shownArea()
								});
							});
							genericMenu.AddItem(EditorGUIUtility.TextContent("Edit Skin"), false, delegate
							{
								Selection.set_activeObject(DirectorStyles.Instance.customSkin);
							});
						}
						genericMenu.ShowAsContext();
					}
				}
			}
		}

		private bool AddStandardFrameRateMenu(GenericMenu menu, string name, float value)
		{
			bool flag = this.state.frameRate.Equals(value);
			menu.AddItem(EditorGUIUtility.TextContent(name), flag, delegate(object r)
			{
				this.state.frameRate = (float)r;
			}, value);
			return flag;
		}

		private void ChangeTimeCode(object obj)
		{
			string a = obj.ToString();
			if (a == "frames")
			{
				this.state.timeInFrames = true;
			}
			else
			{
				this.state.timeInFrames = false;
			}
		}

		private void EmptyViewGUI(Rect clientRect, TimelineWindow.TimelineState state, TimelineModeGUIState trackState)
		{
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.Box("Hierarchy Pane", new GUILayoutOption[]
			{
				GUILayout.Width(state.sequencerHeaderWidth),
				GUILayout.Height(clientRect.get_height())
			});
			GUILayout.Box("Main Pane", new GUILayoutOption[]
			{
				GUILayout.Width(state.mainAreaWidth - state.sequencerHeaderWidth),
				GUILayout.Height(clientRect.get_height())
			});
			GUILayout.EndHorizontal();
		}

		private void ClearSequencerHeaderGUI()
		{
			Rect rect = Rect.MinMaxRect(0f, 0f, this.sequenceHeaderBounds.get_width(), this.sequenceHeaderBounds.get_height());
			EditorGUI.DrawRect(rect, DirectorStyles.Instance.customSkin.colorTimecodeBackground);
		}

		private void SequencerHeaderGUI()
		{
			bool flag = this.timeline == null;
			this.ClearSequencerHeaderGUI();
			EditorGUI.BeginDisabledGroup(flag);
			GUILayout.BeginVertical(new GUILayoutOption[0]);
			this.TransportToolbarGUI();
			this.TrackOptionsGUI();
			GUILayout.EndVertical();
			EditorGUI.EndDisabledGroup();
		}

		private void EditAssetModeToolbarGUI()
		{
			GUILayout.BeginHorizontal(EditorStyles.get_toolbarButton(), new GUILayoutOption[]
			{
				GUILayout.Width(this.sequenceHeaderBounds.get_width())
			});
			GUILayout.Label(DirectorStyles.timelineAssetEditModeTitle, new GUILayoutOption[0]);
			GUILayout.EndHorizontal();
		}

		private void TransportToolbarGUI()
		{
			GUILayout.BeginHorizontal(EditorStyles.get_toolbarButton(), new GUILayoutOption[]
			{
				GUILayout.Width(this.sequenceHeaderBounds.get_width())
			});
			using (new EditorGUI.DisabledScope(this.currentMode.ToolbarState(this.state) == TimelineModeGUIState.Disabled))
			{
				this.PreviewModeButtonGUI();
				this.GotoBeginingSequenceGUI();
				this.PreviousEventButtonGUI();
				this.PlayButtonGUI();
				this.NextEventButtonGUI();
				this.GotoEndSequenceGUI();
				GUILayout.Space(15f);
				this.PlayRangeButtonGUI();
				GUILayout.FlexibleSpace();
				this.TimeCodeGUI();
			}
			GUILayout.EndHorizontal();
		}

		private void PreviewModeButtonGUI()
		{
			EditorGUI.BeginChangeCheck();
			bool flag = this.state.previewMode;
			flag = GUILayout.Toggle(flag, DirectorStyles.previewContent, EditorStyles.get_toolbarButton(), new GUILayoutOption[0]);
			if (EditorGUI.EndChangeCheck())
			{
				this.state.previewMode = flag;
				if (!flag)
				{
					this.state.playing = false;
				}
				else if (this.state.previewMode)
				{
					this.state.rebuildGraph = true;
				}
			}
		}

		private void TrackOptionsGUI()
		{
			GUILayout.BeginHorizontal(new GUILayoutOption[]
			{
				GUILayout.Width(this.sequenceHeaderBounds.get_width())
			});
			if (this.state.timeline != null)
			{
				GUILayout.Space(DirectorStyles.kBaseIndent);
				this.AddButtonGUI();
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();
		}

		private void GotoBeginingSequenceGUI()
		{
			if (GUILayout.Button(DirectorStyles.gotoBeginingContent, EditorStyles.get_toolbarButton(), new GUILayoutOption[0]))
			{
				this.state.time = 0.0;
				this.state.EnsurePlayHeadIsVisible();
			}
		}

		private void PlayButtonGUIEditor()
		{
			EditorGUI.BeginChangeCheck();
			bool flag = GUILayout.Toggle(this.state.playing, DirectorStyles.playContent, EditorStyles.get_toolbarButton(), new GUILayoutOption[0]);
			if (EditorGUI.EndChangeCheck())
			{
				this.state.GetWindow().Simulate(flag);
				this.state.playing = flag;
			}
		}

		private void PlayButtonGUIPlayMode()
		{
			bool flag = this.state.currentDirector != null && this.state.currentDirector.get_isActiveAndEnabled();
			using (new EditorGUI.DisabledScope(!flag))
			{
				this.PlayButtonGUIEditor();
			}
		}

		private void PlayButtonGUI()
		{
			if (!Application.get_isPlaying())
			{
				this.PlayButtonGUIEditor();
			}
			else
			{
				this.PlayButtonGUIPlayMode();
			}
		}

		private void NextEventButtonGUI()
		{
			if (GUILayout.Button(DirectorStyles.nextFrameContent, EditorStyles.get_toolbarButton(), new GUILayoutOption[0]))
			{
				this.state.frame = this.state.frame + 1;
			}
		}

		private void PreviousEventButtonGUI()
		{
			if (GUILayout.Button(DirectorStyles.previousFrameContent, EditorStyles.get_toolbarButton(), new GUILayoutOption[0]))
			{
				this.state.frame = this.state.frame - 1;
			}
		}

		private void GotoEndSequenceGUI()
		{
			if (GUILayout.Button(DirectorStyles.gotoEndContent, EditorStyles.get_toolbarButton(), new GUILayoutOption[0]))
			{
				this.state.time = this.state.timeline.get_duration();
				this.state.EnsurePlayHeadIsVisible();
			}
		}

		private void PlayRangeButtonGUI()
		{
			using (new EditorGUI.DisabledScope(EditorApplication.get_isPlaying()))
			{
				this.state.playRangeEnabled = GUILayout.Toggle(this.state.playRangeEnabled, DirectorStyles.Instance.playrangeContent, EditorStyles.get_toolbarButton(), new GUILayoutOption[0]);
			}
		}

		private void AddButtonGUI()
		{
			if (this.currentMode.trackOptionsState.newButton != TimelineModeGUIState.Hidden)
			{
				using (new EditorGUI.DisabledScope(this.currentMode.trackOptionsState.newButton == TimelineModeGUIState.Disabled))
				{
					if (EditorGUILayout.DropdownButton(DirectorStyles.newContent, 2, "Dropdown", new GUILayoutOption[0]))
					{
						TrackAsset parentTrack = null;
						List<TrackAsset> list = SelectionManager.SelectedTracks().ToList<TrackAsset>();
						if (list.Count == 1)
						{
							parentTrack = (list.First<TrackAsset>() as GroupTrack);
						}
						this.ShowNewTracksContextMenu(parentTrack);
					}
				}
			}
		}

		private void TimeCodeGUI()
		{
			string text;
			if (this.state.timeline != null)
			{
				text = this.state.TimeAsString(this.state.time, "F2");
				bool flag = TimeUtility.OnFrameBoundary(this.state.time, (double)this.state.frameRate);
				if (this.state.timeInFrames)
				{
					if (flag)
					{
						text = this.state.frame.ToString();
					}
					else
					{
						text = TimeUtility.ToExactFrames(this.state.time, (double)this.state.frameRate).ToString("F2");
					}
				}
			}
			else
			{
				text = "0";
			}
			EditorGUI.BeginChangeCheck();
			string text2 = EditorGUILayout.DelayedTextField(text, EditorStyles.get_toolbarTextField(), new GUILayoutOption[0]);
			bool flag2 = EditorGUI.EndChangeCheck();
			if (flag2)
			{
				if (this.state.timeInFrames)
				{
					int frame = this.state.frame;
					double d = 0.0;
					if (double.TryParse(text2, out d))
					{
						frame = Math.Max(0, (int)Math.Floor(d));
					}
					this.state.frame = frame;
				}
				else
				{
					double num = TimeUtility.ParseTimeCode(text2, (double)this.state.frameRate, -1.0);
					if (num > 0.0)
					{
						this.state.time = num;
					}
				}
			}
		}

		public virtual bool OnEvent(Event evt, TimelineWindow.TimelineState state, bool isCaptureSession)
		{
			return false;
		}

		public void DrawOverlays(Event evt, TimelineWindow.TimelineState state)
		{
			if (this.Overlay != null)
			{
				this.Overlay(this, evt, state);
			}
		}

		public bool IsMouseOver(Vector2 mousePosition)
		{
			return base.get_position().Contains(mousePosition);
		}

		private static bool NoOp(object target, Event e, TimelineWindow.TimelineState state)
		{
			return false;
		}

		private void InitializeIControl()
		{
			if (TimelineWindow.<>f__mg$cache0 == null)
			{
				TimelineWindow.<>f__mg$cache0 = new TimelineUIEvent(TimelineWindow.NoOp);
			}
			this.MouseMove += TimelineWindow.<>f__mg$cache0;
			if (TimelineWindow.<>f__mg$cache1 == null)
			{
				TimelineWindow.<>f__mg$cache1 = new TimelineUIEvent(TimelineWindow.NoOp);
			}
			this.MouseDown += TimelineWindow.<>f__mg$cache1;
			if (TimelineWindow.<>f__mg$cache2 == null)
			{
				TimelineWindow.<>f__mg$cache2 = new TimelineUIEvent(TimelineWindow.NoOp);
			}
			this.MouseDrag += TimelineWindow.<>f__mg$cache2;
			if (TimelineWindow.<>f__mg$cache3 == null)
			{
				TimelineWindow.<>f__mg$cache3 = new TimelineUIEvent(TimelineWindow.NoOp);
			}
			this.MouseUp += TimelineWindow.<>f__mg$cache3;
			if (TimelineWindow.<>f__mg$cache4 == null)
			{
				TimelineWindow.<>f__mg$cache4 = new TimelineUIEvent(TimelineWindow.NoOp);
			}
			this.DoubleClick += TimelineWindow.<>f__mg$cache4;
			if (TimelineWindow.<>f__mg$cache5 == null)
			{
				TimelineWindow.<>f__mg$cache5 = new TimelineUIEvent(TimelineWindow.NoOp);
			}
			this.KeyDown += TimelineWindow.<>f__mg$cache5;
			if (TimelineWindow.<>f__mg$cache6 == null)
			{
				TimelineWindow.<>f__mg$cache6 = new TimelineUIEvent(TimelineWindow.NoOp);
			}
			this.KeyUp += TimelineWindow.<>f__mg$cache6;
			if (TimelineWindow.<>f__mg$cache7 == null)
			{
				TimelineWindow.<>f__mg$cache7 = new TimelineUIEvent(TimelineWindow.NoOp);
			}
			this.DragPerform += TimelineWindow.<>f__mg$cache7;
			if (TimelineWindow.<>f__mg$cache8 == null)
			{
				TimelineWindow.<>f__mg$cache8 = new TimelineUIEvent(TimelineWindow.NoOp);
			}
			this.DragExited += TimelineWindow.<>f__mg$cache8;
			if (TimelineWindow.<>f__mg$cache9 == null)
			{
				TimelineWindow.<>f__mg$cache9 = new TimelineUIEvent(TimelineWindow.NoOp);
			}
			this.DragUpdated += TimelineWindow.<>f__mg$cache9;
			if (TimelineWindow.<>f__mg$cacheA == null)
			{
				TimelineWindow.<>f__mg$cacheA = new TimelineUIEvent(TimelineWindow.NoOp);
			}
			this.MouseWheel += TimelineWindow.<>f__mg$cacheA;
			if (TimelineWindow.<>f__mg$cacheB == null)
			{
				TimelineWindow.<>f__mg$cacheB = new TimelineUIEvent(TimelineWindow.NoOp);
			}
			this.ContextClick += TimelineWindow.<>f__mg$cacheB;
		}

		private void InitializeManipulators()
		{
			this.AddManipulator(new RectangleSelect());
			this.AddManipulator(new TrackZoom());
			this.AddManipulator(new Jog());
			this.AddManipulator(new TimelinePanManipulator());
			this.AddManipulator(new MouseWheelHorizontalScroll());
			this.AddManipulator(new TimelineZoomManipulator());
			this.AddManipulator(new TimelineShortcutManipulator());
			this.AddManipulator(new NewTrackContextMenu());
		}

		private void AddManipulator(Manipulator manipulator)
		{
			manipulator.Init(this);
			this.m_Manipulators.Add(manipulator);
		}

		private void ProcessManipulators()
		{
			if (!(this.state.timeline == null))
			{
				if (Event.get_current().get_type() == null && EditorGUI.IsEditingTextField())
				{
					EditorGUI.EndEditingActiveTextField();
				}
				Event current = Event.get_current();
				bool flag = false;
				switch (Event.get_current().get_type())
				{
				case 0:
				{
					float num = Time.get_realtimeSinceStartup() - this.m_MouseDownTime;
					if (this.m_MouseDownState == Control.MouseDownState.None)
					{
						this.m_MouseDownState = Control.MouseDownState.SingleClick;
					}
					else if (this.m_MouseDownState == Control.MouseDownState.SingleClick)
					{
						this.m_MouseDownState = ((num >= Control.doubleClickSpeed) ? Control.MouseDownState.SingleClick : Control.MouseDownState.DoubleClick);
					}
					else if (this.m_MouseDownState == Control.MouseDownState.DoubleClick)
					{
						this.m_MouseDownState = Control.MouseDownState.SingleClick;
					}
					this.m_MouseDownTime = Time.get_realtimeSinceStartup();
					if (this.m_MouseDownState == Control.MouseDownState.SingleClick)
					{
						flag = Control.InvokeEvents(this.MouseDown, this, current, this.state);
					}
					if (this.m_MouseDownState == Control.MouseDownState.DoubleClick)
					{
						flag = Control.InvokeEvents(this.DoubleClick, this, current, this.state);
					}
					break;
				}
				case 1:
					flag = Control.InvokeEvents(this.MouseUp, this, current, this.state);
					break;
				case 2:
					flag = Control.InvokeEvents(this.MouseMove, this, current, this.state);
					break;
				case 3:
					flag = Control.InvokeEvents(this.MouseDrag, this, current, this.state);
					break;
				case 4:
					flag = Control.InvokeEvents(this.KeyDown, this, current, this.state);
					break;
				case 5:
					flag = Control.InvokeEvents(this.KeyUp, this, current, this.state);
					break;
				case 6:
					flag = Control.InvokeEvents(this.MouseWheel, this, current, this.state);
					break;
				case 9:
					flag = Control.InvokeEvents(this.DragUpdated, this, current, this.state);
					break;
				case 10:
					flag = Control.InvokeEvents(this.DragPerform, this, current, this.state);
					break;
				case 13:
					flag = Control.InvokeEvents(this.ValidateCommand, this, current, this.state);
					break;
				case 14:
					flag = Control.InvokeEvents(this.ExecuteCommand, this, current, this.state);
					break;
				case 15:
					flag = Control.InvokeEvents(this.DragExited, this, current, this.state);
					break;
				case 16:
					flag = Control.InvokeEvents(this.ContextClick, this, current, this.state);
					break;
				}
				if (flag)
				{
					if (Event.get_current().get_type() == 1)
					{
						GUIUtility.set_hotControl(0);
					}
					Event.get_current().Use();
				}
			}
		}

		private void PlayRangeGUI(TimelineWindow.TimelineItemArea area)
		{
			if (this.currentMode.ShouldShowPlayRange(this.state) && this.treeView != null)
			{
				if (!(this.timeline != null) || this.timeline.tracks.Count != 0)
				{
					if (this.m_PlayRangeStart == null)
					{
						this.m_PlayRangeStart = new TimeAreaItem(TimelineWindow.styles.playTimeRangeStart, new Action<double, bool>(this.OnTrackHeadMinSelectDrag));
						Vector2 boundOffset = new Vector2(-2f, 0f);
						this.m_PlayRangeStart.boundOffset = boundOffset;
					}
					if (this.m_PlayRangeEnd == null)
					{
						this.m_PlayRangeEnd = new TimeAreaItem(TimelineWindow.styles.playTimeRangeEnd, new Action<double, bool>(this.OnTrackHeadMaxSelectDrag));
						Vector2 boundOffset2 = new Vector2(2f, 0f);
						this.m_PlayRangeEnd.boundOffset = boundOffset2;
					}
					if (area == TimelineWindow.TimelineItemArea.Header)
					{
						this.DrawPlayRange(true, false);
					}
					else if (area == TimelineWindow.TimelineItemArea.Lines)
					{
						this.DrawPlayRange(false, true);
					}
				}
			}
		}

		private void DrawPlayRange(bool drawHeads, bool drawLines)
		{
			Rect timeAreaBounds = this.timeAreaBounds;
			timeAreaBounds.set_height(this.clientArea.get_height());
			if (Event.get_current().get_type() == null)
			{
				if (this.m_PlayRangeEnd.bounds.Contains(Event.get_current().get_mousePosition()))
				{
					if (this.m_PlayRangeEnd.OnEvent(Event.get_current(), this.state, false))
					{
						Event.get_current().Use();
					}
				}
				else if (this.m_PlayRangeStart.bounds.Contains(Event.get_current().get_mousePosition()))
				{
					if (this.m_PlayRangeStart.OnEvent(Event.get_current(), this.state, false))
					{
						Event.get_current().Use();
					}
				}
			}
			if (this.state.playRangeTime == TimelineAssetViewModel.kNoPlayRangeSet)
			{
				float num = 0.01f;
				float num2 = Mathf.Max(0f, this.state.PixelToTime(this.state.timeAreaRect.get_xMin()));
				float num3 = Mathf.Min((float)this.state.duration, this.state.PixelToTime(this.state.timeAreaRect.get_xMax()));
				if (Mathf.Abs(num3 - num2) <= num)
				{
					this.state.playRangeTime = new Vector2(num2, num3);
					return;
				}
				float num4 = (num3 - num2) * 0.25f / 2f;
				num2 += num4;
				num3 -= num4;
				if (num3 < num2)
				{
					float num5 = num2;
					num2 = num3;
					num3 = num5;
				}
				if (Mathf.Abs(num3 - num2) < num)
				{
					if (num2 - num > 0f)
					{
						num2 -= num;
					}
					else if ((double)(num3 + num) < this.state.duration)
					{
						num3 += num;
					}
				}
				this.state.playRangeTime = new Vector2(num2, num3);
			}
			this.m_PlayRangeStart.drawHead = drawHeads;
			this.m_PlayRangeStart.drawLine = drawLines;
			this.m_PlayRangeEnd.drawHead = drawHeads;
			this.m_PlayRangeEnd.drawLine = drawLines;
			Vector2 playRangeTime = this.state.playRangeTime;
			this.m_PlayRangeStart.Draw(timeAreaBounds, this.state, (double)playRangeTime.x);
			this.m_PlayRangeEnd.Draw(timeAreaBounds, this.state, (double)playRangeTime.y);
			if (this.state.playRangeEnabled && this.m_PlayHead != null)
			{
				Rect rect = Rect.MinMaxRect(Mathf.Clamp(this.state.TimeToPixel((double)playRangeTime.x), this.timeAreaBounds.get_xMin(), this.timeAreaBounds.get_xMax()), this.m_PlayHead.bounds.get_yMax(), Mathf.Clamp(this.state.TimeToPixel((double)playRangeTime.y), this.timeAreaBounds.get_xMin(), this.timeAreaBounds.get_xMax()), timeAreaBounds.get_height() + this.timeAreaBounds.get_height());
				EditorGUI.DrawRect(rect, DirectorStyles.Instance.customSkin.colorRange);
				rect.set_height(3f);
				EditorGUI.DrawRect(rect, Color.get_white());
			}
		}

		private void OnTrackHeadMinSelectDrag(double newTime, bool initialFrame)
		{
			Vector2 playRangeTime = this.state.playRangeTime;
			playRangeTime.x = (float)newTime;
			this.state.playRangeTime = playRangeTime;
			this.m_PlayRangeStart.showTooltip = true;
		}

		private void OnTrackHeadMaxSelectDrag(double newTime, bool initialFrame)
		{
			Vector2 playRangeTime = this.state.playRangeTime;
			playRangeTime.y = (float)newTime;
			this.state.playRangeTime = playRangeTime;
			this.m_PlayRangeEnd.showTooltip = true;
		}

		public void Simulate(bool start)
		{
			if (start && this.state.currentDirector != null && this.state.currentDirector.get_state() == null)
			{
				this.state.currentDirector.Play();
			}
			if (!start && this.state.currentDirector != null && this.state.currentDirector.get_state() == 1)
			{
				if (Application.get_isPlaying())
				{
					this.state.currentDirector.Pause();
				}
			}
			this.state.playing = start;
		}

		public void SimulateFrames(int frames)
		{
			for (int i = 0; i < frames; i++)
			{
				base.RepaintImmediately();
				if (this.state.playing)
				{
					this.OnPreviewPlay();
				}
			}
		}

		private void OnPreviewPlayModeChanged(bool active)
		{
			if (!active || !EditorApplication.get_isPlaying())
			{
				this.m_PreviousTime = (double)Time.get_realtimeSinceStartup();
				if (active)
				{
					this.PreparePreviewPlay();
					EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Combine(EditorApplication.update, new EditorApplication.CallbackFunction(this.OnPreviewPlay));
				}
				else
				{
					EditorApplication.update = (EditorApplication.CallbackFunction)Delegate.Remove(EditorApplication.update, new EditorApplication.CallbackFunction(this.OnPreviewPlay));
				}
			}
		}

		private void PreparePreviewPlay()
		{
			if (this.state != null && this.timeline != null && this.state.currentDirector != null)
			{
				bool playRangeEnabled = this.state.playRangeEnabled;
				Vector2 playRangeTime = this.state.playRangeTime;
				double num = (!playRangeEnabled) ? this.timeline.get_duration() : ((double)playRangeTime.y);
				if (this.state.time >= num)
				{
					this.state.time = (double)((!playRangeEnabled) ? 0f : playRangeTime.x);
				}
			}
		}

		private static TimelineWindow.PreviewPlayMode CurrentPreviewPlayMode(TimelineWindow.TimelineState state, bool playRangeEnabled)
		{
			TimelineWindow.PreviewPlayMode result;
			if (playRangeEnabled)
			{
				result = ((!state.playRangeLoopMode) ? TimelineWindow.PreviewPlayMode.Once : TimelineWindow.PreviewPlayMode.Loop);
			}
			else
			{
				DirectorWrapMode extrapolationMode = state.currentDirector.get_extrapolationMode();
				if (extrapolationMode == null)
				{
					result = TimelineWindow.PreviewPlayMode.Hold;
				}
				else if (extrapolationMode == 1)
				{
					result = TimelineWindow.PreviewPlayMode.Loop;
				}
				else
				{
					result = TimelineWindow.PreviewPlayMode.None;
				}
			}
			return result;
		}

		internal void OnPreviewPlay()
		{
			if (this.state != null)
			{
				if (!(this.timeline == null))
				{
					if (!(this.state.currentDirector == null))
					{
						if (this.state.currentDirector.get_timeUpdateMode() == 3)
						{
							base.Repaint();
						}
						else
						{
							bool playRangeEnabled = this.state.playRangeEnabled;
							double num;
							double num2;
							if (playRangeEnabled)
							{
								Vector2 playRangeTime = this.state.playRangeTime;
								num = (double)playRangeTime.x;
								num2 = (double)playRangeTime.y;
							}
							else
							{
								num = 0.0;
								num2 = this.timeline.get_duration();
							}
							if (this.state.isJogging)
							{
								this.state.time = Math.Max(this.state.time + (double)this.state.playbackSpeed, num);
								this.state.time = Math.Min(this.state.time, num2);
							}
							else
							{
								double num3 = (double)Time.get_realtimeSinceStartup() - this.m_PreviousTime;
								this.m_PreviousTime = (double)Time.get_realtimeSinceStartup();
								double num4 = TimelineWindow.IncrementTime(this.state.time, num3, this.state.currentDirector.get_timeUpdateMode());
								bool flag = false;
								if (num4 > num2)
								{
									this.state.time = num2;
									num3 = Math.Max(0.0, num3 - (num4 - num2));
									switch (TimelineWindow.CurrentPreviewPlayMode(this.state, playRangeEnabled))
									{
									case TimelineWindow.PreviewPlayMode.Hold:
										num3 = 0.0;
										break;
									case TimelineWindow.PreviewPlayMode.Once:
										this.state.EvaluateImmediate();
										this.state.time = num2;
										flag = true;
										break;
									case TimelineWindow.PreviewPlayMode.Loop:
										this.state.EvaluateImmediate();
										this.state.time = num;
										this.state.EvaluateImmediate();
										break;
									case TimelineWindow.PreviewPlayMode.None:
										this.state.EvaluateImmediate();
										this.state.time = num;
										flag = true;
										break;
									}
								}
								if (flag)
								{
									this.state.playing = false;
								}
								else
								{
									num4 = TimelineWindow.IncrementTime(this.state.time, num3, this.state.currentDirector.get_timeUpdateMode());
									this.state.time = Math.Min(Math.Max(num4, num), num2);
								}
								base.Repaint();
							}
						}
					}
				}
			}
		}

		private static double IncrementTime(double time, double deltaTime, DirectorUpdateMode timeUpdateMode)
		{
			float val = (timeUpdateMode != 1) ? 1f : Time.get_timeScale();
			float num = Math.Max(val, 0f);
			return time + Math.Abs(deltaTime) * (double)num;
		}

		public void OnSelectionChange()
		{
			if (this.locked || (this.state != null && this.state.recording))
			{
				this.RestoreLastSelection();
			}
			else
			{
				Object @object = Selection.get_activeObject() as TimelineAsset;
				if (@object != null)
				{
					this.SetCurrentSelection(@object);
				}
				else
				{
					@object = Selection.get_activeGameObject();
					if (@object != null)
					{
						if (!TimelineUtility.IsPrefabOrAsset(@object))
						{
							this.SetCurrentSelection(@object);
							return;
						}
					}
					this.RestoreLastSelection();
				}
			}
		}

		private void RestoreLastSelection()
		{
			Object @object = EditorUtility.InstanceIDToObject(this.m_LastSelectedObjectID);
			if (@object != null)
			{
				this.SetCurrentSelection(@object);
			}
			else
			{
				this.SetCurrentTimeline(null, null);
				this.locked = false;
			}
			base.Repaint();
		}

		private void SetCurrentSelection(Object obj)
		{
			GameObject gameObject = obj as GameObject;
			if (gameObject != null)
			{
				PlayableDirector directorComponentForGameObject = TimelineUtility.GetDirectorComponentForGameObject(gameObject);
				TimelineAsset timelineAssetForDirectorComponent = TimelineUtility.GetTimelineAssetForDirectorComponent(directorComponentForGameObject);
				if (this.state != null && this.state.currentDirector == null)
				{
					this.breadcrumbPath = this.m_BreadcrumbPath;
				}
				if (this.state == null || timelineAssetForDirectorComponent != this.state.timeline || directorComponentForGameObject != this.state.currentDirector)
				{
					this.SetCurrentTimeline(timelineAssetForDirectorComponent, directorComponentForGameObject);
				}
			}
			else
			{
				TimelineAsset timelineAsset = obj as TimelineAsset;
				if (timelineAsset != null)
				{
					this.SetCurrentTimeline(timelineAsset, null);
				}
			}
			base.Repaint();
		}

		private void InitializeStateChange()
		{
			this.OnStateChange += delegate(object sender, TimelineWindow.StateEventArgs stateChangeEventArgs)
			{
				string propertyChanged = stateChangeEventArgs.propertyChanged;
				if (propertyChanged != null)
				{
					if (!(propertyChanged == "playing"))
					{
						if (!(propertyChanged == "dirtyStamp"))
						{
							if (!(propertyChanged == "rebuildGraph"))
							{
								if (!(propertyChanged == "currentDirector"))
								{
									if (!(propertyChanged == "time"))
									{
										if (propertyChanged == "recording")
										{
											if (!this.state.recording)
											{
												TrackAssetRecordingExtensions.ClearRecordingState();
											}
										}
									}
									else
									{
										if (!EditorApplication.get_isPlaying())
										{
											this.state.UpdateRecordingState();
											EditorApplication.SetSceneRepaintDirty();
										}
										this.state.Evaluate();
										InspectorWindow.RepaintAllInspectors();
									}
								}
								else if (stateChangeEventArgs.state.currentDirector != null)
								{
									stateChangeEventArgs.state.time = stateChangeEventArgs.state.currentDirector.get_time();
								}
							}
							else if (!this.state.rebuildGraph)
							{
								if (this.treeView != null)
								{
									List<TimelineTrackBaseGUI> allTrackGuis = this.treeView.allTrackGuis;
									if (allTrackGuis != null)
									{
										for (int i = 0; i < allTrackGuis.Count; i++)
										{
											allTrackGuis[i].OnGraphRebuilt();
										}
									}
								}
							}
						}
						else
						{
							this.state.UpdateRecordingState();
							if (this.treeView != null)
							{
								this.treeView.Reload();
							}
						}
					}
					else
					{
						this.OnPreviewPlayModeChanged(stateChangeEventArgs.state.playing);
					}
				}
			};
		}

		private void InitializeTimeArea()
		{
			if (this.m_TimeArea == null)
			{
				TimeArea timeArea = new TimeArea(false);
				timeArea.set_hRangeLocked(false);
				timeArea.set_vRangeLocked(true);
				timeArea.set_margin(10f);
				timeArea.set_scaleWithWindow(true);
				timeArea.set_hSlider(false);
				timeArea.set_vSlider(false);
				timeArea.set_hRangeMin(0f);
				timeArea.set_rect(this.timeAreaBounds);
				this.m_TimeArea = timeArea;
				this.InitTimeAreaFrameRate();
				this.SyncTimeAreaShownRange();
			}
		}

		private void TimelineGUI()
		{
			if (this.currentMode.ShouldShowTimeArea(this.state))
			{
				Rect timeAreaBounds = this.timeAreaBounds;
				this.m_TimeArea.set_rect(new Rect(timeAreaBounds.get_x(), timeAreaBounds.get_y(), timeAreaBounds.get_width(), this.clientArea.get_height() - timeAreaBounds.get_y()));
				if (this.m_LastFrameRate != this.state.frameRate)
				{
					this.InitTimeAreaFrameRate();
				}
				this.SyncTimeAreaShownRange();
				this.m_TimeArea.TimeRuler(timeAreaBounds, this.state.frameRate, true, false, 1f, (!this.state.timeInFrames) ? 1 : 2);
				this.ContextMenuTimelineGUI(timeAreaBounds);
			}
		}

		private void InitTimeAreaFrameRate()
		{
			this.m_LastFrameRate = this.state.frameRate;
			this.m_TimeArea.get_hTicks().SetTickModulosForFrameRate(this.m_LastFrameRate);
		}

		private void SyncTimeAreaShownRange()
		{
			Vector2 timeAreaShownRange = this.state.timeAreaShownRange;
			if (!Mathf.Approximately(timeAreaShownRange.x, this.m_TimeArea.get_shownArea().get_x()) || !Mathf.Approximately(timeAreaShownRange.y, this.m_TimeArea.get_shownArea().get_xMax()))
			{
				this.m_TimeArea.SetShownHRange(timeAreaShownRange.x, timeAreaShownRange.y);
				this.state.TimeAreaChanged();
			}
		}

		private void ContextMenuTimelineGUI(Rect rect)
		{
			if (Event.get_current().get_type() == 16)
			{
				if (rect.Contains(Event.get_current().get_mousePosition()))
				{
					GenericMenu genericMenu = new GenericMenu();
					IEnumerator enumerator = Enum.GetValues(typeof(TimelineAsset.DurationMode)).GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							object current = enumerator.Current;
							TimelineAsset.DurationMode mode = (TimelineAsset.DurationMode)current;
							GUIContent gUIContent = EditorGUIUtility.TextContent("Duration Mode/" + ObjectNames.NicifyVariableName(mode.ToString()));
							if (this.state.recording)
							{
								genericMenu.AddDisabledItem(gUIContent);
							}
							else
							{
								genericMenu.AddItem(gUIContent, this.state.timeline.durationMode == mode, delegate
								{
									TimelineWindow.SelectDurationCallback(this.state, mode);
								});
							}
						}
					}
					finally
					{
						IDisposable disposable;
						if ((disposable = (enumerator as IDisposable)) != null)
						{
							disposable.Dispose();
						}
					}
					genericMenu.ShowAsContext();
					Event.get_current().Use();
				}
			}
		}

		private static void SelectDurationCallback(TimelineWindow.TimelineState state, TimelineAsset.DurationMode mode)
		{
			if (state.timeline.durationMode == TimelineAsset.DurationMode.BasedOnClips && mode == TimelineAsset.DurationMode.FixedLength)
			{
				state.timeline.fixedDuration = state.duration;
			}
			state.timeline.durationMode = mode;
		}

		private void TimeCursorGUI(TimelineWindow.TimelineItemArea area)
		{
			if (this.CanDrawTimeCursor(area))
			{
				if (this.m_PlayHead == null)
				{
					this.m_PlayHead = new TimeAreaItem(TimelineWindow.styles.timeCursor, new Action<double, bool>(this.OnTrackHeadDrag));
					this.m_PlayHead.AddManipulator(new TrackheadContextMenu());
				}
				bool flag = area == TimelineWindow.TimelineItemArea.Header;
				this.DrawTimeCursor(flag, !flag);
			}
		}

		private bool CanDrawTimeCursor(TimelineWindow.TimelineItemArea area)
		{
			return this.currentMode.ShouldShowTimeCursor(this.state) && this.treeView != null && !(this.timeline == null) && (!(this.timeline != null) || this.timeline.tracks.Count != 0) && (area != TimelineWindow.TimelineItemArea.Lines || this.state.TimeIsInRange((float)this.state.time));
		}

		private void DrawTimeCursor(bool drawHead, bool drawline)
		{
			Rect timeAreaBounds = this.timeAreaBounds;
			timeAreaBounds.set_height(this.clientArea.get_height());
			timeAreaBounds.set_y(timeAreaBounds.get_y() - 3f);
			if (this.m_PlayHead.bounds.Contains(Event.get_current().get_mousePosition()))
			{
				if (this.m_PlayHead.OnEvent(Event.get_current(), this.state, false))
				{
					Event.get_current().Use();
				}
			}
			if (Event.get_current().get_type() == null && Event.get_current().get_button() == 0)
			{
				if (this.timeAreaBounds.Contains(Event.get_current().get_mousePosition()))
				{
					this.state.playing = false;
					this.m_PlayHead.OnEvent(Event.get_current(), this.state, false);
					if (EditorApplication.get_isPlaying() && this.state.currentDirector != null)
					{
						if (this.state.currentDirector.get_state() == 1)
						{
							this.state.currentDirector.Pause();
						}
					}
					this.state.time = Math.Max(0.0, this.state.GetSnappedTimeAtMousePosition(Event.get_current().get_mousePosition()));
				}
			}
			this.m_PlayHead.dottedLine = this.state.isClipSnapping;
			this.state.isClipSnapping = false;
			this.m_PlayHead.drawLine = drawline;
			this.m_PlayHead.drawHead = drawHead;
			this.m_PlayHead.Draw(timeAreaBounds, this.state, this.state.time);
		}

		private void OnTrackHeadDrag(double newTime, bool initialFrame)
		{
			this.state.time = Math.Max(0.0, newTime);
			this.m_PlayHead.showTooltip = true;
		}

		private void TracksGUI(Rect clientRect, TimelineWindow.TimelineState state, TimelineModeGUIState trackState)
		{
			if (Event.get_current().get_type() == 7 && this.treeView != null)
			{
				state.quadTree.SetSize(new Rect(0f, 0f, base.get_position().get_width(), this.treeView.contentSize.y));
				state.quadTree.set_screenSpaceOffset(new Vector2(0f, 42f - this.treeView.scrollPosition.y));
			}
			EditorGUI.DrawRect(clientRect, DirectorStyles.Instance.customSkin.colorSequenceBackground);
			if (state != null && state.timeline != null && state.timeline.tracks.Count > 0)
			{
				this.m_TimeArea.DrawMajorTicks(this.tracksBounds, state.frameRate);
			}
			GUILayout.BeginVertical(new GUILayoutOption[0]);
			GUILayout.Space(5f);
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			if (this.timeline == null)
			{
				this.DrawNoSequenceGUI(state);
			}
			else if (this.timeline.tracks.Count == 0)
			{
				this.DrawEmptySequenceGUI(clientRect, trackState);
			}
			else
			{
				this.DrawTracksGUI(clientRect, trackState);
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			this.DrawShadowUnderTimeline();
		}

		private void DrawShadowUnderTimeline()
		{
			Rect timeAreaBounds = this.timeAreaBounds;
			timeAreaBounds.set_xMin(0f);
			timeAreaBounds.set_yMin(this.timeAreaBounds.get_yMax());
			timeAreaBounds.set_height(15f);
			GUI.Box(timeAreaBounds, GUIContent.none, DirectorStyles.Instance.bottomShadow);
		}

		private void DrawEmptySequenceGUI(Rect clientRect, TimelineModeGUIState trackState)
		{
			GUILayout.BeginVertical(new GUILayoutOption[0]);
			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal(new GUILayoutOption[0]);
			GUILayout.FlexibleSpace();
			this.DrawTracksGUI(clientRect, trackState);
			GUILayout.Label(DirectorStyles.emptyTimelineMessage, new GUILayoutOption[0]);
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
		}

		private void DrawNoSequenceGUI(TimelineWindow.TimelineState timelineState)
		{
			bool flag = false;
			GameObject gameObject = (!(Selection.get_activeObject() != null)) ? null : (Selection.get_activeObject() as GameObject);
			GUIContent gUIContent = DirectorStyles.noTimelineAssetSelected;
			PlayableDirector playableDirector = (!(gameObject != null)) ? null : gameObject.GetComponent<PlayableDirector>();
			PlayableAsset playableAsset = (!(playableDirector != null)) ? null : playableDirector.get_playableAsset();
			if (gameObject != null && !TimelineUtility.IsPrefabOrAsset(gameObject) && playableAsset == null)
			{
				flag = true;
				gUIContent = new GUIContent(string.Format(DirectorStyles.createTimelineOnSelection.get_text(), gameObject.get_name(), "a Director component and a Timeline asset"));
			}
			GUILayout.FlexibleSpace();
			GUILayout.BeginVertical(new GUILayoutOption[0]);
			GUILayout.FlexibleSpace();
			GUILayout.Label(gUIContent, new GUILayoutOption[0]);
			if (flag)
			{
				GUILayout.BeginHorizontal(new GUILayoutOption[0]);
				GUILayout.Space(GUI.get_skin().get_label().CalcSize(gUIContent).x / 2f - 35f);
				if (GUILayout.Button("Create", new GUILayoutOption[]
				{
					GUILayout.Width(70f)
				}))
				{
					string text = EditorUtility.SaveFilePanelInProject(DirectorStyles.createNewTimelineText.get_text(), gameObject.get_name() + "Timeline", "playable", DirectorStyles.createNewTimelineText.get_text(), ProjectWindowUtil.GetActiveFolderPath());
					if (!string.IsNullOrEmpty(text))
					{
						TimelineAsset timelineAsset = ScriptableObject.CreateInstance<TimelineAsset>();
						AssetDatabase.CreateAsset(timelineAsset, text);
						Undo.IncrementCurrentGroup();
						if (playableDirector == null)
						{
							playableDirector = Undo.AddComponent<PlayableDirector>(gameObject);
						}
						playableDirector.set_playableAsset(timelineAsset);
						this.SetCurrentTimeline(timelineAsset, playableDirector);
						TrackAsset asset = timelineState.GetWindow().AddTrack(typeof(AnimationTrack), null, null);
						timelineState.previewMode = false;
						TimelineUtility.SetSceneGameObject(timelineState.currentDirector, asset, gameObject);
					}
				}
				GUILayout.EndHorizontal();
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
		}

		private void DrawTracksGUI(Rect clientRect, TimelineModeGUIState trackState)
		{
			GUILayout.BeginVertical(new GUILayoutOption[]
			{
				GUILayout.Height(clientRect.get_height())
			});
			using (new EditorGUI.DisabledScope(trackState == TimelineModeGUIState.Disabled))
			{
				this.TreeViewGUI(this.treeviewBounds);
			}
			GUILayout.EndVertical();
		}

		private void TreeViewGUI(Rect area)
		{
			if (this.state.captured.Count > 0 && Event.get_current().get_type() == 7)
			{
				Rect rect = area;
				rect.set_width(this.state.sequencerHeaderWidth);
				float num = 0f;
				if (rect.Contains(Event.get_current().get_mousePosition()))
				{
					if (this.state.timeAreaRect.get_x() >= 0f)
					{
						num = 2f;
					}
				}
				else
				{
					Rect rect2 = area;
					rect2.set_x(this.clientArea.get_width());
					rect2.set_width(50f);
					if (rect2.Contains(Event.get_current().get_mousePosition()))
					{
						num = -2f;
					}
				}
				if (Mathf.Abs(num) > 1.401298E-45f)
				{
					Vector3 vector = this.state.timeAreaTranslation;
					vector.x += num;
					this.state.SetTimeAreaTransform(vector, this.state.timeAreaScale);
					Event @event = new Event(Event.get_current());
					@event.set_type(3);
					@event.set_delta(new Vector2(-num, 0f));
					for (int num2 = 0; num2 != this.state.captured.Count; num2++)
					{
						this.state.captured[num2].OnEvent(@event, this.state, true);
					}
				}
			}
			if (this.treeView != null)
			{
				this.treeView.OnGUI(area);
			}
		}

		private void Upgrade(TimelineAsset timeline)
		{
			if (!(timeline == null))
			{
				string text = "";
				foreach (TrackAsset current in timeline.tracks)
				{
					if (current != null && current.clips != null)
					{
						TimelineClip[] clips = current.clips;
						for (int i = 0; i < clips.Length; i++)
						{
							TimelineClip timelineClip = clips[i];
							if (timelineClip.parentTrack == null)
							{
								timelineClip.parentTrack = current;
								Debug.LogWarning(timelineClip.displayName + " was parented to " + current.get_name());
								EditorUtility.SetDirty(current);
								text = "PARENT TRACK FIX-UP";
							}
							if (timelineClip.asset != null && timelineClip.asset is AnimationClip)
							{
								Debug.LogWarning("AnimationClips should be wrapped in an AnimationPlayableAsset object - Auto fixing");
								AnimationPlayableAsset animationPlayableAsset = ScriptableObject.CreateInstance<AnimationPlayableAsset>();
								animationPlayableAsset.clip = (timelineClip.asset as AnimationClip);
								timelineClip.asset = animationPlayableAsset;
								TimelineCreateUtilities.SaveAssetIntoObject(animationPlayableAsset, current);
							}
						}
					}
				}
				for (int j = timeline.tracks.Count - 1; j >= 0; j--)
				{
					TrackAsset trackAsset = timeline.tracks[j];
					TrackAsset trackAsset2 = trackAsset.parent as TrackAsset;
					if (trackAsset2 != null)
					{
						timeline.tracks.Remove(trackAsset);
					}
				}
				string assetPath = AssetDatabase.GetAssetPath(timeline);
				foreach (TrackAsset current2 in timeline.tracks)
				{
					if (AssetDatabase.GetAssetPath(current2) == assetPath)
					{
						TrackAsset expr_1C4 = current2;
						expr_1C4.set_hideFlags(expr_1C4.get_hideFlags() | 1);
						TimelineClip[] clips2 = current2.clips;
						for (int k = 0; k < clips2.Length; k++)
						{
							TimelineClip timelineClip2 = clips2[k];
							if (timelineClip2.asset != null)
							{
								string assetPath2 = AssetDatabase.GetAssetPath(timelineClip2.asset);
								if (assetPath2 == assetPath)
								{
									Object expr_221 = timelineClip2.asset;
									expr_221.set_hideFlags(expr_221.get_hideFlags() | 1);
								}
							}
							if (timelineClip2.curves != null)
							{
								AnimationClip expr_248 = timelineClip2.curves;
								expr_248.set_hideFlags(expr_248.get_hideFlags() | 1);
							}
						}
					}
				}
				foreach (TrackAsset current3 in timeline.flattenedTracks)
				{
					AnimationTrack animationTrack = current3 as AnimationTrack;
					if (animationTrack != null)
					{
						if (animationTrack.openClipTimeOffset > 0.0)
						{
							text += " time offset fixup";
							if (animationTrack.animClip != null)
							{
								animationTrack.animClip.ShiftBySeconds((float)(-(float)animationTrack.openClipTimeOffset));
							}
							animationTrack.openClipTimeOffset = 0.0;
						}
					}
				}
				IEnumerable<TimelineClip> enumerable = from x in timeline.flattenedTracks.SelectMany((TrackAsset x) => x.clips)
				where double.IsInfinity(x.start)
				select x;
				foreach (TimelineClip current4 in enumerable)
				{
					Debug.LogWarning("Removing " + current4.displayName + " becaue it has an invalid start time ");
					text = text + "Removing " + current4.displayName + " becaue it has an invalid start time ";
					current4.parentTrack.RemoveClip(current4);
				}
				foreach (TrackAsset current5 in timeline.tracks)
				{
					if (current5.parent == null)
					{
						current5.parent = this.state.timeline;
						text += " Fix parenting error";
					}
				}
				if (text.Length > 0)
				{
					Debug.LogWarning("SEQUENCE WAS UPGRADED (" + text + ") PLEASE RE-SAVE!");
					EditorUtility.SetDirty(timeline);
				}
				timeline.Invalidate();
			}
		}
	}
}
