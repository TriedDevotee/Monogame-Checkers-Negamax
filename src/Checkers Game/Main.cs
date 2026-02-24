using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Comp_Sci_NEA;

namespace Checkers
{   
    public class Main
    {
        public Moves moves;
        public bool wasLastMoveValid;
        public bool waitingForBranchInput;
        public List<Position> previousPositions;
        public Piece[][] displayBoard;

        public Main()
        {
            moves = new Moves(true);
            wasLastMoveValid = true;
            previousPositions = [new Position(moves.WhitePieces.board, moves.BlackPieces.board, moves.Kings.board)];

            displayBoard = new Piece[8][];

            for (int i = 0; i < 8; i++)
            {
                displayBoard[i] = new Piece[8];
            }
        }

        public void displayPosition(Position posToDisplay)
        {
            int boardHeight = 8;
            int boardWidth = 8;

            for (int h = 0; h < boardHeight; h++)
            {
                for (int w = 0; w < boardWidth; w++)
                {
                    int square = 63 - ((h * 8) + w);

                    Piece newPiece = new Piece();

                    newPiece.isFull = true;
                    newPiece.isKing = false;

                    if (posToDisplay.whitePieces.isSquareUsed(square))
                    {
                        newPiece.isWhite = true;
                    }

                    else if (posToDisplay.blackPieces.isSquareUsed(square))
                    {
                        newPiece.isWhite = false;
                    }

                    else
                    {
                        newPiece.isFull = false;
                    }

                    if (posToDisplay.kings.isSquareUsed(square))
                    {
                        newPiece.isKing = true;
                    }

                    displayBoard[h][w] = newPiece;
                }
            }
        }

        public int searchInMoves(moveData moveCode, moveData[] moves)
        {
            for (int i = 0; i < moves.Length; i++)
            {
                if (moves[i].start == moveCode.start && moves[i].moveTo == moveCode.moveTo)
                {
                    return i;
                }
            }
            return -1;
        }

        
        public bitboardWrapper moveMaker(Bitboard pieces, Bitboard enemies, Bitboard kings, moveData move, bool whiteTurn)
        {
            //Console.WriteLine($"Making move {move.start} to {move.moveTo} taking {move.captureSquare}");

            pieces.clearSquare(move.start);
            pieces.setSquare(move.moveTo);

            if (move.captureSquare != -1)
            {
                enemies.clearSquare(move.captureSquare);
                kings.clearSquare(move.captureSquare);
            }

            if (move.moveTo / 8 == 7 || move.moveTo / 8 == 0)
            {
                //Console.WriteLine($"Made a new King at {move.moveTo}");

                kings.setSquare(move.moveTo);
            }

            if (kings.isSquareUsed(move.start))
            {
                kings.clearSquare(move.start);
                kings.setSquare(move.moveTo);
            }

            if (whiteTurn)
            {
                return new bitboardWrapper(pieces, enemies, kings);   
            } else
            {
                return new bitboardWrapper(enemies, pieces, kings);
            }
        }

        public bool MakeHumanMove(moveCache inputMove)
        {
            moveData[] allPossibleMoves = moves.GetAllMoves();
            int foundAtIndex = searchInMoves(new moveData(inputMove.start, inputMove.moveTo), allPossibleMoves);
            
            if (foundAtIndex != -1)
            {
                moveData actualMove = allPossibleMoves[foundAtIndex];

                if (actualMove.captureSquare == -1){
                    waitingForBranchInput = false;

                    bitboardWrapper wrapper;
                    if (moves.WhiteTurn){
                        wrapper = moveMaker(moves.WhitePieces, moves.BlackPieces, moves.Kings, actualMove, moves.WhiteTurn);
                    } else {
                        wrapper = moveMaker(moves.BlackPieces, moves.WhitePieces, moves.Kings, actualMove, moves.WhiteTurn);
                    }

                    moves.SetUpPosition(wrapper.white.board, wrapper.black.board, wrapper.kings.board);

                    moves.ToggleTurn();

                    return true;
                }
                else
                {

                    Position currentPosition = new Position(moves.WhitePieces.board, moves.BlackPieces.board, moves.Kings.board);

                    ChainedCaptures(currentPosition, actualMove);

                    if (!waitingForBranchInput) moves.ToggleTurn();

                    return true;

                }
            } 
            else
            {
                return false;
            }
        }

