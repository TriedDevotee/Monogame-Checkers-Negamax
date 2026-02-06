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

//using System.Drawing;


namespace Comp_Sci_NEA;

public enum GameState
{
    MoveInput,
    WaitingForBranchInput,
    BotMoving,
    GameOver,
    BrowsingPastPositions,
}

public struct shapeColor
{
    public float red;
    public float green;
    public float blue;

    public shapeColor(float r = 255.0f, float g = 255.0f, float b = 255.0f)
    {
        red = r;
        green = g;
        blue = b;
    }
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

public class Particle
{
    Texture2D particleTexture;
    float acceleration;
    Vector2 velocity;
    Vector2 position;
    public float life;
    float maxLife;
    float angle;
    float opacity;
    float speed;
    float size;
    int screenWidth;
    int screenHeight;

    public Particle(Texture2D texture, 
                    float startX = 0.0f, 
                    float startY = 0.0f, 
                    float accMag = 2.0f, 
                    float velMag = 10.0f, 
                    float inputLife = 100.0f,  
                    float inputAngle = 45,
                    float startSize = 1.0f,
                    float startOpacity = 1.0f)
    {
        position = new Vector2(startX, startY);

        velocity = new Vector2(velMag, velMag);
        speed =  velMag;
        acceleration = accMag;

        life = inputLife;
        maxLife = inputLife;

        angle = inputAngle;

        screenWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
        screenHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

        particleTexture = texture;

        size = startSize;
        opacity = startOpacity;
    }

    public void updateParticle(float dt)
    {
        //Do velocity update
        speed += acceleration * dt;

        velocity.X = MathF.Cos(angle) * speed;
        velocity.Y = MathF.Sin(angle) * speed;

        //Do position update
        position += velocity * dt ;

        //Update life
        life -= dt;

        if (isOOB())
        {
            life = 0;
        }

        size += 0.03f;

    }

    public bool isOOB()
    {
        return position.X < -50 || position.X > screenWidth + 50 || position.Y < -50 || position.Y > screenHeight + 50;
    }

    public void Draw(SpriteBatch sprites, Texture2D texture, Vector2 transformer)
    {

        if (life > maxLife * 0.9f) return;

        float alpha = opacity * (life / maxLife); 

        //alpha = 1.0f;

        Color color;
        float layer = 0.8f;
        
        //color = new Color((MathF.Cos(angle) + 1f) * 0.5f, (MathF.Cos(angle + 2.094f) + 1f) * 0.5f, (MathF.Cos(angle + 4.188f) + 1f) * 0.5f) * alpha ;

        color = new Color((MathF.Cos(life) + 1f) * 0.5f, (MathF.Cos(life + 2.094f) + 1f) * 0.5f, (MathF.Cos(life + 4.188f) + 1f) * 0.5f) * alpha ;

        //color = Color.Magenta;

        sprites.Draw(
            texture, 
            position + transformer,
            null, 
            color,
            angle, 
            new Vector2(texture.Width / 2f, texture.Height / 2f),  
            size, 
            SpriteEffects.None, 
            layer
        );

    }
}

public class Background
{
    static readonly Random randint = new Random();
    List<Particle> particles;
    Vector2 centrePoint;
    Texture2D particleTexture;
    Vector2 transformer = new Vector2(0.0f, 0.0f);

    float screenWidth;
    float screenHeight;

    public float angle = 0.0f;

    public Background(float xStart, float yStart, Texture2D inputTexture, float width, float height)
    {
        particles = new List<Particle>();
        centrePoint = new Vector2(xStart, yStart);
        particleTexture = inputTexture;

        screenWidth = width;
        screenHeight = height;
        

        frameZeroSetup();
    }

    public void AddParticles()
    {

        Particle newParticle = new Particle(
            texture: particleTexture,
            startX: centrePoint.X,
            startY: centrePoint.Y,
            accMag: 2.0f,
            velMag: 10.0f,
            inputLife: 30.0f,
            inputAngle: angle,
            startSize: 10.0f,
            startOpacity: 0.5f
        );

        angle += 0.2f;


        particles.Add(newParticle);

        centrePoint += transformer;
    }

