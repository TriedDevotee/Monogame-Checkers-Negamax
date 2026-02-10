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

public class moveCache
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

    public void SetValue(int square)
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
