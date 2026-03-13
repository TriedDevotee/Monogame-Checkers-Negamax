using System.ComponentModel;
using Comp_Sci_NEA;

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Checkers;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System;
using System.Globalization;

public enum ScreenTypes
{
    None,
    Game,
    Title,
    SinglePlayer,
    Multiplayer,
    Options,
}

public interface IScreen
{
    public abstract void UpdateScreen(GameTime gameTime);
    public abstract void DrawScreen(SpriteBatch spriteBatch);
    public abstract ScreenTypes getParentScreen();
}

public interface IMenuScreen : IScreen
{
    protected abstract ButtonManager SetUpButtons();
    protected abstract ScreenTypes[] getAllConnectedScreens();
    protected abstract void RunForButtons();
}
public enum PlayerType
{
    Bot,
    Human
}

public class GameScreen : IScreen
{
    private Session session;
    private readonly Texture2D _texture;
    private MouseState mouseState;
    private bool leftClicked;
    private MouseState previousMouseState;
    private ScreenManager screens;
    private PlayerType player1;
    private PlayerType player2;
    private bool doBoardFlip;
    private bool runFlipping;

    public GameScreen(Session currentSession, Texture2D texture, ScreenManager manager, PlayerType P1, PlayerType P2)
    {
        session = currentSession;
        _texture = texture;

        mouseState = new MouseState();
        previousMouseState = Mouse.GetState();

        session.SetState(GameState.MoveInput);

        screens = manager;

        player1 = P1;
        player2 = P2;

        doBoardFlip = false;

        if (P1 == P2 && P1 == PlayerType.Human)
        {
            runFlipping = true;
        }
    }

    public ScreenTypes getParentScreen()
    {
        return ScreenTypes.Title;
    }

    public void UpdateScreen(GameTime gameTime)
    {
        previousMouseState = mouseState;
        mouseState = Mouse.GetState();

        leftClicked = mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released;

        if (mouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released)
        {
            session.ValidClickChecker = true;
        }

        float dt = (float) gameTime.ElapsedGameTime.TotalSeconds;

        session.Back.AddParticles();
        session.Back.updateParticles(dt);

        PlayerType currPlayer = session.Game.moves.WhiteTurn ? player1 : player2;

        if (currPlayer == PlayerType.Human)
        { 
            session.IsPlayerWhite = session.Game.moves.WhiteTurn;
            if (session.PlayedMove.start != -1 && session.PlayedMove.moveTo != -1){
                moveCache inpMove = new(63-session.PlayedMove.start, 63-session.PlayedMove.moveTo);

                bool movePlayed = session.Game.MakeHumanMove(inpMove);

                if (movePlayed){
                    session.SetState(GameState.MoveResolved);
                    session.UpdateMove(new moveCache(session.PlayedMove.start, -1));
                }

                if (session.state == GameState.MoveResolved && !session.Game.waitingForBranchInput)
                {
                    session.SetState(GameState.BotMoving);
                    UpdatePositionLog();

                    if (runFlipping)
                    {
                        doBoardFlip = !doBoardFlip;
                    }
                }
            }
        }
        else
        {
            session.Game.runForAI(session.Game.moves);
            session.SetState(GameState.MoveInput);
            UpdatePositionLog();
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
            session: session,
            previousMouseState,
            mouseState,
            doBoardFlip
        );
    }

    private void UpdatePositionLog()
    {
       session.Game.previousPositions.Add(
            new Position(
                session.Game.moves.WhitePieces.board, 
                session.Game.moves.BlackPieces.board, 
                session.Game.moves.Kings.board
            )
        );
        
        session.UpdatePosition(session.Game.previousPositions.Last()); 

        session.IndexOfCurrentPosition++;
    }  
}

public class TitleScreen : IMenuScreen
{
    private readonly Session session;
    private MenuBackground background;
    private Texture2D baseTexture;
    private ButtonManager buttons;
    ScreenTypes[] connections;
    ScreenTypes parentScreen;
    ScreenManager screens;
    MouseState prevState;
    MouseState state;

    public TitleScreen(Session currentSession, Texture2D _texture, ScreenManager screenManager)
    {
        baseTexture = _texture;

        session = currentSession;
        background = new(session, baseTexture);

        buttons = SetUpButtons();

        connections = getAllConnectedScreens();
        parentScreen = getParentScreen();

        screens = screenManager;

        prevState = Mouse.GetState();
        state = Mouse.GetState();
    }

