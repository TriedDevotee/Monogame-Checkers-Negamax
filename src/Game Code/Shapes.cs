using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

//WHY DO YOU EVEN EXIST???
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

public class ShapeManager
{
    private List<Shape> shapes;
    private Texture2D baseTexture;
    
    public ShapeManager(Texture2D texture)
    {
        baseTexture = texture;
        shapes = new();
    }

    public void AddShapes(int x, int y, Color color, int xDim, int yDim)
    {
        shapes.Add(new(baseTexture, x, y, color, shapes.Count, yDim, xDim));
    }

    public void DrawShapes(SpriteBatch batch, Color color)
    {
        foreach (Shape shape in shapes)
        {
            shape.DrawShape(batch, color);
        }
    }

    public void DrawSpecials(SpriteBatch batch, int[] specials, Color specialColor)
    {
        foreach (int special in specials)
        {
            shapes[special].DrawShape(batch, specialColor);
        }
    }

    public void DrawAll(SpriteBatch batch)
    {
        foreach (Shape shape in shapes)
        {
            shape.DrawShape(batch, Color.White, true);
        }
    }

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

    public Shape getSelectedShapes(int index)
    {
        return shapes[index];
    }
}

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

public class ButtonManager
{
    List<Button> buttons;

    public ButtonManager()
    {
        buttons = new();
    }

    public void AddButton(string text, Color color, Color textColor, int x, int y, int xDim, int yDim, ButtonFunctions purpose, float scale = 1.25f)
    {
        buttons.Add(new(buttons.Count, text, color, textColor, x, y, xDim, yDim, purpose, scale));
    }

    public void DrawButtons(SpriteBatch batch, SpriteFont font, Texture2D texture)
    {
        foreach (Button button in buttons)
        {
            button.Draw(batch, font, texture);
        }
    }

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

    public Button getButton(int index)
    {
        return buttons[index];
    }

}

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

    public void updateSlider(MouseState state)
    {
        if (Bar.Contains(state.Position) && state.LeftButton == ButtonState.Pressed)
        {
            Knob.Y = state.Y;
        }

        fractionMade = (Knob.Y - Bar.Y) / (float)barLength;
    }

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

    public void drawSliders(SpriteBatch batch)
    {
        foreach (Slider slider in sliders)
        {
            slider.drawSlider(batch);
        }
    }
}