    public void frameZeroSetup()
    {
        for (int i = 0; i < 2000; i++)
        {
            AddParticles();
            updateParticles(1f / 60f);
        }   
    }

    public void updateParticles(float dt)
    {
        foreach (Particle particle in particles)
        {
            particle.updateParticle(dt);
        }

        particles.RemoveAll(p => p.life == 0.0f);

        updateTransformer();
    }

    public void drawParticles(SpriteBatch sprite, Texture2D texture)
    {
        foreach (Particle particle in particles)
        {
            particle.Draw(sprite, texture, transformer);
        }
    }

    public void updateTransformer()
    {
        if (centrePoint.X > screenWidth || centrePoint.X < 0.0f) transformer.X *= -1;
        if (centrePoint.Y > screenHeight || centrePoint.Y < 0.0f) transformer.Y *= -1;
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
    public int height;
    public int width;
    public Shape[][] boardStore;
    public ShapeBound[][] shapeBounds;
    public Vector2 position;
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

    public void DrawBoard(SpriteBatch batch, Texture2D baseTexture, moveCache playedMove, Main main, Position currentPosition)
    {
        //Backdrop

        Rectangle boardBack = new Rectangle((int) position.X - 5, (int) position.Y - 5, width + 10, height + 10);

        batch.Draw(baseTexture, boardBack, Color.SlateGray);


        Color selectedColor = Color.Blue;
        Color clickedColor = Color.Red;
        Color moveStart = Color.Lime;
        Color moveEnd = Color.Purple;

        int moveStartIndex = playedMove.start;
        int moveEndIndex = playedMove.moveTo;

        main.displayPosition(currentPosition);

        Piece[][] pieces = main.displayBoard;

        for (int i = 0; i < heightNum; i++)
        {
            for (int j = 0; j < widthNum; j++)
            {
                Shape currentShape = boardStore[i][j];

                if (currentShape.isSelected)
                {
                    if (currentShape.isClicked)
                    {
                        batch.Draw(currentShape.texture, currentShape.shapeObj, clickedColor);
                    } else 
                    {
                        batch.Draw(currentShape.texture, currentShape.shapeObj, selectedColor);
                    }
                } else
                {
                    batch.Draw(currentShape.texture, currentShape.shapeObj, currentShape.currentColor);
                }

                if (currentShape.index == moveStartIndex && moveStartIndex >= 0)
                {
                    batch.Draw(currentShape.texture, currentShape.shapeObj, moveStart);
                }

                if (currentShape.index == moveEndIndex && moveEndIndex >= 0)
                {
                    batch.Draw(currentShape.texture, currentShape.shapeObj, moveEnd);
                }

                drawPieces(i, j, pieces[i][j], batch, baseTexture);


            }
        }
    }

    public void drawPieces(int squareY, int squareX, Piece currentPiece, SpriteBatch batch, Texture2D baseTexture)
    {

        Rectangle pieceDims = boardStore[squareY][squareX].shapeObj;

        pieceDims.X += 5;
        pieceDims.Y += 5;
        pieceDims.Height -= 10;
        pieceDims.Width -= 10;

        Color usingColor;

        if (currentPiece.isFull){

            if (currentPiece.isWhite)
            {
                usingColor = Color.HotPink;

                if (currentPiece.isKing)
                {
                    usingColor = Color.Yellow;
                }
                
            } else
            {
                usingColor = Color.Indigo;

                if (currentPiece.isKing)
                {
                    usingColor = Color.Orange;
                }
            }

            batch.Draw(baseTexture, pieceDims, usingColor);
        }
    }

}

public class Game1 : Game
{

    //BUILT IN ATTRIBUTES
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    
    //BASE TEXTURE
    Texture2D _texture;

    //GAME CLASSES
    Board newBoard;
    Main main;
    Background back;

    //INPUT HANDLING
    MouseState mouse;
    moveCache playedMove;
    bool validClickChecker;
    int frameTimerForButtonPushing;

