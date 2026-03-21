using Comp_Sci_NEA;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Checkers;
using System.Linq;
using Microsoft.Xna.Framework.Input;

/// <summary>
/// Stores all the available screen types.
/// Note: There is a None type which has no associated screen. 
/// There is safeguards in place to prevent issues with that.
/// </summary>
public enum ScreenTypes
{
    None,
    Game,
    Title,
    SinglePlayer,
    MultiplayerGame,
    Options,
    GameOver
}

/// <summary>
/// Details all the different ways the game can end for the end game pipeline.
/// Note: None is ran when there is no game over.
/// Stored in Session.
/// </summary>
public enum GameOverState
{
    None,
    Player1Win,
    Player2Win,
    BotWin,
    Draw
}

/// <summary>
/// Base screen interface. Templates for Updating, Drawing and producing the screen's Parent
/// </summary>
public interface IScreen
{
    public abstract void UpdateScreen(GameTime gameTime);
    public abstract void DrawScreen(SpriteBatch spriteBatch);
    public abstract ScreenTypes getParentScreen();
}

/// <summary>
/// Specified for Menu based screens with buttons.
/// Ensures all the button logic is present.
/// </summary>
public interface IMenuScreen : IScreen
{
    protected abstract ButtonManager SetUpButtons();
    protected abstract ScreenTypes[] getAllConnectedScreens();
    protected abstract void RunForButtons();
}

/// <summary>
/// Works out if a player is a human or AI.
/// Designed to be expandable.
/// </summary>
public enum PlayerType
{
    Bot,
    Human
}

/// <summary>
/// Handles all game logic behind the rendering, input and interfacing with the backend. 
/// Uitlised twice in a singleplayer and multiplayer instance.
/// Tracks player types to distinguish this, and edits the rendering due to this matter.
/// </summary>
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

    /// <summary>
    /// Hardcoded to Title screen. 
    /// </summary>
    /// <returns></returns>
    public ScreenTypes getParentScreen()
    {
        return ScreenTypes.Title;
    }

    /// <summary>
    /// Updates all the necessary components. 
    /// Updates the mouse states, followed by the board, calculates all the moves form either the payer or the bot.
    /// Also inverts the flip variable to ensure the board flips correctly (in multiplayer game).
    /// Finally backs up the position in the position log.
    /// </summary>
    /// <param name="gameTime"></param>
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
            session.setPlayerColor(session.Game.moves.WhiteTurn);
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
        if (isGameOver == 1)
        {
            if (player1 == PlayerType.Human){
                session.gameOverState = GameOverState.Player1Win;
            }
            else
            {
                session.gameOverState = GameOverState.BotWin;
            }
        } else if (isGameOver == 2)
        {
            if (player2 == PlayerType.Human){
                session.gameOverState = GameOverState.Player2Win;
            }
            else
            {
                session.gameOverState = GameOverState.BotWin;
            }
        } else if (isGameOver == 3)
        {
            session.gameOverState = GameOverState.Draw;
        }

        if (isGameOver != 0)
        {
            screens.currScreenType = ScreenTypes.GameOver;
            screens.UpdateScreenManager();
        }
    }

    /// <summary>
    /// Draws the screen and all associated UI components.
    /// Draws the background, board amnd pieces.
    /// This is where all the Draw() methods are called.
    /// </summary>
    /// <param name="_spriteBatch"></param>
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

    /// <summary>
    /// Helper method to ensure the positions are added to the log easily
    /// </summary>
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

