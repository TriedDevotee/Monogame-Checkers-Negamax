using System;
using System.Collections.Generic;

namespace Checkers
{
    class ChainTree
    {
        public List<ChainNode> captureTree;
        public Position currentPosition;
        public bool whiteTurn;
        public Moves moves;

        public ChainTree(Position current, bool turn)
        {
            captureTree = new List<ChainNode>();
            currentPosition = current;
            whiteTurn = turn;

            moves = new Moves(whiteTurn);
            moves.setUpPosition(currentPosition.whitePieces.board, currentPosition.blackPieces.board, currentPosition.kings.board);

            manageChains(); 
        }

        public void manageChains()
        {
            
            moveData[] captures = moves.getCaptures();
            AddBaseCaptures(captures);

            int count = 0;

            foreach (ChainNode node in captureTree)
            {
                ExploreCaptures(node.move, currentPosition, node);
                count ++;
            }
        }

        public void AddBaseCaptures(moveData[] captures)
        {
            for (int i = 0; i < captures.Length; i++)
            {
                captureTree.Add(new ChainNode(captures[i]));
            }
        }

        public void ExploreCaptures(moveData newPos, Position currentPosition, ChainNode fromNode)
        {
            /*
                - Get a list of all base level captures
                - For each capture:
                    - Update position (pass in move and originating node)
                    - Find all captures

                    - If length == 0, return
                    - Else, recurse through the captures
            */

            Position newPosition = new Position(
                currentPosition.whitePieces.board,
                currentPosition.blackPieces.board,
                currentPosition.kings.board
            );

            newPosition.makePositionalMove(newPos, whiteTurn);

            Moves moves1 = new Moves();
            moves1.setUpPosition(newPosition.whitePieces.board, newPosition.blackPieces.board, newPosition.kings.board);

            moveData[] newCaptures = findValidCaptures(moves1);


            for (int n = 0; n < newCaptures.Length; n++)
            {
                Console.WriteLine($"Found captures: {newCaptures[n].start} to {newCaptures[n].moveTo} taking {newCaptures[n].captureSquare}");
            }

            if (newCaptures.Length == 0) return;

            foreach (moveData capture in newCaptures)
            {
                ChainNode newNode = new ChainNode(capture);
                fromNode.children.Add(newNode);
                ExploreCaptures(capture, newPosition, newNode);
            }

        }

        public moveData[] findValidCaptures(Moves move)
        {
            moveData[] newCaptures = move.getCaptures();
            return newCaptures;
        }
    }
}