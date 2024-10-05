using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Editor.Objects
{
	[DebuggerDisplay("Frame = {Frame}, Value = {Value}")]
	public class Keyframe : IComparable<Keyframe>
	{
		public KeyframeableValue ContainingValue;
		[JsonInclude]
		private object value;
		[JsonPropertyName("containing_link")]
		[JsonInclude]
		private KeyframeLink _containingLink;

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

		public static implicit operator Keyframe(int value) => new Keyframe(null, value, default);

		public override int GetHashCode() => Value.GetType().GetHashCode() ^ Frame;
	}
	public class KeyframeLink
	{
		[JsonPropertyName("keyframes")]
		[JsonInclude]
		private List<int> _keyframes;
		[JsonIgnore]
		public ReadOnlyCollection<int> Keyframes => _keyframes.AsReadOnly();
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
			_keyframes.Sort();

			InterpolationType = InterpolationType.Lineal;
		}

		private void AddRange(IEnumerable<Keyframe> keyframes)
		{
			_keyframes.AddRange(keyframes.Where(v => ContainingValue == v.ContainingValue).Select(v => v.Frame));
		}

		public Keyframe this[int index] => _keyframes[index];
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
		public Keyframe FirstKeyframe => _keyframes.FirstOrDefault(-1);
		public Keyframe LastKeyframe => _keyframes.LastOrDefault(1);

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

		public bool Contains(Keyframe item) => ContainingValue == item.ContainingValue && _keyframes.Contains(item.Frame);

		public bool ContainsFrame(int frame) => _keyframes.Contains(frame);

		public void CopyTo(int[] array, int arrayIndex)
		{
			_keyframes.CopyTo(array, arrayIndex);
		}

		public void Add(Keyframe item)
		{
			_keyframes.Add(item.Frame);
			_keyframes.Sort();
		}

		public void Add(int frame)
		{
			_keyframes.Add(frame);
			_keyframes.Sort();
		}

		public bool Remove(Keyframe item)
		{
			return item.ContainingValue == ContainingValue && _keyframes.Remove(item.Frame);
		}

		public bool IsFrameOnRange(int valueFrame)
		{
			return valueFrame >= FirstKeyframe.Frame && valueFrame <= LastKeyframe.Frame;
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