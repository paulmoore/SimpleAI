namespace SimpleAI.Framework
{
	public interface CutoffTest <S, A> where S : State<A>
	{
		void Begin ();
		
		void End ();
		
		bool Test (S state);
	}
}
