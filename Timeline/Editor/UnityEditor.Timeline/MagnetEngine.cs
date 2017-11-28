using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;

namespace UnityEditor.Timeline
{
	internal class MagnetEngine
	{
		internal class MagnetInfo
		{
			public double time
			{
				get;
				set;
			}

			public double timeBeforeSnap
			{
				get;
				set;
			}

			public double durationBeforeSnap
			{
				get;
				set;
			}

			public bool IsInInfluenceZone(double currentTime, ITimelineState state, AttractedEdge direction)
			{
				float num = state.TimeToPixel(currentTime);
				float num2 = state.TimeToPixel(this.time);
				bool result;
				if (direction == AttractedEdge.Left)
				{
					result = (num > num2 - MagnetEngine.k_Epsilon && num < num2 + MagnetEngine.k_MagnetInfluenceInPixels);
				}
				else
				{
					result = (num > num2 - MagnetEngine.k_MagnetInfluenceInPixels - MagnetEngine.k_Epsilon && num < num2);
				}
				return result;
			}

			public bool IsInBothInfluenceZone(double currentTime, ITimelineState state)
			{
				float num = state.TimeToPixel(currentTime);
				float num2 = state.TimeToPixel(this.time);
				return num > num2 - MagnetEngine.k_MagnetInfluenceInPixels - MagnetEngine.k_Epsilon && num < num2 + MagnetEngine.k_MagnetInfluenceInPixels;
			}
		}

		internal class EdgeInfo
		{
			public double time
			{
				get;
				set;
			}

			public AttractedEdge edge
			{
				get;
				set;
			}

			public double deltaTime
			{
				get;
				set;
			}

			public double exitTime
			{
				get
				{
					return this.time + this.deltaTime;
				}
			}

			public bool IsAttractedBy(MagnetEngine.MagnetInfo magnet, ITimelineState state)
			{
				return magnet != null && magnet.IsInInfluenceZone(this.exitTime, state, this.edge);
			}

			public bool IsAttractedBothSide(MagnetEngine.MagnetInfo magnet, ITimelineState state)
			{
				return magnet != null && magnet.IsInBothInfluenceZone(this.exitTime, state);
			}
		}

		public static readonly float k_MagnetInfluenceInPixels = 10f;

		public static bool displayDebugLayout;

		private static readonly float k_Epsilon = 0.001f;

		private IAttractable m_Attractable;

		private IAttractionHandler m_AttractionHandler;

		private TimelineWindow.TimelineState m_State;

		private List<MagnetEngine.MagnetInfo> m_Magnets = new List<MagnetEngine.MagnetInfo>();

		private MagnetEngine.MagnetInfo m_ActiveMagnet = null;

		private ManipulateEdges m_ManipulateEdges = ManipulateEdges.None;

		private MagnetEngine.EdgeInfo m_AttractedEdge = null;

		private SnapState m_SnapState = SnapState.Free;

		public bool isSnapped
		{
			get
			{
				return this.m_ActiveMagnet != null;
			}
		}

		public MagnetEngine(IAttractable attractable, IAttractionHandler attractionHandler, TimelineWindow.TimelineState state)
		{
			this.m_Attractable = attractable;
			this.m_AttractionHandler = attractionHandler;
			this.m_State = state;
			Rect timeAreaBounds = TimelineWindow.instance.timeAreaBounds;
			timeAreaBounds.set_height(3.40282347E+38f);
			List<IBounds> elementsInRectangle = Manipulator.GetElementsInRectangle(state.quadTree, timeAreaBounds);
			IEnumerable<ISnappable> enumerable = elementsInRectangle.OfType<ISnappable>();
			this.AddMagnet(0.0, timeAreaBounds, state);
			this.AddMagnet(state.time, timeAreaBounds, state);
			this.AddMagnet(state.timeline.get_duration(), timeAreaBounds, state);
			foreach (ISnappable current in enumerable)
			{
				IEnumerable<Edge> enumerable2 = current.SnappableEdgesFor(attractable);
				foreach (Edge current2 in enumerable2)
				{
					this.AddMagnet(current2.time, timeAreaBounds, state);
				}
			}
		}

		public bool HasMagnetAt(double time)
		{
			return this.m_Magnets.Any((MagnetEngine.MagnetInfo x) => x.time > time - (double)MagnetEngine.k_Epsilon && x.time < time + (double)MagnetEngine.k_Epsilon);
		}