/// <summary>
/// Title screen class. This is what is used as the parent for most classes, 
/// and also is the entry point to the application.
/// </summary>
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

    /// <summary>
    /// Returns all connected screens. Unused (unnecessary)
    /// </summary>
    /// <returns></returns>
    public ScreenTypes[] getAllConnectedScreens()
    {
        return [ScreenTypes.Options, ScreenTypes.SinglePlayer, ScreenTypes.MultiplayerGame];
    }

    /// <summary>
    /// Returns the parent. Note: Here parent is None. This is accounted for in Game1
    /// </summary>
    /// <returns></returns>
    public ScreenTypes getParentScreen()
    {
        return ScreenTypes.None;
    }

    /// <summary>
    /// Assigns the values to all of the buttons which exist within the screen. These include:<br/>
    ///  - Single player button<br/>
    ///  - Multiplayer button<br/>
    ///  - Options button<br/>
    ///  - Quit button<br/>
    /// </summary>
    /// <returns></returns>
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
    
    /// <summary>
    /// Updates all of the buttons, modifies the internal mouse state and updates the background
    /// </summary>
    /// <param name="gameTime"></param>
    public void UpdateScreen(GameTime gameTime)
    {
        background.update((float) gameTime.ElapsedGameTime.TotalSeconds);

        prevState = state;
        state = Mouse.GetState();

        RunForButtons();
    }

    /// <summary>
    /// Draws the UI components. This is all the UI buttons, the logos and the background. 
    /// Additionally, this is the only method that handles the texture loading within the object.
    /// </summary>
    /// <param name="spriteBatch"></param>
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

    /// <summary>
    /// Allows for all the button's functionalities. #
    /// These run in an else if statement.
    /// </summary>
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
                else if (function == ButtonFunctions.MultiplayerMenu)
                {
                    screens.currScreenType = ScreenTypes.MultiplayerGame;
                    screens.UpdateScreenManager();
                }
            }
        }
    }
}

/// <summary>
/// Details all the functions of the buttons. 
/// The main use of this is to prevent "magic" values being thrown around :D.
/// Note: None means null function. 
/// This is used purely as a placeholder as implementing methods will never implement None.
/// </summary>
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

/// <summary>
/// Enum that determines the bot's difficulty. 
/// Again, used purely to avoid the existance of "magic" numbers
/// </summary>
public enum BotDifficulty
{
    Easy,
    Medium,
    Hard
}

/// <summary>
/// Implementation of the IMenuScreen interface.
/// Used to update and render the options UI, which is used to change values about the game.
/// These are the piece colors, board colors and the difficulty of the bot.
/// Note: Only the color changes are backed up to the config.json file
/// </summary>
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

    /// <summary>
    /// Sets up the difficulties dictionary with the enumeration and the associated string, ready for rendering the button.
    /// </summary>
    private void fillDiffDictionary()
    {
        BotDiffString = new();
        BotDiffString.Add(BotDifficulty.Easy, "Easy");
        BotDiffString.Add(BotDifficulty.Medium, "Medium");
        BotDiffString.Add(BotDifficulty.Hard, "Hard");
    }

    /// <summary>
    /// Unused method.
    /// </summary>
    /// <returns></returns>
    public ScreenTypes[] getAllConnectedScreens()
    {
        return [ScreenTypes.None];
    }

    /// <summary>
    /// Returns ScreenTypes.Title to allow for the game to seek back to the Title when certain conditions are met.
    /// </summary>
    /// <returns></returns>
    public ScreenTypes getParentScreen()
    {
        return ScreenTypes.Title;
    }

    /// <summary>
    /// Assign all of the buttons to the ButtonManager. This includes:<br/> 
    ///  - Player1 button<br/> 
    ///  - Player2 button<br/> 
    ///  - Board1 button<br/> 
    ///  - Board2 button<br/> 
    ///  - Difficulty button<br/> 
    ///  - Save Changes button<br/> 
    ///  - Back button<br/> 
    /// </summary>
    /// <returns></returns>
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

    /// <summary>
    /// Assigns all the necessary shapes to the ShapeManager object.
    /// These will be the colors that demo the pieces new selected color.
    /// Additionally, I added the background for these colors here.
    /// </summary>
    /// <returns></returns>
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
    
    /// <summary>
    /// Runs all necessary updates.
    /// This includes updating the mouse state, setting the background color, 
    /// updating the GameBackground, updating the sliders and running button functions.
    /// </summary>
    /// <param name="gameTime"></param>
    public void UpdateScreen(GameTime gameTime)
    {
        prevState = state;
        state = Mouse.GetState();
        
        session.setBgColor(Color.Gray * 0.2f);

        background.update((float) gameTime.ElapsedGameTime.TotalSeconds);

        if (showSliders){
            sliders.updateSliders(state);
        }

        RunForButtons();
    }

    /// <summary>
    /// Runs all button functions from the selected buttons in the ButtonManager. 
    /// Stored in an if-else statement.
    /// Note: this is where indexColorBeingSet is used, to determine which index of the shapeManager class is having the color set.
    /// This is a fragile system, that relies on not tampering with the setup of the shapes. 
    /// In future, modify to use some determiner, probably with an enum.
    /// Note 2: To allow for the button's text to change with difficulty, I add a new button to the list. 
    /// May be more efficient to pop, but that runs a risk of losing key components. 
    /// Slight memory leak though if button is pushed too many times (its a lot though)
    /// </summary>
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
                    else if (difficulty == BotDifficulty.Medium)
                    {
                        difficulty = BotDifficulty.Hard;
                    }
                    else
                    {
                        difficulty = BotDifficulty.Easy;
                    }

                    buttons.AddButton(
                        "Bot difficulty: " + BotDiffString[difficulty],
                        Color.Gray,
                        Color.White,
                        (int) (session.Width * 0.7),
                        (int) (session.Height * 0.85),
                        (int) session.Width / 4, (int) session.Height / 8,
                        ButtonFunctions.ChangeDifficulty,
                        scale: 0.8f
                    );

                    int depth = difficulty == BotDifficulty.Easy ? 7 : 9;
                    depth = difficulty == BotDifficulty.Medium ? 9 : 11;

                    session.setDepth(depth);

                    session.ResetGame();
                }
            }
        }

        //Toggles on and off when a color setting button is pressed.
        if (showSliders)
        {
            shapes.getSelectedShapes(indexColorBeingSet).currentColor = sliders.color;
        }
    }

    /// <summary>
    /// Draws all necessary UI components. 
    /// This includes the screen title, all the buttons, all the shapes and if the sliders are visible, them too
    /// </summary>
    /// <param name="spriteBatch"></param>
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

