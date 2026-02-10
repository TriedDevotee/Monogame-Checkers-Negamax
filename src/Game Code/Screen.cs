using System.ComponentModel;
using Comp_Sci_NEA;

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Checkers;
using System.Linq;
using Microsoft.Xna.Framework.Input;

public interface IScreen
{
    public abstract void UpdateScreen(GameTime gameTime);
    public abstract void DrawScreen(SpriteBatch spriteBatch);
}

public class GameScreen : IScreen
{
    private Session session;
    private readonly Texture2D _texture;
    private MouseState mouseState;
    private MouseState previousMouseState;

    public GameScreen(Session currentSession, Texture2D texture)
    {
        session = currentSession;
        _texture = texture;

        mouseState = new MouseState();
        previousMouseState = new MouseState();
    }

    public void UpdateScreen(GameTime gameTime)
    {
        previousMouseState = mouseState;
        mouseState = Mouse.GetState();
        FindSelectedSquare();

        if (mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
        {
            session.ValidClickChecker = true;
        }

        float dt = (float) gameTime.ElapsedGameTime.TotalSeconds;

        session.Back.AddParticles();
        session.Back.updateParticles(dt);

        session.Game.previousPositions.Add(
            new Position(
                session.Game.moves.WhitePieces.board, 
                session.Game.moves.BlackPieces.board, 
                session.Game.moves.Kings.board
            )
        );
        
        session.IndexOfCurrentPosition++;
        session.UpdatePosition(session.Game.previousPositions.Last());

        if (session.Game.moves.WhiteTurn)
        {
            session.Game.makeHumanMove(session.PlayedMove);
            session.SetState(GameState.MoveInput);
            session.UpdateMove(new moveCache(session.PlayedMove.start, -1));
        }
        else
        {
            session.Game.runForAI(session.Game.moves);
            session.SetState(GameState.BotMoving);
        }

        int isGameOver = session.Game.checkForGameOver();
        if (isGameOver == 1 || isGameOver == 2 || isGameOver == 3) session.SetState(GameState.GameOver);
    }

    public void DrawScreen(SpriteBatch _spriteBatch)
    {
        session.Back.drawParticles(_spriteBatch, _texture);

        session.Board.DrawBoard(
            batch: _spriteBatch, 
            baseTexture: _texture, 
            playedMove: session.PlayedMove, 
            main: session.Game, 
            currentPosition: session.CurrentPosition
        );
    }

    public void FindSelectedSquare()
    {
        for (int i = 0; i < session.Board.heightNum; i++)
        {
            for (int j = 0; j < session.Board.widthNum; j++)
            {
                ShapeBound bound = session.Board.shapeBounds[i][j];
                if (mouseState.Position.X >= bound.startX 
                    && mouseState.Position.X < bound.farX 
                    && mouseState.Position.Y >= bound.startY 
                    && mouseState.Position.Y < bound.farY)
                {
                    session.Board.boardStore[i][j].isSelected = true;

                    if (mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
                    {
                        session.ValidClickChecker = false;

                        session.Board.boardStore[i][j].isClicked = true;

                        session.PlayedMove.SetValue(session.Board.boardStore[i][j].index);
                    } else
                    {
                        session.Board.boardStore[i][j].isClicked = false;
                    }
                } 
                else
                {
                    session.Board.boardStore[i][j].isSelected = false;
                }
            }
        }
    }
}

public class MenuScreen : IScreen
{
    private readonly Session session;

    public MenuScreen(Session currentSession)
    {
        session = currentSession;
    }
    
    public void UpdateScreen(GameTime gameTime)
    {
        
    }

    public void DrawScreen(SpriteBatch spriteBatch)
    {
        
    }
}