		public void DisplayDebugLayout(bool display)
		{
			MagnetEngine.displayDebugLayout = display;
		}

		private void ChangeState(SnapState newState)
		{
			this.m_SnapState = newState;
		}

		public void AddMagnet(double magnetTime)
		{
			this.m_Magnets.Add(new MagnetEngine.MagnetInfo
			{
				time = magnetTime
			});
		}

		private void AddMagnet(double magnetTime, Rect visibleRect, TimelineWindow.TimelineState state)
		{
			if (MagnetEngine.IsMagnetVisibleInRect(magnetTime, visibleRect, state))
			{
				this.m_Magnets.Add(new MagnetEngine.MagnetInfo
				{
					time = magnetTime
				});
			}
		}

		private static bool IsMagnetVisibleInRect(double time, Rect visibleRect, TimelineWindow.TimelineState state)
		{
			float num = state.TimeToPixel(time);
			return num > visibleRect.get_xMin() && num < visibleRect.get_xMax();
		}

		public void Snap(float offsetInPixels)
		{
			this.Snap(offsetInPixels, ManipulateEdges.Both);
		}

		public void Snap(float offsetInPixels, ManipulateEdges edges)
		{
			this.m_ManipulateEdges = edges;
			if (offsetInPixels <= -MagnetEngine.k_Epsilon || offsetInPixels >= MagnetEngine.k_Epsilon)
			{
				if (this.m_Magnets.Count != 0)
				{
					SnapState snapState = this.m_SnapState;
					if (snapState != SnapState.Free)
					{
						if (snapState == SnapState.Snapped)
						{
							this.ProcessSnappedState(offsetInPixels);
						}
					}
					else
					{
						this.ProcessFreeState(offsetInPixels);
					}
				}
			}
		}

		private void ProcessFreeState(float offsetInPixels)
		{
			bool flag = false;
			if (this.m_ManipulateEdges == ManipulateEdges.Left)
			{
				flag = this.m_Magnets.Any((MagnetEngine.MagnetInfo x) => this.IsLeftEdgeAttracted(x));
			}
			else if (this.m_ManipulateEdges == ManipulateEdges.Right)
			{
				flag = this.m_Magnets.Any((MagnetEngine.MagnetInfo x) => this.IsRightEdgeAttracted(x));
			}
			else if (this.m_ManipulateEdges == ManipulateEdges.Both && offsetInPixels > 0f)
			{
				flag = this.m_Magnets.Any((MagnetEngine.MagnetInfo x) => this.IsRightEdgeAttracted(x));
			}
			else if (this.m_ManipulateEdges == ManipulateEdges.Both && offsetInPixels < 0f)
			{
				flag = this.m_Magnets.Any((MagnetEngine.MagnetInfo x) => this.IsLeftEdgeAttracted(x));
			}
			if (flag)
			{
				this.ChangeState(SnapState.Snapped);
			}
		}

		private void ProcessSnappedState(float offsetInPixels)
		{
			if (this.m_ActiveMagnet != null)
			{
				this.m_AttractedEdge.deltaTime += (double)this.ToDeltaTime(offsetInPixels);
				if (!this.m_AttractedEdge.IsAttractedBothSide(this.m_ActiveMagnet, this.m_State))
				{
					this.ExitMagnet();
				}
				else
				{
					this.m_AttractionHandler.OnAttractedEdge(this.m_Attractable, AttractedEdge.None, this.m_ActiveMagnet.timeBeforeSnap, this.m_ActiveMagnet.durationBeforeSnap);
				}
			}
		}

		private float ToDeltaTime(float deltaPixel)
		{
			float num = this.m_State.PixelToTime(0f);
			float num2 = this.m_State.PixelToTime(deltaPixel);
			return num2 - num;
		}

