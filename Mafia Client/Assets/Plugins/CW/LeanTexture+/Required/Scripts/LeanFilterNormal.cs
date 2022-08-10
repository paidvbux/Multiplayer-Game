using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using CW.Common;

namespace Lean.Texture
{
	/// <summary>This allows you to convert the texture into a normal map based on height data extracted from the RGBA values in the specified way.</summary>
	[System.Serializable]
	public class LeanFilterNormal : LeanFilter
	{
		public override string Title
		{
			get
			{
				return "Convert To Normal";
			}
		}

		/// <summary>The texture heights will be calculated based on this aspect of the pixels.</summary>
		public LeanHeight.Source HeightSource { set { heightSource = (int)value; } get { return (LeanHeight.Source)heightSource; } } [SerializeField] [LeanEnum(typeof(LeanHeight.Source))] private int heightSource = (int)LeanHeight.Source.Luminance;

		/// <summary>The overall strength of the output normals.</summary>
		public float Strength { set { strength = value; } get { return strength; } } [SerializeField] private float strength = 1.0f;

		/// <summary>The contribution to the output normals based on 2 samples across each axis.</summary>
		public float Influence2 { set { influence2 = value; } get { return influence2; } } [SerializeField] [Range(0.0f, 1.0f)] private float influence2 = 1.0f;

		/// <summary>The contribution to the output normals based on 4 samples across each axis.</summary>
		public float Influence4 { set { influence4 = value; } get { return influence4; } } [SerializeField] [Range(0.0f, 1.0f)] private float influence4 = 1.0f;

		[BurstCompile]
		struct FilterJob : IJobParallelFor
		{
			[WriteOnly] public NativeArray<float4> OUT;

			[ReadOnly] public NativeArray<float4>  IN;
			[ReadOnly] public int2                 Size;
			//[ReadOnly] public NativeArray<float4>  SrcHalf;
			//[ReadOnly] public int2                 SrcHalfSize;
			//[ReadOnly] public NativeArray<float4>  SrcHalfHalf;
			//[ReadOnly] public int2                 SrcHalfHalfSize;
			[ReadOnly] public TextureWrapMode      WrapU;
			[ReadOnly] public TextureWrapMode      WrapV;
			[ReadOnly] public LeanHeight.Source    HeightSource;
			[ReadOnly] public float                Strength;
			[ReadOnly] public float                Influence2;
			[ReadOnly] public float                Influence4;

			public void Execute(int index)
			{
				var x  = index % Size.x;
				var y  = index / Size.x;
				var uv = new float2(x, y) / (Size - 1);

				var heightL = SampleHeight(x, y, -1,  0, WrapU);
				var heightR = SampleHeight(x, y, +1,  0, WrapU);
				var heightB = SampleHeight(x, y,  0, -1, WrapV);
				var heightT = SampleHeight(x, y,  0, +1, WrapV);

				var deltaX = (heightL - heightR) * Strength;
				var deltaY = (heightB - heightT) * Strength;

				var normal = (float3)math.normalize(new double3(deltaX, deltaY, 1.0f));

				OUT[index] = new float4(normal * 0.5f + 0.5f, 0.0f);
			}

			private float SampleHeight_0(int x, int y, TextureWrapMode wrap)
			{
				var total = 0.0f;

				total += SampleHeight(x, y, wrap);

				return total;
			}

			private float SampleHeight_1(int x, int y, TextureWrapMode wrap)
			{
				var total = 0.0f;

				total += SampleHeight(x-1, y, wrap);
				total += SampleHeight(x+1, y, wrap);
				total += SampleHeight(x, y-1, wrap);
				total += SampleHeight(x, y+1, wrap);

				return total / 4.0f;
			}

			private float SampleHeight(int x, int y, int d_x, int d_y, TextureWrapMode wrap)
			{
				var total = 0.0f;

				total += SampleHeight_0(x + d_x, y + d_y, wrap) * Influence2;
				total += SampleHeight_1(x + d_x * 2, y + d_y * 2, wrap) * Influence4;

				return total / 2.0f;
			}

			private float SampleHeight(int x, int y, TextureWrapMode wrap)
			{
				var sample = default(float4);

				if (wrap == TextureWrapMode.Repeat)
				{
					sample = LeanSample.Tex2D_Point_WrapXY(IN, Size, x, y);
				}
				else
				{
					sample = LeanSample.Tex2D_Point(IN, Size, x, y);
				}

				return LeanHeight.Calculate(sample, HeightSource);
			}
		}

		public override void Schedule(LeanPendingTexture data)
		{
			var filter = new FilterJob();

			//data.Downsample(data.Pixels, data.Size, ref filter.SrcHalf, ref filter.SrcHalfSize);
			//data.Downsample(filter.SrcHalf, filter.SrcHalfSize, ref filter.SrcHalfHalf, ref filter.SrcHalfHalfSize);

			data.DoubleBuffer(ref filter.IN, ref filter.OUT);

			filter.Size         = data.Size;
			filter.HeightSource = HeightSource;
			filter.Strength     = strength;
			filter.Influence2   = influence2;
			filter.Influence4   = influence4;
			filter.WrapU        = data.WrapU;
			filter.WrapV        = data.WrapV;

			data.Handle = filter.Schedule(data.Pixels.Length, 32, data.Handle);
		}

#if UNITY_EDITOR
		protected override void DrawInspector()
		{
			CwEditor.Draw("heightSource", "The texture heights will be calculated based on this aspect of the pixels.");
			CwEditor.Draw("strength", "The overall strength of the output normals.");
			CwEditor.Draw("influence2", "The contribution to the output normals based on 2 samples across each axis.");
			CwEditor.Draw("influence4", "The contribution to the output normals based on 4 samples across each axis.");
		}
#endif
	}
}