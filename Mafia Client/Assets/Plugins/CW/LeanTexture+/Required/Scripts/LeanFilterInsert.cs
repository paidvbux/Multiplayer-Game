using UnityEngine;
using CW.Common;

namespace Lean.Texture
{
	/// <summary>This allows you to insert all filters on the specified texture into this one, as if this filter .</summary>
	[System.Serializable]
	public class LeanFilterInsert : LeanFilter
	{
		public override string Title
		{
			get
			{
				return "Copy Filters From";
			}
		}

		/// <summary>The texture whose filters will be copied.</summary>
		public LeanTextureContainer Texture { set { texture = value; } get { return texture; } } [SerializeField] private LeanTextureContainer texture;

		public override void Schedule(LeanPendingTexture data)
		{
			if (texture != null)
			{
				foreach (var filter in texture.Data.Filters)
				{
					if (filter != null)
					{
						if (filter is LeanFilterInsert)
						{
							continue;
						}

						filter.Schedule(data);
					}
				}
			}
		}

#if UNITY_EDITOR
		protected override void DrawInspector()
		{
			CwEditor.BeginError(ObjectExists("texture") == false);
				CwEditor.Draw("texture", "The texture whose filters will be copied.");
			CwEditor.EndError();
		}
#endif
	}
}