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
                    bool movePlayed = session.Game.MakeHumanMove(session.PlayedMove);

                    if (movePlayed) session.SetState(GameState.MoveResolved);

                    session.UpdateMove(new moveCache(session.PlayedMove.start, -1));

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

        FindSelectedSquare();

        int isGameOver = session.Game.checkForGameOver();
        if (isGameOver == 1 || isGameOver == 2 || isGameOver == 3) session.SetState(GameState.GameOver);
    }

    public void DrawScreen(SpriteBatch _spriteBatch)
    {
        session.Back.drawParticles(_spriteBatch, _texture);

        session.Board.DrawBoard(
            batch: _spriteBatch, 
            baseTexture: _texture, 
            session: session
        );
    }

    private void FindSelectedSquare()
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

public class TitleScreen : IScreen
{
    private readonly Session session;
    private MenuBackground background;
    private Texture2D baseTexture;
    private ButtonManager buttons;

    public TitleScreen(Session currentSession, Texture2D _texture)
    {
        baseTexture = _texture;

        session = currentSession;
        background = new(session, baseTexture);

        buttons = new();
        setUpButtons();
    }

    private void setUpButtons()
    {
        buttons.AddButton(
            "Single Player", 
            Color.White, 
            Color.Black, 
            (int) (session.Width * 0.2), 
            (int) (session.Height * 0.45), 
            (int) (session.Width * 0.6), 
            (int) (session.Height * 0.1)
        );

        buttons.AddButton(
            "Multiplayer", 
            Color.White, 
            Color.Black, 
            (int) (session.Width * 0.2), 
            (int) (session.Height * 0.57), 
            (int) (session.Width * 0.6), 
            (int) (session.Height * 0.1)
        );
        
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

        buttons.DrawButtons(spriteBatch, session.LoadedFonts["Monocraft"], session.LoadedTextures["MenuButton"]);
    }
}

public class Button
{
    string ButtonContent;
    Rectangle Position;
    Color color;
    Color textColor;

    public Button(string BC, Color Color, Color tColor, int x, int y, int xDim, int yDim)
    {
        ButtonContent = BC;
        color = Color;
        textColor = tColor;
        Position = new Rectangle(x, y, xDim, yDim);
    }

    public void Draw(SpriteBatch batch, SpriteFont font, Texture2D texture)
    {
        Vector2 textSize = font.MeasureString(ButtonContent);
        Vector2 origin = textSize / 2f;
        
        Vector2 textPos = new(
            Position.X + (Position.Width / 2f), 
            Position.Y + (Position.Height / 2f));

        batch.Draw(texture, Position, color);

        batch.DrawString(font, ButtonContent, textPos, textColor, rotation: 0, origin, 1.25f, SpriteEffects.None, 0);
    }
}

public class ButtonManager
{
    List<Button> buttons;

    public ButtonManager()
    {
        buttons = new();
    }

    public void AddButton(string text, Color color, Color textColor, int x, int y, int xDim, int yDim)
    {
        buttons.Add(new(text, color, textColor, x, y, xDim, yDim));
    }

    public void DrawButtons(SpriteBatch batch, SpriteFont font, Texture2D texture)
    {
        foreach (Button button in buttons)
        {
            button.Draw(batch, font, texture);
        }
    }
}