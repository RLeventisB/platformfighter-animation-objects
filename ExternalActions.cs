using Microsoft.Xna.Framework.Graphics;

using System.Runtime.CompilerServices;

namespace Editor.Objects
{
	public static class ExternalActions
	{
		public static GetCurrentFrameDelegate GetCurrentFrame = () => 0;
		public static OnChangeLinkPropertyDelegate OnChangeLinkPropertyProperty = _ => { };
		public static OnDeleteLinkDelegate OnDeleteLink = _ => { };
		public static AreKeyframesSetOnModifyDelegate AreKeyframesSetOnModify = () => true;
		public static GetGraphicsDeviceDelegate GetGraphicsDevice = () => null;
		public static GetTextureFrameByNameDelegate GetTextureFrameByName = _ => null;
		public static GetTextureAnimationObjectByNameDelegate GetTextureAnimationObjectByName = _ => null;
		public static GetHitboxAnimationObjectByNameDelegate GetHitboxAnimationObjectByName = _ => null;
		public static CalculateQuadPointsDelegate CalculateQuadPoints = delegate(float _, float _, float _, float _, float _, float _, float _, float _, out float tlX, out float tlY, out float trX, out float trY, out float blX, out float blY, out float brX, out float brY)
		{
			tlX = 0;
			tlY = 0;
			trX = 0;
			trY = 0;
			blX = 0;
			blY = 0;
			brX = 0;
			brY = 0;
		};
		public static BindTextureDelegate BindTexture = _ => nint.Zero;
		public static GetTextureDelegate GetTexture = _ => null;
		public static UnbindTextureDelegate UnbindTexture = _ => false;
		public static GetTextureDictionaryForRawUseDelegate GetTextureDictionaryForRawUse = () => ref Unsafe.NullRef<Dictionary<nint, Texture2D>>();
		public static OnRemoveTextureDelegate OnRemoveTexture = _ => { };

		public static GetCameraZoomDelegate GetCameraZoom = () => 1;
		public static IsHitboxModeActiveDelegate IsHitboxModeActive = () => false;

		public delegate void OnChangeLinkPropertyDelegate(KeyframeLink link);
		public delegate void OnDeleteLinkDelegate(KeyframeLink link);
		public delegate void OnRemoveTextureDelegate(TextureFrame frame);

		public delegate GraphicsDevice GetGraphicsDeviceDelegate();
		public delegate TextureFrame GetTextureFrameByNameDelegate(string name);
		public delegate TextureAnimationObject GetTextureAnimationObjectByNameDelegate(string name);
		public delegate HitboxAnimationObject GetHitboxAnimationObjectByNameDelegate(string name);
		public delegate int GetCurrentFrameDelegate();
		public delegate bool AreKeyframesSetOnModifyDelegate();

		public delegate nint BindTextureDelegate(Texture2D texture);
		public delegate Texture2D GetTextureDelegate(nint id);
		public delegate void UnbindTextureDelegate(nint id);
		public delegate Dictionary<nint, Texture2D> GetTextureDictionaryForRawUseDelegate();
		public delegate float GetCameraZoomDelegate();
		public delegate bool IsHitboxModeActiveDelegate();

		public delegate void CalculateQuadPointsDelegate(float centerX, float centerY, float pivotX, float pivotY, float sizeX, float sizeY, float sin, float cos,
			out float tlX, out float tlY,
			out float trX, out float trY,
			out float blX, out float blY,
			out float brX, out float brY
		);
	}
}