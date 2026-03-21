using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

//WHY DO YOU EVEN EXIST???
/// <summary>
/// Details the outer bounds of the shape. 
/// Not really sure why its still used (mostly for debugging purposes)
/// </summary>
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

/// <summary>
/// Basic shape class that allows me to render things like the board or basic shapes elsewhere.
/// Has no text or function.
/// </summary>
public class Shape
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

    /// <summary>
    /// Draws the shape.
    /// If the dontHold var is fakse, it "holds", using the inputted holdColor.
    /// Otherwise, it uses the color assigned at instantiation.
    /// This has some uses.
    /// </summary>
    /// <param name="batch"></param>
    /// <param name="holdColor"></param>
    /// <param name="dontHold"></param>
    public void DrawShape(SpriteBatch batch, Color holdColor, bool dontHold = false)
    {
        if (!dontHold)
        {
            batch.Draw(texture, shapeObj, holdColor);
        }
        else
        {
            batch.Draw(texture, shapeObj, currentColor);
        }
    }
}

/// <summary>
/// Manages all the shapes. 
/// This means it stores them all, and handles all their rendering and updating.
/// </summary>
public class ShapeManager
{
    private List<Shape> shapes;
    private Texture2D baseTexture;
    
    public ShapeManager(Texture2D texture)
    {
        baseTexture = texture;
        shapes = new();
    }

    /// <summary>
    /// Creates a new shape. 
    /// Takes in its x and y positions, its dimensions and its instantiation color.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="color"></param>
    /// <param name="xDim"></param>
    /// <param name="yDim"></param>
    public void AddShapes(int x, int y, Color color, int xDim, int yDim)
    {
        shapes.Add(new(baseTexture, x, y, color, shapes.Count, yDim, xDim));
    }

    /// <summary>
    /// Draws all the shapes with the inputted color.
    /// </summary>
    /// <param name="batch"></param>
    /// <param name="color"></param>
    public void DrawShapes(SpriteBatch batch, Color color)
    {
        foreach (Shape shape in shapes)
        {
            shape.DrawShape(batch, color);
        }
    }

    /// <summary>
    /// Only draws shapes that correlate with the given indices.
    /// Probably should have some error checking (but that would slow it down).
    /// Plus the UI prevents errors from ocurring.
    /// </summary>
    /// <param name="batch"></param>
    /// <param name="specials"></param>
    /// <param name="specialColor"></param>
    public void DrawSpecials(SpriteBatch batch, int[] specials, Color specialColor)
    {
        foreach (int special in specials)
        {
            shapes[special].DrawShape(batch, specialColor);
        }
    }

    /// <summary>
    /// Draws all the shapes with the colors they were given at instantiation.
    /// </summary>
    /// <param name="batch"></param>
    public void DrawAll(SpriteBatch batch)
    {
        foreach (Shape shape in shapes)
        {
            shape.DrawShape(batch, Color.White, true);
        }
    }

    /// <summary>
    /// Returns the index of a selected shape, if any. Also makes it clicked if it is clicked.
    /// </summary>
    /// <param name="prevState"></param>
    /// <param name="mouse"></param>
    /// <returns></returns>
    public int checkForSelectedShapes(MouseState prevState, MouseState mouse)
    {
        int retIndex = -1;

        for (int i = 0; i < shapes.Count; i++)
        {
            Shape shape = shapes[i];
            if (shape.shapeObj.Contains(new Point(mouse.X, mouse.Y))){
                shape.isSelected = true;

                if (mouse.LeftButton == ButtonState.Pressed 
                    && prevState.LeftButton == ButtonState.Released)
                {
                    shape.isClicked = true;
                } else
                {
                    shape.isClicked = false;
                }

                retIndex = i;
            }
            else
            {
                shape.isSelected = false;
                shape.isClicked = false;
            }
        }

        return retIndex;
    }

    /// <summary>
    /// Returns the shape that correlates with the index given.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Shape getSelectedShapes(int index)
    {
        return shapes[index];
    }
}

