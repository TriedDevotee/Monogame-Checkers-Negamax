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

namespace Comp_Sci_NEA;

public enum GameState
{
    MoveInput,
    WaitingForBranchInput,
    BotMoving,
    GameOver
}

public struct userMove
{
    int startSquare;
    int moveSquare;

    public userMove(int start, int move)
    {
        startSquare = start;
        moveSquare = move;
    }
}
public struct ShapeBound
{
    public int startX;
    public int startY;
    public int farX;
    public int farY;

    public ShapeBound(int sx, int sy, int fx, int fy)
    {
        startX = sx;
        startY = sy;
        farX = fx;
        farY = fy;
    }
}

public struct moveCache
{
    public int start;
    public int moveTo;
    int indexFilling;

    public moveCache(int s = -1, int m = -1)
    {
        start = s;
        moveTo = m;

        indexFilling = 0;
    }

    public void setValue(int square)
    {
        if (start == square && start != -1)
        {
            start = -1;
            moveTo = -1;
            indexFilling = 0;
            return;
        }

        if (indexFilling == 0)
        {
            start = square;
            moveTo = -1;
            indexFilling = 1;
            return;
        }

        if (indexFilling == 1)
        {
            moveTo = square;
            indexFilling = 0;
            return;
        }
    }

    public void clearCache()
    {
        start = -1;
        moveTo = -1;
        indexFilling = 0;
    }
}

public class GameManager
{
    public moveCache move;
    public Main newGame;


}
public struct Shape
{
    public Texture2D texture;
    public Rectangle shapeObj;
    public Color currentColor;
    public Vector2 position;
    public int index;
    public bool isClicked;
    public bool isSelected;

    public Shape(Texture2D newTexture, int x, int y, Color newCol, int i, int height = 100, int width = 100)
    {
        texture = newTexture;
        shapeObj = new Rectangle(x, y, height, width);
        currentColor = newCol;
        position = new Vector2(x, y);
        index = i;
        isClicked = false;
        isSelected = false;
    }
}
public class Board
{
    public int heightNum;
    public int widthNum;
    int height;
    int width;
    public Shape[][] boardStore;
    public ShapeBound[][] shapeBounds;
    Vector2 position;
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

