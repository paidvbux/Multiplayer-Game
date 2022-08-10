using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using CW.Common;

namespace Lean.Texture
{
	/// <summary>This allows you to offset the texture by the specified amount.</summary>
	[System.Serializable]
	public class LeanFilterOffset : LeanFilter
	{
		public override string Title
		{
			get
			{
				return "Offset Pixels";
			}
		}

		public enum CoordinateType
		{
			ZeroOne,
			Pixels
		}

		public enum ChannelType
		{
			AllTheSame,
			Individual
		}

		/// <summary>Should this filter apply to all channels the same, or should each channel be separate?</summary>
		public ChannelType Channel { set { channel = (int)value; } get { return (ChannelType)channel; } } [SerializeField] [LeanEnum(typeof(ChannelType))] private int channel;

		/// <summary>The coordinate space of the offset.</summary>
		public CoordinateType Coordinate { set { coordinate = (int)value; } get { return (CoordinateType)coordinate; } } [SerializeField] [LeanEnum(typeof(CoordinateType))] private int coordinate = (int)CoordinateType.Pixels;

		/// <summary>The amount the texture will be shifted across the x (right) and y (up) axes.</summary>
		public float2 Offset { set { offset = value; } get { return offset; } } [SerializeField] private float2 offset;

		/// <summary>The amount the texture will be shifted across the x (right) and y (up) axes.</summary>
		public float2 OffsetR { set { offsetR = value; } get { return offsetR; } } [SerializeField] private float2 offsetR;

		/// <summary>The amount the texture will be shifted across the x (right) and y (up) axes.</summary>
		public float2 OffsetG { set { offsetG = value; } get { return offsetG; } } [SerializeField] private float2 offsetG;

		/// <summary>The amount the texture will be shifted across the x (right) and y (up) axes.</summary>
		public float2 OffsetB { set { offsetB = value; } get { return offsetB; } } [SerializeField] private float2 offsetB;

		/// <summary>The amount the texture will be shifted across the x (right) and y (up) axes.</summary>
		public float2 OffsetA { set { offsetA = value; } get { return offsetA; } } [SerializeField] private float2 offsetA;

		[BurstCompile]
		struct FilterJob : IJobParallelFor
		{
			[WriteOnly] public NativeArray<float4> OUT;

			[ReadOnly] public NativeArray<float4> IN;

			[ReadOnly] public int2 Size;
			[ReadOnly] public int2 OffsetR;
			[ReadOnly] public int2 OffsetG;
			[ReadOnly] public int2 OffsetB;
			[ReadOnly] public int2 OffsetA;

			public void Execute(int index)
			{
				var x = index % Size.x;
				var y = index / Size.x;
				var p = default(float4);

				p.x = Sample(x + OffsetR.x, y + OffsetR.y).x;
				p.y = Sample(x + OffsetG.x, y + OffsetG.y).y;
				p.z = Sample(x + OffsetB.x, y + OffsetB.y).z;
				p.w = Sample(x + OffsetA.x, y + OffsetA.y).w;

				OUT[index] = p;
			}

			private float4 Sample(int x, int y)
			{
				return LeanSample.Tex2D_Point_WrapXY(IN, Size, x, y);
			}
		}

		public override void Schedule(LeanPendingTexture data)
		{
			var filter = new FilterJob();
			

			data.DoubleBuffer(ref filter.IN, ref filter.OUT);

			filter.Size = data.Size;

			if (Channel == ChannelType.Individual)
			{
				filter.OffsetR = GetOffset(offsetR, data.Size);
				filter.OffsetG = GetOffset(offsetG, data.Size);
				filter.OffsetB = GetOffset(offsetB, data.Size);
				filter.OffsetA = GetOffset(offsetA, data.Size);
			}
			else
			{
				filter.OffsetR = filter.OffsetG = filter.OffsetB = filter.OffsetA = GetOffset(offset, data.Size);
			}

			data.Handle = filter.Schedule(data.Pixels.Length, 32, data.Handle);
		}

		private int2 GetOffset(float2 o, int2 size)
		{
			var pixels = o;

			if (Coordinate == CoordinateType.ZeroOne)
			{
				pixels *= size;
			}

			return (int2)pixels;
		}

#if UNITY_EDITOR
		protected override void DrawInspector()
		{
			CwEditor.Draw("channel", "Should this filter apply to all channels the same, or should each channel be separate?");
			CwEditor.Draw("coordinate", "The coordinate space of the offset.");
			if (CwEditor.GetProperty("channel").intValue == (int)ChannelType.Individual)
			{
				CwEditor.Draw("offsetR", "The amount the texture will be shifted across the x (right) and y (up) axes.");
				CwEditor.Draw("offsetG", "The amount the texture will be shifted across the x (right) and y (up) axes.");
				CwEditor.Draw("offsetB", "The amount the texture will be shifted across the x (right) and y (up) axes.");
				CwEditor.Draw("offsetA", "The amount the texture will be shifted across the x (right) and y (up) axes.");
			}
			else
			{
				CwEditor.Draw("offset", "The amount the texture will be shifted across the x (right) and y (up) axes.");
			}
		}
#endif
	}
}