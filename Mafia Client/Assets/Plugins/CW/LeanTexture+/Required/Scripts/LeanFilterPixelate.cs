using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using CW.Common;

namespace Lean.Texture
{
	/// <summary>This allows you to pixelate the texture by the specified amount.</summary>
	[System.Serializable]
	public class LeanFilterPixelate : LeanFilter
	{
		public override string Title
		{
			get
			{
				return "Pixelate";
			}
		}

		/// <summary>The scale of the pixelation.</summary>
		public float Scale { set { scale = value; } get { return scale; } } [SerializeField] [Range(0.0f, 1.0f)] private float scale = 1.0f;

		[BurstCompile]
		struct FilterJob : IJobParallelFor
		{
			[WriteOnly] public NativeArray<float4> OUT;

			[ReadOnly] public NativeArray<float4> IN;
			[ReadOnly] public int2                Size;
			[ReadOnly] public int2                ScaledSize;

			public void Execute(int index)
			{
				var x  = index % Size.x;
				var y  = index / Size.x;
				var uv = new float2(x, y) / (Size - 1);

				uv = (math.floor(uv * ScaledSize)) / ScaledSize;

				OUT[index] = LeanSample.Tex2D_Point(IN, Size, uv);
			}
		}

		public override void Schedule(LeanPendingTexture data)
		{
			var filter = new FilterJob();

			data.DoubleBuffer(ref filter.IN, ref filter.OUT);

			filter.Size       = data.Size;
			filter.ScaledSize = (int2)((float2)(data.Size - 1) * scale);

			data.Handle = filter.Schedule(data.Pixels.Length, 32, data.Handle);
		}

#if UNITY_EDITOR
		protected override void DrawInspector()
		{
			CwEditor.Draw("scale", "The scale of the pixelation.");
		}
#endif
	}
}