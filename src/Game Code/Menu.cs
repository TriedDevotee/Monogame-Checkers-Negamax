using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Net.Mime;

namespace Comp_Sci_NEA;

public struct MenuItem
{
    public string content {get; private set;}
    public int itemNumber {get; private set;}

    public MenuItem(string name, int id)
    {
        content = name;
        itemNumber = id;
    }
}

class Menu
{

    public Menu()
    {

    }
    public void UpdateMenu()
    {
        
    }

    public void DrawMenu()
    {
        
    }
}
