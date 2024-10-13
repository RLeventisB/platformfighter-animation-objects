using Microsoft.Xna.Framework;

using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Editor.Objects
{
	[DebuggerDisplay("{Name}")]
	public class TextureAnimationObject : IAnimationObject
	{
		[JsonConstructor]
		public TextureAnimationObject()
		{
			Name = null;
			TextureName = null;

			Scale = new Vector2KeyframeValue(this, Vector2.One, PropertyNames.ScaleProperty, false);
			FrameIndex = new IntKeyframeValue(this, 0, PropertyNames.FrameIndexProperty, false);
			Rotation = new FloatKeyframeValue(this, 0f, PropertyNames.RotationProperty, false);
			Position = new Vector2KeyframeValue(this, Vector2.Zero, PropertyNames.PositionProperty, false);
			Transparency = new FloatKeyframeValue(this, 1f, PropertyNames.TransparencyProperty, false);
			ZIndex = new FloatKeyframeValue(this, 0f, PropertyNames.ZIndexProperty, false);
		}

		public TextureAnimationObject(string name, string textureName)
		{
			Name = name;
			TextureName = textureName;

			Scale = new Vector2KeyframeValue(this, Vector2.One, PropertyNames.ScaleProperty);
			FrameIndex = new IntKeyframeValue(this, 0, PropertyNames.FrameIndexProperty);
			Rotation = new FloatKeyframeValue(this, 0f, PropertyNames.RotationProperty);
			Position = new Vector2KeyframeValue(this, Vector2.Zero, PropertyNames.PositionProperty);
			Transparency = new FloatKeyframeValue(this, 1f, PropertyNames.TransparencyProperty);
			ZIndex = new FloatKeyframeValue(this, 0f, PropertyNames.ZIndexProperty);
		}

		public string TextureName { get; set; }
		public Vector2KeyframeValue Scale { get; set; }
		public IntKeyframeValue FrameIndex { get; set; }
		public FloatKeyframeValue Rotation { get; set; }
		public string Name { get; set; }
		public Vector2KeyframeValue Position { get; set; }
		public FloatKeyframeValue Transparency { get; set; }
		public FloatKeyframeValue ZIndex { get; set; }

		public bool IsBeingHovered(Vector2 mouseWorld, int? frame)
		{
			frame ??= ExternalActions.GetCurrentFrame();
			TextureFrame texture = ExternalActions.GetTextureFrameByName(TextureName);
			Vector2 scale = Scale.Interpolate(frame.Value);
			Vector2 size = texture.FrameSize.ToVector2() * scale.Abs();

			float rotation = Rotation.Interpolate(frame.Value);

			return MiscellaneousFunctions.IsPointInsideRotatedRectangle(Position.Interpolate(frame.Value), size, rotation, -texture.Pivot * scale.Abs(), mouseWorld);
		}

		public List<KeyframeableValue> EnumerateKeyframeableValues() => [Position, Scale, Rotation, FrameIndex, Transparency, ZIndex];
	}
}