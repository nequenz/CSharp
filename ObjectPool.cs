using System;
using System.Collections;
using System.Collections.Generic;

namespace Tired
{
	public delegate void IUpdatableHandler(IUpdatable updatable);

	public sealed class EnumeratedPool<T> : IEnumerable where T : IUpdatable
	{
		private readonly List<T> _updateablesList = null;

		public EnumeratedPool(List<T> updatables) => _updateablesList = updatables;
		public int Capacity { get; private set; } = 256;
		public int Amount
		{
			get
			{
				return _updateablesList.Count;
			}
		}

		public T AddInstance(T instance)
		{
			if (_updateablesList.Count < Capacity)
			{
				_updateablesList.Add(instance);
			}

			return instance;
		}

		public List<T> FindAllUpdatableObject(Predicate<T> predicate) => _updateablesList.FindAll(predicate);

		public T FindUpdatableObject(Predicate<T> predicate) => _updateablesList.Find(predicate);

		public T GetUpdatableObjectByIndex(int index)
		{
			if (_updateablesList == null || _updateablesList.Count == 0
				|| (index < 0 && index >= _updateablesList.Count)) return default(T);

			return _updateablesList[index];
		}

		public IEnumerator GetEnumerator() => _updateablesList.GetEnumerator();
	}

	public class UpdatablePool<T> where T : IUpdatable
	{
		private readonly EnumeratedPool<T> _protectedPool = null;
		private readonly List<T> _updatablesList = new List<T>();
		private readonly List<int> _garbageIndexList = new List<int>();

		private event IUpdatableHandler _onDestroyEvent;

		public UpdatablePool()
		{
			_protectedPool = new EnumeratedPool<T>(_updatablesList);
		}

		public EnumeratedPool<T> GetEnumeratedPool() => _protectedPool;

		private void CollectGarbage()
		{
			for (int i = _garbageIndexList.Count - 1; i >= 0; i--)
			{
				_updatablesList.RemoveAt(_garbageIndexList[i]);
			}

			_garbageIndexList.Clear();
		}

		public void Update()
		{
			for (int i = 0; i < _updatablesList.Count; i++)
			{
				T updatable = _updatablesList[i];

				if (updatable.IsDestroyed == false)
				{
					updatable.TakeEnumaratedPool(_protectedPool);
					updatable.Update();
					updatable.Draw();
				}
				else
				{
					_onDestroyEvent?.Invoke(updatable);
					_garbageIndexList.Add(i);
				}
			}

			CollectGarbage();
		}

		public void AddListenerOnDestroyEvent(IUpdatableHandler updatableHandler) => _onDestroyEvent += updatableHandler;

		public void RemoveListenerOnDestroyEvent(IUpdatableHandler updatableHandler) => _onDestroyEvent -= updatableHandler;
	}

	public interface IUpdatable
	{
		public bool IsDestroyed { get; }
		public void Destroy();
		public void Update();
		public void Draw();
		public void TakeEnumaratedPool<T>(EnumeratedPool<T> enumeratedPool) where T : IUpdatable;
	}


}