    //GAME STATE
    GameState state;

    //ELSE
    Position currentPosition;
    int indexOfCurrentPosition;
    float width;
    float height;

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

        playedMove = new moveCache(-1, -1);
        validClickChecker = true;
    
        main = new Main();
        state = GameState.MoveInput;

        main.previousPositions.Add(new Position(main.moves.WhitePieces.board, main.moves.BlackPieces.board, main.moves.Kings.board));
        indexOfCurrentPosition ++;
        currentPosition = main.previousPositions.Last();

        frameTimerForButtonPushing = 0;

        width = _graphics.PreferredBackBufferWidth;
        height = _graphics.PreferredBackBufferHeight;

        back = new Background(newBoard.position.X + newBoard.width / 2.0f, newBoard.position.Y + newBoard.height / 2.0f, _texture, width, height);
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

        mouse = Mouse.GetState();

        if (mouse.LeftButton == ButtonState.Released)
        {
            validClickChecker = true;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.Left) && indexOfCurrentPosition > 1 && frameTimerForButtonPushing == 0)
        {
            Console.WriteLine($"Left button pushed! \nLen of past pos = {main.previousPositions.Count}\nIndex of Current Pos = {indexOfCurrentPosition}");

            state = GameState.BrowsingPastPositions;

            indexOfCurrentPosition -= 1;

            currentPosition = main.previousPositions[indexOfCurrentPosition];

            frameTimerForButtonPushing = 10;
        }

        if (Keyboard.GetState().IsKeyDown(Keys.Right) && indexOfCurrentPosition != main.previousPositions.Count - 1 && frameTimerForButtonPushing == 0)
        {
            indexOfCurrentPosition += 1;

            currentPosition = main.previousPositions[indexOfCurrentPosition];

            if (indexOfCurrentPosition == main.previousPositions.Count - 1)
            {
                state = GameState.MoveInput;
            }
            
            frameTimerForButtonPushing = 10;
        }

        float dt = (float) gameTime.ElapsedGameTime.TotalSeconds;

        if (Keyboard.GetState().IsKeyDown(Keys.M))
        {
            Console.WriteLine($"Left button pushed! \nLen of past pos = {main.previousPositions.Count}\nIndex of Current Pos = {indexOfCurrentPosition}\nFrame delay = {frameTimerForButtonPushing}");
        }

        //Finds which squares are covered by mouse
        FindSelectedSquare();

        if (state != GameState.GameOver && state != GameState.BrowsingPastPositions){

            if (main.moves.WhiteTurn)
            {
                if (playedMove.start != -1 && playedMove.moveTo != -1)
                {
                    state = GameState.MoveInput;
                    main.makeHumanMove(playedMove);

                    main.previousPositions.Add(new Position(main.moves.WhitePieces.board, main.moves.BlackPieces.board, main.moves.Kings.board));
                    indexOfCurrentPosition ++;
                    currentPosition = main.previousPositions.Last();

                    playedMove.moveTo = -1;
                }
            } else
            {
                state = GameState.BotMoving;
                main.runForAI(main.moves);

                main.previousPositions.Add(new Position(main.moves.WhitePieces.board, main.moves.BlackPieces.board, main.moves.Kings.board));
                indexOfCurrentPosition ++;
                currentPosition = main.previousPositions.Last();
            }

            int isGameOver = main.checkForGameOver();

            if (isGameOver == 1 || isGameOver == 2 || isGameOver == 3) state = GameState.GameOver;
        }

        if (frameTimerForButtonPushing != 0) frameTimerForButtonPushing --;


        back.AddParticles();
        back.updateParticles(dt);




        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        _spriteBatch.Begin();


        back.drawParticles(_spriteBatch, _texture);

        newBoard.DrawBoard(
            batch: _spriteBatch, 
            baseTexture: _texture, 
            playedMove: playedMove, 
            main: main, 
            currentPosition: currentPosition);


        _spriteBatch.End();

        base.Draw(gameTime);
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

                        playedMove.setValue(newBoard.boardStore[i][j].index);
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
}
