using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using CW.Common;

namespace Lean.Texture
{
	/// <summary>This allows you to mirror the texture coordinates.</summary>
	[System.Serializable]
	public class LeanFilterMirror : LeanFilter
	{
		public override string Title
		{
			get
			{
				return "Mirror";
			}
		}

		public enum ChannelType
		{
			AllTheSame,
			Individual
		}

		/// <summary>How should the mirroring be applied to the texture channels?</summary>
		public ChannelType Channels { set { channels = (int)value; } get { return (ChannelType)channels; } } [SerializeField] [LeanEnum(typeof(ChannelType))] private int channels;

		/// <summary>The horizontal and vertices axes that will be mirrored.</summary>
		public bool2 MirrorAxes { set { mirrorAxes = value; } get { return mirrorAxes; } } [SerializeField] private bool2 mirrorAxes;

		/// <summary>The horizontal and vertices axes that will be mirrored.</summary>
		public bool2 MirrorAxesR { set { mirrorAxesR = value; } get { return mirrorAxesR; } } [SerializeField] private bool2 mirrorAxesR;

		/// <summary>The horizontal and vertices axes that will be mirrored.</summary>
		public bool2 MirrorAxesG { set { mirrorAxesG = value; } get { return mirrorAxesG; } } [SerializeField] private bool2 mirrorAxesG;

		/// <summary>The horizontal and vertices axes that will be mirrored.</summary>
		public bool2 MirrorAxesB { set { mirrorAxesB = value; } get { return mirrorAxesB; } } [SerializeField] private bool2 mirrorAxesB;

		/// <summary>The horizontal and vertices axes that will be mirrored.</summary>
		public bool2 MirrorAxesA { set { mirrorAxesA = value; } get { return mirrorAxesA; } } [SerializeField] private bool2 mirrorAxesA;

		[BurstCompile]
		struct FilterJob : IJobParallelFor
		{
			[WriteOnly] public NativeArray<float4> OUT;

			[ReadOnly] public NativeArray<float4> IN;
			[ReadOnly] public int2                Size;
			[ReadOnly] public float4              WeightR;
			[ReadOnly] public float4              WeightG;
			[ReadOnly] public float4              WeightB;
			[ReadOnly] public float4              WeightA;

			public void Execute(int index)
			{
				var x       = index % Size.x;
				var y       = index / Size.x;
				var z       = Size.x - x - 1;
				var w       = Size.y - y - 1;
				var pixelBL = IN[index];
				var pixelBR = LeanSample.Tex2D_Point(IN, Size, z, y);
				var pixelTL = LeanSample.Tex2D_Point(IN, Size, x, w);
				var pixelTR = LeanSample.Tex2D_Point(IN, Size, z, w);
				var pixel   = default(float4);

				pixel.x = math.dot(WeightR, new float4(pixelBL.x, pixelBR.x, pixelTL.x, pixelTR.x));
				pixel.y = math.dot(WeightG, new float4(pixelBL.y, pixelBR.y, pixelTL.y, pixelTR.y));
				pixel.z = math.dot(WeightB, new float4(pixelBL.z, pixelBR.z, pixelTL.z, pixelTR.z));
				pixel.w = math.dot(WeightA, new float4(pixelBL.w, pixelBR.w, pixelTL.w, pixelTR.w));

				OUT[index] = pixel;
			}
		}

		public override void Schedule(LeanPendingTexture data)
		{
			var filter = new FilterJob();

			data.DoubleBuffer(ref filter.IN, ref filter.OUT);

			filter.Size = data.Size;

			if (Channels == ChannelType.Individual)
			{
				filter.WeightR = GetWeights(mirrorAxesR);
				filter.WeightG = GetWeights(mirrorAxesG);
				filter.WeightB = GetWeights(mirrorAxesB);
				filter.WeightA = GetWeights(mirrorAxesA);
			}
			else
			{
				filter.WeightR = filter.WeightG = filter.WeightB = filter.WeightA = GetWeights(mirrorAxes);
			}

			data.Handle = filter.Schedule(data.Pixels.Length, 32, data.Handle);
		}

		private static float4 GetWeights(bool2 mirrorAxes)
		{
			if (mirrorAxes.x == true)
			{
				if (mirrorAxes.y == true)
				{
					return new float4(0, 0, 0, 1);
				}
				else
				{
					return new float4(0, 1, 0, 0);
				}
			}
			else
			{
				if (mirrorAxes.y == true)
				{
					return new float4(0, 0, 1, 0);
				}
				else
				{
					return new float4(1, 0, 0, 0);
				}
			}
		}

#if UNITY_EDITOR
		protected override void DrawInspector()
		{
			CwEditor.Draw("channels", "How should the mirroring be applied to the texture channels?");
			if (CwEditor.GetProperty("channels").intValue == (int)ChannelType.Individual)
			{
				CwEditor.Draw("mirrorAxesR", "The horizontal and vertices axes that will be mirrored.");
				CwEditor.Draw("mirrorAxesG", "The horizontal and vertices axes that will be mirrored.");
				CwEditor.Draw("mirrorAxesB", "The horizontal and vertices axes that will be mirrored.");
				CwEditor.Draw("mirrorAxesA", "The horizontal and vertices axes that will be mirrored.");
			}
			else
			{
				CwEditor.Draw("mirrorAxes", "The horizontal and vertices axes that will be mirrored.");
			}
		}
#endif
	}
}