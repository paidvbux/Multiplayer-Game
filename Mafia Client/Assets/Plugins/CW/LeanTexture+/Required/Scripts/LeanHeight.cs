using Unity.Mathematics;

namespace Lean.Texture
{
	/// <summary>This class can calculate the height of a texture based on the specified technique.</summary>
	public static class LeanHeight
	{
		public enum Source
		{
			None,
			Luminance,
			Red,
			Green,
			Blue,
			Alpha
		}

		public static float Calculate(float4 color, Source heightSource)
		{
			switch (heightSource)
			{
				case Source.Luminance: return 0.2126f * color.x + 0.7152f * color.y + 0.0722f * color.z;
				case Source.Red: return color.x;
				case Source.Green: return color.y;
				case Source.Blue: return color.z;
				case Source.Alpha: return color.w;
			}

			return 0.0f;
		}
	}
}