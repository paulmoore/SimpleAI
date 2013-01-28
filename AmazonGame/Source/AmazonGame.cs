using System;
using SimpleAI.Framework;

namespace AmazonGame
{
	public class Amazon
	{
		public static void Main (string[] args)
		{
			Console.WriteLine("Starting Game...");
			var search = new Search<AmazonState, AmazonAction, int, byte, AmazonPlayer>(100 * 1000);
			search.Builder = new AmazonBuilder();
			search.Cutoff = new AmazonCutoffTest(30 * 1000);
			search.EvalFunc = new AmazonEvaluationFunction();
			search.SuccessorFunc = new AmazonSuccessorFunction();
			var state = new AmazonState();
			AmazonAction decision = search.MinimaxDecision(state, AmazonPlayer.WHITE, AmazonPlayer.BLACK);
			Console.WriteLine(string.Format("Made decision {0}", decision));
		}
	}
}
