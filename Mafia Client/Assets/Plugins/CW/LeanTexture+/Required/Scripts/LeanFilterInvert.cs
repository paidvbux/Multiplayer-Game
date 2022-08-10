using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using CW.Common;

namespace Lean.Texture
{
	/// <summary>This allows you to invert the pixels of the current texture.</summary>
	[System.Serializable]
	public class LeanFilterInvert : LeanFilter
	{
		public override string Title
		{
			get
			{
				return "Invert";
			}
		}

		public enum ChannelType
		{
			AllTheSame,
			Individual
		}

		/// <summary>Should this filter apply to all channels the same, or should each channel be separate?</summary>
		public ChannelType Channel { set { channel = (int)value; } get { return (ChannelType)channel; } } [SerializeField] [LeanEnum(typeof(ChannelType))] private int channel;

		/// <summary>The strength of the inversion.</summary>
		public float Strength { set { strength = value; } get { return strength; } } [SerializeField] [Range(0.0f, 1.0f)] private float strength = 1.0f;

		/// <summary>The strength of the inversion.</summary>
		public float StrengthR { set { strengthR = value; } get { return strengthR; } } [SerializeField] [Range(0.0f, 1.0f)] private float strengthR = 1.0f;

		/// <summary>The strength of the inversion.</summary>
		public float StrengthG { set { strengthG = value; } get { return strengthG; } } [SerializeField] [Range(0.0f, 1.0f)] private float strengthG = 1.0f;

		/// <summary>The strength of the inversion.</summary>
		public float StrengthB { set { strengthB = value; } get { return strengthB; } } [SerializeField] [Range(0.0f, 1.0f)] private float strengthB = 1.0f;

		/// <summary>The strength of the inversion.</summary>
		public float StrengthA { set { strengthA = value; } get { return strengthA; } } [SerializeField] [Range(0.0f, 1.0f)] private float strengthA = 1.0f;

		[BurstCompile]
		struct FilterJob : IJobParallelFor
		{
			public NativeArray<float4> INOUT;

			[ReadOnly] public float4 Strength;

			public void Execute(int index)
			{
				var pixel = INOUT[index];

				pixel = math.lerp(pixel, 1.0f - pixel, Strength);

				INOUT[index] = pixel;
			}
		}

		public override void Schedule(LeanPendingTexture data)
		{
			var filter = new FilterJob();

			filter.INOUT    = data.Pixels;

			if (Channel == ChannelType.Individual)
			{
				filter.Strength.x = strengthR;
				filter.Strength.y = strengthG;
				filter.Strength.z = strengthB;
				filter.Strength.w = strengthA;
			}
			else
			{
				filter.Strength = strength;
			}

			data.Handle = filter.Schedule(data.Pixels.Length, 32, data.Handle);
		}

#if UNITY_EDITOR
		protected override void DrawInspector()
		{
			CwEditor.Draw("channel", "Should this filter apply to all channels the same, or should each channel be separate?");
			if (CwEditor.GetProperty("channel").intValue == (int)ChannelType.Individual)
			{
				CwEditor.Draw("strengthR", "The strength of the inversion.");
				CwEditor.Draw("strengthG", "The strength of the inversion.");
				CwEditor.Draw("strengthB", "The strength of the inversion.");
				CwEditor.Draw("strengthA", "The strength of the inversion.");
			}
			else
			{
				CwEditor.Draw("strength", "The strength of the inversion.");
			}
		}
#endif
	}
}