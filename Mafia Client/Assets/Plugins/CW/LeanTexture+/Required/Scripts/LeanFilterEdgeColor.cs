using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using CW.Common;

namespace Lean.Texture
{
	/// <summary>This allows you to color the edges of the texture.</summary>
	[System.Serializable]
	public class LeanFilterEdgeColor : LeanFilter
	{
		public override string Title
		{
			get
			{
				return "Color Edges";
			}
		}

		/// <summary>The texture heights will be calculated based on this aspect of the pixels.</summary>
		public LeanHeight.Source HeightSource { set { heightSource = (int)value; } get { return (LeanHeight.Source)heightSource; } } [SerializeField] [LeanEnum(typeof(LeanHeight.Source))] private int heightSource = (int)LeanHeight.Source.Luminance;

		/// <summary>The color that it will be applied to the edges.</summary>
		public Color Color { set { color = value; } get { return color; } } [SerializeField] private Color color = Color.green;

		/// <summary>You can lower this value to reduce edge noise from smaller differences.</summary>
		public float Threshold { set { threshold = value; } get { return threshold; } } [SerializeField] [Range(1.0f, 5.0f)] private float threshold = 4.0f;

		/// <summary>The strength of the edge effect.</summary>
		public float Strength { set { strength = value; } get { return strength; } } [SerializeField] private float strength = 25.0f;

		[BurstCompile]
		struct FilterJob : IJobParallelFor
		{
			[WriteOnly] public NativeArray<float4> OUT;

			[ReadOnly] public NativeArray<float4> IN;
			[ReadOnly] public int2                Size;
			[ReadOnly] public float3              Color;
			[ReadOnly] public float               Strength;
			[ReadOnly] public float               Threshold;
			[ReadOnly] public bool                RepeatU;
			[ReadOnly] public bool                RepeatV;
			[ReadOnly] public LeanHeight.Source   HeightSource;

			public void Execute(int index)
			{
				var x       = index % Size.x;
				var y       = index / Size.x;
				var pixel   = IN[index];
				var heights = default(float4);

				if (RepeatU == true)
				{
					heights.x = LeanHeight.Calculate(LeanSample.Tex2D_Point_WrapXY(IN, Size, x - 1, y), HeightSource);
					heights.y = LeanHeight.Calculate(LeanSample.Tex2D_Point_WrapXY(IN, Size, x + 1, y), HeightSource);
				}
				else
				{
					heights.x = LeanHeight.Calculate(LeanSample.Tex2D_Point(IN, Size, x - 1, y), HeightSource);
					heights.y = LeanHeight.Calculate(LeanSample.Tex2D_Point(IN, Size, x + 1, y), HeightSource);
				}

				if (RepeatV == true)
				{
					heights.z = LeanHeight.Calculate(LeanSample.Tex2D_Point_WrapXY(IN, Size, x, y - 1), HeightSource);
					heights.w = LeanHeight.Calculate(LeanSample.Tex2D_Point_WrapXY(IN, Size, x, y + 1), HeightSource);
				}
				else
				{
					heights.z = LeanHeight.Calculate(LeanSample.Tex2D_Point(IN, Size, x, y - 1), HeightSource);
					heights.w = LeanHeight.Calculate(LeanSample.Tex2D_Point(IN, Size, x, y + 1), HeightSource);
				}

				var delta = math.abs(heights.x - heights.y) + math.abs(heights.z - heights.w);

				pixel.xyz = math.lerp(pixel.xyz, Color, delta * math.pow(delta, Threshold) * Strength);

				OUT[index] = pixel;
			}
		}

		public override void Schedule(LeanPendingTexture data)
		{
			var filter = new FilterJob();

			data.DoubleBuffer(ref filter.IN, ref filter.OUT);

			filter.Size         = data.Size;
			filter.HeightSource = HeightSource;
			filter.RepeatU      = data.WrapU == TextureWrapMode.Repeat;
			filter.RepeatV      = data.WrapV == TextureWrapMode.Repeat;
			filter.Color        = (Vector3)(Vector4)color;
			filter.Strength     = strength;
			filter.Threshold    = 5.0f - threshold;

			data.Handle = filter.Schedule(data.Pixels.Length, 32, data.Handle);
		}

#if UNITY_EDITOR
		protected override void DrawInspector()
		{
			CwEditor.Draw("heightSource", "The texture heights will be calculated based on this aspect of the pixels.");
			CwEditor.Draw("color", "The color that it will be applied to the edges.");
			CwEditor.Draw("threshold", "You can lower this value to reduce edge noise from smaller differences.");
			CwEditor.Draw("strength", "The strength of the edge effect.");
		}
#endif
	}
}