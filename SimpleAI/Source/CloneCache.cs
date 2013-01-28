using System;
using System.Collections.Generic;

namespace SimpleAI.Framework
{
	internal sealed class CloneCache <C>
	{
		public MutableClone<C> Parent
		{
			get;
			set;
		}
		
		public List<C> BackingStore
		{
			get;
			private set;
		}
		
		public CloneCache (int initialCapacity = 0, MutableClone<C> initialParent = null)
		{
			BackingStore = new List<C>(initialCapacity);
			Parent = initialParent;
		}
		
		public C Get ()
		{
			int count = BackingStore.Count;
			if (count > 0) {
				C cached = BackingStore[count - 1];
				BackingStore.RemoveAt(count - 1);
				return cached;
			}
			if (Parent != null) {
				return Parent.Clone();
			}
			return default(C);
		}
		
		public void Put (C cached)
		{
			BackingStore.Add(cached);
		}
		
		public void Clear ()
		{
			BackingStore.Clear();
		}
	}
}