    public ScreenTypes[] getAllConnectedScreens()
    {
        return [ScreenTypes.Options, ScreenTypes.SinglePlayer, ScreenTypes.Multiplayer];
    }

    public ScreenTypes getParentScreen()
    {
        return ScreenTypes.None;
    }

    public ButtonManager SetUpButtons()
    {
        ButtonManager newButtons = new();

        newButtons.AddButton(
            "Single Player", 
            Color.Gray, 
            Color.White, 
            (int) (session.Width * 0.2), 
            (int) (session.Height * 0.45), 
            (int) (session.Width * 0.6), 
            (int) (session.Height * 0.1),
            ButtonFunctions.SinglePlayerMenu
        );

        newButtons.AddButton(
            "Multiplayer", 
            Color.Gray, 
            Color.White, 
            (int) (session.Width * 0.2), 
            (int) (session.Height * 0.57), 
            (int) (session.Width * 0.6), 
            (int) (session.Height * 0.1),
            ButtonFunctions.MultiplayerMenu
        );

        newButtons.AddButton(
            "Options", 
            Color.Gray, 
            Color.White, 
            (int) (session.Width * 0.2), 
            (int) (session.Height * 0.69), 
            (int) (session.Width * 0.28), 
            (int) (session.Height * 0.1),
            ButtonFunctions.OptionsMenu
        );

        newButtons.AddButton(
            "Quit Game", 
            Color.Gray, 
            Color.White, 
            (int) (session.Width * 0.52), 
            (int) (session.Height * 0.69), 
            (int) (session.Width * 0.28), 
            (int) (session.Height * 0.1),
            ButtonFunctions.Quit
        );
        
        return newButtons;
    }
    
    public void UpdateScreen(GameTime gameTime)
    {
        background.update((float) gameTime.ElapsedGameTime.TotalSeconds);

        prevState = state;
        state = Mouse.GetState();

        RunForButtons();
    }

    public void DrawScreen(SpriteBatch spriteBatch)
    {
        background.Draw(spriteBatch);

        //Draws logo
        Rectangle LogoShape = new(
            (int) (session.Width * 0.2), 
            (int) (session.Height * 0.1), 
            (int) (session.Width * 0.6), 
            (int) (session.Height * 0.3)
        );

        Rectangle EditionShape = new(
            (int) (session.Width * 0.15),
            (int) (session.Height * 0.28),
            (int) (session.Width * 0.7),
            (int) (session.Height * 0.15)
        );

        spriteBatch.Draw(session.LoadedTextures["LogoImage"], LogoShape, Color.White);
        spriteBatch.Draw(session.LoadedTextures["EditionImage"], EditionShape, Color.White);

        buttons.DrawButtons(spriteBatch, session.LoadedFonts["Monocraft"], baseTexture);
    }   

    public void RunForButtons()
    {
        int selectedButton = buttons.checkForSelectedButtons(prevState, state);

        if (selectedButton != -1)
        {
            Button button = buttons.getButton(selectedButton);

            if (button.isClicked)
            {
                ButtonFunctions function = button.purpose;

                if (function == ButtonFunctions.OptionsMenu)
                {
                    screens.currScreenType = ScreenTypes.Options;
                    screens.UpdateScreenManager();
                }
                else if (function == ButtonFunctions.Quit)
                {
                    session.doExit = true;
                }
                else if (function == ButtonFunctions.SinglePlayerMenu)
                {
                    screens.currScreenType = ScreenTypes.Game;
                    screens.UpdateScreenManager();
                }
            }
        }
    }
}

public enum ButtonFunctions
{
    None,
    SinglePlayerMenu,
    MultiplayerMenu,
    OptionsMenu,
    Quit,
    Player1Color,
    Player2Color,
    Board1Color,
    Board2Color,
    SaveChanges,
    GoBack,
    ChangeDifficulty
}

public enum BotDifficulty
{
    Easy,
    Medium,
    Hard
}

public class OptionsScreen : IMenuScreen
{

    private Dictionary<BotDifficulty, string> BotDiffString;
    private readonly Session session;
    private MenuBackground background;
    private Texture2D baseTexture;
    private ShapeManager shapes;
    private ButtonManager buttons;
    private SliderColorMaker sliders;
    ScreenTypes[] connections;
    ScreenTypes parentScreen;
    MouseState prevState;
    MouseState state;
    ScreenManager screens;
    BotDifficulty difficulty;
    bool showSliders;

    int indexColorBeingSet;


