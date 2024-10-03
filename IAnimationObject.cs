using Microsoft.Xna.Framework;

namespace Editor.Objects
{
	public interface IAnimationObject
	{
		public string Name { get; set; }

		public bool IsBeingHovered(Vector2 mouseWorld, int? frame);

		public List<KeyframeableValue> EnumerateKeyframeableValues();
	}
}