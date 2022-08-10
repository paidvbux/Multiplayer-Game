using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using CW.Common;

namespace Lean.Texture
{
	/// <summary>This allows you to swap the channels in the current texture.</summary>
	[System.Serializable]
	public class LeanFilterSwizzle : LeanFilter
	{
		public override string Title
		{
			get
			{
				return "Swap Channels";
			}
		}

		public enum ChannelType
		{
			Red,
			Green,
			Blue,
			Alpha
		}

		/// <summary>The channel of the current texture that will end up in the red channel of the output texture.</summary>
		public ChannelType RedChannel { set { redChannel = (int)value; } get { return (ChannelType)redChannel; } } [SerializeField] [LeanEnum(typeof(ChannelType))] private int redChannel = (int)ChannelType.Red;

		/// <summary>The channel of the current texture that will end up in the green channel of the output texture.</summary>
		public ChannelType GreenChannel { set { greenChannel = (int)value; } get { return (ChannelType)greenChannel; } } [SerializeField] [LeanEnum(typeof(ChannelType))] private int greenChannel = (int)ChannelType.Green;

		/// <summary>The channel of the current texture that will end up in the blue channel of the output texture.</summary>
		public ChannelType BlueChannel { set { blueChannel = (int)value; } get { return (ChannelType)blueChannel; } } [SerializeField] [LeanEnum(typeof(ChannelType))] private int blueChannel = (int)ChannelType.Blue;

		/// <summary>The channel of the current texture that will end up in the alpha channel of the output texture.</summary>
		public ChannelType AlphaChannel { set { alphaChannel = (int)value; } get { return (ChannelType)alphaChannel; } } [SerializeField] [LeanEnum(typeof(ChannelType))] private int alphaChannel = (int)ChannelType.Alpha;

		[BurstCompile]
		struct FilterJob : IJobParallelFor
		{
			public NativeArray<float4> INOUT;

			[ReadOnly] public float4 WeightR;
			[ReadOnly] public float4 WeightG;
			[ReadOnly] public float4 WeightB;
			[ReadOnly] public float4 WeightA;

			public void Execute(int index)
			{
				var pixelI = INOUT[index];
				var pixelO = default(float4);

				pixelO.x = math.dot(pixelI, WeightR);
				pixelO.y = math.dot(pixelI, WeightG);
				pixelO.z = math.dot(pixelI, WeightB);
				pixelO.w = math.dot(pixelI, WeightA);

				INOUT[index] = pixelO;
			}
		}

		public override void Schedule(LeanPendingTexture data)
		{
			var filter = new FilterJob();

			filter.INOUT   = data.Pixels;
			filter.WeightR = GetWeight(  RedChannel);
			filter.WeightG = GetWeight(GreenChannel);
			filter.WeightB = GetWeight( BlueChannel);
			filter.WeightA = GetWeight(AlphaChannel);

			data.Handle = filter.Schedule(data.Pixels.Length, 32, data.Handle);
		}

		private static float4 GetWeight(ChannelType channel)
		{
			var weight = float4.zero;

			switch (channel)
			{
				case ChannelType.Red:   weight.x = 1.0f; break;
				case ChannelType.Green: weight.y = 1.0f; break;
				case ChannelType.Blue:  weight.z = 1.0f; break;
				case ChannelType.Alpha: weight.w = 1.0f; break;
			}

			return weight;
		}

#if UNITY_EDITOR
		protected override void DrawInspector()
		{
			CwEditor.Draw("redChannel", "The channel of the current texture that will end up in the red channel of the output texture.", "Red =");
			CwEditor.Draw("greenChannel", "The channel of the current texture that will end up in the green channel of the output texture.", "Green =");
			CwEditor.Draw("blueChannel", "The channel of the current texture that will end up in the blue channel of the output texture.", "Blue =");
			CwEditor.Draw("alphaChannel", "The channel of the current texture that will end up in the alpha channel of the output texture.", "Alpha =");
		}
#endif
	}
}