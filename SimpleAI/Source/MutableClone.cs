namespace SimpleAI.Framework
{
	/// <summary>
	/// A mutable clone should return a deep clone of itself.
	/// </summary>
	public interface MutableClone <C>
	{
		/// <summary>
		/// Clone this instance.
		/// </summary>
		C Clone ();
	}
}
