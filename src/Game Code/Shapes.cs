using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

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

public class Button
{
    public int buttonID {get; private set;}
    string ButtonContent;
    Rectangle Position;
    Color color;
    Color textColor;
    public ShapeBound bounding {get; private set;}
    bool doShadow = true;

    bool isSelected;
    bool isClicked;

    public Button(int ID, string BC, Color Color, Color tColor, int x, int y, int xDim, int yDim)
    {
        buttonID = ID;
        ButtonContent = BC;
        color = Color;
        textColor = tColor;
        Position = new Rectangle(x, y, xDim, yDim);

        bounding = new(x, y, x + xDim, y + yDim);
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

        if (doShadow) batch.DrawString(font, ButtonContent, shadowPos, Color.Black * 0.25f, rotation: 0, origin, 1.25f, SpriteEffects.None, 0);
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
        buttons.Add(new(buttons.Count, text, color, textColor, x, y, xDim, yDim));
    }

    public void DrawButtons(SpriteBatch batch, SpriteFont font, Texture2D texture)
    {
        foreach (Button button in buttons)
        {
            button.Draw(batch, font, texture);
        }
    }

    public int getTouchedButtons(MouseState state)
    {
        Point mousePosition = new(state.X, state.Y);

        foreach (Button button in buttons)
        {
            if (mousePosition.X > button.bounding.startX 
                && mousePosition.X <= button.bounding.farX 
                && mousePosition.Y > button.bounding.startY 
                && mousePosition.Y <= button.bounding.farY)
            {
                return button.buttonID;
            }
        }

        return -1;
    }

    public bool isMouseClicked(MouseState state)
    {
        return state.LeftButton == ButtonState.Pressed;
    }
}
