using System.Collections.Generic;

namespace SimpleAI.Framework
{
	public interface ActionBuilder <A, V> : IComparer<V>
	{
		V PosInf {get;}
		
		V NegInf {get;}
		
		V ExtractValue (A action);
		
		A InsertValue (A action, V val);
		
		A Clone (A action);
	}
}