    public OptionsScreen(Session currentSession, Texture2D _texture, ScreenManager screenManager)
    {
        fillDiffDictionary();

        baseTexture = _texture;

        session = currentSession;
        background = new(session, baseTexture);

        buttons = SetUpButtons();
        shapes = SetUpShapes();

        connections = getAllConnectedScreens();
        parentScreen = getParentScreen();

        prevState = Mouse.GetState();
        state = Mouse.GetState();

        sliders = new SliderColorMaker(baseTexture, (int) (session.Width * 0.8), (int) (session.Height * 0.3), 25, 0, (int) (session.Height * 0.45));

        showSliders = false;

        indexColorBeingSet = -1;
        screens = screenManager;

        difficulty = BotDifficulty.Easy;
    }

    private void fillDiffDictionary()
    {
        BotDiffString = new();
        BotDiffString.Add(BotDifficulty.Easy, "Easy");
        BotDiffString.Add(BotDifficulty.Medium, "Medium");
        BotDiffString.Add(BotDifficulty.Hard, "Hard");
    }

    public ScreenTypes[] getAllConnectedScreens()
    {
        return [ScreenTypes.None];
    }

    public ScreenTypes getParentScreen()
    {
        return ScreenTypes.Title;
    }

    public ButtonManager SetUpButtons()
    {
        int w = (int) session.Width;
        int h = (int) session.Height;

        ButtonManager newButtons = new();

        newButtons.AddButton(
            "Player 1",
            Color.Gray,
            Color.White,
            (int) (w * 0.15), 
            (int) (h * 0.3),
            w / 4, h / 8,
            ButtonFunctions.Player1Color
        );

        newButtons.AddButton(
            "Player 2",
            Color.Gray,
            Color.White,
            (int) (w * 0.15), 
            (int) (h * 0.45),
            w / 4, h / 8,
            ButtonFunctions.Player2Color
        );

        newButtons.AddButton(
            "Board 1",
            Color.Gray,
            Color.White,
            (int) (w * 0.15), 
            (int) (h * 0.6),
            w / 4, h / 8,
            ButtonFunctions.Board1Color
        );

        newButtons.AddButton(
            "Board 2",
            Color.Gray,
            Color.White,
            (int) (w * 0.15), 
            (int) (h * 0.75),
            w / 4, h / 8,
            ButtonFunctions.Board2Color
        );

        newButtons.AddButton(
            "Save Changes",
            Color.Gray,
            Color.White,
            (int) (w * 0.5), 
            (int) (h * 0.1),
            w / 4, h / 8,
            ButtonFunctions.SaveChanges
        );

        newButtons.AddButton(
            "Back",
            Color.Gray,
            Color.White,
            (int) (w * 0.8), 
            (int) (h * 0.1),
            w / 8, h / 8,
            ButtonFunctions.GoBack
        );

        newButtons.AddButton(
            "Bot difficulty: " + BotDiffString[difficulty],
            Color.Gray,
            Color.White,
            (int) (w * 0.7),
            (int) (h * 0.85),
            w / 4, h / 8,
            ButtonFunctions.ChangeDifficulty,
            scale: 0.8f
        );
        
        return newButtons;
    }

    public ShapeManager SetUpShapes()
    {
        ShapeManager newShapes = new(baseTexture);

        int w = (int) session.Width;
        int h = (int) session.Height;

        newShapes.AddShapes((int) (w * 0.5) - 10, (int) (h * 0.3) - 10, Color.Gray, (int) (h * 0.575) + 20, (int) (w * 0.125) + 20);

        newShapes.AddShapes((int) (w * 0.5), (int) (h * 0.3), session.userConfig.config.white_player_color.GetColor(), h / 8, w / 8);

        newShapes.AddShapes((int) (w * 0.5), (int) (h * 0.45), session.userConfig.config.black_player_color.GetColor(), h / 8, w / 8);

        newShapes.AddShapes((int) (w * 0.5), (int) (h * 0.6), session.userConfig.config.board_color_1.GetColor(), h / 8, w / 8);

        newShapes.AddShapes((int) (w * 0.5), (int) (h * 0.75), session.userConfig.config.board_color_2.GetColor(), h / 8, w / 8);

        return newShapes;
    }
    
    public void UpdateScreen(GameTime gameTime)
    {
        prevState = state;
        state = Mouse.GetState();
        
        session.bgColor = Color.Gray * 0.2f;

        background.update((float) gameTime.ElapsedGameTime.TotalSeconds);

        sliders.updateSliders(state);

        RunForButtons();
    }

