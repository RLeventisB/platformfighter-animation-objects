using System.Collections;

namespace Editor.Objects
{
	public class ReadonlyKeyframeList : IReadOnlyList<Keyframe>
	{
		private KeyframeList _list;

		public ReadonlyKeyframeList(KeyframeList list)
		{
			_list = list;
		}
		public IEnumerator<Keyframe> GetEnumerator() => _list.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public int Count => _list.Count;
		public Keyframe this[int index] => _list[index];
	}
	public class KeyframeList : ICollection<Keyframe>
	{
		public List<Keyframe> _backingList;

		public KeyframeList()
		{
			_backingList = new List<Keyframe>();
		}

		public KeyframeList(int capacity)
		{
			_backingList = new List<Keyframe>(capacity);
		}

		public KeyframeList(IEnumerable<Keyframe> collection)
		{
			_backingList = new List<Keyframe>(collection);
		}

		public IEnumerator<Keyframe> GetEnumerator() => _backingList.GetEnumerator();

		public void SetOrModifyRange(IEnumerable<Keyframe> keyframes, bool onlySetValueOnModify = true)
		{
			foreach (Keyframe keyframe in keyframes)
			{
				SetOrModify(keyframe, onlySetValueOnModify);
			}
		}
		public void SetOrModify(Keyframe item, bool onlySetValueOnModify = true)
		{
			int index = FindIndexByKeyframe(item);

			if (index >= 0)
			{
				if (onlySetValueOnModify)
				{
					_backingList[index].Value = item.Value;
				}
				else
				{
					_backingList[index] = item;
				}
			}
			else
			{
				_backingList.Insert(~index, item);
			}
		}
		public int FindIndexByKeyframe(Keyframe value) => _backingList.BinarySearch(value);

		public int GetIndexOrNext(Keyframe value)
		{
			int foundIndex = FindIndexByKeyframe(value);

			return foundIndex >= 0 ? foundIndex : ~foundIndex;
		}

		public void Add(Keyframe item)
		{
			SetOrModify(item);
		}

		public void Clear()
		{
			_backingList.Clear();
		}

		public bool Contains(Keyframe item) => _backingList.Contains(item);

		public void CopyTo(Keyframe[] array, int arrayIndex)
		{
			_backingList.CopyTo(array, arrayIndex);
		}

		public bool Remove(Keyframe item) => _backingList.Remove(item);

		public int Count => _backingList.Count;
		public bool IsReadOnly => false;

		public int IndexOf(Keyframe item) => _backingList.IndexOf(item);
		
		public void RemoveAt(int index)
		{
			_backingList.RemoveAt(index);
		}

		public Keyframe this[int index]
		{
			get => _backingList[index];
			set => _backingList[index] = value;
		}

		public List<Keyframe> GetRange(int index, int count)
		{
			return _backingList.GetRange(index, count);
		}

		public int BinarySearch(Keyframe item)
		{
			return _backingList.BinarySearch(item);
		}

		public void Sort()
		{
			_backingList.Sort();
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
}