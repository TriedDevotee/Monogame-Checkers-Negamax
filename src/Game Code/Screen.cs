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
}

public interface IMenuScreen : IScreen
{
    protected abstract ButtonManager SetUpButtons();
    protected abstract ScreenTypes[] getAllConnectedScreens();
    protected abstract ScreenTypes getParentScreen();
}

public class GameScreen : IScreen
{
    private Session session;
    private readonly Texture2D _texture;
    private MouseState mouseState;
    private bool leftClicked;
    private MouseState previousMouseState;

    public GameScreen(Session currentSession, Texture2D texture)
    {
        session = currentSession;
        _texture = texture;

        mouseState = new MouseState();
        previousMouseState = Mouse.GetState();

        session.SetState(GameState.MoveInput);
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

        if (session.Game.moves.WhiteTurn)
        {
            if (session.state == GameState.MoveInput)
            {
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
                    }
                }
            }
        }
        else
        {
            if (session.state == GameState.BotMoving)
            {
                session.Game.runForAI(session.Game.moves);
                session.SetState(GameState.MoveInput);
                UpdatePositionLog();
            }
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
            mouseState
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

    public TitleScreen(Session currentSession, Texture2D _texture)
    {
        baseTexture = _texture;

        session = currentSession;
        background = new(session, baseTexture);

        buttons = SetUpButtons();

        connections = getAllConnectedScreens();
        parentScreen = getParentScreen();
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
            (int) (session.Height * 0.1)
        );

        newButtons.AddButton(
            "Multiplayer", 
            Color.Gray, 
            Color.White, 
            (int) (session.Width * 0.2), 
            (int) (session.Height * 0.57), 
            (int) (session.Width * 0.6), 
            (int) (session.Height * 0.1)
        );

        newButtons.AddButton(
            "Options", 
            Color.Gray, 
            Color.White, 
            (int) (session.Width * 0.2), 
            (int) (session.Height * 0.69), 
            (int) (session.Width * 0.28), 
            (int) (session.Height * 0.1)
        );

        newButtons.AddButton(
            "Quit Game", 
            Color.Gray, 
            Color.White, 
            (int) (session.Width * 0.52), 
            (int) (session.Height * 0.69), 
            (int) (session.Width * 0.28), 
            (int) (session.Height * 0.1)
        );
        
        return newButtons;
    }
    
    public void UpdateScreen(GameTime gameTime)
    {
        background.update((float) gameTime.ElapsedGameTime.TotalSeconds);
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
}

public enum SliderSetting
{
    Player1,
    Player2,
    Board1,
    Board2
}

public class OptionsScreen : IMenuScreen
{
    private readonly Session session;
    private MenuBackground background;
    private Texture2D baseTexture;
    private ShapeManager shapes;
    private ButtonManager buttons;
    private SliderColorMaker sliders;
    ScreenTypes[] connections;
    ScreenTypes parentScreen;
    MouseState state;
    bool showSliders;


