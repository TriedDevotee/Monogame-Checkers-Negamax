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

    /*private void FindSelectedSquare()
    {
        for (int i = 0; i < session.Board.heightNum; i++)
        {
            for (int j = 0; j < session.Board.widthNum; j++)
            {
                ShapeBound bound = session.Board.shapeBounds[i][j];

                bool isSelected = mouseState.Position.X >= bound.startX 
                    && mouseState.Position.X < bound.farX 
                    && mouseState.Position.Y >= bound.startY 
                    && mouseState.Position.Y < bound.farY;

                session.Board.boardStore[i][j].isSelected = isSelected;

                bool IsClickableSquare = session.Game.displayBoard[i][j].isFull && session.PlayedMove.start == -1 && session.IsPlayerWhite == session.Game.displayBoard[i][j].isWhite;

                if (leftClicked && isSelected)
                {
                    if (IsClickableSquare || session.PlayedMove.start != -1){
                        session.Board.boardStore[i][j].isClicked = true;
                        session.AddToMove(session.Board.boardStore[i][j].index);
                    } else
                    {
                        session.UpdateMove(new());
                    }
                } else
                {
                    session.Board.boardStore[i][j].isClicked = false;
                }
            }
        }
    }*/

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

public class OptionsScreen : IMenuScreen
{
    private readonly Session session;
    private MenuBackground background;
    private Texture2D baseTexture;
    private ButtonManager buttons;
    ScreenTypes[] connections;
    ScreenTypes parentScreen;

    public OptionsScreen(Session currentSession, Texture2D _texture)
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