namespace breaking_worse.Input.Enums;

public enum UserAction
{
    /// <summary>
    /// enum of all user actions
    /// </summary>
    Interact,       // trigger interaction
    Inventory,      // open/close inventory
    PauseMenu,      // open pause-menu
    Up,             // walk up
    Down,           // walk down
    Left,           // walk left
    Right,          // walk right
    Execute,        // user your fists or shoot a gun
    SwitchWeapon,   // switch between fists and gun
}
