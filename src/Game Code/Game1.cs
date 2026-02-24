using System;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Checkers;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using System.Runtime.Serialization;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.ComponentModel;

namespace Comp_Sci_NEA;

public enum GameState
{
    InMenu,
    MoveInput,
    MoveResolved,
    WaitingForBranchInput,
    BotMoving,
    GameOver,
    BrowsingPastPositions,
}

public class Session
{
    public Board Board {get;}
    public Main Game {get;}
    public Background Back {get;}
    public moveCache PlayedMove {get; private set;}
    public Position CurrentPosition {get; private set;}
    public GameState state {get; private set;}
    public bool ValidClickChecker {get; set;}
    public bool IsPlayerWhite {get;}
    public int FrameTimerForButtonPushing {get; set;}
    public int IndexOfCurrentPosition {get; set;}

    public Session(Board newBoard, Main newMain, Background newBack, moveCache newMove, Position newPos, bool Color)
    {
        Board = newBoard;
        Game = newMain;
        Back = newBack;
        PlayedMove = newMove;
        CurrentPosition = newPos;

        state = GameState.InMenu;
        ValidClickChecker = true;
        FrameTimerForButtonPushing = 0;
        IndexOfCurrentPosition = 0;

        IsPlayerWhite = Color;
    }

    public void UpdateMove(moveCache newMove)
    {
        PlayedMove = newMove;
    }

    public void AddToMove(int square)
    {
        PlayedMove.SetValue(square);
    }

    public void UpdatePosition(Position newPos)
    {
        CurrentPosition = newPos;
    }

    public void SetState(GameState newState)
    {
        state = newState;
    }
}

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _texture;

    float width;
    float height;

    Session session; 

    IScreen currentScreen;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _texture = new Texture2D(GraphicsDevice, 1, 1);
        _texture.SetData([Color.White]);

        width = _graphics.PreferredBackBufferWidth;
        height = _graphics.PreferredBackBufferHeight;

        Board newBoard = new(_texture);
        Main newGame = new();
        Background back = new(newBoard.position.X + newBoard.width / 2.0f, newBoard.position.Y + newBoard.height / 2.0f, _texture, width, height);
        moveCache playedMove = new(-1, -1);
        Position initialMove = new();

        session = new(newBoard, newGame, back, playedMove, initialMove, true);

        session.Game.previousPositions.Add(new Position(
            session.Game.moves.WhitePieces.board, 
            session.Game.moves.BlackPieces.board, 
            session.Game.moves.Kings.board
        ));

        session.IndexOfCurrentPosition++;
        session.UpdatePosition(session.Game.previousPositions.Last());

        currentScreen = new GameScreen(session, _texture);

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        Viewport viewport = _graphics.GraphicsDevice.Viewport;
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        if (Keyboard.GetState().IsKeyDown(Keys.Left) && session.IndexOfCurrentPosition > 1 && session.FrameTimerForButtonPushing == 0)
        {

            session.SetState(GameState.BrowsingPastPositions);

            session.IndexOfCurrentPosition -= 1;

            session.UpdatePosition(session.Game.previousPositions[session.IndexOfCurrentPosition]);

            session.FrameTimerForButtonPushing = 10;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.Right) && session.IndexOfCurrentPosition != session.Game.previousPositions.Count - 1 && session.FrameTimerForButtonPushing == 0)
        {
            session.SetState(GameState.BrowsingPastPositions);

            session.IndexOfCurrentPosition += 1;

            session.UpdatePosition(session.Game.previousPositions[session.IndexOfCurrentPosition]);
            
            session.FrameTimerForButtonPushing = 10;
        }

        if (session.FrameTimerForButtonPushing != 0) session.FrameTimerForButtonPushing--;

        if (session.state == GameState.BrowsingPastPositions && 
            session.IndexOfCurrentPosition == session.Game.previousPositions.Count() - 1)
        {
            session.SetState(GameState.MoveInput);
        }

        Console.WriteLine(session.state);

        currentScreen.UpdateScreen(gameTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();

        currentScreen.DrawScreen(_spriteBatch);
        
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}