    public OptionsScreen(Session currentSession, Texture2D _texture)
    {
        baseTexture = _texture;

        session = currentSession;
        background = new(session, baseTexture);

        buttons = SetUpButtons();
        shapes = SetUpShapes();

        connections = getAllConnectedScreens();
        parentScreen = getParentScreen();

        state = Mouse.GetState();
        sliders = new SliderColorMaker(baseTexture, (int) (session.Width * 0.8), (int) (session.Height * 0.3), 25, 0, (int) (session.Height * 0.45));

        showSliders = true;
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
        int w = (int) session.Width;
        int h = (int) session.Height;

        ButtonManager newButtons = new();

        newButtons.AddButton(
            "Player 1",
            Color.Gray,
            Color.White,
            (int) (w * 0.15), 
            (int) (h * 0.3),
            w / 4, h / 8
        );

        newButtons.AddButton(
            "Player 2",
            Color.Gray,
            Color.White,
            (int) (w * 0.15), 
            (int) (h * 0.45),
            w / 4, h / 8
        );

        newButtons.AddButton(
            "Board 1",
            Color.Gray,
            Color.White,
            (int) (w * 0.15), 
            (int) (h * 0.6),
            w / 4, h / 8
        );

        newButtons.AddButton(
            "Board 2",
            Color.Gray,
            Color.White,
            (int) (w * 0.15), 
            (int) (h * 0.75),
            w / 4, h / 8
        );

        newButtons.AddButton(
            "Save Changes",
            Color.Gray,
            Color.White,
            (int) (w * 0.5), 
            (int) (h * 0.1),
            w / 4, h / 8
        );

        newButtons.AddButton(
            "Back",
            Color.Gray,
            Color.White,
            (int) (w * 0.8), 
            (int) (h * 0.1),
            w / 8, h / 8
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
        state = Mouse.GetState();
        
        session.bgColor = Color.Gray * 0.2f;

        background.update((float) gameTime.ElapsedGameTime.TotalSeconds);

        sliders.updateSliders(state);
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
public class SinglePlayerScreen : IMenuScreen
{
    private readonly Session session;
    private MenuBackground background;
    private Texture2D baseTexture;
    private ButtonManager buttons;
    ScreenTypes[] connections;
    ScreenTypes parentScreen;

    public SinglePlayerScreen(Session currentSession, Texture2D _texture)
    {
        baseTexture = _texture;

        session = currentSession;
        background = new(session, baseTexture);

        buttons = SetUpButtons();

        connections = getAllConnectedScreens();
        parentScreen = getParentScreen();
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
            (int) (session.Height * 0.1)
        );

        newButtons.AddButton(
            "Multiplayer", 
            Color.Gray, 
            Color.White, 
            (int) (session.Width * 0.2), 
            (int) (session.Height * 0.57), 
            (int) (session.Width * 0.6), 
            (int) (session.Height * 0.1)
        );

        newButtons.AddButton(
            "Options", 
            Color.Gray, 
            Color.White, 
            (int) (session.Width * 0.2), 
            (int) (session.Height * 0.69), 
            (int) (session.Width * 0.28), 
            (int) (session.Height * 0.1)
        );

        newButtons.AddButton(
            "Quit Game", 
            Color.Gray, 
            Color.White, 
            (int) (session.Width * 0.52), 
            (int) (session.Height * 0.69), 
            (int) (session.Width * 0.28), 
            (int) (session.Height * 0.1)
        );
        
        return newButtons;
    }
    
    public void UpdateScreen(GameTime gameTime)
    {
        background.update((float) gameTime.ElapsedGameTime.TotalSeconds);
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
}
public class MultiplayerScreen : IMenuScreen
{
    private readonly Session session;
    private MenuBackground background;
    private Texture2D baseTexture;
    private ButtonManager buttons;
    ScreenTypes[] connections;
    ScreenTypes parentScreen;

    public MultiplayerScreen(Session currentSession, Texture2D _texture)
    {
        baseTexture = _texture;

        session = currentSession;
        background = new(session, baseTexture);

        buttons = SetUpButtons();

        connections = getAllConnectedScreens();
        parentScreen = getParentScreen();
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
            (int) (session.Height * 0.1)
        );

        newButtons.AddButton(
            "Multiplayer", 
            Color.Gray, 
            Color.White, 
            (int) (session.Width * 0.2), 
            (int) (session.Height * 0.57), 
            (int) (session.Width * 0.6), 
            (int) (session.Height * 0.1)
        );

        newButtons.AddButton(
            "Options", 
            Color.Gray, 
            Color.White, 
            (int) (session.Width * 0.2), 
            (int) (session.Height * 0.69), 
            (int) (session.Width * 0.28), 
            (int) (session.Height * 0.1)
        );

        newButtons.AddButton(
            "Quit Game", 
            Color.Gray, 
            Color.White, 
            (int) (session.Width * 0.52), 
            (int) (session.Height * 0.69), 
            (int) (session.Width * 0.28), 
            (int) (session.Height * 0.1)
        );
        
        return newButtons;
    }
    
    public void UpdateScreen(GameTime gameTime)
    {
        background.update((float) gameTime.ElapsedGameTime.TotalSeconds);
    }

    public void DrawScreen(SpriteBatch spriteBatch)
    {
        //background.Draw(spriteBatch);


        buttons.DrawButtons(spriteBatch, session.LoadedFonts["Monocraft"], baseTexture);
    }   
}

public class ScreenManager
{
    Session session;
    Texture2D basetexture;
    Dictionary<ScreenTypes, IScreen> Screens;
    ScreenTypes currScreenType;
    IScreen currScreen;
    ScreenTypes[] currScreenConnections;
    ScreenTypes PreviousScreen;

    public ScreenManager(Session currSession, Texture2D texture, ScreenTypes initialScreen = ScreenTypes.Title)
    {
        session = currSession;
        basetexture = texture;
        currScreenType = initialScreen;

        Screens = CollectScreenTypes();
    }

    private Dictionary<ScreenTypes, IScreen> CollectScreenTypes()
    {
        Dictionary<ScreenTypes, IScreen> screens = new();

        screens.Add(ScreenTypes.Game, new GameScreen(session, basetexture));
        screens.Add(ScreenTypes.Title, new TitleScreen(session, basetexture));
        screens.Add(ScreenTypes.Options, new OptionsScreen(session, basetexture));
        screens.Add(ScreenTypes.SinglePlayer, new SinglePlayerScreen(session, basetexture));
        screens.Add(ScreenTypes.Multiplayer, new MultiplayerScreen(session, basetexture));

        return screens;
    }
}