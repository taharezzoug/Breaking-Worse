namespace breaking_worse.Input.Enums;

public enum PressType
{
    /// <summary>
    /// enum of the different types of presses the users can execute
    /// </summary>
    PressAndRelease,        // One Press/Release Cycle is instantly evaluated as one press
    OnlyRelease,            // One Press/Release Cycle is evaluated as one press when button is released
    HoldPressAndRelease,    // One Press/Release Cycle is evaluated as one press if key is held down for a set duration
    HoldWithoutRelease      // Evaluated as pressed every frame (no release needed) -> needed for movement
}