/// <summary>
/// Screen that is displayed at the end of the game.
/// </summary>
public class EndScreen : IMenuScreen
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

    public EndScreen(Session currentSession, Texture2D _texture, ScreenManager screenManager)
    {
        baseTexture = _texture;

        session = currentSession;
        screens = screenManager;
        background = new(session, baseTexture);

        buttons = SetUpButtons();

        connections = getAllConnectedScreens();
        parentScreen = getParentScreen();


        prevState = Mouse.GetState();
        state = Mouse.GetState();

        background = new(session, baseTexture);
    }

    /// <summary> 
    /// Draws all the necessary UI components, 
    /// such as the buttons (which are set up here (allows for local modifying)),
    /// and the end logo
    /// </summary>
    /// <param name="spriteBatch"></param>
    public void DrawScreen(SpriteBatch spriteBatch)
    {
        buttons = SetUpButtons();

        background.Draw(spriteBatch);

        Rectangle EndLogoShape = new(
            (int) (session.Width * 0.15), 
            (int) (session.Height * 0.1), 
            (int) (session.Width * 0.7), 
            (int) (session.Height * 0.3)
        );

        Texture2D endLogo = session.LoadedTextures["GameOver"];

        spriteBatch.Draw(endLogo, EndLogoShape, Color.White);

        SpriteFont buttonFont = session.LoadedFonts["Monocraft"];
        buttons.DrawButtons(spriteBatch, buttonFont, baseTexture);
    }

    /// <summary>
    /// Unused method
    /// </summary>
    /// <returns></returns>
    public ScreenTypes[] getAllConnectedScreens()
    {
        return [];
    }

    /// <summary>
    /// Returns ScreenTypes.Title, so the program knows where to backtrack too
    /// </summary>
    /// <returns></returns>
    public ScreenTypes getParentScreen()
    {
        return ScreenTypes.Title;
    }

    /// <summary>
    /// Runs all the functions of the buttons. 
    /// In this case, that is exclusively to go back to the title screen.
    /// </summary>
    public void RunForButtons()
    {
        int selectedButton = buttons.checkForSelectedButtons(prevState, state);

        if (selectedButton != -1)
        {
            Button button = buttons.getButton(selectedButton);

            if (button.isClicked)
            {
                ButtonFunctions function = button.purpose;
        

                if (function == ButtonFunctions.GoBack)
                {
                    screens.currScreenType = parentScreen;
                    screens.UpdateScreenManager();
                }
            }
        }
    }

    /// <summary>
    /// Assigns all the buttons to the ButtonManager object.
    /// This is a button that returns to the title screen, and one that does nothing (but has the result text).
    /// </summary>
    /// <returns></returns>
    public ButtonManager SetUpButtons()
    {
        string statusMessage = GetGameStatus();

        ButtonManager manager = new();
        manager.AddButton(
            "Go Home",
            Color.Gray,
            Color.White,
            (int) (session.Width * 0.25), 
            (int) (session.Height * 0.7),
            (int) (session.Width * 0.5), (int) (session.Height * 0.1),
            ButtonFunctions.GoBack
        );

        manager.AddButton(
            statusMessage,
            Color.Gray,
            Color.White,
            (int) (session.Width * 0.25), 
            (int) (session.Height * 0.5),
            (int) (session.Width * 0.5), (int) (session.Height * 0.1),
            ButtonFunctions.None
        );

        return manager;
    }

    /// <summary>
    /// Returns the result of the game as a result of the GameOverState, stored in Session.
    /// Note: Has a safeguard in place where if the screen was switched too soon and the state is None, 
    /// it returns to the title (and returns an empty string)
    /// </summary>
    /// <returns></returns>
    private string GetGameStatus()
    {
        GameOverState state = session.gameOverState;

        if (state == GameOverState.None)
        {
            screens.currScreenType = ScreenTypes.Title;
            screens.UpdateScreenManager();
            return string.Empty;
        } 
        else if (state == GameOverState.Player1Win)
        {
            return "Player 1 wins!";
        }
        else if (state == GameOverState.Player2Win)
        {
            return "Player 2 wins!";
        }
        else if (state == GameOverState.BotWin)
        {
            return "AI wins!";
        } 
        else
        {
            return "Draw!";
        }
    }

    /// <summary>
    /// Runs all the necessary updates. 
    /// This just does button functions, updates mouse movements and updates the background.
    /// </summary>
    /// <param name="gameTime"></param>
    public void UpdateScreen(GameTime gameTime)
    {
        prevState = state;
        state = Mouse.GetState();

        background.update((float) gameTime.ElapsedGameTime.TotalSeconds);

        RunForButtons();
    }
}

