using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Editor.Objects
{
	[DebuggerDisplay("{Name}")]
	public class TextureFrame : IAnimationObject, IDisposable
	{
		public string Path { get; set; }
		public string Name { get; set; }

		public NVector2 Pivot { get; set; }
		public Point FrameSize { get; set; }
		public Point FramePosition { get; set; }

		[JsonConstructor]
		public TextureFrame()
		{
			Name = null;
			Path = null;
			FrameSize = Point.Zero;
			FramePosition = Point.Zero;
			Pivot = NVector2.One;
		}

		public TextureFrame(string name, string path, Point? frameSize = null, Point? framePosition = null, NVector2? pivot = null)
		{
			Name = name;
			Path = path;
			FrameSize = frameSize ?? new Point(1);
			FramePosition = framePosition ?? Point.Zero;
			Pivot = pivot ?? new NVector2(1 / 2f);
			TextureId = ExternalActions.BindTexturePath(path);
		}

		public TextureFrame(string name, Texture2D texture, Point frameSize, Point? framePosition = null, NVector2? pivot = null)
		{
			Name = name;
			Path = "Syntetic";
			FrameSize = frameSize;
			FramePosition = framePosition ?? Point.Zero;
			Pivot = pivot ?? NVector2.Zero;
			TextureId = ExternalActions.BindTexture(texture);
		}

		[JsonIgnore]
		public nint? TextureId { get; private set; }
		
		public bool IsBeingHovered(Vector2 mouseWorld, int? frame) => false;

		public List<KeyframeableValue> EnumerateKeyframeableValues() => [];

		public void Dispose()
		{
			Dispose(true);
		}
		public void Dispose(bool unbind)
		{
			if (TextureId.HasValue && unbind)
			{
				ExternalActions.UnbindTexture(TextureId.Value);
				GC.SuppressFinalize(this);
			}
		}
	}
}