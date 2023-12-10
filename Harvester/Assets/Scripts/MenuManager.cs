using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using UnityEngine.UI;

public enum MenuID
{
    NOTHING,
    PAUSE,
    INVENTORY,
    INSTANTCRAFTING,
    CRAFTING,
    FARM
}

public class MenuManager : MonoBehaviour
{
    /*
     * -1 => nothing
     * 0 => PauseMenu
     * 1 => Inventory
     * 2 => crafting 
     * 3 => instant crafting
     * 4 => farm
    */


    public static MenuID menuOpen;
    public MenuID temp;

    public void Update()
    {
        temp = menuOpen;
    }

    public static bool IsMenuOpen()
    {
        return menuOpen == MenuID.NOTHING;
    }
    public static bool CanOpenMenuSet(MenuID menuID)
    {
        print("menuOpen: " + menuOpen);
        if (menuOpen == MenuID.NOTHING)
        {
            menuOpen = menuID;
            return true;
        }
        return false;
    }

    public static bool IsCurrentMenuClose(MenuID menuID)
    {
        if (menuOpen == menuID)
        {
            menuOpen = MenuID.NOTHING;
            return true;
        }
        return false;
    }
}
