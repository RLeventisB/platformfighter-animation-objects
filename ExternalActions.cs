using Microsoft.Xna.Framework.Graphics;

namespace Editor.Objects
{
	public static class ExternalActions
	{
		public static GetCurrentFrameDelegate GetCurrentFrame = () => 0;
		public static OnChangeLinkPropertyDelegate OnChangeLinkPropertyProperty = _ => { };
		public static OnDeleteLinkDelegate OnDeleteLink = _ => { };
		public static AreKeyframesSetOnModifyDelegate AreKeyframesSetOnModify = () => true;
		public static AreKeyframesAddedToLinkOnModifyDelegate AreKeyframesAddedToLinkOnModify = () => true;
		public static GetTextureFrameByNameDelegate GetTextureFrameByName = _ => null;
		
		public delegate void OnChangeLinkPropertyDelegate(KeyframeLink link);
		public delegate void OnDeleteLinkDelegate(KeyframeLink link);

		public delegate TextureFrame GetTextureFrameByNameDelegate(string name);
		public delegate int GetCurrentFrameDelegate();
		public delegate bool AreKeyframesSetOnModifyDelegate();
		public delegate bool AreKeyframesAddedToLinkOnModifyDelegate();
	}
}