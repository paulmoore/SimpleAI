using System;
using System.Collections;
using System.Text;
using SimpleAI.Framework;
	
namespace AmazonGame
{
	internal enum AmazonCell : byte
	{
		FREE,
		WHITE_QUEEN,
		BLACK_QUEEN,
		ARROW
	}
	
	internal enum AmazonPlayer : byte
	{
		WHITE,
		BLACK
	}
	
	internal struct AmazonAction
	{
		public AmazonPlayer role;
		
		public int v;
		
		// queen initial row, column
		public sbyte qr, qc;
		// queen final row, column
		public sbyte qfr, qfc;
		// queen arrow row, column
		public sbyte ar, ac;
		
		public override string ToString ()
		{
			return string.Format("[AmazonAction] qr:{0}, qc:{1}, qfr:{2}, qfc:{3}, ar:{4}, ac:{5}, v:{6}, role:{7}", qr, qc, qfr, qfc, ar, ac, v, role);
		}
	}
	
	internal sealed class AmazonBuilder : ActionBuilder<AmazonAction, int>
	{
		public int PosInf
		{
			get {
				return int.MaxValue;
			}
		}
		
		public int NegInf
		{
			get {
				return int.MinValue;
			}
		}
		
		public int ExtractValue (AmazonAction action)
		{
			return action.v;
		}
		
		public AmazonAction InsertValue (AmazonAction action, int val)
		{
			action.v = val;
			return action;
		}
		
		public AmazonAction Clone (AmazonAction action)
		{
			return action;
		}
		
		public int Compare (int v0, int v1)
		{
			return v0.CompareTo(v1);
		}
	}
	
	internal sealed class AmazonState : State<AmazonAction>, MutableClone<AmazonState>
	{
		public const sbyte NUM_ROWS = 10;
		public const sbyte NUM_COLS = 10;
		
		public static readonly sbyte[,] dirs = {{1, 1}, {-1, 1}, {1, -1}, {-1, -1}, {0, 1}, {0, -1}, {1, 0}, {-1, 0}};
		
		private readonly sbyte[,] whiteQueens = new sbyte[4, 2];
		private readonly sbyte[,] blackQueens = new sbyte[4, 2];
		
		private readonly AmazonCell[,] board = new AmazonCell[NUM_ROWS, NUM_COLS];
		
		public AmazonState ()
		{	
			whiteQueens[0, 0] = 3;
			whiteQueens[0, 1] = 0;
			whiteQueens[1, 0] = 0;
			whiteQueens[1, 1] = 3;
			whiteQueens[2, 0] = 0;
			whiteQueens[2, 1] = 6;
			whiteQueens[3, 0] = 3;
			whiteQueens[3, 1] = 9;
			
			blackQueens[0, 0] = 6;
			blackQueens[0, 1] = 0;
			blackQueens[1, 0] = 9;
			blackQueens[1, 1] = 3;
			blackQueens[2, 0] = 9;
			blackQueens[2, 1] = 6;
			blackQueens[3, 0] = 6;
			blackQueens[3, 1] = 9;
			
			board[whiteQueens[0, 0], whiteQueens[0, 1]] = AmazonCell.WHITE_QUEEN;
			board[whiteQueens[1, 0], whiteQueens[1, 1]] = AmazonCell.WHITE_QUEEN;
			board[whiteQueens[2, 0], whiteQueens[2, 1]] = AmazonCell.WHITE_QUEEN;
			board[whiteQueens[3, 0], whiteQueens[3, 1]] = AmazonCell.WHITE_QUEEN;
			
			board[blackQueens[0, 0], blackQueens[0, 1]] = AmazonCell.BLACK_QUEEN;
			board[blackQueens[1, 0], blackQueens[1, 1]] = AmazonCell.BLACK_QUEEN;
			board[blackQueens[2, 0], blackQueens[2, 1]] = AmazonCell.BLACK_QUEEN;
			board[blackQueens[3, 0], blackQueens[3, 1]] = AmazonCell.BLACK_QUEEN;
		}
		
		public sbyte[,] GetQueens (AmazonPlayer player)
		{
			if (player == AmazonPlayer.WHITE) {
				return whiteQueens;
			}
			return blackQueens;
		}
		
		public bool InBounds (int row, int col)
		{
			return row >= 0 && row < NUM_ROWS && col >= 0 && col < NUM_COLS;
		}
		
		public bool IsFree (int row, int col)
		{
			return board[row, col] == AmazonCell.FREE;
		}
		
