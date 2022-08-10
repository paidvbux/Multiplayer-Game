using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using CW.Common;
using System.Collections.Generic;

namespace Lean.Texture
{
	/// <summary>This allows you to blur the texture by the specified radius.</summary>
	[System.Serializable]
	public class LeanFilterBlur : LeanFilter
	{
		public override string Title
		{
			get
			{
				return "Blur";
			}
		}

		/// <summary>The radius of the blur in pixels.</summary>
		public float Radius { set { radius = value; } get { return radius; } } [SerializeField] private float radius = 5.0f;

		private static List<float> weights = new List<float>();

		[BurstCompile]
		struct FilterJob_X : IJobParallelFor
		{
			[WriteOnly] public NativeArray<float4> OUT;

			[ReadOnly] public NativeArray<float4> IN;
			[ReadOnly] public NativeArray<float> Weights;

			[ReadOnly] public int2 Size;
			[ReadOnly] public bool Repeat;

			public void Execute(int index)
			{
				var x = index % Size.x;
				var y = index / Size.x;
				var t = float4.zero;

				if (Repeat == true)
				{
					t += LeanSample.Tex2D_Point_WrapXY(IN, Size, x, y) * Weights[0];

					for (var i = 1; i < Weights.Length; i++)
					{
						t += (LeanSample.Tex2D_Point_WrapXY(IN, Size, x - i, y) + LeanSample.Tex2D_Point_WrapXY(IN, Size, x + i, y)) * Weights[i];
					}
				}
				else
				{
					t += LeanSample.Tex2D_Point(IN, Size, x, y) * Weights[0];

					for (var i = 1; i < Weights.Length; i++)
					{
						t += (LeanSample.Tex2D_Point(IN, Size, x - i, y) + LeanSample.Tex2D_Point(IN, Size, x + i, y)) * Weights[i];
					}
				}

				OUT[index] = t;
			}
		}

		[BurstCompile]
		struct FilterJob_Y : IJobParallelFor
		{
			[WriteOnly] public NativeArray<float4> OUT;

			[ReadOnly] public NativeArray<float4> IN;
			[ReadOnly] public NativeArray<float> Weights;

			[ReadOnly] public int2 Size;
			[ReadOnly] public bool Repeat;

			public void Execute(int index)
			{
				var x = index % Size.x;
				var y = index / Size.x;
				var t = float4.zero;

				if (Repeat == true)
				{
					t += LeanSample.Tex2D_Point_WrapXY(IN, Size, x, y) * Weights[0];

					for (var i = 1; i < Weights.Length; i++)
					{
						t += (LeanSample.Tex2D_Point_WrapXY(IN, Size, x, y - i) + LeanSample.Tex2D_Point_WrapXY(IN, Size, x, y + i)) * Weights[i];
					}
				}
				else
				{
					t += LeanSample.Tex2D_Point(IN, Size, x, y) * Weights[0];

					for (var i = 1; i < Weights.Length; i++)
					{
						t += (LeanSample.Tex2D_Point(IN, Size, x, y - i) + LeanSample.Tex2D_Point(IN, Size, x, y + i)) * Weights[i];
					}
				}

				OUT[index] = t;
			}
		}

		public override void Schedule(LeanPendingTexture data)
		{
			if (radius > 0.0f)
			{
				var steps = Mathf.CeilToInt(radius);
				var step  = 1.0f / (steps - 1);
				var total = 0.0f;

				// Calculate weights
				weights.Clear();

				for (var i = 0; i < steps; i++)
				{
					var weight = math.smoothstep(1.0f, 0.0f, i * step);

					weights.Add(weight);

					total += weight * 2.0f;
				}

				total -= weights[0];

				// Scale to 1.0 total and write to NativeArray
				var tempWeights = new NativeArray<float>(weights.Count, Allocator.TempJob); data.Register(tempWeights);

				for (var i = 0; i < steps; i++)
				{
					tempWeights[i] = weights[i] / total;
				}

				// Blur horizontally
				var filter_x = new FilterJob_X();

				data.DoubleBuffer(ref filter_x.IN, ref filter_x.OUT);

				filter_x.Size    = data.Size;
				filter_x.Weights = tempWeights;
				filter_x.Repeat  = data.WrapU == TextureWrapMode.Repeat;

				data.Handle = filter_x.Schedule(data.Pixels.Length, 32, data.Handle);

				// Blur vertically
				var filter_y = new FilterJob_Y();

				data.DoubleBuffer(ref filter_y.IN, ref filter_y.OUT);

				filter_y.Size    = data.Size;
				filter_y.Weights = tempWeights;
				filter_y.Repeat  = data.WrapV == TextureWrapMode.Repeat;

				data.Handle = filter_y.Schedule(data.Pixels.Length, 32, data.Handle);
			}
		}

#if UNITY_EDITOR
		protected override void DrawInspector()
		{
			CwEditor.Draw("radius", "The radius of the blur in pixels.");
		}
#endif
	}
}