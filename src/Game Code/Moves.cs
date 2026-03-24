/// <summary>
/// Class that tracks the move made by user. Default values are -1 and -1 for start and moveTo. 
/// Fills start if start = -1, or both start and moveTo aren't -1
/// Otherwise fill moveTo
/// </summary>
public class moveCache
{
    public int start {get; private set;}
    public int moveTo {get; private set;}
    int indexFilling;

    public moveCache(int s = -1, int m = -1)
    {
        start = s;
        moveTo = m;

        if (start == -1)
            indexFilling = 0;
        else if (moveTo == -1)
            indexFilling = 1;
        else
            indexFilling = 0;
    }

    /// <summary>
    /// Sets the value of the cache with the square number given
    /// Called in session exclusively for encapsulations sake. (this needs to not be tampered with)
    /// </summary>
    /// <param name="square"></param>
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

    /// <summary>
    /// Resets all the values to default values (-1, -1)
    /// </summary>
    public void clearCache()
    {
        start = -1;
        moveTo = -1;
        indexFilling = 0;
    }
}