        public void ChainedCaptures(Position currentPosition, moveData actualMove)
        {
            ChainTree chains = new ChainTree(currentPosition, moves.WhiteTurn);

            ChainNode node = new ChainNode();

            foreach (ChainNode root in chains.CaptureTree)
            {
                if (root.move.start == actualMove.start &&
                    root.move.moveTo == actualMove.moveTo)
                {
                    node = root;
                }
            }
            
            List<List<ChainNode>> allPaths = chainTraverser(node);

            List<moveData> chosenPath = buildPathingTree(allPaths);

            //Console.WriteLine($"Number of moves to make: {chosenPath.Count}");

            for (int i = 0; i < chosenPath.Count; i++)
            {
                //Console.WriteLine($"Applying move {chosenPath[i].start} to {chosenPath[i].moveTo}");

                bitboardWrapper wrapper;

                if (moves.WhiteTurn){
                    wrapper = moveMaker(moves.WhitePieces, moves.BlackPieces, moves.Kings, chosenPath[i], moves.WhiteTurn);
                } else {
                    wrapper = moveMaker(moves.BlackPieces, moves.WhitePieces, moves.Kings, chosenPath[i], moves.WhiteTurn);
                }

                moves.SetUpPosition(wrapper.white.board, wrapper.black.board, wrapper.kings.board);
            }

        }

        public List<moveData> buildPathingTree(List<List<ChainNode>> Paths, bool isBot = false, int depthRemaining = 0, bool whiteTurn = true)
        {
            //Console.WriteLine($"Pathing function called.");

            int generation = 0;
            bool remainingNodes = true;

            List<moveData> finalPath = [];

            while (remainingNodes)
            {

                remainingNodes = false;

                List<moveData> Deviants = [];

                for (int i = 0; i < Paths.Count; i++)
                {
                    if (Paths[i].Count > generation)
                    {
                        remainingNodes = true;

                        if (Deviants.IndexOf(Paths[i][generation].move) == -1)
                        {
                            Deviants.Add(Paths[i][generation].move);
                        }
                    }

                }

                if (Deviants.Count == 1)
                {
                    finalPath.Add(Deviants[0]);
                } else if (Deviants.Count > 1)
                {
                    waitingForBranchInput = true;

                    return finalPath;
                }

                generation++;
            }

            waitingForBranchInput = false;

            return finalPath;
        }
        public void runForAI(Moves moves)
        {
            NegamaxHandler negamax = new NegamaxHandler(moves.WhitePieces, moves.BlackPieces, moves.Kings, moves.WhiteTurn);

            moveData bestMove = negamax.BestMove;

            bitboardWrapper wrapper;


            if (bestMove.captureSquare != -1)
            {
                Position currentPosition = new Position(moves.WhitePieces.board, moves.BlackPieces.board, moves.Kings.board);

                ChainedCaptures(currentPosition, bestMove);

                if (!waitingForBranchInput)
                    moves.ToggleTurn();

                return;

            }

            if (moves.WhiteTurn)
            {
                wrapper = moveMaker(moves.WhitePieces, moves.BlackPieces, moves.Kings, bestMove, moves.WhiteTurn);
            } else
            {
                wrapper = moveMaker(moves.BlackPieces, moves.WhitePieces, moves.Kings, bestMove, moves.WhiteTurn);
            }

            moves.SetUpPosition(wrapper.white.board, wrapper.black.board, wrapper.kings.board);

            moves.ToggleTurn();
        }

        static List<List<ChainNode>> chainTraverser(ChainNode startPoint)
        {
            List<List<ChainNode>> completedPaths = [];
            Queue<List<ChainNode>> pathQueue = [];

            pathQueue.Enqueue([startPoint]);

            while (pathQueue.Count > 0)
            {
                List<ChainNode> currentPath = pathQueue.Dequeue();
                ChainNode lastNode = currentPath.Last();

                if (lastNode.children.Count == 0)
                {
                    completedPaths.Add(currentPath);
                } else
                {
                    currentPath.Add(lastNode.children[0]);
                    pathQueue.Enqueue(currentPath);

                    for (int i = 1; i < lastNode.children.Count; i++)
                    {
                        List<ChainNode> newPath = deepClone(currentPath, currentPath.Count - 1);
                        newPath.Add(lastNode.children[i]);
                        pathQueue.Enqueue(newPath);
                    }
                }
            }

            return completedPaths;
        }

        static List<ChainNode> deepClone(List<ChainNode> cloning, int maxIndex)
        {
            List<ChainNode> cloned = [];

            for (int i = 0; i < maxIndex; i++)
            {
                cloned.Add(cloning[i]);
            }

            return cloned;
        }

        public int checkForGameOver()
        {
            // 0 - No gameOver
            // 1 - White Wins
            // 2 - Black Wins
            // 3 - Draw

            static int countPieces(ulong board)
            {
                int numPieces = 0;

                for (int i = 0; i < 64; i++)
                {
                    if ((board & ((ulong) 1 << i)) != 0) numPieces++;
                }

                return numPieces;
            }

            moveData[] availableMoves = moves.GetAllMoves();

            int whitePieces = countPieces(moves.WhitePieces.board);
            int blackPieces = countPieces(moves.BlackPieces.board);

            if (whitePieces == 0)
            {
                return 2;
            } else if (blackPieces == 0)
            {
                return 1;
            } else if (availableMoves.Length == 0)
            {
                return 3;
            } else
            {
                return 0;
            }
        }
    }
}