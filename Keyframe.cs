using Microsoft.Xna.Framework;

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Editor.Objects
{
	[DebuggerDisplay("Frame = {Frame}, Value = {Value}")]
	public class Keyframe : IComparable<Keyframe>
	{
		public KeyframeableValue ContainingValue;
		[JsonInclude]
		[JsonConverter(typeof(KeyframeValueJsonConverter))]
		internal object value;

		[JsonConstructor]
		public Keyframe()
		{
			Frame = -1;
			Value = null;
		}

		public Keyframe(KeyframeableValue containingValue, int frame, object data)
		{
			ContainingValue = containingValue;
			Frame = frame;
			Value = data;
		}

		public int Frame { get; set; }
		[JsonIgnore]
		public object Value
		{
			get => value;
			set
			{
				this.value = value;
				ContainingValue?.InvalidateCachedValue();
			}
		}

		public int CompareTo(Keyframe other) => Frame.CompareTo(other.Frame);

		public void SetValueWithoutUpdate(object value)
		{
			this.value = value;
		}
		public static Keyframe CreateDummyKeyframe(int frame)
		{
			return new Keyframe(null, frame, default);
		}

		public override int GetHashCode() => Value.GetType().GetHashCode() ^ Frame;

		public class KeyframeValueJsonConverter : JsonConverter<object>
		{
			public override bool CanConvert(Type typeToConvert) => true;

			public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				switch (reader.TokenType)
				{
					case JsonTokenType.StartObject: // is a vector2
						reader.Read();

						if (reader.TokenType != JsonTokenType.PropertyName) // x
							throw new JsonException();

						reader.Read();
						float x = reader.GetSingle();

						reader.Read();

						if (reader.TokenType != JsonTokenType.PropertyName) // y
							throw new JsonException();

						reader.Read();
						float y = reader.GetSingle();

						reader.Read();

						if (reader.TokenType != JsonTokenType.EndObject) // y
							throw new JsonException();

						return new Vector2(x, y);
					case JsonTokenType.Number: // oh no
						if (!reader.TryGetInt32(out int valueInt))
						{
							if (!reader.TryGetSingle(out float valueSingle))
							{
								throw new JsonException();
							}

							return valueSingle;
						}

						return valueInt;
				}

				throw new JsonException();
			}

			public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
			{
				switch (value)
				{
					case JsonElement element:
						string data = element.GetRawText();
						string[] lines = data.Split(Environment.NewLine).Where(v => !v.Contains("\"$id\"")).ToArray(); // no
						data = string.Join(Environment.NewLine, lines);
						writer.WriteRawValue(data);

						break;
					case int int32:
						writer.WriteNumberValue(int32);

						break;
					case float single:
						string number = single.ToString(NumberFormatInfo.InvariantInfo);

						if (!number.Contains('.'))
						{
							number += ".0";
						}

						writer.WriteRawValue(number);

						break;
					case Vector2 vector2:
						writer.WriteStartObject();
						writer.WriteNumber("x", vector2.X);
						writer.WriteNumber("y", vector2.Y);
						writer.WriteEndObject();

						break;
				}
			}
		}
	}
	[DebuggerDisplay("Count = {Count}, ContainingValue = {ContainingValue}")]
	public class KeyframeLink
	{
		[JsonPropertyName("keyframes")]
		[JsonInclude]
		private List<int> _keyframes;
		[JsonIgnore]
		public ReadOnlyCollection<int> Frames => _keyframes.AsReadOnly();
		public KeyframeableValue ContainingValue;
		private InterpolationType _interpolationType;
		public bool UseRelativeProgressCalculation = true;

		[JsonConstructor]
		public KeyframeLink()
		{
			_keyframes = new List<int>();
			InterpolationType = InterpolationType.Lineal;
		}

		public KeyframeLink(KeyframeableValue containingValue, IEnumerable<Keyframe> keyframes) : this()
		{
			ContainingValue = containingValue;
			AddRange(keyframes);

			InterpolationType = InterpolationType.Lineal;
		}

		public KeyframeLink(KeyframeableValue containingValue, IEnumerable<int> frames) : this()
		{
			ContainingValue = containingValue;
			AddRange(frames);

			InterpolationType = InterpolationType.Lineal;
		}

		public void AddRange(IEnumerable<Keyframe> keyframes)
		{
			AddRange(keyframes.Where(HaveSameContainingValue).Select(v => v.Frame));
		}

		public void AddRange(IEnumerable<int> frames)
		{
			_keyframes.AddRange(frames);
			_keyframes.Sort();
		}

		public int this[int index] => _keyframes[index];
		[JsonInclude]
		public InterpolationType InterpolationType
		{
			get => _interpolationType;
			set
			{
				_interpolationType = value;

				ExternalActions.OnChangeLinkPropertyProperty(this);
			}
		}
		public int Count => _keyframes.Count;
		public Keyframe FirstKeyframe => Count == 0 ? Keyframe.CreateDummyKeyframe(-1) : ContainingValue.GetKeyframe(_keyframes[0]);
		public Keyframe LastKeyframe => Count == 0 ? Keyframe.CreateDummyKeyframe(-1) : ContainingValue.GetKeyframe(_keyframes[Count - 1]);

		public Keyframe GetKeyframeClamped(int index) => ContainingValue.GetKeyframe(_keyframes[Math.Clamp(index, 0, Count - 1)]);

		public IEnumerator<int> GetEnumerator() => _keyframes.GetEnumerator();

		public void Clear()
		{
			_keyframes.Clear();
		}

		public void Sort()
		{
			_keyframes.Sort();
		}

		public bool Contains(Keyframe keyframe) => HaveSameContainingValue(keyframe) && _keyframes.Contains(keyframe.Frame);

		public bool ContainsFrame(int frame) => _keyframes.Contains(frame);

		public void CopyTo(int[] array, int arrayIndex)
		{
			_keyframes.CopyTo(array, arrayIndex);
		}

		public void Add(Keyframe keyframe)
		{
			if (HaveSameContainingValue(keyframe))
				Add(keyframe.Frame);
		}

		public void Add(int frame)
		{
			if (ContainsFrame(frame))
			{
				Debug.Write("trying to add already linked frame");

				return;
			}

			_keyframes.Add(frame);
			_keyframes.Sort();
		}

		public bool Remove(Keyframe keyframe)
		{
			return HaveSameContainingValue(keyframe) && Remove(keyframe.Frame);
		}
		public bool Remove(int frame)
		{
			return _keyframes.Remove(frame);
		}

		public bool HaveSameContainingValue(Keyframe keyframe)
		{
			return ReferenceEquals(keyframe.ContainingValue, ContainingValue);
		}

		public bool IsFrameOnRange(int valueFrame)
		{
			return valueFrame >= FirstKeyframe.Frame && valueFrame <= LastKeyframe.Frame;
		}

		public void SanitizeValues()
		{
			List<int> indices = new List<int>();

			foreach (int frame in _keyframes)
			{
				if (indices.Contains(frame))
					continue;

				indices.Add(frame);
			}

			_keyframes = indices;
		}

		public IEnumerable<Keyframe> GetKeyframes()
		{
			return _keyframes.Select(v => ContainingValue.GetKeyframe(v));
		}
	}
	public enum InterpolationType : byte
	{
		Lineal,
		Squared, InverseSquared,
		BounceIn, BounceOut, BounceInOut,
		ElasticIn, ElasticOut, ElasticInOut,
		SmoothStep,
		Cubed, InverseCubed, CubedSmoothStep,
		SineIn, SineOut, SineInOut,
		ExponentialIn, ExponentialOut, ExponentialInOut,
		CircularIn, CircularOut, CircularInOut,
		BackIn, BackOut, BackInOut
	}
}