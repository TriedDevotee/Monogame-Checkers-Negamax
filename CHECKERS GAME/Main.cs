using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

        public Main()
        {
            moves = new Moves();
            wasLastMoveValid = true;
        }

        public void MoveHandler(moveCache currentMove)
        {
            if (currentMove.start == -1 || currentMove.moveTo == -1) return;
            if (moves.whiteTurn)
            {
                makeHumanMove(currentMove); 
            } else
            {
                runForAI(moves);
            }
        }

        public void displayBoard(ulong whiteBoard, ulong blackBoard, ulong kings)
        {
            string whiteDisplay = Convert.ToString((long) whiteBoard, 2);
            string blackDisplay = Convert.ToString((long) blackBoard, 2);
            string kingsDisplay = Convert.ToString((long) kings, 2);

            while (whiteDisplay.Length < 64)
            {
                whiteDisplay = '0' + whiteDisplay;
            }
            while (blackDisplay.Length < 64)
            {
                blackDisplay = '0' + blackDisplay;
            }
            while (kingsDisplay.Length < 64)
            {
                kingsDisplay = '0' + kingsDisplay;
            }

            for (int i = 0; i < whiteDisplay.Length ; i++)
            {
                if (i % 8 == 0) Console.WriteLine();

                if (whiteDisplay[i] == '0' && blackDisplay[i] == '0')
                {
                    Console.Write("0");
                } 
                else if (whiteDisplay[i] != '0' && kingsDisplay[i] != '0')
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write("K");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else if (blackDisplay[i] != '0' && kingsDisplay[i] != '0')
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("K");
                    Console.ForegroundColor = ConsoleColor.White;
                }
                else if (whiteDisplay[i] != '0')
                {
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write("X");
                    Console.ForegroundColor = ConsoleColor.White;
                }  
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("X");
                    Console.ForegroundColor = ConsoleColor.White;
                }
            }

            Console.WriteLine();
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

        public moveData inputHandler()
        {
            string pattern = "[0-9]+";
            string userInput;

            Console.Write("Enter an input square:\n>>>  ");
            userInput = Console.ReadLine();

            while (!Regex.IsMatch(userInput, pattern))
            {
                Console.Write("Try again. Enter an input square:\n>>>  ");
                userInput = Console.ReadLine();
            }
            int start = Convert.ToInt32(userInput);

            Console.Write("Enter a square to move too:\n>>>  ");
            userInput = Console.ReadLine();

            while (!Regex.IsMatch(userInput, pattern))
            {
                Console.Write("Try again. Enter a square to move too:\n>>>  ");
                userInput = Console.ReadLine();
            }
            
            int moveTo = Convert.ToInt32(userInput);

            return new moveData(start, moveTo);
        }

        public bitboardWrapper moveMaker(Bitboard pieces, Bitboard enemies, Bitboard kings, moveData move, bool whiteTurn)
        {
            Console.WriteLine($"Making move {move.start} to {move.moveTo} taking {move.captureSquare}");

            pieces.clearSquare(move.start);
            pieces.setSquare(move.moveTo);

            if (move.captureSquare != -1)
            {
                enemies.clearSquare(move.captureSquare);
                kings.clearSquare(move.captureSquare);
            }

            if (move.moveTo / 8 == 7 || move.moveTo / 8 == 0)
            {
                Console.WriteLine($"Made a new King at {move.moveTo}");

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

        public void makeHumanMove(moveCache inputMove)
        {
            moveData[] allPossibleMoves = moves.getAllMoves();
            int foundAtIndex = searchInMoves(new moveData(inputMove.start, inputMove.moveTo), allPossibleMoves);
            
            if (foundAtIndex != -1)
            {
                moveData actualMove = allPossibleMoves[foundAtIndex];

                if (actualMove.captureSquare == -1){
                    waitingForBranchInput = false;

                    bitboardWrapper wrapper;
                    if (moves.whiteTurn){
                        wrapper = moveMaker(moves.whitePieces, moves.blackPieces, moves.kings, actualMove, moves.whiteTurn);
                    } else {
                        wrapper = moveMaker(moves.blackPieces, moves.whitePieces, moves.kings, actualMove, moves.whiteTurn);
                    }

                    moves.setUpPosition(wrapper.white.board, wrapper.black.board, wrapper.kings.board);

                    moves.whiteTurn = !moves.whiteTurn;

                    return;
                }

                if (actualMove.captureSquare != -1)
                {

                    Position currentPosition = new Position(moves.whitePieces.board, moves.blackPieces.board, moves.kings.board);

                    ChainedCaptures(currentPosition, actualMove);

                    moves.overallScore += moves.whiteTurn ? 1 : -1;

                    if (!waitingForBranchInput) moves.whiteTurn = !moves.whiteTurn;

                    return;

                }
                
            } else {
                Console.WriteLine($"Length of moves: {allPossibleMoves.Length}");
                for (int i = 0; i < allPossibleMoves.Length; i++)
                {
                    Console.WriteLine($"Possible move {i} -> {allPossibleMoves[i].start}, {allPossibleMoves[i].moveTo}");
                }
            }
        }

        public void ChainedCaptures(Position currentPosition, moveData actualMove)
        {
            ChainTree chains = new ChainTree(currentPosition, moves.whiteTurn);

            ChainNode node = new ChainNode();

            foreach (ChainNode root in chains.captureTree)
            {
                if (root.move.start == actualMove.start &&
                    root.move.moveTo == actualMove.moveTo)
                {
                    node = root;
                }
            }
            
            List<List<ChainNode>> allPaths = chainTraverser(node);

            List<moveData> chosenPath = buildPathingTree(allPaths);

            Console.WriteLine($"Number of moves to make: {chosenPath.Count}");

            for (int i = 0; i < chosenPath.Count; i++)
            {
                Console.WriteLine($"Applying move {chosenPath[i].start} to {chosenPath[i].moveTo}");

                bitboardWrapper wrapper;

                if (moves.whiteTurn){
                    wrapper = moveMaker(moves.whitePieces, moves.blackPieces, moves.kings, chosenPath[i], moves.whiteTurn);
                } else {
                    wrapper = moveMaker(moves.blackPieces, moves.whitePieces, moves.kings, chosenPath[i], moves.whiteTurn);
                }

                moves.setUpPosition(wrapper.white.board, wrapper.black.board, wrapper.kings.board);
            }

        }

        public List<moveData> buildPathingTree(List<List<ChainNode>> Paths, bool isBot = false, int depthRemaining = 0, bool whiteTurn = true, NegamaxHandler Ai = null)
        {
            Console.WriteLine($"Pathing function called.");

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

                            Console.WriteLine($"Found a deviant! {Paths[i][generation].move.start} to {Paths[i][generation].move.moveTo}, generation = {generation}");
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
            NegamaxHandler negamax = new NegamaxHandler(moves.whitePieces, moves.blackPieces, moves.kings, moves.whiteTurn);

            moveData bestMove = negamax.getMove();

            bitboardWrapper wrapper;

            if (moves.whiteTurn)
            {
                wrapper = moveMaker(moves.whitePieces, moves.blackPieces, moves.kings, bestMove, moves.whiteTurn);
            } else
            {
                wrapper = moveMaker(moves.blackPieces, moves.whitePieces, moves.kings, bestMove, moves.whiteTurn);
            }

            moves.setUpPosition(wrapper.white.board, wrapper.black.board, wrapper.kings.board);

            moves.whiteTurn = !moves.whiteTurn;
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

            static int countPieces(ulong board)
            {
                string boardString = Convert.ToString((long) board, 2);
                int count = 0;

                for (int i = 0; i < boardString.Length; i++)
                {
                    if (boardString[i] == '1') count++;
                }

                return count;
            }

            int whitePieces = countPieces(moves.whitePieces.board);
            int blackPieces = countPieces(moves.blackPieces.board);

            if (whitePieces == 0)
            {
                return 2;
            } else if (blackPieces == 0)
            {
                return 1;
            } else
            {
                return 0;
            }
        }
    }
}