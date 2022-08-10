using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using CW.Common;

namespace Lean.Texture
{
	/// <summary>This allows you to modify the contrast of the texture.</summary>
	[System.Serializable]
	public class LeanFilterContrast : LeanFilter
	{
		public override string Title
		{
			get
			{
				return "Modify Contrast";
			}
		}

		/// <summary>The contrast of the texture will be multiplied by this value.</summary>
		public float Multiplier { set { multiplier = value; } get { return multiplier; } } [SerializeField] [Range(0.0f, 10.0f)] private float multiplier = 1.0f;

		[BurstCompile]
		struct FilterJob : IJobParallelFor
		{
			public NativeArray<float4> INOUT;

			[ReadOnly] public float4x4 Matrix;

			public void Execute(int index)
			{
				var pixel = INOUT[index];

				pixel = math.mul(Matrix, pixel);

				INOUT[index] = pixel;
			}
		}

		public override void Schedule(LeanPendingTexture data)
		{
			var filter = new FilterJob();
			var m      = multiplier;
			var t      = ( 1.0f - multiplier ) / 2.0f;

			filter.INOUT  = data.Pixels;
			filter.Matrix = new float4x4( m, 0, 0, t, 0, m, 0, t, 0, 0, m, t, 0, 0, 0, 1 );

			data.Handle = filter.Schedule(data.Pixels.Length, 32, data.Handle);
		}

#if UNITY_EDITOR
		protected override void DrawInspector()
		{
			CwEditor.Draw("multiplier", "The contrast of the texture will be multiplied by this value.");
		}
#endif
	}
}