/// <summary>
/// Manager class for the screen system. 
/// Tracks the current screen and the type of screen it is.
/// </summary>
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

    /// <summary>
    /// Gives a screen to each screenType. 
    /// Mostly used in debugging, but also used in instantiation, when no screens are defined yet.
    /// </summary>
    /// <returns></returns>
    private Dictionary<ScreenTypes, IScreen> CollectScreenTypes()
    {
        Dictionary<ScreenTypes, IScreen> screens = new();

        screens.Add(ScreenTypes.Game, new GameScreen(session, basetexture, this, PlayerType.Human, PlayerType.Bot));
        screens.Add(ScreenTypes.Title, new TitleScreen(session, basetexture, this));
        screens.Add(ScreenTypes.Options, new OptionsScreen(session, basetexture, this));
        screens.Add(ScreenTypes.MultiplayerGame, new GameScreen(session, basetexture, this, PlayerType.Human, PlayerType.Human));
        screens.Add(ScreenTypes.GameOver, new EndScreen(session, basetexture, this));
        return screens;
    }

    /// <summary>
    /// Takes the current screen type (which is often set externally) and updates the current screen to match that. 
    /// </summary>
    public void UpdateScreenManager()
    {
        if (currScreenType == ScreenTypes.Game || currScreenType == ScreenTypes.MultiplayerGame)
        {
            session.ResetGame();

            PlayerType p2 = currScreenType == ScreenTypes.Game ? PlayerType.Bot : PlayerType.Human;
            currScreen = new GameScreen(session, basetexture, this, PlayerType.Human, p2);
        }
        else if (currScreenType == ScreenTypes.Title)
        {
            currScreen = new TitleScreen(session, basetexture, this);
        }
        else if (currScreenType == ScreenTypes.Options)
        {
            currScreen = new OptionsScreen(session, basetexture, this);
        }
        else if (currScreenType == ScreenTypes.GameOver)
        {
            currScreen = new EndScreen(session, basetexture, this);
        }
    }
}