		public bool QueenAt (AmazonPlayer player, int row, int col)
		{
			return board[row, col] == (player == AmazonPlayer.WHITE ? AmazonCell.WHITE_QUEEN : AmazonCell.BLACK_QUEEN);
		}
		
		public void ApplyAction (AmazonAction action)
		{
			sbyte[,] queens;
			AmazonCell marker;
			if (action.role == AmazonPlayer.WHITE) {
				queens = whiteQueens;
				marker = AmazonCell.WHITE_QUEEN;
			} else {
				queens = blackQueens;
				marker = AmazonCell.BLACK_QUEEN;
			}
			for (int i = 0; i < queens.GetLength(0); i++) {
				// find the correct queen, and move it
				if (queens[i, 0] == action.qr && queens[i, 1] == action.qc) {
					queens[i, 0] = action.qfr;
					queens[i, 1] = action.qfc;
					break;
				}
			}
			board[action.qr, action.qc] = AmazonCell.FREE;
			board[action.qfr, action.qfc] = marker;
			// this has to be done last, since the queen can fire back on herself
			board[action.ar, action.ac] = AmazonCell.ARROW;
		}
		
		public void UndoAction (AmazonAction action)
		{
			sbyte[,] queens;
			AmazonCell marker;
			if (action.role == AmazonPlayer.WHITE) {
				queens = whiteQueens;
				marker = AmazonCell.WHITE_QUEEN;
			} else {
				queens = blackQueens;
				marker = AmazonCell.BLACK_QUEEN;
			}
			for (int i = 0; i < queens.GetLength(0); i++) {
				if (queens[i, 0] == action.qfr && queens[i, 1] == action.qfc) {
					queens[i, 0] = action.qr;
					queens[i, 1] = action.qc;
					break;
				}
			}
			board[action.qfr, action.qfc] = AmazonCell.FREE;
			board[action.ar, action.ac] = AmazonCell.FREE;
			board[action.qr, action.qc] = marker;
		}
		
		public AmazonState Clone ()
		{
			AmazonState clone = new AmazonState();
			Array.Copy(board, clone.board, board.Length);
			Array.Copy(whiteQueens, clone.whiteQueens, whiteQueens.Length);
			Array.Copy(blackQueens, clone.blackQueens, blackQueens.Length);
			return clone;
		}
		
		public override string ToString ()
		{
			// prints out a human readable representation of the board
			StringBuilder sb = new StringBuilder();
			sb.Append("  +");
			for (int j = 0; j < board.GetLength(1) * 2 - 1; j++) {
				sb.Append("-");
			}
			sb.Append("+");
			sb.AppendLine();
			for (int i = board.GetLength(0) - 1; i >= 0; i--) {
				if (i + 1 < 10) {
					sb.Append(i + 1);
					sb.Append(" ");
				} else {
					sb.Append(i + 1);
				}
				sb.Append("|");
				for (int j = 0; j < board.GetLength(1); j++) {
					switch (board[i, j]) {
						case AmazonCell.WHITE_QUEEN:
							sb.Append("W");
							break;
						case AmazonCell.BLACK_QUEEN:
							sb.Append("B");
							break;
						case AmazonCell.ARROW:
							sb.Append("A");
							break;
						case AmazonCell.FREE:
							sb.Append(" ");
							break;
					}
					sb.Append("|");
				}
				sb.AppendLine();
			}
			sb.Append("  +");
			for (int j = 0; j < board.GetLength(1) * 2 - 1; j++) {
				sb.Append("-");
			}
			sb.Append("+");
			sb.AppendLine();
			sb.Append("   ");
			for (int j = 0; j < board.GetLength(1); j++) {
				sb.Append((char)('a' + j));
				sb.Append(" ");
			}
			return sb.ToString();
		}
	}
	
	internal sealed class AmazonCutoffTest : CutoffTest<AmazonState, AmazonAction>
	{
		private readonly int maxSearchTime;
		
		private int startTime;
		
		public AmazonCutoffTest (int maxSearchTime)
		{
			this.maxSearchTime = maxSearchTime;
		}
		
		public void Begin ()
		{
			startTime = Environment.TickCount;
			Console.WriteLine(string.Format("CT Beginning at {0}s", startTime / 1000));
		}
		
		public void End ()
		{
			Console.WriteLine(string.Format("CT Search lasted {0}s", (Environment.TickCount - startTime) / 1000));
		}
		
		public bool Test (AmazonState state)
		{
			// really all we are concerned about is time, we don't have to worry about memory since it is mostly allocated up front
			return Environment.TickCount - startTime > maxSearchTime;
		}
	}
	
