﻿using System.Diagnostics;
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
		[JsonIgnore]
		public KeyframeLink ContainingLink
		{
			get => _containingLink;
			set => _containingLink = value;
		}

		public int CompareTo(Keyframe other) => Frame.CompareTo(other.Frame);

		public static implicit operator Keyframe(int value) => new Keyframe(null, value, default);

		public override int GetHashCode() => Value.GetType().GetHashCode() ^ Frame;
	}
	public class KeyframeLink
	{
		[JsonPropertyName("keyframes")]
		[JsonInclude]
		private KeyframeList _keyframes;
		[JsonIgnore]
		public ReadonlyKeyframeList Keyframes => new ReadonlyKeyframeList(_keyframes);
		public KeyframeableValue ContainingValue;
		private InterpolationType _interpolationType;
		public bool UseRelativeProgressCalculation = true;

		[JsonConstructor]
		public KeyframeLink()
		{
			_keyframes = new KeyframeList();
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
			_keyframes.SetOrModifyRange(keyframes);
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

		public Keyframe GetKeyframeClamped(int index) => Count == 0 ? null : _keyframes[Math.Clamp(index, 0, Count - 1)];

		public IEnumerator<Keyframe> GetEnumerator() => _keyframes.ToList().GetEnumerator();
		
		public void Clear()
		{
			_keyframes.Clear();
		}

		public bool Contains(Keyframe item) => _keyframes.Contains(item);

		public void CopyTo(Keyframe[] array, int arrayIndex)
		{
			_keyframes.CopyTo(array, arrayIndex);
		}

		public void Add(Keyframe item)
		{
			_keyframes.Add(item);
			item.ContainingLink = this;
			_keyframes.Sort();
		}

		public bool Remove(Keyframe item)
		{
			bool remove = _keyframes.Remove(item);
			_keyframes.Sort();

			return remove;
		}

		public bool IsFrameOnRange(int valueFrame)
		{
			return FirstKeyframe.Frame - valueFrame < LastKeyframe.Frame - FirstKeyframe.Frame;
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