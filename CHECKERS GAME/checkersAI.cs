using System;

namespace Checkers
{

    /* 
        RIGHT - now this is the hard part:

        The algorithm is as follows:
            - Check for a win/loss, return the score back up the tree
            - Ensure the depth > 0
            - Calculate all the possible moves for the position
            - Update the position
            - Update the score for that position
            - Run the position back through the function
            - 
        *Spoiler alert - this was not the hard part.
        I never want to write chaining again :(

    */
    public class NegamaxHandler
    {
        bool whiteTurn;
        moveData bestMove;

        public NegamaxHandler(Bitboard whitePieces, Bitboard blackPieces, Bitboard kings, bool turn)
        {
            Position gamePos = new Position(
                whitePieces.board,
                blackPieces.board,
                kings.board
            );

            whiteTurn = turn;

            bestMove = GetBestMove(gamePos, whiteTurn);
        }

        public moveData getMove()
        {
            return bestMove;
        }

        public int countPieces(ulong board)
        {
            string boardString = Convert.ToString((long) board, 2);
            int count = 0;

            for (int i = 0; i < boardString.Length; i++)
            {
                if (boardString[i] == '1') count++;
            }

            return count;
        }

        public int evaluation(Position board, bool whiteTurn)
        {
            int whitePieces = countPieces(board.whitePieces.board);
            int blackPieces = countPieces(board.blackPieces.board);
            
            int whiteKings = countPieces(board.kings.board & board.whitePieces.board);
            int blackKings = countPieces(board.kings.board & board.blackPieces.board);

            if (whiteTurn)
            {
                return 3 * (whitePieces - blackPieces) + (whiteKings - blackKings);
            } else
            {
                 return 3 * (blackPieces - whitePieces) + (blackKings - whiteKings);
            } 

        }

        public int Negamax(int depth, Position board, bool whiteTurn, int alpha, int beta)
        {
            if (depth == 0) return evaluation(board, whiteTurn);
            if (board.isGameOver()) return -1000;

            Moves moves = new Moves();
            moves.setUpPosition(board.whitePieces.board, board.blackPieces.board, board.kings.board);

            moveData[] possibleMoves = moves.getAllMoves();

            if (possibleMoves.Length == 0) return 0;

            int bestScore = int.MinValue;

            foreach (moveData move in possibleMoves)
            {
            
                Position newPos = new Position(board.whitePieces.board, board.blackPieces.board, board.kings.board);

                if (whiteTurn)
                {
                    bool settingKing = newPos.kings.isSquareUsed(move.start) && newPos.whitePieces.isSquareUsed(move.start);
                    bool becomesKing = move.moveTo / 8 == 0 || move.moveTo / 8 == 7;

                    newPos.whitePieces.setSquare(move.moveTo);
                    newPos.whitePieces.clearSquare(move.start);
                    if (move.captureSquare != -1)
                    {
                        newPos.blackPieces.clearSquare(move.captureSquare);
                    }
                    if (settingKing || becomesKing)
                    {
                        newPos.kings.clearSquare(move.start);
                        newPos.kings.setSquare(move.moveTo);
                    }
                } else
                {
                    bool settingKing = newPos.kings.isSquareUsed(move.start) && newPos.blackPieces.isSquareUsed(move.start);
                    bool becomesKing = move.moveTo / 8 == 0 || move.moveTo / 8 == 7;

                    newPos.blackPieces.setSquare(move.moveTo);
                    newPos.blackPieces.clearSquare(move.start);

                    if (move.captureSquare != -1)
                    {
                        newPos.whitePieces.clearSquare(move.captureSquare);
                    }
                    if (settingKing || becomesKing)
                    {
                        newPos.kings.clearSquare(move.start);
                        newPos.kings.setSquare(move.moveTo);
                    }
                }

                Moves chainChecker = new Moves();
                chainChecker.setUpPosition(newPos.whitePieces.board, newPos.blackPieces.board, newPos.kings.board);

                int score = -Negamax(depth - 1, newPos, !whiteTurn, -alpha, -beta);

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

        public moveData GetBestMove(Position board, bool whiteTurn)
        {

            int maxDepth = 11;

            Moves moves = new Moves();

            moves.whiteTurn = whiteTurn;

            Console.WriteLine($"AI MOVE GENERATION FOR: {(moves.whiteTurn ? "WHITE" : "BLACK")}");

            moves.setUpPosition(board.whitePieces.board, board.blackPieces.board, board.kings.board);

            moveData[] possibleMoves = moves.getAllMoves();

            int bestScore = int.MinValue;

            moveData bestMove = new moveData();


            foreach (moveData move in possibleMoves)
            {
            
                Position newPos = new Position(board.whitePieces.board, board.blackPieces.board, board.kings.board);

                if (whiteTurn){
                    bool settingKing = newPos.kings.isSquareUsed(move.start) && newPos.whitePieces.isSquareUsed(move.start);

                    newPos.whitePieces.setSquare(move.moveTo);
                    newPos.whitePieces.clearSquare(move.start);
                    if (move.captureSquare != -1)
                    {
                        newPos.blackPieces.clearSquare(move.captureSquare);
                    }
                    if (settingKing)
                    {
                        newPos.kings.clearSquare(move.start);
                        newPos.kings.setSquare(move.moveTo);
                    }
                } else
                {
                    bool settingKing = newPos.kings.isSquareUsed(move.start) && newPos.blackPieces.isSquareUsed(move.start); 

                    newPos.blackPieces.setSquare(move.moveTo);
                    newPos.blackPieces.clearSquare(move.start);

                    if (move.captureSquare != -1)
                    {
                        newPos.whitePieces.clearSquare(move.captureSquare);
                    }
                    if (settingKing)
                    {
                        newPos.kings.clearSquare(move.start);
                        newPos.kings.setSquare(move.moveTo);
                    }
                }

                int score = -Negamax(depth: maxDepth, newPos, !whiteTurn, int.MinValue + 1, int.MaxValue);

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