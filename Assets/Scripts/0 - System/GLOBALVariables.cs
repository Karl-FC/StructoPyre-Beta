using UnityEngine;
using UnityEngine.InputSystem;

// Assuming UnitSystem enum is accessible or defined elsewhere (e.g., in MaterialProperties.cs)

public static class GlobalVariables
{

//STRUCTURE PROPERTIES
    public static Vector3 ModelSize { get; set; }
    public static string SelectedMaterial { get; set; } = "Concrete";
    public static float Thickness { get; set; } = 0.2f;
    public static bool EvacuationEnabled { get; set; } = false;


//SETTINGS
    public static bool isDPadEnabled { get; set; } = false;
    public static bool userOnMobile { get; set; } = false;
    public static bool isMouseVisible { get; set; } = false;
    public static bool playerCanMove { get; set; } = false;

//EVENTS
    public static string ImportedModelPath { get; set; }
    public static GameObject ImportedModel { get; set; }

// Add a variable to store the currently selected UI element
    public static GameObject selectedUIElement = null;

// Default control scheme
    public static string currentControlScheme = "Keyboard&Mouse";

    public static UnitSystem DisplayUnitSystem = UnitSystem.Metric; // Default to Metric (millimeters)
}