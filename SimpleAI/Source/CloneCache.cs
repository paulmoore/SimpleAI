using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace SimpleAI.Framework
{
	/// <summary>
	/// A thread safe class for creating and caching clones.
	/// </summary>
	internal sealed class CloneCache <C>
	{
		/// <summary>
		/// Gets or sets the parent.
		/// </summary>
		/// <value>
		/// The parent is the object which will be cloned if the cache is empty.
		/// </value>
		public MutableClone<C> Parent
		{
			[MethodImpl(MethodImplOptions.Synchronized)]
			get;
			[MethodImpl(MethodImplOptions.Synchronized)]
			set;
		}
		
		/// <summary>
		/// Gets or sets the backing store.
		/// </summary>
		/// <value>
		/// The backing collection used to store the clones, which may be iterated over.
		/// </value>
		public ConcurrentBag<C> BackingStore
		{
			get; private set;
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="SimpleAI.Framework.CloneCache`1"/> class.
		/// </summary>
		/// <param name='initialParent'>
		/// The initial parent used for clones.
		/// </param>
		public CloneCache (MutableClone<C> initialParent = null)
		{
			BackingStore = new ConcurrentBag<C>();
			Parent = initialParent;
		}
		
		/// <summary>
		/// Gets a clone.
		/// If the cache is empty, a new clone is created from the parent.
		/// If the parent is null, the default value of the parent type is returned.
		/// </summary>
		public C Get ()
		{
			C cached;
			if (BackingStore.TryTake(out cached)) {
				if (cached != null) {
					return cached;
				}
			}
			var parent = Parent;
			if (parent != null) {
				return parent.Clone();
			}
			return default(C);
		}
		
		/// <summary>
		/// Puts a clone back into the cache.
		/// </summary>
		/// <param name='cached'>
		/// The clone.
		/// </param>
		public void Put (C cached)
		{
			if (cached != null) {
				BackingStore.Add(cached);
			}
		}
		
		/// <summary>
		/// Removes all clones from the cache.
		/// </summary>
		public void Clear ()
		{
			while (!BackingStore.IsEmpty) {
				C cached;
				BackingStore.TryTake(out cached);
			}
		}
	}
}