/// <summary>
/// Button UI element. Like the shape, but has text and a purpose.
/// </summary>
public class Button
{
    public int buttonID {get; private set;}
    string ButtonContent;
    public Rectangle Position;
    Color color;
    Color textColor;
    public ShapeBound bounding {get; private set;}
    bool doShadow = true;

    public ButtonFunctions purpose;

    public bool isSelected;
    public bool isClicked;

    private float scale;

    public Button(int ID, string BC, Color Color, Color tColor, int x, int y, int xDim, int yDim, ButtonFunctions func, float buttonScale = 1.25f)
    {
        buttonID = ID;
        ButtonContent = BC;
        color = Color;
        textColor = tColor;
        Position = new Rectangle(x, y, xDim, yDim);

        bounding = new(x, y, x + xDim, y + yDim);

        purpose = func;
        
        scale = buttonScale;
    }

    /// <summary>
    /// Draws the button. 
    /// Always draws it with its instantiated color and inputted font.
    /// </summary>
    /// <param name="batch"></param>
    /// <param name="font"></param>
    /// <param name="texture"></param>
    public void Draw(SpriteBatch batch, SpriteFont font, Texture2D texture)
    {
        Vector2 textSize = font.MeasureString(ButtonContent);
        Vector2 origin = textSize / 2f;
        
        Vector2 textPos = new(
            Position.X + (Position.Width / 2f), 
            Position.Y + (Position.Height / 2f));
        
        Vector2 shadowPos = new(
            textPos.X + 3, textPos.Y + 3
        );

        Rectangle backDrop = new(
            Position.X - 3,
            Position.Y - 3,
            Position.Width + 6,
            Position.Height + 6
        );

        batch.Draw(texture, backDrop, Color.Black * 0.8f);
        batch.Draw(texture, Position, color);

        if (doShadow) batch.DrawString(font, ButtonContent, shadowPos, Color.Black * 0.25f, rotation: 0, origin, scale, SpriteEffects.None, 0);
        batch.DrawString(font, ButtonContent, textPos, textColor, rotation: 0, origin, scale, SpriteEffects.None, 0);
    }
}

/// <summary>
/// Manager class for the Button class.
/// Stores them all and updates and draws them.
/// </summary>
public class ButtonManager
{
    List<Button> buttons;

    public ButtonManager()
    {
        buttons = new();
    }

    /// <summary>
    /// Adds a new button. Takes in a lot of parameters - 
    /// Text that is written on the button.
    /// The color of the button.
    /// The color of the text on the button.
    /// The x position of the button.
    /// The y position of the button.
    /// The x dimension of the button.
    /// The y dimension of the button.
    /// The purpose of the button.
    /// The scale of the text on the button.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="color"></param>
    /// <param name="textColor"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="xDim"></param>
    /// <param name="yDim"></param>
    /// <param name="purpose"></param>
    /// <param name="scale"></param>
    public void AddButton(string text, Color color, Color textColor, int x, int y, int xDim, int yDim, ButtonFunctions purpose, float scale = 1.25f)
    {
        buttons.Add(new(buttons.Count, text, color, textColor, x, y, xDim, yDim, purpose, scale));
    }

    /// <summary>
    /// Draws all the buttons.
    /// </summary>
    /// <param name="batch"></param>
    /// <param name="font"></param>
    /// <param name="texture"></param>
    public void DrawButtons(SpriteBatch batch, SpriteFont font, Texture2D texture)
    {
        foreach (Button button in buttons)
        {
            button.Draw(batch, font, texture);
        }
    }

