using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using CW.Common;

namespace Lean.Texture
{
	/// <summary>This allows you to resize the current the texture.</summary>
	[System.Serializable]
	public class LeanFilterResize : LeanFilter
	{
		public override string Title
		{
			get
			{
				return "Resize";
			}
		}

		/// <summary>The new size of the texture.
		/// -1 = No change.</summary>
		public int2 Size { set { size = value; } get { return size; } } [SerializeField] private int2 size = new int2(-1, -1);

		[BurstCompile]
		struct FilterJob : IJobParallelFor
		{
			[WriteOnly] public NativeArray<float4> Out;
			[ ReadOnly] public int2                OutSize;
			[ ReadOnly] public NativeArray<float4> In;
			[ ReadOnly] public int2                InSize;

			public void Execute(int index)
			{
				var x  = index % OutSize.x;
				var y  = index / OutSize.x;
				var uv = new float2(x, y) / (OutSize - 1);

				Out[index] = LeanSample.Tex2D_Linear(In, InSize, uv);
			}
		}

		public override void Schedule(LeanPendingTexture data)
		{
			var filter = new FilterJob();

			data.DoubleBufferResize(ref filter.In, ref filter.Out, ref filter.InSize, ref filter.OutSize, size);

			data.Handle = filter.Schedule(data.Pixels.Length, 32, data.Handle);
		}

#if UNITY_EDITOR
		protected override void DrawInspector()
		{
			CwEditor.Draw("size", "The new size of the texture.\n\n-1 = No change.");
		}
#endif
	}
}