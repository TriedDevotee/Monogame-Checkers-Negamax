using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Checkers;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Comp_Sci_NEA;
public class Board
{
    public int heightNum;
    public int widthNum;
    public int height;
    public int width;
    public ShapeManager shapes;
    public ShapeBound[][] shapeBounds;
    public Vector2 position;
    Texture2D baseTexture;
    int selectedShapeIndex;

    public Board(Texture2D Texture, int h = 8, int w = 8, int height2 = 400, int width2 = 400, int x = 50, int y = 50)
    {

        heightNum = h;
        widthNum = w;

        position = new Vector2(x, y);

        baseTexture = Texture;

        height = height2;
        width = width2;

        shapes = new(baseTexture);

        PopulateCheckersBoard();

        selectedShapeIndex = -1;
    }

    private void PopulateCheckersBoard()
    {
        bool whiteColor = false;
        for (int i = 0; i < heightNum; i++)
        {
            for (int j = 0; j < widthNum; j++)
            {
                int shapeWidth = width / widthNum;
                int shapeHeight = height / heightNum;
                
                int startX = j * shapeWidth + (int) position.X;
                int startY = i * shapeHeight + (int) position.Y;

                Color shapeColor = whiteColor ? Color.White : Color.DarkSlateGray;

                shapes.AddShapes(startX, startY, shapeColor, shapeHeight, shapeWidth);
                
                whiteColor = !whiteColor;
            }
            whiteColor = !whiteColor;
        }
    }


    public void DrawBoard(SpriteBatch batch, Texture2D baseTexture, Session session, MouseState prevState, MouseState state, bool flipBoard)
    {
        Rectangle boardBack = new Rectangle((int) position.X - 5, (int) position.Y - 5, width + 10, height + 10);

        batch.Draw(baseTexture, boardBack, Color.SlateGray);


        Color player1 = session.userConfig.config.white_player_color.GetColor();
        Color player2 = session.userConfig.config.black_player_color.GetColor();
        Color board1 = session.userConfig.config.board_color_1.GetColor();
        Color board2 = session.userConfig.config.board_color_2.GetColor();

        DrawGameBoard(batch, board1, board2);

        session.Game.displayPosition(session.CurrentPosition);

        Piece[][] pieces = session.Game.displayBoard;

        HandleClicks(session, prevState, state, flipBoard);

        if (selectedShapeIndex != -1){

            Shape selectedShape = shapes.getSelectedShapes(selectedShapeIndex);

            Color useColor = Color.Blue;

            if (selectedShape.isClicked)
            {
                useColor = Color.Red;
            }

            shapes.DrawSpecials(batch, [selectedShapeIndex], useColor);
        }

        if (session.PlayedMove.start != -1)
        {
            int colorIndex = session.PlayedMove.start;

            if (flipBoard) colorIndex = 63 - colorIndex; 

            shapes.DrawSpecials(batch, [colorIndex], Color.Green);
        }

        if (session.PlayedMove.moveTo != -1)
        {
            int colorIndex = session.PlayedMove.moveTo;

            if (flipBoard) colorIndex = 63 - colorIndex; 

            shapes.DrawSpecials(batch, [colorIndex], Color.Purple);
        }

        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                int row, col;

                if (flipBoard)
                {
                    row = 7 - x;
                    col = 7 - y;
                }
                else
                {
                    row = x;
                    col = y;
                }

                int shapeIndex = (row * 8) + col;
                DrawPieces(shapes.getSelectedShapes(shapeIndex), pieces[x][y], batch, baseTexture, player1, player2);
            }
        }

    }

    private void DrawPieces(Shape currShape, Piece currentPiece, SpriteBatch batch, Texture2D baseTexture, Color player1, Color player2)
    {

        Rectangle pieceDims = new(
            currShape.shapeObj.X + 5, 
            currShape.shapeObj.Y + 5, 
            currShape.shapeObj.Width - 10, 
            currShape.shapeObj.Height - 10);

        Color usingColor = currentPiece.isWhite ? player1 : player2;
        usingColor = currentPiece.isWhite && currentPiece.isKing ? Color.Yellow : usingColor;
        usingColor = !currentPiece.isWhite && currentPiece.isKing ? Color.Orange : usingColor;

        if(currentPiece.isFull) batch.Draw(baseTexture, pieceDims, usingColor);
    }

    private void DrawGameBoard(SpriteBatch batch, Color board1, Color board2)
    {
        shapes.DrawShapes(batch, board1);

        List<int> blackIndices = [];
        int offset = 0;
        for (int i = 0; i < 32; i++)
        {
            blackIndices.Add(i * 2 + offset);

            if (blackIndices.Count % 4 == 0)
            {
                offset = offset == 1 ? 0 : 1;
            }
        }

        shapes.DrawSpecials(batch, blackIndices.ToArray(), board2);
    }

    private void HandleClicks(Session session, MouseState prevState, MouseState state, bool flipBoard)
    {
        selectedShapeIndex = shapes.checkForSelectedShapes(prevState, state);
        if (selectedShapeIndex != -1)
        {
            int boardIndex = selectedShapeIndex;
            
            if (flipBoard)
            {
                int x = selectedShapeIndex / 8;
                int y = selectedShapeIndex % 8;

                boardIndex = (7 - x) * 8 + (7 - y);
            }

            int row = boardIndex / 8;
            int col = boardIndex % 8;

            bool IsClickableSquare = session.Game.displayBoard[row][col].isFull 
                && session.PlayedMove.start == -1 
                && session.IsPlayerWhite == session.Game.displayBoard[row][col].isWhite;

            if (shapes.getSelectedShapes(selectedShapeIndex).isClicked)
            {
                if (session.PlayedMove.start == -1){
                    if (IsClickableSquare)
                    {
                        session.AddToMove(boardIndex);
                    }
                }
                else
                {
                    session.AddToMove(boardIndex);
                }
            }
        }
    }
}
