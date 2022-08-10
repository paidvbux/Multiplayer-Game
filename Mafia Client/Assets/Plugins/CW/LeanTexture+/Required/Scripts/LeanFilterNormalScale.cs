using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using CW.Common;

namespace Lean.Texture
{
	/// <summary>This allows you to modify the scale/strength of the texture if it's a normal map.</summary>
	[System.Serializable]
	public class LeanFilterNormalScale : LeanFilter
	{
		public override string Title
		{
			get
			{
				return "Modify Normal Strength";
			}
		}

		/// <summary>The normal strength will be multiplied by this.</summary>
		public float Multiplier { set { multiplier = value; } get { return multiplier; } } [SerializeField] [Range(0.0f, 10.0f)] private float multiplier = 1.0f;

		[BurstCompile]
		struct FilterJob : IJobParallelFor
		{
			public NativeArray<float4> INOUT;

			[ReadOnly] public float Multiplier;

			public void Execute(int index)
			{
				var pixel  = INOUT[index];
				var normal = pixel.xyz * 2.0f - 1.0f;

				normal.xy *= Multiplier;

				pixel.xyz = math.normalize(normal) * 0.5f + 0.5f;

				INOUT[index] = pixel;
			}
		}

		public override void Schedule(LeanPendingTexture data)
		{
			var filter = new FilterJob();

			filter.INOUT      = data.Pixels;
			filter.Multiplier = multiplier;

			data.Handle = filter.Schedule(data.Pixels.Length, 32, data.Handle);
		}

#if UNITY_EDITOR
		protected override void DrawInspector()
		{
			CwEditor.Draw("multiplier", "The normal strength will be multiplied by this.");
		}
#endif
	}
}