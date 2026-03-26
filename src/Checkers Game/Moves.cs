using System.Collections.Generic;

namespace Checkers
{
    /// <summary>
    /// Main move handler and board storage class. Stores the BitBoards, and handles all possible moves.
    /// </summary>
    public class Moves
    {
        public Bitboard WhitePieces { get; private set; }
        public Bitboard BlackPieces { get; private set; }
        public Bitboard AllPieces { get; private set; }
        public Bitboard Kings { get; private set; }
        public bool WhiteTurn {get; private set;}

        private PieceMasks masks;
        private const ulong whiteStart = 0x000000000055AA55;
        private const ulong blackStart = 0xAA55AA0000000000;
        private const ulong kingstart = 0; 

        public Moves(bool turn)
        {
            WhitePieces = new Bitboard(whiteStart);
            BlackPieces = new Bitboard(blackStart);
            Kings = new Bitboard(kingstart);

            //WhitePieces = new Bitboard(0x0000000000000040);
            //BlackPieces = new Bitboard(0x0000000008000000);

            masks = new PieceMasks();
            AllPieces = new Bitboard(WhitePieces.board | BlackPieces.board);
            WhiteTurn = turn;
        }

        /// <summary>
        /// Basic method that takes in a set of ulongs, and translates them into BitBoards, which are stored in the class implementation
        /// </summary>
        /// <param name="white"></param>
        /// <param name="black"></param>
        /// <param name="k"></param>
        public void SetUpPosition(ulong white, ulong black, ulong k)
        {
            WhitePieces = new Bitboard(white);
            BlackPieces = new Bitboard(black);
            Kings = new Bitboard(k);
        }

        /// <summary>
        /// Method that returns all possible moves in a position. Also does capture filtering, by only looking for moves if no captures are found
        /// </summary>
        /// <returns></returns>
        public moveData[] GetAllMoves()
        {
            moveData[] captures = GetCaptures();

            if (captures.Length == 0)
            {
                return GetNormalMoves();
            }
            return captures;
        }

        /// <summary>
        /// Finds all the captures in a position. Can be filtered by indices on what to search for
        /// </summary>
        /// <param name="firstIndex"></param>
        /// <param name="lastIndex"></param>
        /// <returns></returns>
        public moveData[] GetCaptures(int firstIndex = 0, int lastIndex = 64)
        {

            void TryCapture(List<moveData> moves, int startSquare, int landing, int capture, Bitboard reversePieces)
            {
                if (landing == -1 || capture == -1) return;
                if (!AllPieces.isSquareUsed(landing) && reversePieces.isSquareUsed(capture))
                {
                    moves.Add(new moveData(startSquare, landing, capture));
                }
            }

            GetAllPieces();

            Bitboard currentKings;
            Bitboard currentPieces;
            Bitboard reversePieces;
            Dictionary<int, int[]> currentCaptures;
            Dictionary<int, int[]> reverseCaptures;
            Dictionary<int, int[]> nextMoveMask;
            Dictionary<int, int[]> nextEnemyMoveMask;

            if (WhiteTurn)
            {
                currentKings = new Bitboard(WhitePieces.board & Kings.board);
                currentPieces = new Bitboard(WhitePieces.board);
                reversePieces = new Bitboard(BlackPieces.board);
                currentCaptures = masks.WhiteCaptures;
                reverseCaptures = masks.BlackCaptures;
                nextMoveMask = masks.WhiteMasks;
                nextEnemyMoveMask = masks.BlackMasks;
            } else
            {
                currentKings = new Bitboard(BlackPieces.board & Kings.board);
                currentPieces = new Bitboard(BlackPieces.board);
                reversePieces = new Bitboard(WhitePieces.board);
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

                    TryCapture(moves, i, availables[0], nextMove[0], reversePieces);
                    TryCapture(moves, i, availables[1], nextMove[1], reversePieces);

                    if (currentKings.isSquareUsed(i))
                    {
                        availables = reverseCaptures[i];
                        nextMove = nextEnemyMoveMask[i];

                        TryCapture(moves, i, availables[0], nextMove[0], reversePieces);
                        TryCapture(moves, i, availables[1], nextMove[1], reversePieces);
                    }
                }
            }

            return moves.ToArray();
        }

        /// <summary>
        /// Finds all non-capture moves in a position
        /// </summary>
        /// <returns></returns>
        public moveData[] GetNormalMoves()
        {
            Bitboard currentKings;
            Bitboard currentPieces;
            Dictionary<int, int[]> currentMask;
            Dictionary<int, int[]> reverseMask;

            if (WhiteTurn)
            {
                currentKings = new Bitboard(WhitePieces.board & Kings.board);
                currentPieces = new Bitboard(WhitePieces.board);
                currentMask = masks.WhiteMasks;
                reverseMask = masks.BlackMasks;
            } else
            {
                currentKings = new Bitboard(BlackPieces.board & Kings.board);
                currentPieces = new Bitboard(BlackPieces.board);
                currentMask = masks.BlackMasks;
                reverseMask = masks.WhiteMasks;
            }

            List<moveData> moves = new List<moveData>(); 

            GetAllPieces();

            for (int i = 0; i < 64; i++)
            {
                if (currentPieces.isSquareUsed(i))
                {
                    int[] availables = currentMask[i];

                    if (!AllPieces.isSquareUsed(availables[0]) && availables[0] != -1)
                    {
                        moves.Add(new moveData(i, availables[0]));
                    }

                    if (!AllPieces.isSquareUsed(availables[1]) && availables[1] != -1)
                    {
                        moves.Add(new moveData(i, availables[1]));
                    }

                    if (currentKings.isSquareUsed(i))
                    {
                        availables = reverseMask[i];

                        if (!AllPieces.isSquareUsed(availables[0]) && availables[0] != -1)
                        {
                            moves.Add(new moveData(i, availables[0]));
                        }

                        if (!AllPieces.isSquareUsed(availables[1]) && availables[1] != -1)
                        {
                            moves.Add(new moveData(i, availables[1]));
                        }
                    }
                }
            }

            return moves.ToArray();
        }

        /// <summary>
        /// Returns the combined white and black Bitboards in a new Bitboard
        /// </summary>
        /// <returns></returns>
        public Bitboard GetAllPieces()
        {
            AllPieces = new Bitboard(WhitePieces.board | BlackPieces.board);

            return AllPieces;
        }

        /// <summary>
        /// Toggles the variable which tracks whose turn it is (locally stored in class implementation)
        /// </summary>
        public void ToggleTurn()
        {
            WhiteTurn = !WhiteTurn;
        }
    }
}