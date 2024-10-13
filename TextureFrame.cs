using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Editor.Objects
{
	[DebuggerDisplay("{Name}")]
	public class TextureFrame : IAnimationObject
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
		}

		public TextureFrame(string name, Texture2D texture, Point frameSize, Point? framePosition = null, NVector2? pivot = null)
		{
			Name = name;
			Path = "Syntetic";
			FrameSize = frameSize;
			FramePosition = framePosition ?? Point.Zero;
			Pivot = pivot ?? NVector2.Zero;
		}

		[JsonIgnore]
		private nint? _textureId;
		[JsonIgnore]
		public nint TextureId
		{
			get => _textureId.Value;
			set => _textureId = value;
		}
		public bool IsBinded => _textureId.HasValue;
		
		public bool IsBeingHovered(Vector2 mouseWorld, int? frame) => false;

		public List<KeyframeableValue> EnumerateKeyframeableValues() => [];
	}
}