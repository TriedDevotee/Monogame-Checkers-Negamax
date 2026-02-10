using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Checkers;

namespace Comp_Sci_NEA;
public class Board
{
    public int heightNum;
    public int widthNum;
    public int height;
    public int width;
    public Shape[][] boardStore;
    public ShapeBound[][] shapeBounds;
    public Vector2 position;
    Texture2D baseTexture;

    public Board(Texture2D Texture, int h = 8, int w = 8, int height2 = 400, int width2 = 400, int x = 50, int y = 50)
    {
        heightNum = h;
        widthNum = w;

        boardStore = new Shape[heightNum][];
        shapeBounds = new ShapeBound[heightNum][];
        for (int i = 0; i < boardStore.Length; i++)
        {
            boardStore[i] = new Shape[widthNum];
            shapeBounds[i] = new ShapeBound[widthNum];
        }

        position = new Vector2(x, y);

        baseTexture = Texture;

        height = height2;
        width = width2;

        PopulateCheckersBoard();

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

                boardStore[i][j] = new Shape(baseTexture, startX, startY, shapeColor, 63 - ((i * 8) + j), shapeHeight, shapeWidth);
                shapeBounds[i][j] = new ShapeBound(startX, startY, startX + shapeWidth, startY +shapeHeight);
                
                whiteColor = !whiteColor;
            }
            whiteColor = !whiteColor;
        }
    }

    public void DrawBoard(SpriteBatch batch, Texture2D baseTexture, Session session)
    {
        Rectangle boardBack = new Rectangle((int) position.X - 5, (int) position.Y - 5, width + 10, height + 10);

        batch.Draw(baseTexture, boardBack, Color.SlateGray);


        Color selectedColor = Color.Blue;
        Color clickedColor = Color.Red;
        Color moveStart = Color.Lime;
        Color moveEnd = Color.BlueViolet;

        int moveStartIndex = session.PlayedMove.start;
        int moveEndIndex = session.PlayedMove.moveTo;

        session.Game.displayPosition(session.CurrentPosition);

        Piece[][] pieces = session.Game.displayBoard;

        for (int i = 0; i < heightNum; i++)
        {
            for (int j = 0; j < widthNum; j++)
            {
                Shape currentShape = boardStore[i][j];

                if (currentShape.isSelected)
                {
                    if (currentShape.isClicked)
                    {
                        batch.Draw(currentShape.texture, currentShape.shapeObj, clickedColor);
                    } else 
                    {
                        batch.Draw(currentShape.texture, currentShape.shapeObj, selectedColor);
                    }
                } else
                {
                    batch.Draw(currentShape.texture, currentShape.shapeObj, currentShape.currentColor);
                }

                if (currentShape.index == moveStartIndex && moveStartIndex >= 0)
                {
                    batch.Draw(currentShape.texture, currentShape.shapeObj, moveStart);
                }

                if (currentShape.index == moveEndIndex && moveEndIndex >= 0)
                {
                    batch.Draw(currentShape.texture, currentShape.shapeObj, moveEnd);
                }

                DrawPieces(i, j, pieces[i][j], batch, baseTexture);


            }
        }
    }

    private void DrawPieces(int squareY, int squareX, Piece currentPiece, SpriteBatch batch, Texture2D baseTexture)
    {

        Rectangle pieceDims = boardStore[squareY][squareX].shapeObj;

        pieceDims.X += 5;
        pieceDims.Y += 5;
        pieceDims.Height -= 10;
        pieceDims.Width -= 10;

        Color usingColor;

        if (currentPiece.isFull){

            if (currentPiece.isWhite)
            {
                usingColor = Color.HotPink;

                if (currentPiece.isKing)
                {
                    usingColor = Color.Yellow;
                }
                
            } else
            {
                usingColor = Color.Indigo;

                if (currentPiece.isKing)
                {
                    usingColor = Color.Orange;
                }
            }

            batch.Draw(baseTexture, pieceDims, usingColor);
        }
    }
}
