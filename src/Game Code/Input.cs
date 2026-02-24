using Microsoft.Xna.Framework.Input;

public class InputHandler
{
    public MouseState MouseState {get; private set;}
    public MouseState PreviousMouseState {get; private set;}
    public bool IsClicked {get; private set;}
    public bool IsDragging {get; private set;}
    public int XPos {get; private set;}
    public int YPos {get; private set;}

    public InputHandler(MouseState currState)
    {
        MouseState = currState;
        
        IsClicked = false;
        IsDragging = false;

        XPos = MouseState.X;
        YPos = MouseState.Y;
    }

    public void Update()
    {
        MouseState = Mouse.GetState();

        XPos = MouseState.X;
        YPos = MouseState.Y;

        if (MouseState.LeftButton == ButtonState.Pressed)
        {
            IsClicked = true;
        }


        PreviousMouseState = MouseState;
    }
}