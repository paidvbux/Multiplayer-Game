using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using CW.Common;

namespace Lean.Texture
{
	/// <summary>This allows you to convert the texture into a seamless one.</summary>
	[System.Serializable]
	public class LeanFilterSeamless : LeanFilter
	{
		public override string Title
		{
			get
			{
				return "Make Seamless";
			}
		}

		/// <summary>The thickness of the transition region.</summary>
		public float Thickness { set { thickness = value; } get { return thickness; } } [SerializeField] [Range(0.01f, 1.0f)] private float thickness = 0.5f;

		/// <summary>The position along the texture the seam will be filled in with.</summary>
		public float OffsetX { set { offsetX = value; } get { return offsetX; } } [SerializeField] [Range(0.0f, 1.0f)] private float offsetX = 0.5f;

		/// <summary>The position along the texture the seam will be filled in with.</summary>
		public float OffsetY { set { offsetY = value; } get { return offsetY; } } [SerializeField] [Range(0.0f, 1.0f)] private float offsetY = 0.5f;

		/// <summary>The texture transition will be modified based on height data from this aspect of the pixels.</summary>
		public LeanHeight.Source HeightSource { set { heightSource = (int)value; } get { return (LeanHeight.Source)heightSource; } } [SerializeField] [LeanEnum(typeof(LeanHeight.Source))] private int heightSource = (int)LeanHeight.Source.Luminance;

		/// <summary>The amount of impact the height data has on the transition.</summary>
		public float HeightInfluence { set { heightInfluence = value; } get { return heightInfluence; } } [SerializeField] [Range(0.0f, 1.0f)] private float heightInfluence = 1.0f;

		/// <summary>The frequency of the noise data.</summary>
		public float NoiseTiling { set { noiseTiling = value; } get { return noiseTiling; } } [SerializeField] [Range(1.0f, 20.0f)] private float noiseTiling = 5.0f;

		/// <summary>The amount of impact the noise data has on the transition.</summary>
		public float NoiseInfluence { set { noiseInfluence = value; } get { return noiseInfluence; } } [SerializeField] [Range(0.0f, 0.5f)] private float noiseInfluence = 0.5f;

		[BurstCompile]
		struct FilterJob_X : IJobParallelFor
		{
			[WriteOnly] public NativeArray<float4> OUT;

			[ReadOnly] public NativeArray<float4> IN;
			[ReadOnly] public int2                Size;
			[ReadOnly] public float               Thickness;
			[ReadOnly] public int                 Offset;
			[ReadOnly] public LeanHeight.Source HeightSource;
			[ReadOnly] public float               HeightInfluence;
			[ReadOnly] public float               NoiseTiling;
			[ReadOnly] public float               NoiseInfluence;

			public void Execute(int index)
			{
				var x  = index % Size.x;
				var y  = index / Size.x;
				var uv = new float2(x, y) / (Size - 1);
				var a  = LeanSample.Tex2D_Point_WrapXY(IN, Size, x, y);
				var b  = LeanSample.Tex2D_Point_WrapXY(IN, Size, x + Offset, y);
				var t  = math.saturate(math.abs((uv.x - 0.5f) * 2.0f));

				var k = Thickness;
				k = math.lerp(k, 1.0f, (noise.snoise(new float2(0.0f, uv.y) * NoiseTiling) + 1.0f) * NoiseInfluence);
				k = math.lerp(k, k * LeanHeight.Calculate(a, HeightSource), HeightInfluence);

				t = math.pow(t, 1.0f / (k + 0.0001f));

				OUT[index] = math.lerp(a, b, t);
			}
		}

		[BurstCompile]
		struct FilterJob_Y : IJobParallelFor
		{
			[WriteOnly] public NativeArray<float4> OUT;

			[ReadOnly] public NativeArray<float4> IN;
			[ReadOnly] public int2                Size;
			[ReadOnly] public float               Thickness;
			[ReadOnly] public int                 Offset;
			[ReadOnly] public LeanHeight.Source   HeightSource;
			[ReadOnly] public float               HeightInfluence;
			[ReadOnly] public float               NoiseTiling;
			[ReadOnly] public float               NoiseInfluence;

			public void Execute(int index)
			{
				var x  = index % Size.x;
				var y  = index / Size.x;
				var uv = new float2(x, y) / (Size - 1);
				var a  = LeanSample.Tex2D_Point_WrapXY(IN, Size, x, y);
				var b  = LeanSample.Tex2D_Point_WrapXY(IN, Size, x, y + Offset);
				var t  = math.saturate(math.abs((uv.y - 0.5f) * 2.0f));

				var k = Thickness;
				k = math.lerp(k, 1.0f, (noise.snoise(new float2(uv.x, 0.0f) * NoiseTiling) + 1.0f) * NoiseInfluence);
				k = math.lerp(k, k * LeanHeight.Calculate(a, HeightSource), HeightInfluence);

				t = math.pow(t, 1.0f / (k + 0.0001f));

				OUT[index] = math.lerp(a, b, t);
			}
		}

		public override void Schedule(LeanPendingTexture data)
		{
			if (data.WrapU == TextureWrapMode.Repeat)
			{
				var filter = new FilterJob_X();

				data.DoubleBuffer(ref filter.IN, ref filter.OUT);

				filter.Size            = data.Size;
				filter.Thickness       = thickness;
				filter.Offset          = (int)(data.Size.x * offsetX);
				filter.HeightSource    = HeightSource;
				filter.HeightInfluence = heightInfluence;
				filter.NoiseTiling     = noiseTiling;
				filter.NoiseInfluence  = noiseInfluence;

				data.Handle = filter.Schedule(data.Pixels.Length, 32, data.Handle);
			}

			if (data.WrapV == TextureWrapMode.Repeat)
			{
				var filter = new FilterJob_Y();

				data.DoubleBuffer(ref filter.IN, ref filter.OUT);

				filter.Size            = data.Size;
				filter.Thickness       = thickness;
				filter.Offset          = (int)(data.Size.y * offsetY);
				filter.HeightSource    = HeightSource;
				filter.HeightInfluence = heightInfluence;
				filter.NoiseTiling     = noiseTiling;
				filter.NoiseInfluence  = noiseInfluence;

				data.Handle = filter.Schedule(data.Pixels.Length, 32, data.Handle);
			}
		}

#if UNITY_EDITOR
		protected override void DrawInspector()
		{
			CwEditor.Draw("thickness", "The thickness of the transition region.");
			CwEditor.Draw("offsetX", "The position along the texture the seam will be filled in with.");
			CwEditor.Draw("offsetY", "The position along the texture the seam will be filled in with.");
			CwEditor.Draw("heightSource", "The texture transition will be modified based on height data from this aspect of the pixels.");
			CwEditor.Draw("heightInfluence", "The amount of impact the height data has on the transition.");
			CwEditor.Draw("noiseTiling", "The frequency of the noise data.");
			CwEditor.Draw("noiseInfluence", "The amount of impact the noise data has on the transition.");
		}
#endif
	}
}