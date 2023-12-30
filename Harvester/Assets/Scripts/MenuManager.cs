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

/// <summary>
/// Updates the temporary variable to reflect the current state of menu openness.
/// </summary>
/// <remarks>
/// This method updates the temporary variable <code>temp</code> with the current state of menu openness.
/// </remarks>
    public void Update()
    {
        temp = menuOpen;
    }

/// <summary>
/// Checks if any menu is currently open.
/// </summary>
/// <returns>True if no menu is open; false otherwise.</returns>
    public static bool IsMenuOpen()
    {
        return menuOpen == MenuID.NOTHING;
    }
/// <summary>
/// Checks if the specified menu can be opened and sets it as the current open menu if possible.
/// </summary>
/// <param name="menuID">The ID of the menu to potentially open.</param>
/// <returns>True if the menu can be opened; false otherwise.</returns>
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
/// <summary>
/// Checks if the current open menu is the specified menu and closes it.
/// </summary>
/// <param name="menuID">The ID of the menu to check and close.</param>
/// <returns>True if the current open menu is the specified menu; false otherwise.</returns>
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
