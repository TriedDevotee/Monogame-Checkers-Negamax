using System.Collections.Generic;

namespace Checkers
{
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

        public bool isGameOver()
        {
            if (whitePieces.board == 0ul || blackPieces.board == 0ul)
            {
                return true;
            }
            return false;
        }

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
    public struct Bitboard
    {
        public ulong board { get; private set; }

        public Bitboard(ulong inputBoard = 0)
        {
            board = inputBoard;
        }

        public void setSquare(int square)
        {
            board |= (ulong)1 << square;
        }

        public void clearSquare(int square)
        {
            board &= ~((ulong)1 << square);
        }

        public bool isSquareUsed(int square)
        {
            return (((ulong)1 << square) & board) != 0;
        }
    }

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

    public struct returnData
    {
        bool isWhite;
        bool isKing;
    }
}