        populateCheckersBoard();

    }

    public void populateCheckersBoard()
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

    


}

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    public Texture2D _texture;
    public Board newBoard;
    public MouseState mouse;
    public SpriteFont testFont;
    public bool validClickChecker;
    public int squareDispley;
    public moveCache testCache;
    public Song TestMusic;
    public SoundEffect moveSound;

    public Main main;
    public GameState state;

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

        newBoard = new Board(_texture);

        mouse = new MouseState();

        base.Initialize();

        squareDispley = -1;
        testCache = new moveCache(-1, -1);
        validClickChecker = true;
    
        TestMusic = Content.Load<Song> ("ForWhomTheBellTollsRemastered");

        moveSound = Content.Load<SoundEffect> ("ScreamNoise");

        //MediaPlayer.Play(TestMusic);

        main = new Main();

        state = GameState.MoveInput;
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        testFont = Content.Load<SpriteFont> ("Monocraft");

        Viewport viewport = _graphics.GraphicsDevice.Viewport;
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        mouse = Mouse.GetState();

        if (mouse.LeftButton == ButtonState.Released)
        {
            validClickChecker = true;
        }

        //Finds which squares are covered by mouse
        FindSelectedSquare();

        if (MediaPlayer.State == MediaState.Stopped && Keyboard.GetState().IsKeyDown(Keys.M))
        {
            MediaPlayer.Play(TestMusic);
        }

        if (MediaPlayer.State == MediaState.Playing && Keyboard.GetState().IsKeyDown(Keys.N))
        {
            MediaPlayer.Stop();
        }

        if (state != GameState.GameOver){

            if (main.moves.whiteTurn)
            {
                if (testCache.start != -1 && testCache.moveTo != -1)
                {
                    state = GameState.MoveInput;
                    main.makeHumanMove(testCache);
                }
            } else
            {
                state = GameState.BotMoving;
                main.runForAI(main.moves);
            }

            int isGameOver = main.checkForGameOver();

            if (isGameOver == 1 || isGameOver == 2) state = GameState.GameOver;
        }




        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

        DrawBoard();

        string output = Convert.ToString(testCache.start);
        string output2 = Convert.ToString(testCache.moveTo);

        Vector2 fontOrigin = testFont.MeasureString(output) / 2;
        Vector2 fontPos = new Vector2(600, 100);

        _spriteBatch.DrawString(testFont, output, fontPos, Color.Black, 0, fontOrigin, 5.0f, SpriteEffects.None, 0.5f);

        fontOrigin = testFont.MeasureString(output2) / 2;
        fontPos = new Vector2(600, 300);

        _spriteBatch.DrawString(testFont, output2, fontPos, Color.Black, 0, fontOrigin, 5.0f, SpriteEffects.None, 0.5f);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    public void DrawBoard()
    {
        Color selectedColor = Color.Blue;
        Color clickedColor = Color.Red;
        Color moveStart = Color.Lime;
        Color moveEnd = Color.Purple;

        int moveStartIndex = testCache.start;
        int moveEndIndex = testCache.moveTo;

        for (int i = 0; i < newBoard.heightNum; i++)
        {
            for (int j = 0; j < newBoard.widthNum; j++)
            {
                Shape currentShape = newBoard.boardStore[i][j];

                if (currentShape.isSelected)
                {
                    if (currentShape.isClicked)
                    {
                        _spriteBatch.Draw(currentShape.texture, currentShape.shapeObj, clickedColor);
                    } else 
                    {
                        _spriteBatch.Draw(currentShape.texture, currentShape.shapeObj, selectedColor);
                    }
                } else
                {
                    _spriteBatch.Draw(currentShape.texture, currentShape.shapeObj, currentShape.currentColor);
                }

                if (currentShape.index == moveStartIndex && moveStartIndex >= 0)
                {
                    _spriteBatch.Draw(currentShape.texture, currentShape.shapeObj, moveStart);
                }

                if (currentShape.index == moveEndIndex && moveEndIndex >= 0)
                {
                    _spriteBatch.Draw(currentShape.texture, currentShape.shapeObj, moveEnd);
                }

                drawPieces(i, j);


            }
        }
    }

    public void FindSelectedSquare()
    {
        for (int i = 0; i < newBoard.heightNum; i++)
        {
            for (int j = 0; j < newBoard.widthNum; j++)
            {
                ShapeBound bound = newBoard.shapeBounds[i][j];
                if (mouse.Position.X >= bound.startX 
                    && mouse.Position.X < bound.farX 
                    && mouse.Position.Y >= bound.startY 
                    && mouse.Position.Y < bound.farY)
                {
                    newBoard.boardStore[i][j].isSelected = true;

                    if (mouse.LeftButton == ButtonState.Pressed && validClickChecker)
                    {
                        validClickChecker = false;

                        newBoard.boardStore[i][j].isClicked = true;

                        testCache.setValue(newBoard.boardStore[i][j].index);


                        moveSound.Play();
                    } else
                    {
                        newBoard.boardStore[i][j].isClicked = false;
                    }
                } 
                else
                {
                    newBoard.boardStore[i][j].isSelected = false;
                }
            }
        }
    }

    public void drawPieces(int squareY, int squareX)
    {
        Bitboard whitePieces = main.moves.whitePieces;
        Bitboard blackPieces = main.moves.blackPieces;
        Bitboard kings = main.moves.kings;

        Rectangle pieceDims = newBoard.boardStore[squareY][squareX].shapeObj;

        pieceDims.X += 5;
        pieceDims.Y += 5;
        pieceDims.Height -= 10;
        pieceDims.Width -= 10;

        int squareID = 63 - (squareY *8 + squareX); 

        if (whitePieces.isSquareUsed(squareID))
        {
            if (kings.isSquareUsed(squareID))
            {
                _spriteBatch.Draw(_texture, pieceDims, Color.Yellow);
            } else
            {
                _spriteBatch.Draw(_texture, pieceDims, Color.SkyBlue);
            }
        }

        if (blackPieces.isSquareUsed(squareID))
        {
            if (kings.isSquareUsed(squareID))
            {
                _spriteBatch.Draw(_texture, pieceDims, Color.Orange);
            } else
            {
                _spriteBatch.Draw(_texture, pieceDims, Color.Maroon);
            }
        }
    }
}
