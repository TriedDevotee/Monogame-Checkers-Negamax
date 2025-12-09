using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Checkers
{
    public class Moves
    {
        public Bitboard whitePieces { get; private set; }
        public Bitboard blackPieces { get; private set; }
        public Bitboard allPieces { get; private set; }
        public Bitboard kings { get; private set; }
        private PieceMasks masks;
        public bool whiteTurn;
        public int overallScore;

        public Moves(bool turn = true)
        {
            whitePieces = new Bitboard(0x000000000055AA55);
            blackPieces = new Bitboard(0xAA55AA0000000000);

            //PROMOTION CHAIN TESTING

            //whitePieces = new Bitboard(0x0000000000002000);
            //blackPieces = new Bitboard(0x0014004000400000);

            //ANTIPROMOTION CHAIN TESTING

            //whitePieces = new Bitboard(0x0000000000008000);
            //blackPieces = new Bitboard(0x0000001400400000);

            //BRANCH CHAIN TESTING

            //whitePieces = new Bitboard(0x0000000000008000);
            //blackPieces = new Bitboard(0x0014001400400000);

            kings = new Bitboard(0);
            masks = new PieceMasks();
            allPieces = new Bitboard(whitePieces.board | blackPieces.board);
            whiteTurn = turn;

            overallScore = 0;
        }

        public void setUpPosition(ulong white, ulong black, ulong k)
        {
            whitePieces = new Bitboard(white);
            blackPieces = new Bitboard(black);
            kings = new Bitboard(k);
        }

        public moveData[] getAllMoves()
        {
            moveData[] captures = getCaptures();

            if (captures.Length == 0)
            {
                return getNormalMoves();
            }
            return captures;
        }

        public moveData[] getChainedCaptures(int startSquare)
        {

            return getCaptures(startSquare, startSquare+1);
        }

        public moveData[] getCaptures(int firstIndex = 0, int lastIndex = 64)
        {

            void tryCapture(List<moveData> moves, int startSquare, int landing, int capture, Bitboard reversePieces)
            {
                if (landing == -1 || capture == -1) return;
                if (!allPieces.isSquareUsed(landing) && reversePieces.isSquareUsed(capture))
                {
                    moves.Add(new moveData(startSquare, landing, capture));
                }
            }

            getAllPieces();

            Bitboard currentKings;
            Bitboard currentPieces;
            Bitboard reversePieces;
            Dictionary<int, int[]> currentCaptures;
            Dictionary<int, int[]> reverseCaptures;
            Dictionary<int, int[]> nextMoveMask;
            Dictionary<int, int[]> nextEnemyMoveMask;

            if (whiteTurn)
            {
                currentKings = new Bitboard(whitePieces.board & kings.board);
                currentPieces = new Bitboard(whitePieces.board);
                reversePieces = new Bitboard(blackPieces.board);
                currentCaptures = masks.WhiteCaptures;
                reverseCaptures = masks.BlackCaptures;
                nextMoveMask = masks.WhiteMasks;
                nextEnemyMoveMask = masks.BlackMasks;
            } else
            {
                currentKings = new Bitboard(blackPieces.board & kings.board);
                currentPieces = new Bitboard(blackPieces.board);
                reversePieces = new Bitboard(whitePieces.board);
                currentCaptures = masks.BlackCaptures;
                reverseCaptures = masks.WhiteCaptures;
                nextMoveMask = masks.BlackMasks;
                nextEnemyMoveMask = masks.WhiteMasks;

            }

            List<moveData> moves = new List<moveData>();

            for (int i = firstIndex; i < lastIndex; i++)
            {
                if (currentPieces.isSquareUsed(i))
                {
                    int[] availables = currentCaptures[i];
                    int[] nextMove = nextMoveMask[i];

                    tryCapture(moves, i, availables[0], nextMove[0], reversePieces);
                    tryCapture(moves, i, availables[1], nextMove[1], reversePieces);

                    if (currentKings.isSquareUsed(i))
                    {
                        //Console.WriteLine($"Found king on square {i}");

                        availables = reverseCaptures[i];
                        nextMove = nextEnemyMoveMask[i];

                        tryCapture(moves, i, availables[0], nextMove[0], reversePieces);
                        tryCapture(moves, i, availables[1], nextMove[1], reversePieces);
                    }
                }
            }

            return moves.ToArray();
        }

        public moveData[] getNormalMoves()
        {
            Bitboard currentKings;
            Bitboard currentPieces;
            Dictionary<int, int[]> currentMask;
            Dictionary<int, int[]> reverseMask;

            if (whiteTurn)
            {
                currentKings = new Bitboard(whitePieces.board & kings.board);
                currentPieces = new Bitboard(whitePieces.board);
                currentMask = masks.WhiteMasks;
                reverseMask = masks.BlackMasks;
            } else
            {
                currentKings = new Bitboard(blackPieces.board & kings.board);
                currentPieces = new Bitboard(blackPieces.board);
                currentMask = masks.BlackMasks;
                reverseMask = masks.WhiteMasks;
            }

            List<moveData> moves = new List<moveData>(); 

            getAllPieces();

            for (int i = 0; i < 64; i++)
            {
                if (currentPieces.isSquareUsed(i))
                {
                    int[] availables = currentMask[i];

                    if (!allPieces.isSquareUsed(availables[0]) && availables[0] != -1)
                    {
                        moves.Add(new moveData(i, availables[0]));
                    }

                    if (!allPieces.isSquareUsed(availables[1]) && availables[1] != -1)
                    {
                        moves.Add(new moveData(i, availables[1]));
                    }

                    if (currentKings.isSquareUsed(i))
                    {
                        availables = reverseMask[i];

                        if (!allPieces.isSquareUsed(availables[0]) && availables[0] != -1)
                        {
                            moves.Add(new moveData(i, availables[0]));
                        }

                        if (!allPieces.isSquareUsed(availables[1]) && availables[1] != -1)
                        {
                            moves.Add(new moveData(i, availables[1]));
                        }
                    }
                }
            }

            return moves.ToArray();
        }

        public Bitboard getAllPieces()
        {
            allPieces = new Bitboard(whitePieces.board | blackPieces.board);

            return allPieces;
        }
    }
}