    /// <summary>
    /// Returns the index of the selected button. 
    /// Also makes it clicked it mouse is clicked :D
    /// </summary>
    /// <param name="prevState"></param>
    /// <param name="mouse"></param>
    /// <returns></returns>
    public int checkForSelectedButtons(MouseState prevState, MouseState mouse)
    {
        int retIndex = -1;

        for (int i = 0; i < buttons.Count; i++)
        {
            Button shape = buttons[i];
            if (shape.Position.Contains(new Point(mouse.X, mouse.Y))){
                shape.isSelected = true;

                if (mouse.LeftButton == ButtonState.Pressed 
                    && prevState.LeftButton == ButtonState.Released)
                {
                    shape.isClicked = true;
                } else
                {
                    shape.isClicked = false;
                }

                retIndex = i;
            }
            else
            {
                shape.isSelected = false;
                shape.isClicked = false;
            }
        }

        return retIndex;
    }

    /// <summary>
    /// Returns the button correlating with the provided index.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Button getButton(int index)
    {
        return buttons[index];
    }

}

/// <summary>
/// UI component that renders a scale which can be modified by the user, and tracks its percentage filled.
/// I literally only use this for colors, but I built it to be nice and modular in case I needed it elsewhere
/// (I didn't).
/// </summary>
public class Slider
{
    Rectangle Bar;
    Rectangle Knob;
    Color barColor;
    Color knobColor;
    Texture2D baseTexture;
    int barLength;
    public float fractionMade {get; private set;}
    public Slider(Texture2D texture, int xPos, int yPos, int length, int height = 20, int knobDIm = 20)
    {
        Bar = new Rectangle(xPos, yPos, height, length);
        Knob = new Rectangle(xPos, yPos, height, knobDIm);

        barColor = Color.Gray;
        knobColor = Color.DarkGray;

        barLength = length;

        fractionMade = 0f;

        baseTexture = texture;
    }

    /// <summary>
    /// Updates the slider.
    /// This means that it edits the location of the knob, and changes the fraction it represents.
    /// </summary>
    /// <param name="state"></param>
    public void updateSlider(MouseState state)
    {
        if (Bar.Contains(state.Position) && state.LeftButton == ButtonState.Pressed)
        {
            Knob.Y = state.Y;
        }

        fractionMade = (Knob.Y - Bar.Y) / (float)barLength;
    }

    /// <summary>
    /// It draws the slider.
    /// </summary>
    /// <param name="batch"></param>
    public void drawSlider(SpriteBatch batch)
    {
        Rectangle offsetSection = new(
            Bar.X,
            Bar.Y + barLength,
            Knob.Width,
            Bar.Width
        );

        batch.Draw(baseTexture, Bar, barColor);
        batch.Draw(baseTexture, offsetSection, barColor);

        batch.Draw(baseTexture, Knob, knobColor);
    }
}

/// <summary>
/// Class that implements the sliders to be used as color setters.
/// Stores 3 sliders - red green and blue.
/// Also mixes a color in the update method.
/// </summary>
public class SliderColorMaker
{
    int xPos;
    int yPos;
    int xOffset;
    int yOffset;
    Slider[] sliders;
    public Color color;
    public SliderColorMaker(Texture2D texture, int x, int y, int xOff, int yOff, int barLength)
    {
        xPos = x;
        yPos = y;
        xOffset = xOff;
        yOffset = yOff;

        sliders = [
            new(texture, xPos, yPos, length : barLength),
            new(texture, xPos + xOffset, yPos + yOffset, length : barLength),
            new(texture, xPos + xOffset * 2, yPos + yOffset * 2, length : barLength),
        ];

        color = Color.Black;
    }

    /// <summary>
    /// Updates all the sliders.
    /// Also computes a new value for the color stored.
    /// </summary>
    /// <param name="state"></param>
    public void updateSliders(MouseState state)
    {
        foreach (Slider slider in sliders)
        {
            slider.updateSlider(state);
        }

        float red = sliders[0].fractionMade;
        float green = sliders[1].fractionMade;
        float blue = sliders[2].fractionMade;

        color = new(red, green, blue);
    }

    /// <summary>
    /// It draws the sliders.
    /// </summary>
    /// <param name="batch"></param>
    public void drawSliders(SpriteBatch batch)
    {
        foreach (Slider slider in sliders)
        {
            slider.drawSlider(batch);
        }
    }
}