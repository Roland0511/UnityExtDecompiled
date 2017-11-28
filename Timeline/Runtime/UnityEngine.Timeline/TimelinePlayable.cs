using System;
using System.Collections.Generic;
using UnityEngine.Animations;
using UnityEngine.Audio;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
	public class TimelinePlayable : PlayableBehaviour
	{
		private struct ConnectionCache
		{
			public Playable playable;

			public int port;

			public Playable parent;

			public float evalWeight;
		}

		private IntervalTree<RuntimeElement> m_IntervalTree = new IntervalTree<RuntimeElement>();

		private List<RuntimeElement> m_ActiveClips = new List<RuntimeElement>();

		private List<RuntimeElement> m_CurrentListOfActiveClips;

		private int m_ActiveBit = 0;

		private List<ITimelineEvaluateCallback> m_EvaluateCallbacks = new List<ITimelineEvaluateCallback>();

		private Dictionary<TrackAsset, TimelinePlayable.ConnectionCache> m_PlayableCache = new Dictionary<TrackAsset, TimelinePlayable.ConnectionCache>();

		public static ScriptPlayable<TimelinePlayable> Create(PlayableGraph graph, IEnumerable<TrackAsset> tracks, GameObject go, bool autoRebalance, bool createOutputs)
		{
			if (tracks == null)
			{
				throw new ArgumentNullException("Tracks list is null", "tracks");
			}
			if (go == null)
			{
				throw new ArgumentNullException("GameObject parameter is null", "go");
			}
			ScriptPlayable<TimelinePlayable> scriptPlayable = ScriptPlayable<TimelinePlayable>.Create(graph, 0);
			TimelinePlayable behaviour = scriptPlayable.GetBehaviour();
			behaviour.Compile(graph, scriptPlayable, tracks, go, autoRebalance, createOutputs);
			return scriptPlayable;
		}

		public void Compile(PlayableGraph graph, Playable timelinePlayable, IEnumerable<TrackAsset> tracks, GameObject go, bool autoRebalance, bool createOutputs)
		{
			if (tracks == null)
			{
				throw new ArgumentNullException("Tracks list is null", "tracks");
			}
			if (go == null)
			{
				throw new ArgumentNullException("GameObject parameter is null", "go");
			}
			List<TrackAsset> list = new List<TrackAsset>(tracks);
			int capacity = list.Count * 2 + list.Count;
			this.m_CurrentListOfActiveClips = new List<RuntimeElement>(capacity);
			this.m_ActiveClips = new List<RuntimeElement>(capacity);
			this.m_EvaluateCallbacks.Clear();
			this.m_PlayableCache.Clear();
			this.AllocateDefaultTracks(graph, timelinePlayable, list, go);
			this.CompileTrackList(graph, timelinePlayable, list, go, createOutputs);
		}

		private void AllocateDefaultTracks(PlayableGraph graph, Playable timelinePlayable, IList<TrackAsset> tracks, GameObject go)
		{
		}

		private void CompileTrackList(PlayableGraph graph, Playable timelinePlayable, IEnumerable<TrackAsset> tracks, GameObject go, bool createOutputs)
		{
			foreach (TrackAsset current in tracks)
			{
				if (current.compilable)
				{
					if (!this.m_PlayableCache.ContainsKey(current))
					{
						current.SortClips();
						this.CreateTrackPlayable(graph, timelinePlayable, current, go, createOutputs);
					}
				}
			}
		}

		private void CreateTrackOutput(PlayableGraph graph, TrackAsset track, Playable playable, int port)
		{
			if (!track.isSubTrack)
			{
				IEnumerable<PlayableBinding> outputs = track.get_outputs();
				foreach (PlayableBinding current in outputs)
				{
					if (current.get_streamType() == null)
					{
						AnimationPlayableOutput animationPlayableOutput = AnimationPlayableOutput.Create(graph, current.get_streamName(), null);
						PlayableOutputExtensions.SetReferenceObject<AnimationPlayableOutput>(animationPlayableOutput, current.get_sourceObject());
						PlayableOutputExtensions.SetSourcePlayable<AnimationPlayableOutput, Playable>(animationPlayableOutput, playable);
						PlayableOutputExtensions.SetSourceInputPort<AnimationPlayableOutput>(animationPlayableOutput, port);
						this.m_EvaluateCallbacks.Add(new AnimationOutputWeightProcessor(animationPlayableOutput));
					}
					else if (current.get_streamType() == 1)
					{
						AudioPlayableOutput audioPlayableOutput = AudioPlayableOutput.Create(graph, current.get_streamName(), null);
						PlayableOutputExtensions.SetReferenceObject<AudioPlayableOutput>(audioPlayableOutput, current.get_sourceObject());
						PlayableOutputExtensions.SetSourcePlayable<AudioPlayableOutput, Playable>(audioPlayableOutput, playable);
						PlayableOutputExtensions.SetSourceInputPort<AudioPlayableOutput>(audioPlayableOutput, port);
					}
					else
					{
						if (current.get_streamType() != 3)
						{
							throw new NotImplementedException("Unsupported stream type");
						}
						ScriptPlayableOutput scriptPlayableOutput = ScriptPlayableOutput.Create(graph, current.get_streamName());
						PlayableOutputExtensions.SetReferenceObject<ScriptPlayableOutput>(scriptPlayableOutput, current.get_sourceObject());
						PlayableOutputExtensions.SetSourcePlayable<ScriptPlayableOutput, Playable>(scriptPlayableOutput, playable);
						PlayableOutputExtensions.SetSourceInputPort<ScriptPlayableOutput>(scriptPlayableOutput, port);
					}
				}
			}
		}

		private static Playable CreatePlayableGraph(PlayableGraph graph, TrackAsset asset, GameObject go, IntervalTree<RuntimeElement> tree)
		{
			return asset.CreatePlayableGraph(graph, go, tree);
		}

		private Playable CreateTrackPlayable(PlayableGraph graph, Playable timelinePlayable, TrackAsset track, GameObject go, bool createOutputs)
		{
			Playable result;
			TimelinePlayable.ConnectionCache connectionCache;
			if (!track.compilable)
			{
				result = timelinePlayable;
			}
			else if (this.m_PlayableCache.TryGetValue(track, out connectionCache))
			{
				result = connectionCache.playable;
			}
			else if (track.get_name() == "root")
			{
				result = timelinePlayable;
			}
			else
			{
				TrackAsset trackAsset = track.parent as TrackAsset;
				Playable playable = (!(trackAsset != null)) ? timelinePlayable : this.CreateTrackPlayable(graph, timelinePlayable, trackAsset, go, createOutputs);
				Playable playable2 = TimelinePlayable.CreatePlayableGraph(graph, track, go, this.m_IntervalTree);
				bool flag = false;
				if (!PlayableExtensions.IsValid<Playable>(playable2))
				{
					throw new InvalidOperationException(string.Concat(new object[]
					{
						track.get_name(),
						"(",
						track.GetType(),
						") did not produce a valid playable. Use the compilable property to indicate whether the track is valid for processing"
					}));
				}
				if (PlayableExtensions.IsValid<Playable>(playable) && PlayableExtensions.IsValid<Playable>(playable2))
				{
					int inputCount = PlayableExtensions.GetInputCount<Playable>(playable);
					PlayableExtensions.SetInputCount<Playable>(playable, inputCount + 1);
					flag = graph.Connect<Playable, Playable>(playable2, 0, playable, inputCount);
					PlayableExtensions.SetInputWeight<Playable>(playable, inputCount, 1f);
				}
				if (createOutputs && flag)
				{
					this.CreateTrackOutput(graph, track, playable, PlayableExtensions.GetInputCount<Playable>(playable) - 1);
				}
				this.CacheTrack(track, playable2, (!flag) ? -1 : (PlayableExtensions.GetInputCount<Playable>(playable) - 1), playable);
				result = playable2;
			}
			return result;
		}

		public override void PrepareFrame(Playable playable, FrameData info)
		{
			this.Evaluate(playable, info);
		}

		private void Evaluate(Playable playable, FrameData frameData)
		{
			if (this.m_IntervalTree != null)
			{
				double time = PlayableExtensions.GetTime<Playable>(playable);
				this.m_ActiveBit = ((this.m_ActiveBit != 0) ? 0 : 1);
				this.m_CurrentListOfActiveClips.Clear();
				this.m_IntervalTree.IntersectsWith(DiscreteTime.GetNearestTick(time), this.m_ActiveBit, ref this.m_CurrentListOfActiveClips);
				for (int i = 0; i < this.m_ActiveClips.Count; i++)
				{
					RuntimeElement runtimeElement = this.m_ActiveClips[i];
					if (runtimeElement.intervalBit != this.m_ActiveBit)
					{
						runtimeElement.enable = false;
					}
				}
				for (int j = 0; j < this.m_CurrentListOfActiveClips.Count; j++)
				{
					this.m_CurrentListOfActiveClips[j].EvaluateAt(time, frameData);
				}
				this.m_ActiveClips.Clear();
				this.m_ActiveClips.AddRange(this.m_CurrentListOfActiveClips);
				int count = this.m_EvaluateCallbacks.Count;
				for (int k = 0; k < count; k++)
				{
					this.m_EvaluateCallbacks[k].Evaluate();
				}
			}
		}

		private void CacheTrack(TrackAsset track, Playable playable, int port, Playable parent)
		{
			this.m_PlayableCache[track] = new TimelinePlayable.ConnectionCache
			{
				playable = playable,
				port = port,
				parent = parent,
				evalWeight = 0f
			};
		}
	}
}