		private void ExitMagnet()
		{
			AttractedEdge attractedEdge = this.m_AttractedEdge.edge;
			double num = this.m_AttractedEdge.exitTime;
			if (this.m_ManipulateEdges == ManipulateEdges.Left)
			{
				attractedEdge = AttractedEdge.Left;
			}
			if (this.m_ManipulateEdges == ManipulateEdges.Right)
			{
				attractedEdge = AttractedEdge.Right;
			}
			if (attractedEdge == AttractedEdge.Left || attractedEdge == AttractedEdge.Right)
			{
				if (Event.get_current() != null)
				{
					Vector2 mousePosition = Event.get_current().get_mousePosition();
					num = (double)this.m_State.PixelToTime(mousePosition.x);
				}
			}
			this.m_AttractionHandler.OnAttractedEdge(this.m_Attractable, this.m_AttractedEdge.edge, this.m_AttractedEdge.exitTime, this.m_ActiveMagnet.durationBeforeSnap);
			this.m_AttractedEdge = null;
			this.m_ActiveMagnet = null;
			this.ChangeState(SnapState.Free);
		}

		private bool IsRightEdgeAttracted(MagnetEngine.MagnetInfo magnet)
		{
			bool result;
			if (magnet.IsInInfluenceZone(this.m_Attractable.end, this.m_State, AttractedEdge.Right))
			{
				this.m_ActiveMagnet = magnet;
				this.m_AttractedEdge = new MagnetEngine.EdgeInfo
				{
					time = this.m_Attractable.end,
					edge = AttractedEdge.Right
				};
				double duration = this.m_Attractable.end - this.m_Attractable.start;
				this.m_AttractionHandler.OnAttractedEdge(this.m_Attractable, this.m_AttractedEdge.edge, magnet.time, duration);
				magnet.timeBeforeSnap = this.m_Attractable.start;
				magnet.durationBeforeSnap = this.m_Attractable.end - this.m_Attractable.start;
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

		private bool IsLeftEdgeAttracted(MagnetEngine.MagnetInfo magnet)
		{
			bool result;
			if (magnet.IsInInfluenceZone(this.m_Attractable.start, this.m_State, AttractedEdge.Left))
			{
				this.m_ActiveMagnet = magnet;
				this.m_AttractedEdge = new MagnetEngine.EdgeInfo
				{
					time = this.m_Attractable.start,
					edge = AttractedEdge.Left
				};
				double duration = this.m_Attractable.end - this.m_Attractable.start;
				this.m_AttractionHandler.OnAttractedEdge(this.m_Attractable, this.m_AttractedEdge.edge, magnet.time, duration);
				magnet.timeBeforeSnap = magnet.time;
				magnet.durationBeforeSnap = this.m_Attractable.end - this.m_Attractable.start;
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

		public bool IsSnappedAtTime(double time)
		{
			bool result;
			if (this.m_ActiveMagnet == null)
			{
				result = false;
			}
			else
			{
				float num = this.m_State.TimeToPixel(time);
				float num2 = this.m_State.TimeToPixel(this.m_ActiveMagnet.time);
				result = (num - num2 <= MagnetEngine.k_Epsilon);
			}
			return result;
		}

		public void OnGUI()
		{
			if (MagnetEngine.displayDebugLayout)
			{
				foreach (MagnetEngine.MagnetInfo current in this.m_Magnets)
				{
					TimelineWindow instance = TimelineWindow.instance;
					Rect rect = new Rect(this.m_State.TimeToPixel(current.time) - MagnetEngine.k_MagnetInfluenceInPixels, instance.timeAreaBounds.get_yMax(), 2f * MagnetEngine.k_MagnetInfluenceInPixels, this.m_State.windowHeight);
					EditorGUI.DrawRect(rect, new Color(1f, 0f, 0f, 0.4f));
				}
				Vector2 mousePosition = Event.get_current().get_mousePosition();
				float num = this.m_State.PixelToTime(mousePosition.x);
				Vector2 vector = new Vector2(this.m_State.TimeToPixel((double)num), TimelineWindow.instance.timeAreaBounds.get_yMax());
				Vector2 vector2 = new Vector2(1f, this.m_State.windowHeight);
				EditorGUI.DrawRect(new Rect(vector, vector2), Color.get_blue());
			}
			if (this.m_ActiveMagnet != null)
			{
				TimelineWindow instance2 = TimelineWindow.instance;
				Vector2 vector3 = new Vector2(this.m_State.TimeToPixel(this.m_ActiveMagnet.time), instance2.timeAreaBounds.get_yMax());
				Vector2 vector4 = new Vector2(1f, this.m_State.windowHeight);
				EditorGUI.DrawRect(new Rect(vector3, vector4), DirectorStyles.Instance.customSkin.colorSnapLine);
			}
		}
	}
}
