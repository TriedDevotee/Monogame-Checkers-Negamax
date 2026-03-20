using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Checkers;
using System.Linq;
using System.Collections.Generic;

namespace Comp_Sci_NEA;

/// <summary>
/// Handles the state that the game exists within at any present moment. 
/// This is mostly used to ensure the input pipeline is functioning correctly
/// </summary>
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

/// <summary>
/// Stores all major attributes used in the front end. 
/// This includes the board, backend and position links which are used to coordinate the input pipeline
/// Supports a dependency injection framework
/// </summary>
public class Session
{
    public Board Board {get; private set;}
    public Main Game {get; private set;}
    public GameBackground Back {get; private set;}
    public moveCache PlayedMove {get; private set;}
    public Position CurrentPosition {get; private set;}
    public GameState state {get; private set;}
    public float Width {get; private set;}
    public float Height {get; private set;}
    public bool ValidClickChecker;
    public bool IsPlayerWhite {get; private set;}
    public int FrameTimerForButtonPushing;
    public int IndexOfCurrentPosition;
    public Dictionary<string, Texture2D> LoadedTextures {get; private set;}
    public Dictionary<string, SpriteFont> LoadedFonts {get; private set;}
    public ConfigData userConfig {get; private set;}
    public Color bgColor {get; private set;}
    public bool doExit;
    private Texture2D baseTexture;
    public int gameDepth {get; private set;}
    public GameOverState gameOverState;

    public Session(Board newBoard, Main newMain, GameBackground newBack, moveCache newMove, Position newPos, bool color, float H, float W, Texture2D _texture)
    {
        Board = newBoard;
        Game = newMain;
        Back = newBack;
        PlayedMove = newMove;
        CurrentPosition = newPos;

        Width = W;
        Height = H;

        state = GameState.InMenu;
        ValidClickChecker = true;
        FrameTimerForButtonPushing = 0;
        IndexOfCurrentPosition = 0;

        IsPlayerWhite = color;
        
        LoadedTextures = new();
        LoadedFonts = new();

        userConfig = new();

        bgColor = Color.Black;
        doExit = false;

        baseTexture = _texture;
        gameDepth = 7;

        gameOverState = GameOverState.None;
    }

    public void UpdateMove(moveCache newMove)
    {
        PlayedMove = newMove;
    }

    /// <summary>
    /// Adds to the played move structure the input square. 
    /// This is coordinated within PlayedMove to ensure that the correct value is stored in the correct location
    /// </summary>
    /// <param name="square"></param>
    public void AddToMove(int square)
    {
        PlayedMove.SetValue(square);
    }

    /// <summary>
    /// Changes current position into inputted one
    /// </summary>
    /// <param name="newPos"></param>
    public void UpdatePosition(Position newPos)
    {
        CurrentPosition = newPos;
    }

    /// <summary>
    /// Assigns new state to the GameState structure
    /// </summary>
    /// <param name="newState"></param>
    public void SetState(GameState newState)
    {
        state = newState;
    }

    public void setBgColor(Color color)
    {
        bgColor = color;
    }

    public void setDepth(int depth)
    {
        gameDepth = depth;
    }

    /// <summary>
    /// Instantiates new board and game structures. This means the game is completely reset. 
    /// Additinally, wipes all previous positions and resets other values, like PlayedMove, 
    /// and the position, which is set to a default one to prevent board corruption
    /// </summary>
    public void ResetGame()
    {
        Board = new Board(baseTexture, x: (int) (Width / 2) - 200, y: (int) (Height / 2) - 200);
        Game = new Main(gameDepth);

        PlayedMove = new moveCache();
        CurrentPosition = new Position(
            Game.moves.WhitePieces.board, 
            Game.moves.BlackPieces.board, 
            Game.moves.Kings.board
        );

        ValidClickChecker = true;
        Game.previousPositions.Clear();
    }

    /// <summary>
    /// Toggles the player color with the value of true if white (player1) or false if black (player2)
    /// </summary>
    /// <param name="inColor"></param>
    public void setPlayerColor(bool inColor)
    {
        IsPlayerWhite = inColor;
    }
}

