using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
