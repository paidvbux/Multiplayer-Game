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
	public class LeanFilterQuantize : LeanFilter
	{
		public override string Title
		{
			get
			{
				return "Quantize";
			}
		}

		public enum ChannelType
		{
			AllTheSame,
			Individual
		}

		/// <summary>How should the texture channels should be modified?</summary>
		public ChannelType Channels { set { channels = (int)value; } get { return (ChannelType)channels; } } [SerializeField] [LeanEnum(typeof(ChannelType))] private int channels;

		/// <summary>The amount of possible values each channel can have.</summary>
		public int Values { set { values = value; } get { return values; } } [SerializeField] [Range(2, 256)] private int values = 256;

		/// <summary>The amount of possible values this channel can have.</summary>
		public int ValuesR { set { valuesR = value; } get { return valuesR; } } [SerializeField] [Range(2, 256)] private int valuesR = 256;

		/// <summary>The amount of possible values this channel can have.</summary>
		public int ValuesG { set { valuesG = value; } get { return valuesG; } } [SerializeField] [Range(2, 256)] private int valuesG = 256;

		/// <summary>The amount of possible values this channel can have.</summary>
		public int ValuesB { set { valuesB = value; } get { return valuesB; } } [SerializeField] [Range(2, 256)] private int valuesB = 256;

		/// <summary>The amount of possible values this channel can have.</summary>
		public int ValuesA { set { valuesA = value; } get { return valuesA; } } [SerializeField] [Range(2, 256)] private int valuesA = 256;

		[BurstCompile]
		struct FilterJob : IJobParallelFor
		{
			public NativeArray<float4> INOUT;

			[ReadOnly] public int4 Values;

			public void Execute(int index)
			{
				var pixel = INOUT[index];

				pixel *= Values;

				pixel = math.round(pixel);

				pixel /= Values;

				INOUT[index] = pixel;
			}
		}

		public override void Schedule(LeanPendingTexture data)
		{
			var filter = new FilterJob();

			filter.INOUT = data.Pixels;

			if (Channels == ChannelType.Individual)
			{
				filter.Values.x = valuesR;
				filter.Values.y = valuesG;
				filter.Values.z = valuesB;
				filter.Values.w = valuesA;
			}
			else
			{
				filter.Values = values;
			}

			filter.Values -= 1;

			data.Handle = filter.Schedule(data.Pixels.Length, 32, data.Handle);
		}

#if UNITY_EDITOR
		protected override void DrawInspector()
		{
			CwEditor.Draw("channels", "How should the texture channels should be modified?");
			if (CwEditor.GetProperty("channels").intValue == (int)ChannelType.Individual)
			{
				CwEditor.Draw("valuesR", "The amount of possible values this channel can have.");
				CwEditor.Draw("valuesG", "The amount of possible values this channel can have.");
				CwEditor.Draw("valuesB", "The amount of possible values this channel can have.");
				CwEditor.Draw("valuesA", "The amount of possible values this channel can have.");
			}
			else
			{
				CwEditor.Draw("values", "The amount of possible values each channel can have.");
			}
		}
#endif
	}
}