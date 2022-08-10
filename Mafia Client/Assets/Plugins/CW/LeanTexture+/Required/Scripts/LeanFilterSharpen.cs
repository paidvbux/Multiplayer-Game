using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using CW.Common;

namespace Lean.Texture
{
	/// <summary>This allows you to modify the sharpness of the texture.</summary>
	[System.Serializable]
	public class LeanFilterSharpen : LeanFilter
	{
		public override string Title
		{
			get
			{
				return "Modify Sharpness";
			}
		}

		public enum ChannelType
		{
			AllTheSame,
			Individual
		}

		/// <summary>How should the texture channels should be modified?</summary>
		public ChannelType Channels { set { channels = (int)value; } get { return (ChannelType)channels; } } [SerializeField] [LeanEnum(typeof(ChannelType))] private int channels;

		/// <summary>The sharpness of the texture will be multiplied by this value.</summary>
		public float Multiplier { set { multiplier = value; } get { return multiplier; } } [SerializeField] [Range(0.0f, 10.0f)] private float multiplier = 1.0f;

		/// <summary>The sharpness of the texture will be multiplied by this value.</summary>
		public float MultiplierR { set { multiplierR = value; } get { return multiplierR; } } [SerializeField] [Range(0.0f, 10.0f)] private float multiplierR = 1.0f;

		/// <summary>The sharpness of the texture will be multiplied by this value.</summary>
		public float MultiplierG { set { multiplierG = value; } get { return multiplierG; } } [SerializeField] [Range(0.0f, 10.0f)] private float multiplierG = 1.0f;

		/// <summary>The sharpness of the texture will be multiplied by this value.</summary>
		public float MultiplierB { set { multiplierB = value; } get { return multiplierB; } } [SerializeField] [Range(0.0f, 10.0f)] private float multiplierB = 1.0f;

		/// <summary>The sharpness of the texture will be multiplied by this value.</summary>
		public float MultiplierA { set { multiplierA = value; } get { return multiplierA; } } [SerializeField] [Range(0.0f, 10.0f)] private float multiplierA = 1.0f;

		[BurstCompile]
		struct FilterJob : IJobParallelFor
		{
			[WriteOnly] public NativeArray<float4> OUT;

			[ReadOnly] public NativeArray<float4> IN;
			[ReadOnly] public int2                Size;
			[ReadOnly] public float4              Multiplier;
			[ReadOnly] public bool                RepeatU;
			[ReadOnly] public bool                RepeatV;

			public void Execute(int index)
			{
				var pixel = IN[index];
				var sides = default(float4);
				var x     = index % Size.x;
				var y     = index / Size.x;

				if (RepeatU == true)
				{
					sides += LeanSample.Tex2D_Point_WrapXY(IN, Size, x - 1, y);
					sides += LeanSample.Tex2D_Point_WrapXY(IN, Size, x + 1, y);
				}
				else
				{
					sides += LeanSample.Tex2D_Point(IN, Size, x - 1, y);
					sides += LeanSample.Tex2D_Point(IN, Size, x + 1, y);
				}

				if (RepeatV == true)
				{
					sides += LeanSample.Tex2D_Point_WrapXY(IN, Size, x, y - 1);
					sides += LeanSample.Tex2D_Point_WrapXY(IN, Size, x, y + 1);
				}
				else
				{
					sides += LeanSample.Tex2D_Point(IN, Size, x, y - 1);
					sides += LeanSample.Tex2D_Point(IN, Size, x, y + 1);
				}

				sides *= 0.25f;

				pixel += (pixel - sides) * Multiplier;

				OUT[index] = pixel;
			}
		}

		public override void Schedule(LeanPendingTexture data)
		{
			var filter = new FilterJob();

			data.DoubleBuffer(ref filter.IN, ref filter.OUT);

			filter.Size    = data.Size;
			filter.RepeatU = data.WrapU == TextureWrapMode.Repeat;
			filter.RepeatV = data.WrapV == TextureWrapMode.Repeat;

			if (Channels == ChannelType.Individual)
			{
				filter.Multiplier.x = multiplierR;
				filter.Multiplier.y = multiplierG;
				filter.Multiplier.z = multiplierB;
				filter.Multiplier.w = multiplierA;
			}
			else
			{
				filter.Multiplier = multiplier;
			}

			data.Handle = filter.Schedule(data.Pixels.Length, 32, data.Handle);
		}

#if UNITY_EDITOR
		protected override void DrawInspector()
		{
			CwEditor.Draw("channels", "How should the texture channels should be modified?");
			if (CwEditor.GetProperty("channels").intValue == (int)ChannelType.Individual)
			{
				CwEditor.Draw("multiplierR", "The sharpness of the texture will be multiplied by this value.");
				CwEditor.Draw("multiplierG", "The sharpness of the texture will be multiplied by this value.");
				CwEditor.Draw("multiplierB", "The sharpness of the texture will be multiplied by this value.");
				CwEditor.Draw("multiplierA", "The sharpness of the texture will be multiplied by this value.");
			}
			else
			{
				CwEditor.Draw("multiplier", "The sharpness of the texture will be multiplied by this value.");
			}
		}
#endif
	}
}