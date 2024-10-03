global using NVector2 = System.Numerics.Vector2;
using Microsoft.Xna.Framework;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Editor.Objects
{
	public static class InterpolateFunctions
	{
		public static int InterpolateCatmullRom(int[] values, float progress)
		{
			if (values.Length < 2)
				throw new ArgumentException("At least two points are required for interpolation.");

			List<int> valuesList = new List<int>();
			valuesList.Add(2 * values[0] - values[1]);
			valuesList.AddRange(values);
			valuesList.Add(2 * values[^1] - values[^2]);

			int index = Math.Clamp((int)progress + 1, 1, valuesList.Count - 3);
			float localProgress = progress - index + 1;

			return (int)MathHelper.CatmullRom(valuesList[index - 1], valuesList[index], valuesList[index + 1], valuesList[index + 2], localProgress);
		}

		public static float InterpolateCatmullRom(float[] values, float progress)
		{
			if (values.Length < 2)
				throw new ArgumentException("At least two points are required for interpolation.");

			List<float> valuesList = new List<float>();
			valuesList.Add(2 * values[0] - values[1]);
			valuesList.AddRange(values);
			valuesList.Add(2 * values[^1] - values[^2]);

			int index = Math.Clamp((int)progress + 1, 1, valuesList.Count - 3);
			float localProgress = progress - index + 1;

			return MathHelper.CatmullRom(valuesList[index - 1], valuesList[index], valuesList[index + 1], valuesList[index + 2], localProgress);
		}

		public static Vector2 InterpolateCatmullRom(Vector2[] points, float progress)
		{
			if (points.Length < 2)
				throw new ArgumentException("At least two points are required for interpolation.");

			List<Vector2> pointsList = new List<Vector2>();
			pointsList.Add(2 * points[0] - points[1]);
			pointsList.AddRange(points);
			pointsList.Add(2 * points[^1] - points[^2]);

			int index = Math.Clamp((int)progress, 0, points.Length - 2) + 1;
			float localProgress = progress - index + 1;

			return Vector2.CatmullRom(pointsList[index - 1], pointsList[index], pointsList[index + 1], pointsList[index + 2], localProgress);
		}
		public static float InverseLerp(float value, float min, float max)
		{
			return (value - min) / (max - min);
		}

		public static Vector2 InverseLerp(Vector2 value, Vector2 min, Vector2 max)
		{
			return (value - min) / (max - min);
		}
	}
	public static class MiscellaneousFunctions
	{
		public static unsafe object CloneWithoutReferences(this object obj)
		{
			Type underlyingType = obj.GetType();
			object clone = Activator.CreateInstance(underlyingType);
			
			Unsafe.CopyBlock(&clone, &obj, (uint)Marshal.SizeOf(underlyingType));
			return clone;
		}
		public static bool IsInsideRectangle(Vector2 position, Vector2 size, Vector2 point)
		{
			return Math.Abs(point.X - position.X) <= size.X / 2 && Math.Abs(point.Y - position.Y) <= size.Y / 2;
		}
		public static Color MultiplyAlpha(this Color color, float multiplier)
		{
			color.A = (byte)MathHelper.Clamp(color.A * multiplier, byte.MinValue, byte.MaxValue);

			return color;
		}

		public static Color MultiplyRGB(this Color color, float multiplier)
		{
			color.R = (byte)MathHelper.Clamp(color.R * multiplier, byte.MinValue, byte.MaxValue);
			color.G = (byte)MathHelper.Clamp(color.G * multiplier, byte.MinValue, byte.MaxValue);
			color.B = (byte)MathHelper.Clamp(color.B * multiplier, byte.MinValue, byte.MaxValue);

			return color;
		}
		
		public static bool IsPointInsideRotatedRectangle(Vector2 center, Vector2 size, float rotation, Vector2 pivot, Vector2 point)
		{
			// ew chatgpt again (i am running out of time)
			(float sin, float cos) = MathF.SinCos(rotation);
			ExternalActions.CalculateQuadPoints(center.X, center.Y, pivot.X, pivot.Y, size.X, size.Y, sin, cos,
				out float tlX, out float tlY,
				out float trX, out float trY,
				out float blX, out float blY,
				out float brX, out float brY
			);

			Vector2[] corners = { new Vector2(tlX, tlY), new Vector2(trX, trY), new Vector2(brX, brY), new Vector2(blX, blY) };

			return WindingNumber(point, corners) != 0;
		}

		private static int WindingNumber(Vector2 point, Vector2[] corners)
		{
			int wn = 0;

			for (int i = 0; i < corners.Length; i++)
			{
				Vector2 c1 = corners[i];
				Vector2 c2 = corners[(i + 1) % corners.Length];

				if (c1.Y <= point.Y)
				{
					if (c2.Y > point.Y && IsLeft(c1.X, c1.Y, c2.X, c2.Y, point.X, point.Y) > 0)
						wn++;
				}
				else
				{
					if (c2.Y <= point.Y && IsLeft(c1.X, c1.Y, c2.X, c2.Y, point.X, point.Y) < 0)
						wn--;
				}
			}

			return wn;
		}

		private static float IsLeft(float x1, float y1, float x2, float y2, float px, float py)
		{
			return (x2 - x1) * (py - y1) - (y2 - y1) * (px - x1);
		}
		
		public static Vector2 Abs(this Vector2 vector2)
		{
			return new Vector2(MathF.Abs(vector2.X), MathF.Abs(vector2.Y));
		}
	}
	public static class PropertyNames
	{
		public const string ScaleProperty = "Scale";
		public const string FrameIndexProperty = "Frame Index";
		public const string RotationProperty = "Rotation";
		public const string TransparencyProperty = "Transparency";
		public const string PositionProperty = "Position";
		public const string ZIndexProperty = "ZIndex";
		public const string SizeProperty = "Size";
	}
}