    public void RunForButtons()
    {
        int selectedButton = buttons.checkForSelectedButtons(prevState, state);

        if (selectedButton != -1)
        {
            Button button = buttons.getButton(selectedButton);

            if (button.isClicked)
            {
                ButtonFunctions function = button.purpose;

                if (function == ButtonFunctions.SaveChanges)
                {
                    Color p1color = shapes.getSelectedShapes(1).currentColor;
                    ColorObject player1 = new(p1color.R, p1color.G, p1color.B, p1color.A);
                    Color p2color = shapes.getSelectedShapes(2).currentColor;
                    ColorObject player2 = new(p2color.R, p2color.G, p2color.B, p2color.A);
                    Color b1color = shapes.getSelectedShapes(3).currentColor;
                    ColorObject board1 = new(b1color.R, b1color.G, b1color.B, b1color.A);
                    Color b2color = shapes.getSelectedShapes(4).currentColor;
                    ColorObject board2 = new(b2color.R, b2color.G, b2color.B, b2color.A);

                    GameData config = new(player1, player2, board1, board2);

                    session.userConfig.SaveConfig(config);
                }
                else if (function == ButtonFunctions.Player1Color)
                {
                    showSliders = !showSliders;

                    if (showSliders)
                    {
                        indexColorBeingSet = 1;
                    }
                }
                else if (function == ButtonFunctions.Player2Color)
                {
                    showSliders = !showSliders;

                    if (showSliders)
                    {
                        indexColorBeingSet = 2;
                    }
                }
                else if (function == ButtonFunctions.Board1Color)
                {
                    showSliders = !showSliders;

                    if (showSliders)
                    {
                        indexColorBeingSet = 3;
                    }
                }
                else if (function == ButtonFunctions.Board2Color)
                {
                    showSliders = !showSliders;

                    if (showSliders)
                    {
                        indexColorBeingSet = 4;
                    }
                }
                else if (function == ButtonFunctions.GoBack)
                {
                    screens.currScreenType = parentScreen;
                    screens.UpdateScreenManager();
                }
                else if (function == ButtonFunctions.ChangeDifficulty)
                {
                    if (difficulty == BotDifficulty.Easy)
                    {
                        difficulty = BotDifficulty.Medium;
                        
                    }
                }
            }
        }

        if (showSliders)
        {
            shapes.getSelectedShapes(indexColorBeingSet).currentColor = sliders.color;
        }
    }

    public void DrawScreen(SpriteBatch spriteBatch)
    {
        SpriteFont font = session.LoadedFonts["Monocraft"];
        string title = "OPTIONS";

        Vector2 textSize = font.MeasureString(title);
        Vector2 origin = textSize / 2f;

        spriteBatch.DrawString(
            session.LoadedFonts["Monocraft"], 
            title, 
            new(session.Width * 0.25f, 
                session.Height * 0.1f), 
            Color.White,
            0f,
            origin,
            2f,
            SpriteEffects.None,
            0f
        );

        buttons.DrawButtons(spriteBatch, session.LoadedFonts["Monocraft"], baseTexture);
        shapes.DrawAll(spriteBatch);

        if(showSliders) sliders.drawSliders(spriteBatch);
    }   
}
public class ScreenManager
{
    Session session;
    Texture2D basetexture;
    Dictionary<ScreenTypes, IScreen> Screens;
    public ScreenTypes currScreenType;
    public IScreen currScreen;
    public ScreenManager(Session currSession, Texture2D texture, ScreenTypes initialScreen = ScreenTypes.Title)
    {
        session = currSession;
        basetexture = texture;
        currScreenType = initialScreen;

        Screens = CollectScreenTypes();

        currScreen = Screens[currScreenType];
    }

    private Dictionary<ScreenTypes, IScreen> CollectScreenTypes()
    {
        Dictionary<ScreenTypes, IScreen> screens = new();

        screens.Add(ScreenTypes.Game, new GameScreen(session, basetexture, this, PlayerType.Human, PlayerType.Human));
        screens.Add(ScreenTypes.Title, new TitleScreen(session, basetexture, this));
        screens.Add(ScreenTypes.Options, new OptionsScreen(session, basetexture, this));
        //screens.Add(ScreenTypes.SinglePlayer, new SinglePlayerScreen(session, basetexture));
        //screens.Add(ScreenTypes.Multiplayer, new MultiplayerScreen(session, basetexture));

        return screens;
    }

    public void UpdateScreenManager()
    {
        currScreen = Screens[currScreenType];
    }
}