/// <summary>
/// Instamtiated by framework. Runs main game loop. 
/// All update and draw is handled here
/// </summary>
public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _texture;

    float width;
    float height;

    Session session; 
    ScreenManager screens;
    bool EscKeyLastFrame;
    bool EscKeyThisFrame;


    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    /// <summary>
    /// Assigns all base values of the main class.
    /// Notably, instantiates session, with all the necessary values, and the screen manager, which handles all screenwise rendering
    /// </summary>
    protected override void Initialize()
    {
        _texture = new Texture2D(GraphicsDevice, 1, 1);
        _texture.SetData([Color.White]);

        width = _graphics.PreferredBackBufferWidth;
        height = _graphics.PreferredBackBufferHeight;

        Board newBoard = new(_texture, x: (int) (width / 2) - 200, y: (int) (height / 2) - 200);
        Main newGame = new(7);
        GameBackground back = new(newBoard.position.X + newBoard.width / 4.0f, newBoard.position.Y + newBoard.height / 4.0f, _texture, width, height);
        moveCache playedMove = new(-1, -1);
        Position initialMove = new();

        session = new(newBoard, newGame, back, playedMove, initialMove, true, height, width, _texture);

        session.Game.previousPositions.Add(new Position(
            session.Game.moves.WhitePieces.board, 
            session.Game.moves.BlackPieces.board, 
            session.Game.moves.Kings.board
        ));

        screens = new(session, _texture);

        session.IndexOfCurrentPosition++;
        session.UpdatePosition(session.Game.previousPositions.Last());

        EscKeyLastFrame = Keyboard.GetState().IsKeyDown(Keys.Escape);
        EscKeyThisFrame = Keyboard.GetState().IsKeyDown(Keys.Escape);

        screens.currScreenType = ScreenTypes.GameOver;
        screens.UpdateScreenManager();
        
        base.Initialize();
    }

    /// <summary>
    /// Loads all necessary content from content.mgcb
    /// Stores it in dictionaries stored in session to allow for retrieval anywhere in the program
    /// </summary>
    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        Texture2D LogoImage = Content.Load<Texture2D>("LogoImage");
        Texture2D EditionImage = Content.Load<Texture2D>("EditionImage");
        Texture2D MenuButton =  Content.Load<Texture2D>("menuButton");
        Texture2D GameOver = Content.Load<Texture2D>("GameOver");

        session.LoadedTextures.Add("LogoImage", LogoImage);
        session.LoadedTextures.Add("EditionImage", EditionImage);
        session.LoadedTextures.Add("MenuButton", MenuButton);
        session.LoadedTextures.Add("GameOver", GameOver);

        SpriteFont MonoCraft = Content.LoadLocalized<SpriteFont>("Monocraft");

        session.LoadedFonts.Add("Monocraft", MonoCraft);

        Viewport viewport = _graphics.GraphicsDevice.Viewport;
    }

    /// <summary>
    /// Main update point. 
    /// Updates the screen and handles all keyboard input, such as game quitting and viewing previous positions
    /// </summary>
    /// <param name="gameTime"></param>
    protected override void Update(GameTime gameTime)
    {
        EscKeyLastFrame = EscKeyThisFrame;
        EscKeyThisFrame = Keyboard.GetState().IsKeyDown(Keys.Escape);
        if (EscKeyThisFrame && !EscKeyLastFrame)
        {
            if (screens.currScreen.getParentScreen() != ScreenTypes.None)
            {
                screens.currScreenType = screens.currScreen.getParentScreen();
                screens.UpdateScreenManager();
            }
        }

        if (session.doExit)
        {
            Exit();
        }

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

        screens.currScreen.UpdateScreen(gameTime);

        if (Keyboard.GetState().IsKeyDown(Keys.J))
        {
            session.userConfig.updateConfig();
        }

        base.Update(gameTime);
    }

    /// <summary>
    /// Draws the current screen, and assigns the background color
    /// </summary>
    /// <param name="gameTime"></param>
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(session.bgColor);

        _spriteBatch.Begin();

        screens.currScreen.DrawScreen(_spriteBatch);
        
        _spriteBatch.End();

        base.Draw(gameTime);
    }
}