using System.Collections.Generic;

namespace SimpleAI.Framework
{
	/// <summary>
	/// An implementation of this interface is responsible for manipulating actions.
	/// </summary>
	/// <remarks>
	/// This interface provides a way of manipulating primitive actions, such as ints or longs.
	/// An implementor should be thread safe, and not carry any state.
	/// This class will be called concurrently.
	/// </remarks>
	public interface ActionBuilder <A, V> : IComparer<V>
	{
		/// <summary>
		/// Positive infinity.
		/// </summary>
		/// <value>
		/// This property should return a unique value to represent positive infinity.
		/// </value>
		V PosInf {get;}
		
		/// <summary>
		/// Negative infinity.
		/// </summary>
		/// <value>
		/// This property should return a unique value to represent negative infinity.
		/// </value>
		V NegInf {get;}
		
		/// <summary>
		/// Extract the value of the action without removing it.
		/// </summary>
		/// <returns>
		/// The evaluation value of the action.
		/// </returns>
		/// <param name='action'>
		/// The action.
		/// </param>
		V ExtractValue (A action);
		
		/// <summary>
		/// Inserts a new value into the action.
		/// </summary>
		/// <returns>
		/// The action which contains the inserted evaluation value.
		/// </returns>
		/// <param name='action'>
		/// The action.
		/// </param>
		/// <param name='val'>
		/// The evaluation value of the action.
		/// </param>
		A InsertValue (A action, V val);
		
		/// <summary>
		/// Perform a deep clone of the action.
		/// </summary>
		/// <param name='action'>
		/// The action.
		/// </param>
		A Clone (A action);
	}
}
