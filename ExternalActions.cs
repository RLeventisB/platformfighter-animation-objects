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
		public static GetGraphicsDeviceDelegate GetGraphicsDevice = () => null;
		public static GetTextureFrameByNameDelegate GetTextureFrameByName = _ => null;
		public static GetTextureAnimationObjectByNameDelegate GetTextureAnimationObjectByName = _ => null;
		public static GetHitboxAnimationObjectByNameDelegate GetHitboxAnimationObjectByName = _ => null;
		public static BindTextureDelegate BindTexture = _ => nint.Zero;
		public static GetTextureDelegate GetTexture = _ => null;
		public static UnbindTextureDelegate UnbindTexture = _ => { };
		public static GetTextureDictionaryForRawUseDelegate GetTextureDictionaryForRawUse = () => null;
		public static OnRemoveTextureDelegate OnRemoveTexture = _ => { };
		
		public delegate void OnChangeLinkPropertyDelegate(KeyframeLink link);
		public delegate void OnDeleteLinkDelegate(KeyframeLink link);
		public delegate void OnRemoveTextureDelegate(TextureFrame frame);

		public delegate GraphicsDevice GetGraphicsDeviceDelegate();
		public delegate TextureFrame GetTextureFrameByNameDelegate(string name);
		public delegate TextureAnimationObject GetTextureAnimationObjectByNameDelegate(string name);
		public delegate HitboxAnimationObject GetHitboxAnimationObjectByNameDelegate(string name);
		public delegate int GetCurrentFrameDelegate();
		public delegate bool AreKeyframesSetOnModifyDelegate();
		public delegate bool AreKeyframesAddedToLinkOnModifyDelegate();

		public delegate nint BindTextureDelegate(Texture2D texture);
		public delegate Texture2D GetTextureDelegate(nint id);
		public delegate void UnbindTextureDelegate(nint id);
		public delegate Dictionary<nint, Texture2D> GetTextureDictionaryForRawUseDelegate();
	}
}