	internal sealed class AmazonSuccessorFunction : SuccessorFunction<AmazonState, AmazonAction, byte, AmazonPlayer>
	{
		public void Partition (AmazonState state, AmazonPlayer player, Action<byte> partitioner)
		{
			// partitioning in this case is simple, each queen is one partition
			partitioner(0);
			partitioner(1);
			partitioner(2);
			partitioner(3);
		}
		
		public void Expand (AmazonState state, AmazonPlayer player, byte partition, Func<AmazonAction, bool> expander)
		{
			// take our queen, and find all possible moves she can make
			sbyte[,] queens = state.GetQueens(player);
			sbyte[,] dirs = AmazonState.dirs;
			for (int i = 0; i < dirs.GetLength(0); i++) {
				if (!CalculateQueenMoves(state, player, expander, queens[partition, 0], queens[partition, 1], dirs[i, 0], dirs[i, 1])) {
					return;
				}
			}
		}
		
		private bool CalculateQueenMoves (AmazonState state, AmazonPlayer player, Func<AmazonAction, bool> expander, sbyte qr, sbyte qc, sbyte dr, sbyte dc)
		{
			sbyte[,] dirs = AmazonState.dirs;
			sbyte row = (sbyte)(qr + dr), col = (sbyte)(qc + dc);
			while (state.InBounds(row, col) && state.IsFree(row, col)) {
				for (int i = 0; i < dirs.GetLength(0); i++) {
					if (!CalculateArrowShots(state, player, expander, qr, qc, row, col, dirs[i, 0], dirs[i, 1])) {
						return false;
					}
				}
				row += dr;
				col += dc;
			}
			return true;
		}
		
		private bool CalculateArrowShots (AmazonState state, AmazonPlayer player, Func<AmazonAction, bool> expander, sbyte qr, sbyte qc, sbyte qfr, sbyte qfc, sbyte dr, sbyte dc)
		{
			sbyte row = (sbyte)(qfr + dr), col = (sbyte)(qfc + dc);
			while (state.InBounds(row, col) && (state.IsFree(row, col) || (row == qr && col == qc))) {
				AmazonAction action = new AmazonAction();
				action.role = player;
				action.qr = qr;
				action.qc = qc;
				action.qfr = qfr;
				action.qfc = qfc;
				action.ar = row;
				action.ac = col;
				if (!expander(action)) {
					return false;
				}
				row += dr;
				col += dc;
			}
			return true;
		}
	}
	
	/// <summary>
	/// An Amazon evaluation function which values territory.
	/// </summary>
	/// <remarks>
	/// In this evaluation function, territory is measured by how much more easily we can 'hop' to a square as opposed to our enemy.
	/// The break down is as follows:
	/// * If we can get to a square in at most one hop, and the enemy cannot reach the square in at most two hops, we get 3 points.
	/// * If we can get to a square in at most one hop, and the enemy cannot reach the square in at most one hop, we get 2 points.
	/// * If we can get to a square in at most two hops, and the enemy cannot reach the square in at most two hops, we get 1 point.
	/// The same score for the enemy is negated from our score.
	/// </remarks>
	internal sealed class AmazonEvaluationFunction : EvaluationFunction<AmazonState, AmazonAction, int, AmazonPlayer>
	{
		private readonly BitArray ourOneHop, ourTwoHop, oppOneHop, oppTwoHop;
		
		private AmazonPlayer player;
		
		public AmazonEvaluationFunction ()
		{
			ourOneHop = new BitArray(AmazonState.NUM_ROWS * AmazonState.NUM_COLS);
			ourTwoHop = new BitArray(AmazonState.NUM_ROWS * AmazonState.NUM_COLS);
			oppOneHop = new BitArray(AmazonState.NUM_ROWS * AmazonState.NUM_COLS);
			oppTwoHop = new BitArray(AmazonState.NUM_ROWS * AmazonState.NUM_COLS);
		}
		
		public int Evaluate (AmazonState state, AmazonPlayer player)
		{
			this.player = player;
			CalculateHops(state, player);
			CalculateHops(state, player == AmazonPlayer.WHITE ? AmazonPlayer.BLACK : AmazonPlayer.WHITE);
			int evaluation = 0;
			for (int i = 0; i < AmazonState.NUM_ROWS * AmazonState.NUM_COLS; i++) {
				// calculate our score based on the owned territory
				if (ourOneHop[i] && !oppTwoHop[i]) {
					evaluation += 3;
				} else if (ourOneHop[i] && !oppOneHop[i]) {
					evaluation += 2;
				} else if (ourTwoHop[i] && !oppTwoHop[i]) {
					evaluation += 1;
				}
				// do the same evaluation for the enemy, but negate it from our score
				if (oppOneHop[i] && !ourTwoHop[i]) {
					evaluation -= 4;
				} else if (oppOneHop[i] && !ourOneHop[i]) {
					evaluation -= 2;
				} else if (oppTwoHop[i] && !ourTwoHop[i]) {
					evaluation -= 1;
				}
			}
			this.player = default(AmazonPlayer);
			ourOneHop.SetAll(false);
			ourTwoHop.SetAll(false);
			oppOneHop.SetAll(false);
			oppTwoHop.SetAll(false);
			return evaluation;
		}
		
