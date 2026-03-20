using System.Collections.Generic;

namespace Checkers
{
    /// <summary>
    /// Simpler implementation of Moves, just allowing for positions to be easily stored and modified without the bloat of Moves
    /// </summary>
    public struct Position
    {
        public Bitboard whitePieces;
        public Bitboard blackPieces;
        public Bitboard kings;

        public Position(ulong w, ulong b, ulong k)
        {
            whitePieces = new Bitboard(w);
            blackPieces = new Bitboard(b);
            kings = new Bitboard(k);
        }

        /// <summary>
        /// Detects if game has ended
        /// </summary>
        /// <returns></returns>
        public bool isGameOver()
        {
            if (whitePieces.board == 0ul || blackPieces.board == 0ul)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Makes a move local to the position (modifies the state)
        /// </summary>
        /// <param name="move"></param>
        /// <param name="whiteTurn"></param>
        public void makePositionalMove(moveData move, bool whiteTurn)
        {
            if (whiteTurn)
            {
                bool isKingMove = whitePieces.isSquareUsed(move.start) && kings.isSquareUsed(move.start);

                whitePieces.setSquare(move.moveTo);
                whitePieces.clearSquare(move.start);
                blackPieces.clearSquare(move.captureSquare);

                if (isKingMove)
                {
                    kings.clearSquare(move.start);
                    kings.setSquare(move.moveTo);
                }

                if (move.moveTo >= 56)
                {
                    kings.setSquare(move.moveTo);
                }
            } else
            {
                bool isKingMove = kings.isSquareUsed(move.start) && blackPieces.isSquareUsed(move.start);

                blackPieces.setSquare(move.moveTo);
                blackPieces.clearSquare(move.start);
                whitePieces.clearSquare(move.captureSquare);

                if (isKingMove)
                {
                    kings.clearSquare(move.start);
                    kings.setSquare(move.moveTo);
                }

                if (move.moveTo <= 7)
                {
                    kings.setSquare(move.moveTo);
                }
            }
        }
    }

    /// <summary>
    /// Simple wrapper for the bitboards, to allow for passing between methods, without memory inefficiencies with tuples
    /// </summary>
    public struct bitboardWrapper
    {
        public Bitboard white;
        public Bitboard black;
        public Bitboard kings;

        public bitboardWrapper(Bitboard w, Bitboard b, Bitboard k)
        {
            white = w;
            black = b;
            kings = k;
        }
    }

    /// <summary>
    /// Stores information about a move, such as the start square, finishing square and if any squares are captured.
    /// Note: captureSquare will be -1 if no captures are made
    /// </summary>
    public struct moveData
    {
        public int start;
        public int moveTo;
        public int captureSquare;

        public moveData(int s, int m, int c = -1)
        {
            start = s;
            moveTo = m;
            captureSquare = c;
        }
    }

    /// <summary>
    /// Stores a certain characteristic about the board, such as kings or white pieces. Stored in a ulong with helper methods
    /// </summary>
    public struct Bitboard
    {
        public ulong board { get; private set; }
        public Bitboard(ulong inputBoard = 0)
        {
            board = inputBoard;
        }

        /// <summary>
        /// Sets a specific square in the board
        /// </summary>
        /// <param name="square"></param>
        public void setSquare(int square)
        {
            board |= (ulong)1 << square;
        }

        /// <summary>
        /// Clears a specific square in the board
        /// </summary>
        /// <param name="square"></param>
        public void clearSquare(int square)
        {
            board &= ~((ulong)1 << square);
        }

        /// <summary>
        /// Detects if a square is used in the board
        /// </summary>
        /// <param name="square"></param>
        /// <returns></returns>
        public bool isSquareUsed(int square)
        {
            return (((ulong)1 << square) & board) != 0;
        }
    }

    /// <summary>
    /// Stores information in the chainTree
    /// </summary>
    public struct ChainNode
    {
        public moveData move;
        public List<ChainNode> children;

        public ChainNode(moveData m)
        {
            move = m;
            children = [];
        }
    }

    /// <summary>
    /// Stores basic data about the contents of a certain piece
    /// </summary>
    public struct Piece
    {
        public bool isFull;
        public bool isWhite;
        public bool isKing;
    }
}