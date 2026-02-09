using System.ComponentModel;
using Comp_Sci_NEA;

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Checkers;
using System.Linq;

public interface IScreen
{
    public abstract void UpdateScreen(GameTime gameTime);
    public abstract void DrawScreen(SpriteBatch spriteBatch);
}

public class GameScreen : IScreen
{
    private Session session;
    private readonly Texture2D _texture;
    private GameState _state;

    public GameScreen(Session currentSession, Texture2D texture, GameState state)
    {
        session = currentSession;
        _texture = texture;
        _state = state;
    }

    public void UpdateScreen(GameTime gameTime)
    {
        float dt = (float) gameTime.ElapsedGameTime.TotalSeconds;

        session.Back.AddParticles();
        session.Back.updateParticles(dt);

        session.Game.previousPositions.Add(new Position(session.Game.moves.WhitePieces.board, session.Game.moves.BlackPieces.board, session.Game.moves.Kings.board));
        session.IndexOfCurrentPosition++;
        session.UpdatePosition(session.Game.previousPositions.Last());

        if (session.Game.moves.WhiteTurn)
        {
            session.Game.makeHumanMove(session.PlayedMove);
            _state = GameState.MoveInput;
            session.UpdateMove(new moveCache(session.PlayedMove.start, -1));
        }
        else
        {
            session.Game.runForAI(session.Game.moves);
            _state = GameState.BotMoving;
        }

        int isGameOver = session.Game.checkForGameOver();
        if (isGameOver == 1 || isGameOver == 2 || isGameOver == 3) _state = GameState.GameOver;
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