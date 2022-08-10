using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Burst;
using CW.Common;

namespace Lean.Texture
{
	/// <summary>This allows you to replace the specified color in the texture with another color.</summary>
	[System.Serializable]
	public class LeanFilterReplace : LeanFilter
	{
		public override string Title
		{
			get
			{
				return "Replace Color";
			}
		}

		public enum DistanceType
		{
			MaxChannel,
			Average
		}

		/// <summary>The color that will be replaced.</summary>
		public Color OldColor { set { oldColor = value; } get { return oldColor; } } [SerializeField]private Color oldColor = Color.red;

		/// <summary>The color that it will be replaced with.</summary>
		public Color NewColor { set { newColor = value; } get { return newColor; } } [SerializeField] private Color newColor = Color.green;

		/// <summary>The calculation to find the difference between the current pixel color and the <b>OldColor</b>.
		/// MaxChannel = The maximum difference between any of the color channels.
		/// Average = The average of all channel differences.</summary>
		public DistanceType Distance { set { distance = value; } get { return distance; } } [SerializeField] private DistanceType distance;

		/// <summary>The <b>OldColor</b> can be off by this much.</summary>
		public float Threshold { set { threshold = value; } get { return threshold; } } [SerializeField] [Range(0.0f, 1.0f)] private float threshold = 0.01f;

		/// <summary>How much should colors outside the <b>Threshold</b> be replaced?</summary>
		public float Edge { set { edge = value; } get { return edge; } } [SerializeField] [Range(0.0f, 1.0f)] private float edge = 0.1f;

		/// <summary>The overall color replacement strength.</summary>
		public float Opacity { set { opacity = value; } get { return opacity; } } [SerializeField] [Range(0.0f, 1.0f)] private float opacity = 1.0f;

		[BurstCompile]
		struct FilterJob : IJobParallelFor
		{
			public NativeArray<float4> INOUT;

			[ReadOnly] public float3       OldColor;
			[ReadOnly] public float3       NewColor;
			[ReadOnly] public DistanceType Distance;
			[ReadOnly] public float        Threshold;
			[ReadOnly] public float        Edge;
			[ReadOnly] public float        Opacity;

			public float GetDistance(float4 pixel)
			{
				var delta = math.abs(pixel.xyz - OldColor);

				if (Distance == DistanceType.MaxChannel)
				{
					return math.max(delta.x, math.max(delta.y, delta.z));
				}
				else// if (Distance == DistanceType.Average)
				{
					return (delta.x + delta.y + delta.z) / 3.0f;
				}
			}

			public void Execute(int index)
			{
				var pixel  = INOUT[index];
				var dist   = GetDistance(pixel);
				var weight = 1.0f - math.saturate((dist - Threshold) / Edge);

				pixel.xyz = math.lerp(pixel.xyz, NewColor, weight * Opacity);

				INOUT[index] = pixel;
			}
		}

		public override void Schedule(LeanPendingTexture data)
		{
			var filter = new FilterJob();

			filter.INOUT     = data.Pixels;
			filter.OldColor  = (Vector3)(Vector4)oldColor;
			filter.NewColor  = (Vector3)(Vector4)newColor;
			filter.Threshold = threshold;
			filter.Edge      = math.max(0.0001f, edge);
			filter.Distance  = distance;
			filter.Opacity   = opacity;

			data.Handle = filter.Schedule(data.Pixels.Length, 32, data.Handle);
		}

#if UNITY_EDITOR
		protected override void DrawInspector()
		{
			CwEditor.Draw("oldColor", "The color that will be replaced.");
			CwEditor.Draw("newColor", "The color that it will be replaced with.");
			CwEditor.Draw("distance", "The calculation to find the difference between the current pixel color and the <b>OldColor</b>.\n\nMaxChannel = The maximum difference between any of the color channels.\n\nAverage = The average of all channel differences.");
			CwEditor.Draw("threshold", "The <b>OldColor</b> can be off by this much.");
			CwEditor.Draw("edge", "How much should colors outside the <b>Threshold</b> be replaced?");
			CwEditor.Draw("opacity", "The overall color replacement strength.");
		}
#endif
	}
}