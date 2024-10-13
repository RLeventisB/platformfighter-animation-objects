global using NVector2 = System.Numerics.Vector2;

using Microsoft.Xna.Framework;

using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

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
		public static void GetQuadsPrimitive(float x, float y, float pivotX, float pivotY, float w, float h, float sin, float cos, out float tlX, out float tlY, out float trX, out float trY, out float blX, out float blY, out float brX, out float brY)
		{
			tlX = x + pivotX * cos - pivotY * sin;
			tlY = y + pivotX * sin + pivotY * cos;
			trX = x + (pivotX + w) * cos - pivotY * sin;
			trY = y + (pivotX + w) * sin + pivotY * cos;
			blX = x + pivotX * cos - (pivotY + h) * sin;
			blY = y + pivotX * sin + (pivotY + h) * cos;
			brX = x + (pivotX + w) * cos - (pivotY + h) * sin;
			brY = y + (pivotX + w) * sin + (pivotY + h) * cos;
		}

		public static bool IsInsideRectangle(Vector2 position, Vector2 size, float rotation, Vector2 point)
		{
			// Translate point to local coordinates of the rectangle
			double localX = point.X - position.X;
			double localY = point.Y - position.Y;

			// Rotate point around the rectangle center by the negative of the rectangle angle
			(double sinAngle, double cosAngle) = Math.SinCos(-rotation);

			double rotatedX = localX * cosAngle - localY * sinAngle;
			double rotatedY = localX * sinAngle + localY * cosAngle;

			// Check if the rotated point is inside the unrotated rectangle
			double halfWidth = size.X / 2;
			double halfHeight = size.Y / 2;

			return Math.Abs(rotatedX) <= halfWidth && Math.Abs(rotatedY) <= halfHeight;
		}

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
			GetQuadsPrimitive(center.X, center.Y, pivot.X, pivot.Y, size.X, size.Y, sin, cos,
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

		public static void RemoveInvalidLinks(IAnimationObject animationObject)
		{
			foreach (KeyframeableValue value in animationObject.EnumerateKeyframeableValues())
			{
				for (int index = 0; index < value.links.Count; index++)
				{
					KeyframeLink link = value.links[index];
					link.SanitizeValues();
					List<int> frames = link.Frames.ToList();
					frames.RemoveAll(v => !value.HasKeyframeAtFrame(v));
					link = new KeyframeLink(link.ContainingValue, frames);

					if (link.Count >= 2)
						continue;

					value.links.RemoveAt(index);
					index--;
				}
			}
		}
	}
	public record JsonData(bool looping, bool playingForward, bool playingBackwards, int selectedFps, int currentKeyframe, TextureFrame[] textures, TextureAnimationObject[] graphicObjects, HitboxAnimationObject[] hitboxObjects)
	{
		public static JsonSerializerOptions DefaultSerializerOptions => new JsonSerializerOptions // returns an new instance everytime because caching is brROKEN  I LOST 6 ENTIRE PROJECTS
		{
			PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
			AllowTrailingCommas = true,
			IncludeFields = true,
			WriteIndented = true,
			ReferenceHandler = ReferenceHandler.Preserve,
			IgnoreReadOnlyFields = true,
			IgnoreReadOnlyProperties = true,
			ReadCommentHandling = JsonCommentHandling.Skip,
			DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
			Converters =
			{
				new JsonStringEnumConverter<HitboxType>(JsonNamingPolicy.SnakeCaseLower),
				new JsonStringEnumConverter<HitboxConditions>(JsonNamingPolicy.SnakeCaseLower),
				new JsonStringEnumConverter<LaunchType>(JsonNamingPolicy.SnakeCaseLower),
				new JsonStringEnumConverter<InterpolationType>(JsonNamingPolicy.SnakeCaseLower),
			}
		};

		[JsonConstructor]
		public JsonData() : this(false, false, false, 0, 0, Array.Empty<TextureFrame>(), Array.Empty<TextureAnimationObject>(), Array.Empty<HitboxAnimationObject>())
		{
		}

		public void Fixup()
		{
			TextureAnimationObject textureModel = new TextureAnimationObject();
			foreach (TextureAnimationObject graphicObject in graphicObjects)
			{
				List<KeyframeableValue> list = graphicObject.EnumerateKeyframeableValues();
				List<KeyframeableValue> modelValues = textureModel.EnumerateKeyframeableValues();

				for (int i = 0; i < list.Count; i++)
				{
					FixKeyframeableValue(list[i], modelValues[i]);
				}
			}

			HitboxAnimationObject hitboxModel = new HitboxAnimationObject();
			foreach (HitboxAnimationObject hitboxObject in hitboxObjects)
			{
				List<KeyframeableValue> list = hitboxObject.EnumerateKeyframeableValues();
				List<KeyframeableValue> modelValues = hitboxModel.EnumerateKeyframeableValues();

				for (int i = 0; i < list.Count; i++)
				{
					FixKeyframeableValue(list[i], modelValues[i]);
				}
			}
		}

		private static void FixKeyframeableValue(KeyframeableValue keyframeableValue, KeyframeableValue modelValue)
		{
			keyframeableValue.DefaultValue = modelValue.DefaultValue; // dumb fuck is set as null even if it's ignored

			for (int i = 0; i < keyframeableValue.keyframes.Count; i++)
			{
				Keyframe keyframe = keyframeableValue.keyframes[i];
				
				if (keyframe.Value is null)
					keyframeableValue.RemoveAt(i);
				
				keyframe.Value = ResolveKeyframeValue(keyframe.Value, keyframeableValue);
			}

			foreach (KeyframeLink link in keyframeableValue.links)
			{
				link.SanitizeValues();
			}
		}

		public static void ResolveKeyframeValue(ref object value, KeyframeableValue containingValue)
		{
			value = ResolveKeyframeValue(value, containingValue);
		}

		public static object ResolveKeyframeValue(object value, KeyframeableValue containingValue)
		{
			if (value is int intValue && containingValue is FloatKeyframeValue)
			{
				return (float)intValue;
			}

			return value;
		}

		public static JsonData LoadFromPath(string filePath)
		{
			byte[] text = File.ReadAllBytes(filePath);

			if (BitConverter.ToUInt16(text, 0) == 0x9DD5) // file is compressed
			{
				using (MemoryStream stream = new MemoryStream(text))
				using (DeflateStream deflateStream = new DeflateStream(stream, CompressionMode.Decompress))
				{
					using (MemoryStream outputStream = new MemoryStream())
					{
						deflateStream.CopyTo(outputStream);
						text = outputStream.GetBuffer();
					}
				}
			}

			return JsonSerializer.Deserialize<JsonData>(text, DefaultSerializerOptions);
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