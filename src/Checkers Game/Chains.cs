using System;
using System.Collections.Generic;

namespace Checkers
{
    /// <summary>
    /// Creates a tree of all available captures from point n <br/>
    /// Is later expanded to find all available paths.
    /// </summary>
    class ChainTree
    {
        private bool WhiteTurn;
        private Moves Moves;
        private Position CurrentPosition;
        private readonly List<ChainNode> captureTree;
        public IReadOnlyList<ChainNode> CaptureTree => captureTree;

        public ChainTree(Position current, bool turn)
        {
            captureTree = new List<ChainNode>();
            CurrentPosition = current;
            WhiteTurn = turn;

            Moves = new Moves(WhiteTurn);
            Moves.SetUpPosition(CurrentPosition.whitePieces.board, CurrentPosition.blackPieces.board, CurrentPosition.kings.board);

            ManageChains(); 
        }

        /// <summary>
        /// Main tree loop. Sets up preexisting captures in position, then recursively explores paths for each following capture.
        /// </summary>
        private void ManageChains()
        {
            
            moveData[] captures = Moves.GetCaptures();
            AddBaseCaptures(captures);

            foreach (ChainNode node in captureTree)
            {
                ExploreCaptures(node.move, CurrentPosition, node);
            }
        }

        /// <summary>
        /// Adds all immediate capture moves in the current position.
        /// </summary>
        /// <param name="captures"></param>
        private void AddBaseCaptures(moveData[] captures)
        {
            for (int i = 0; i < captures.Length; i++)
            {
                captureTree.Add(new ChainNode(captures[i]));
            }
        }

        /// <summary>
        /// Recursive capture explorer. <br/>
        ///     - Creates new iteration of position <br/>
        ///     - Finds all captures <br/>
        ///     - Adds them to tree <br/>
        /// </summary>
        /// <param name="newPos"></param>
        /// <param name="position"></param>
        /// <param name="fromNode"></param>
        private void ExploreCaptures(moveData newPos, Position position, ChainNode fromNode)
        {
            Position newPosition = new Position(
                position.whitePieces.board,
                position.blackPieces.board,
                position.kings.board
            );

            newPosition.makePositionalMove(newPos, WhiteTurn);

            Moves nextMoves = new Moves(turn: WhiteTurn);
            nextMoves.SetUpPosition(newPosition.whitePieces.board, newPosition.blackPieces.board, newPosition.kings.board);

            moveData[] newCaptures = FindValidCaptures(nextMoves, fromNode.move.moveTo);

            if (newCaptures.Length == 0) return;

            foreach (moveData capture in newCaptures)
            {
                ChainNode newNode = new ChainNode(capture);
                fromNode.children.Add(newNode);
                ExploreCaptures(capture, newPosition, newNode);
            }

        }

        /// <summary>
        /// Finds all valid captures in a position. <br/>
        /// Index determined by the first/last index parameters.
        /// </summary>
        /// <param name="move"></param>
        /// <param name="firstIndex"></param>
        /// <param name="lastIndex"></param>
        /// <returns></returns>
        private moveData[] FindValidCaptures(Moves move, int firstIndex = 0, int lastIndex = 64)
        {
            moveData[] newCaptures = move.GetCaptures(firstIndex, lastIndex);
            return newCaptures;
        }
    }
}