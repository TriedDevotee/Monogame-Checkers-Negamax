using System;
using System.Numerics;

namespace Checkers
{
    /// <summary>
    /// Class that handles the AI decision-making. Contains the recursive Negamax Algorithm, and quick helper functions
    /// </summary>
    class NegamaxHandler
    {
        private readonly bool whiteTurn;
        public moveData BestMove {get; private set;}

        private int depthLimit;
        private const int losingScore = 1000000000;
        private const float mobilityWeighting = 0.1f; 

        public NegamaxHandler(Bitboard whitePieces, Bitboard blackPieces, Bitboard kings, bool turn, int depth)
        {
            Position gamePos = new Position(
                whitePieces.board,
                blackPieces.board,
                kings.board
            );

            whiteTurn = turn;
            depthLimit = depth;

            BestMove = GetBestMove(gamePos, whiteTurn);
        }

        /// <summary>
        /// Counts the number of pieces, or bits which are 1 in the 64 bit integer "board" and returns that value. 
        /// Uses System.Numerics.BitOperations.PopCount() for a more efficient runtime due to high volume of function calls in the recursive phase.
        /// </summary>
        /// <param name="board"></param>
        /// <returns></returns>

        private int countPieces(ulong board)
        {
            return BitOperations.PopCount(board);
        }

        /// <summary>
        /// Basic evaluation function. Counts pieces, kings and available positions from the current position. 
        /// Most likely the limiting factor in the bots performance, with very simplistic heuristics in play.
        /// Evaluation = 2 x delta pieces + delta kings + (available positions x 0.1)
        /// </summary>
        /// <param name="board"></param>
        /// <param name="whiteTurn"></param>
        /// <returns></returns>
        private float evaluation(Position board, bool whiteTurn)
        {
            int whitePieces = countPieces(board.whitePieces.board);
            int blackPieces = countPieces(board.blackPieces.board);
            
            int whiteKings = countPieces(board.kings.board & board.whitePieces.board);
            int blackKings = countPieces(board.kings.board & board.blackPieces.board);

            float eval = 2 * (whitePieces - blackPieces) + (whiteKings - blackKings);

            Moves positions = new(whiteTurn);

            positions.SetUpPosition(board.whitePieces.board, board.blackPieces.board, board.kings.board);

            eval += positions.GetAllMoves().Length * mobilityWeighting;

            return whiteTurn? eval : -eval; 
        }

        /// <summary>
        /// Recursive component of the Negamax Algorithm. Called from GetBestMove()
        /// </summary>
        /// <param name="depth"></param>
        /// <param name="board"></param>
        /// <param name="whiteTurn"></param>
        /// <param name="alpha"></param>
        /// <param name="beta"></param>
        /// <returns></returns>

        private float Negamax(int depth, Position board, bool whiteTurn, float alpha, float beta)
        {
            if (depth == 0) return evaluation(board, whiteTurn);
            if (board.isGameOver()) return -losingScore;

            Moves moves = new Moves(whiteTurn);
            moves.SetUpPosition(board.whitePieces.board, board.blackPieces.board, board.kings.board);

            moveData[] possibleMoves = moves.GetAllMoves();

            if (possibleMoves.Length == 0) return 0;

            float bestScore = float.NegativeInfinity;

            foreach (moveData move in possibleMoves)
            {
            
                Position newPos = new Position(board.whitePieces.board, board.blackPieces.board, board.kings.board);

                newPos.makePositionalMove(move, whiteTurn);

                float score = -Negamax(depth - 1, newPos, !whiteTurn, -alpha, -beta);

                if (score > bestScore)
                {
                    bestScore = score;
                }

                if (score > alpha)
                {
                    alpha = score;
                }

                if (alpha >= beta)
                {
                    break;
                }

            }

            return bestScore;
        } 

        /// <summary>
        /// Best Move generator. Called from constructor. 
        /// Takes in the current position and the bots turn, and outputs a move. 
        /// Applies Alpha-Beta pruning.
        /// </summary>
        /// <param name="board"></param>
        /// <param name="whiteTurn"></param>
        /// <returns></returns>
        private moveData GetBestMove(Position board, bool whiteTurn)
        {

            Moves moves = new(whiteTurn);

            moves.SetUpPosition(board.whitePieces.board, board.blackPieces.board, board.kings.board);

            moveData[] possibleMoves = moves.GetAllMoves();

            float bestScore = int.MinValue;

            moveData bestMove = new();


            foreach (moveData move in possibleMoves)
            {
            
                Position newPos = new(board.whitePieces.board, board.blackPieces.board, board.kings.board);

                newPos.makePositionalMove(move, whiteTurn);

                float score = -Negamax(depth: depthLimit, newPos, !whiteTurn, float.NegativeInfinity, float.PositiveInfinity);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;

                }
            }

            return bestMove;
        }
    }
}