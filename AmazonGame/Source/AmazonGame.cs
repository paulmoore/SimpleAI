// http://paulmoore.mit-license.org

using System;
using SimpleAI.Framework;

namespace AmazonGame
{
	/// <summary>
	/// A Simple console application to show of SimpleAI.
	/// This game is called 'Game of the Amazons', find out more here: http://en.wikipedia.org/wiki/Game_of_the_Amazons
	/// </summary>
	public sealed class Amazon
	{
		public static void Main (string[] args)
		{
			// TODO allow this to be configured as command line options
			// TODO put new commands in to change these settings during play
			// TODO allow player to choose which player he/she is
			var state = new AmazonState();
			var search = new Search<AmazonState, AmazonAction, int, byte, AmazonPlayer>(1000000);
			search.Builder = new AmazonBuilder();
			search.Cutoff = new AmazonCutoffTest(30 * 1000);
			search.EvalFunc = new AmazonEvaluationFunction();
			search.SuccessorFunc = new AmazonSuccessorFunction();
			search.MaxExplorations = 100;
			Console.WriteLine("Welcome to the Game of the Amazons!");
			Console.WriteLine("Program by Paul Moore: github.com/paulmoore/SimpleAI");
			Console.WriteLine("If you are stuck, try 'help'");
			Console.WriteLine(state);
			Console.WriteLine("You are player WHITE");
			Console.WriteLine("Your turn");
			while (true) {
				Console.Write("amazons> ");
				string input = Console.ReadLine();
				if (input == null) {
					break;
				}
				input = input.Trim().ToLower();
				string[] cmd = input.Split(new string[]{" ", " \t"}, StringSplitOptions.RemoveEmptyEntries);
				if (cmd.Length == 0) {
					continue;
				}
				if (cmd[0] == "quit") {
					break;
				} else if (cmd[0] == "help") {
					Console.WriteLine("Help menu:");
					Console.WriteLine("  help");
					Console.WriteLine();
					Console.WriteLine("Quitting:");
					Console.WriteLine("  quit");
					Console.WriteLine();
					Console.WriteLine("Making a move (ith row, jth column):");
					Console.WriteLine("  move i1 j1 i2 j2 ar ac");
				} else if (cmd[0] == "move") {
					try {
						int i1 = int.Parse(cmd[1]);
						int j1 = char.Parse(cmd[2]) - 'a';
						int i2 = int.Parse(cmd[3]);
						int j2 = char.Parse(cmd[4]) - 'a';
						int ar = int.Parse(cmd[5]);
						int ac = char.Parse(cmd[6]) - 'a';
						AmazonAction move = new AmazonAction();
						move.role = AmazonPlayer.WHITE;
						move.qr = (sbyte)(i1 - 1);
						move.qc = (sbyte)j1;
						move.qfr = (sbyte)(i2 - 1);
						move.qfc = (sbyte)j2;
						move.ar = (sbyte)(ar - 1);
						move.ac = (sbyte)ac;
						Console.WriteLine("You want to make this move: {0}", move);
						string errmsg = AmazonMoveValidator.Validate(state, move);
						if (errmsg == null) {
							state.ApplyAction(move);
							Console.WriteLine(state);
						} else {
							Console.WriteLine("That move is invalid: {0}", errmsg);
							continue;
						}
					} catch {
						Console.WriteLine("Usage: move i1 j1 i2 j2 ar ac");
						continue;
					}
					try {
						Console.WriteLine("My turn");
						Console.WriteLine("I am thinking...");
						AmazonAction decision = search.MinimaxDecision(state, AmazonPlayer.BLACK, AmazonPlayer.WHITE);
						Console.WriteLine(string.Format("I made this decision: {0}", decision));
						state.ApplyAction(decision);
						Console.WriteLine(state);
						Console.WriteLine("Your turn");
					} catch (Exception e) {
						Console.WriteLine("The AI encountered an exception, the program will now close");
						Console.WriteLine(e);
						Console.WriteLine(e.StackTrace);
						break;
					}
					GC.Collect();
					GC.WaitForPendingFinalizers();
				} else {
					Console.WriteLine("Unknown command: {0}", cmd[0]);
				}
			}
			Console.WriteLine("Bye!");
		}
	}
}