		private void CalculateHops (AmazonState state, AmazonPlayer player)
		{
			BitArray oneHop, twoHop;
			if (player == this.player) {
				oneHop = ourOneHop;
				twoHop = ourTwoHop;
			} else {
				oneHop = oppOneHop;
				twoHop = oppTwoHop;
			}
			sbyte[,] dirs = AmazonState.dirs;
			sbyte[,] queens = state.GetQueens(player);
			for (sbyte q = 0; q < queens.GetLength(0); q++) {
				for (sbyte d = 0; d < dirs.GetLength(0); d++) {
					int r = queens[q, 0] + dirs[d, 0];
					int c = queens[q, 1] + dirs[d, 1];
					while (state.InBounds(r, c) && state.IsFree(r, c)) {
						oneHop.Set(r * AmazonState.NUM_ROWS + c, true);
						for (sbyte d2 = 0; d2 < dirs.GetLength(0); d2++) {
							int r2 = r + dirs[d2, 0];
							int c2 = c + dirs[d2, 1];
							while (state.InBounds(r2, c2) && state.IsFree(r2, c2)) {
								twoHop.Set(r2 * AmazonState.NUM_ROWS + c2, true);
								r2 += dirs[d2, 0];
								c2 += dirs[d2, 1];
							}
						}
						r += dirs[d, 0];
						c += dirs[d, 1];
					}
				}
			}
		}
		
		public EvaluationFunction<AmazonState, AmazonAction, int, AmazonPlayer> Clone ()
		{
			var clone = new AmazonEvaluationFunction();
			clone.ourOneHop.Or(ourOneHop);
			clone.ourTwoHop.Or(ourTwoHop);
			clone.oppOneHop.Or(oppOneHop);
			clone.oppTwoHop.Or(oppTwoHop);
			clone.player = player;
			return clone;
		}
	}
	
	internal sealed class AmazonMoveValidator
	{
		public static string Validate (AmazonState state, AmazonAction move)
		{
			// check the opponent has a queen at the starting position
			if (state.QueenAt(move.role, move.qr, move.qc)) {
				// validate the queen's dr
				sbyte dr, dc;
				if (move.qfr < move.qr) {
					dr = -1;
				} else if (move.qfr > move.qr) {
					dr = 1;
				} else {
					dr = 0;
				}
				// validate the queen's dc
				if (move.qfc < move.qc) {
					dc = -1;
				} else if (move.qfc > move.qc) {
					dc = 1;
				} else {
					dc = 0;
				}
				// walk to the queen's final position
				if (dr != 0 || dc != 0) {
					sbyte qr = (sbyte)(move.qr + dr), qc = (sbyte)(move.qc + dc);
					while (state.InBounds(qr, qc) && state.IsFree(qr, qc)) {
						if (qr == move.qfr && qc == move.qfc) {
							// validate the arrow's dr
							if (move.ar < move.qfr) {
								dr = -1;
							} else if (move.ar > move.qfr) {
								dr = 1;
							} else {
								dr = 0;
							}
							// validate the arrow's dc
							if (move.ac < move.qfc) {
								dc = -1;
							} else if (move.ac > move.qfc) {
								dc = 1;
							} else {
								dc = 0;
							}
							if (dr != 0 || dc != 0) {
								// walk to the arrow's final position
								sbyte ar = (sbyte)(move.qfr + dr), ac = (sbyte)(move.qfc + dc);
								while (state.InBounds(ar, ac) && (state.IsFree(ar, ac) || (ar == move.qr && ac == move.qc))) {
									if (ar == move.ar && ac == move.ac) {
										// all conditions are met
										return null;
									}
									ar += dr;
									ac += dc;
								}
								return "The arrow's path is not clear!";
							}
							return "Invalid arrow direction!";
						}
						qr += dr;
						qc += dc;
					}
					return "The queen's path is not clear!";
				}
				return "Invalid queen direction!";
			}
			return "No queen found at starting location!";
		}
	}
}
