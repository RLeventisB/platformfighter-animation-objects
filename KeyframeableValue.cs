using Editor.Objects.Interpolators;

using Microsoft.Xna.Framework;

using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Editor.Objects
{
	public class Vector2KeyframeValue : KeyframeableValue
	{
		[JsonConstructor]
		public Vector2KeyframeValue()
		{
		}

		public Vector2KeyframeValue(IAnimationObject animationObject, Vector2 defaultValue, string name, bool createDefaultKeyframe = true) : base(animationObject, defaultValue, name, typeof(Vector2), createDefaultKeyframe)
		{
		}

		public Vector2 CachedValue => (Vector2)(cachedValue.value ?? Vector2.Zero);

		public Vector2 Interpolate(int frame)
		{
			Interpolate(this, frame, Vector2Interpolator, out object value2);

			return (Vector2)value2;
		}

		public bool TryInterpolate(int frame, out Vector2 value)
		{
			bool success = Interpolate(this, frame, Vector2Interpolator, out object value2);
			value = (Vector2)value2;

			return success;
		}

		public override void CacheValue(int? frame)
		{
			frame ??= ExternalActions.GetCurrentFrame();

			Interpolate(this, frame.Value, Vector2Interpolator, out object value);
			cachedValue = (value, frame.Value);
		}
	}
	public class FloatKeyframeValue : KeyframeableValue
	{
		[JsonConstructor]
		public FloatKeyframeValue()
		{
		}

		public FloatKeyframeValue(IAnimationObject animationObject, float defaultValue, string name, bool createDefaultKeyframe = true) : base(animationObject, defaultValue, name, typeof(float), createDefaultKeyframe)
		{
		}

		public float CachedValue => (float)(cachedValue.value ?? 0f);

		public float Interpolate(int frame)
		{
			Interpolate(this, frame, FloatInterpolator, out object value2);

			return (float)value2;
		}

		public bool TryInterpolate(int frame, out float value)
		{
			bool success = Interpolate(this, frame, FloatInterpolator, out object value2);
			value = (float)value2;

			return success;
		}

		public override void CacheValue(int? frame)
		{
			frame ??= ExternalActions.GetCurrentFrame();

			Interpolate(this, frame.Value, FloatInterpolator, out object value);
			cachedValue = (value, frame.Value);
		}
	}
	public class IntKeyframeValue : KeyframeableValue
	{
		[JsonConstructor]
		public IntKeyframeValue()
		{
		}

		public IntKeyframeValue(IAnimationObject animationObject, int defaultValue, string name, bool createDefaultKeyframe = true) : base(animationObject, defaultValue, name, typeof(int), createDefaultKeyframe)
		{
		}

		public int CachedValue => (int)(cachedValue.value ?? 0); // these nullability checks are for when the value is loading and it has nothing lol

		public int Interpolate(int frame)
		{
			Interpolate(this, frame, FloatInterpolator, out object value2);

			return (int)value2;
		}

		public bool TryInterpolate(int frame, out int value)
		{
			bool success = Interpolate(this, frame, FloatInterpolator, out object value2);
			value = (int)value2;

			return success;
		}

		public override void CacheValue(int? frame)
		{
			frame ??= ExternalActions.GetCurrentFrame();

			Interpolate(this, frame.Value, IntegerInterpolator, out object value);
			cachedValue = (value, frame.Value);
		}
	}
	[DebuggerDisplay("{Name}")]
	public abstract class KeyframeableValue
	{
		// its 1:32 am i dont want to refactor another parameter on this boilerplate code
		public static bool CacheValueOnInterpolate = true;

		public static readonly IInterpolator Vector2Interpolator = new DelegatedInterpolator<Vector2>(
			(fraction, first, second) => first + (second - first) * fraction,
			(fraction, values) => InterpolateFunctions.InterpolateCatmullRom(values, fraction * (values.Length - 1)));
		public static readonly IInterpolator IntegerInterpolator = new DelegatedInterpolator<int>(
			(fraction, first, second) => (int)(first + (second - first) * fraction),
			(fraction, values) => InterpolateFunctions.InterpolateCatmullRom(values, fraction * (values.Length - 1)));
		public static readonly IInterpolator FloatInterpolator = new DelegatedInterpolator<float>(
			(fraction, first, second) => first + (second - first) * fraction,
			(fraction, values) => InterpolateFunctions.InterpolateCatmullRom(values, fraction * (values.Length - 1)));

		public object DefaultValue;

		public KeyframeList keyframes;
		public List<KeyframeLink> links;
		public List<string> tags;
		[JsonIgnore]
		public (object value, int frame) cachedValue;
		public IAnimationObject Owner { get; init; }
		public string Name { get; init; }

		protected KeyframeableValue(IAnimationObject animationObject, object defaultValue, string name, Type type, bool createDefaultKeyframe = true) : this()
		{
			DefaultValue = defaultValue;
			cachedValue = (DefaultValue, -1);
			Owner = animationObject;
			Name = name;

			if (createDefaultKeyframe)
				keyframes.SetOrModify(new Keyframe(this, 0, DefaultValue));
		}

		[JsonConstructor]
		protected KeyframeableValue()
		{
			tags = new List<string>();
			keyframes = new KeyframeList();
			links = new List<KeyframeLink>();
		}

		public int KeyframeCount => keyframes.Count;
		public int FirstFrame => HasKeyframes() ? keyframes[0].Frame : -1;
		public int LastFrame => HasKeyframes() ? keyframes[KeyframeCount - 1].Frame : -1;
		public Keyframe FirstKeyframe => HasKeyframes() ? keyframes[0] : null;
		public Keyframe LastKeyframe => HasKeyframes() ? keyframes[KeyframeCount - 1] : null;

		public void Add(Keyframe value, bool invalidate = true, bool onlySetValueOnModify = true)
		{
			keyframes.SetOrModify(value, onlySetValueOnModify);

			if (invalidate)
				InvalidateCachedValue();

			if (ExternalActions.AreKeyframesAddedToLinkOnModify())
			{
				foreach (KeyframeLink link in links)
				{
					if (link.IsFrameOnRange(value.Frame))
					{
						link.Add(value);
					}
				}
			}
		}

		public void RemoveAt(int index)
		{
			Keyframe keyframe = keyframes[index];

			foreach (KeyframeLink link in links)
			{
				link.Remove(keyframe);
			}

			keyframes.RemoveAt(index);

			InvalidateCachedValue();
		}

		public void AddLink(KeyframeLink link)
		{
			link.ContainingValue = this;
			
			links.Add(link);

			InvalidateCachedValue();
		}

		public void RemoveLink(KeyframeLink link)
		{
			link.ContainingValue = null;
			
			ExternalActions.OnDeleteLink(link);

			links.Remove(link);

			InvalidateCachedValue();
		}

		public List<Keyframe> GetRange(int start, int count) => keyframes.GetRange(start, count);

		public bool HasKeyframes() => keyframes != null && keyframes.Count > 0;

		public bool HasKeyframeAtFrame(int frame) => GetKeyframe(frame) != null;

		public Keyframe GetKeyframe(int frame)
		{
			int foundIndex = FindIndexByKeyframe(Keyframe.CreateDummyKeyframe(frame));

			return foundIndex >= 0 ? keyframes[foundIndex] : null;
		}

		public int FindIndexByKeyframe(Keyframe value) => keyframes.BinarySearch(value);

		public int GetIndexOrNext(Keyframe value)
		{
			int foundIndex = FindIndexByKeyframe(value);

			return foundIndex >= 0 ? foundIndex : ~foundIndex;
		}

		public static bool Interpolate(KeyframeableValue keyframeValue, int frame, IInterpolator interpolator, out object value)
		{
			value = keyframeValue.DefaultValue;

			if (!keyframeValue.HasKeyframes())
				return false;

			if (CacheValueOnInterpolate && keyframeValue.cachedValue.frame == frame)
			{
				value = keyframeValue.cachedValue.value;

				return true;
			}

			int keyFrameIndex = keyframeValue.FindIndexByKeyframe(Keyframe.CreateDummyKeyframe(frame));
			Keyframe keyframe;

			if (keyFrameIndex >= 0)
			{
				keyframe = keyframeValue.keyframes[keyFrameIndex];
			}
			else // esto no es para frames negativos!!!! soy extremadamente estupido!!!!!!!
			{
				keyFrameIndex = ~keyFrameIndex - 1;

				if (keyFrameIndex < 0) // ok pq estaba esto en ingles peor eto pasa cuando el frame es negativo y antes del link creo
					keyFrameIndex = 0;

				keyframe = keyframeValue.keyframes[keyFrameIndex]; // obtener anterior frame
			}

			KeyframeLink link = FindContainingLink(keyframeValue, keyframe);

			if (link is null || link.Count == 1)
			{
				value = keyframe.Value;
				if (CacheValueOnInterpolate)
					keyframeValue.cachedValue = (value, frame);

				return true;
			}

			if (frame <= link.FirstKeyframe.Frame) // fast returns (for real this time)
			{
				value = link.FirstKeyframe.Value;

				return true;
			}

			if (frame >= link.LastKeyframe.Frame)
			{
				value = link.LastKeyframe.Value;

				return true;
			}

			int linkFrameDuration = link.LastKeyframe.Frame - link.FirstKeyframe.Frame;
			float progress;

			if (link.UseRelativeProgressCalculation)
			{
				int firstKeyframeIndex = keyframeValue.FindIndexByKeyframe(link.FirstKeyframe);
				Keyframe nextKeyframe = keyframeValue.keyframes[keyFrameIndex + 1];
				progress = (keyFrameIndex - firstKeyframeIndex + (float)(frame - keyframe.Frame) / (nextKeyframe.Frame - keyframe.Frame)) / (link.Count - 1);
			}
			else
			{
				progress = (frame - link.FirstKeyframe.Frame) / (float)linkFrameDuration;
			}

			float usedProgress = ApplyInterpolation(link.InterpolationType, progress);
			object[] objects = link.GetKeyframes().Select(v => v.Value).ToArray();
			value = interpolator.Interpolate(usedProgress, objects);

			if (CacheValueOnInterpolate)
				keyframeValue.cachedValue = (value, frame);

			return true;
		}

		public static KeyframeLink FindContainingLink(KeyframeableValue value, Keyframe keyframe)
		{
			foreach (KeyframeLink link in value.links.OrderBy(v => v.FirstKeyframe))
			{
				if (link.ContainsFrame(keyframe.Frame))
				{
					return link;
				}
			}

			return null;
		}

		private static float ApplyInterpolation(InterpolationType type, float progress)
		{
			float oldProgress = progress;

			switch (type)
			{
				case InterpolationType.Squared:
					progress *= oldProgress;

					break;
				case InterpolationType.InverseSquared:
					progress = 1 - (1 - oldProgress) * (1 - oldProgress);

					break;
				case InterpolationType.SmoothStep:
					progress = Easing.Quadratic.InOut(oldProgress);

					break;
				case InterpolationType.Cubed:
					progress *= oldProgress * oldProgress;

					break;
				case InterpolationType.InverseCubed:
					progress = 1 - (1 - oldProgress) * (1 - oldProgress) * (1 - oldProgress);

					break;
				case InterpolationType.CubedSmoothStep:
					progress = Easing.Cubic.InOut(oldProgress);

					break;
				case InterpolationType.ElasticOut:
					progress = Easing.Elastic.Out(oldProgress);

					break;
				case InterpolationType.ElasticInOut:
					progress = Easing.Elastic.InOut(oldProgress);

					break;
				case InterpolationType.ElasticIn:
					progress = Easing.Elastic.In(oldProgress);

					break;
				case InterpolationType.BounceIn:
					progress = Easing.Bounce.In(oldProgress);

					break;
				case InterpolationType.BounceOut:
					progress = Easing.Bounce.Out(oldProgress);

					break;
				case InterpolationType.BounceInOut:
					progress = Easing.Bounce.InOut(oldProgress);

					break;
				case InterpolationType.SineIn:
					progress = Easing.Sinusoidal.In(oldProgress);

					break;
				case InterpolationType.SineOut:
					progress = Easing.Sinusoidal.Out(oldProgress);

					break;
				case InterpolationType.SineInOut:
					progress = Easing.Sinusoidal.InOut(oldProgress);

					break;
				case InterpolationType.ExponentialIn:
					progress = Easing.Exponential.In(oldProgress);

					break;
				case InterpolationType.ExponentialOut:
					progress = Easing.Exponential.Out(oldProgress);

					break;
				case InterpolationType.ExponentialInOut:
					progress = Easing.Exponential.InOut(oldProgress);

					break;
				case InterpolationType.CircularIn:
					progress = Easing.Circular.In(oldProgress);

					break;
				case InterpolationType.CircularOut:
					progress = Easing.Circular.Out(oldProgress);

					break;
				case InterpolationType.CircularInOut:
					progress = Easing.Circular.InOut(oldProgress);

					break;
				case InterpolationType.BackIn:
					progress = Easing.Back.In(oldProgress);

					break;
				case InterpolationType.BackOut:
					progress = Easing.Back.Out(oldProgress);

					break;
				case InterpolationType.BackInOut:
					progress = Easing.Back.InOut(oldProgress);

					break;
			}

			return progress;
		}

		public abstract void CacheValue(int? frame);

		public void InvalidateCachedValue()
		{
			cachedValue = (DefaultValue, -1);
			CacheValue(null);
		}

		public static IInterpolator ResolveInterpolator(Type type)
		{
			switch (Activator.CreateInstance(type))
			{
				case float:
					return FloatInterpolator;
				case int:
					return IntegerInterpolator;
				case Vector2:
					return Vector2Interpolator;
			}

			return null;
		}

		public static IInterpolator ResolveInterpolator(KeyframeableValue value)
		{
			switch (value)
			{
				case FloatKeyframeValue:
					return FloatInterpolator;
				case IntKeyframeValue:
					return IntegerInterpolator;
				case Vector2KeyframeValue:
					return Vector2Interpolator;
			}

			return null;
		}

		public Keyframe SetKeyframeValue(int? frame, object data, bool setCachedValue = false)
		{
			frame ??= ExternalActions.GetCurrentFrame();
			Keyframe keyframe = new Keyframe(this, frame.Value, data);

			if (ExternalActions.AreKeyframesSetOnModify() && !setCachedValue)
			{
				Add(keyframe);

				InvalidateCachedValue();
			}
			else
			{
				cachedValue = (data, frame.Value);
			}

			return keyframe;
		}

		public bool RemoveKeyframe(int frame)
		{
			int index = FindIndexByKeyframe(Keyframe.CreateDummyKeyframe(frame));

			if (index < 0)
				return false;

			keyframes.RemoveAt(index);

			InvalidateCachedValue();

			return true;
		}

		public void SortFrames()
		{
			keyframes.Sort();
		}

		public int IndexOfKeyframe(Keyframe keyframe)
		{
			return keyframes.IndexOf(keyframe);
		}

		public KeyframeableValue CloneKeyframeDataFrom(KeyframeableValue other)
		{
			foreach (Keyframe keyframe in other.keyframes)
			{
				Add(new Keyframe(this, keyframe.Frame, keyframe.Value.CloneWithoutReferences()));
			}

			foreach (KeyframeLink link in other.links)
			{
				AddLink(new KeyframeLink(this, link.Frames));
			}

			return this;
		}

		public KeyframeableValue AddTags(IEnumerable<string> list)
		{
			tags.AddRange(list);

			return this;
		}

		public KeyframeableValue AddTag(string tag)
		{
			tags.Add(tag);

			return this;
		}
	}
}