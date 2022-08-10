using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using CW.Common;

namespace Lean.Texture
{
	/// <summary>This allows you to create a new texture based on simplex noise.</summary>
	[System.Serializable]
	public class LeanCreateSimplex : LeanCreate
	{
		public override string Title
		{
			get
			{
				return "Simplex Noise";
			}
		}

		/// <summary>The size of the created texture.</summary>
		public int2 Size { set { size = value; } get { return size; } } [SerializeField] private int2 size = new int2(1024, 1024);

		/// <summary>The tiling/detail of the noise.</summary>
		public float Frequency { set { frequency = value; } get { return frequency; } } [SerializeField] private float frequency = 5.0f;

		/// <summary>The amount of noise layers that get mixed together.</summary>
		public int Octaves { set { octaves = value; } get { return octaves; } } [SerializeField] [Range(1, 25)] private int octaves = 1;

		/// <summary>The random seed used when generating the noise.</summary>
		public int Seed { set { seed = value; } get { return seed; } } [SerializeField] private int seed;

		[BurstCompile]
		struct CreateJob : IJobParallelFor
		{
			[WriteOnly] public NativeArray<float4> OUT;

			[ReadOnly] public int2   Size;
			[ReadOnly] public float  Frequency;
			[ReadOnly] public float2 Offset;
			[ReadOnly] public int    Octaves;

			public void Execute(int index)
			{
				var pixel    = default(float4);
				var x        = index % Size.x;
				var y        = index / Size.x;
				var uv       = new float2(x, y) / (Size - 1);
				var total    = 0.0f;
				var strength = 1.0f;
				var scale    = 0.0f;

				uv *= Frequency;
				uv += Offset;

				for (var i = 0; i < Octaves; i++)
				{
					total += (noise.snoise(uv) * 0.5f + 0.5f) * strength;
					scale += strength;

					strength *= 0.5f;
					uv       *= 2.0f;
				}

				pixel.xyz = total / scale;
				pixel.w   = 1.0f;

				OUT[index] = pixel;
			}
		}

		public override LeanPendingTexture TrySchedule(TextureWrapMode wrapU, TextureWrapMode wrapV, bool linear)
		{
			if (size.x > 0 && size.y > 0 && octaves > 0)
			{
				var data   = LeanPendingTexture.Create(size, wrapU, wrapV, linear);
				var create = new CreateJob();
				var angle  = (float)((seed * 991.0) % math.PI_DBL);

				create.OUT       = data.Pixels;
				create.Size      = data.Size;
				create.Frequency = frequency;
				create.Octaves   = octaves;
				create.Offset    = new float2(math.sin(angle), math.cos(angle)) * 10000.0f;

				data.Handle = create.Schedule(data.Pixels.Length, 32);

				return data;
			}

			return null;
		}

#if UNITY_EDITOR
		protected override void DrawInspector()
		{
			CwEditor.Draw("size", "The size of the created texture.");
			CwEditor.Draw("frequency", "The tiling/detail of the noise.");
			CwEditor.Draw("octaves", "The amount of noise layers that get mixed together.");
			CwEditor.Draw("seed", "The random seed used when generating the noise.");
		}
#endif
	}
}