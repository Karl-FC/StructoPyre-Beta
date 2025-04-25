# Using StructoPyre (Beta)

Welcome to the beta version of StructoPyre! This guide will help you get started with importing models and exploring the fire simulation features currently available.

**Please Note:** This is an early beta. You may encounter performance issues, incomplete features, or occasional bugs. Your feedback is greatly appreciated!

## Launching the Application

After landing on the main website, click the "Launch App" button. This will load the main simulation interface in your browser.

**Display/Scaling:** The application window size adapts to your browser. If the display appears too small or too large, try entering or exiting fullscreen mode in your browser (often the F11 key).

## Core Workflow: Importing Your Model

The first step is to load your 3D structural model (currently supporting the OBJ format).

1.  From the **Main Menu**, click the **Import** button.
2.  A file dialog box will appear. Select your `.obj` model file. Click "Open" or similar.
3.  A second file dialog box will appear. Select the corresponding `.mtl` material file that came with your `.obj` model. It should have the same name as the `.obj` file (e.g., `building.obj` and `building.mtl`). Click "Open".

*(Note: The application will then process the model, including scaling and material identification. You will be transitioned to the 3D viewing space.)*

## Material Mapping

After importing the model and its materials, you will be presented with the **Material Mapper UI**.

*   This panel lists the material names found in your imported `.mtl` file.
*   For each imported material, use the dropdown menu next to it to select a corresponding "Real World Material" (Aggregate Type) from the available options (e.g., Carbonate Concrete, Siliceous Concrete, etc.). This step is crucial for the ACI calculations.
*   Once you have mapped all necessary materials, click the "Confirm Mappings" button.

*(Note: Mapping confirms the link between your 3D model's visual appearance and the physical/thermal properties required for the ACI simulation.)*

## Navigation in the 3D Space

Once the model is loaded and mapped, you can navigate through the 3D environment.

*   **Movement:** Use the **WASD** keys to move forward, left, backward, and right, respectively (like a ghost or spectator camera).
*   **Look Around:** Use the **Mouse** to control your view direction.

*(**Known Issue:** Sometimes, depending on the browser or operating system, there might be occasional conflicts or stuttering with WASD and mouse input. This is often related to browser pointer lock features.)*

## User Interface & Modes

The application features different UI panels and conceptual "modes":

1.  **Main Menu:** (Initial view) Contains options like Import.
2.  **Material Mapper UI:** (Appears after import) Used for mapping materials.
3.  **Header:** (Always visible at the top) Contains general information and access to Settings.
    *   The header height adjusts and opacity changes after leaving the main menu.
4.  **DPad UI:** (Visible after model load, for mobile controls - not fully implemented/used in WebGL yet)
5.  **Properties Panel:** Accessed via the `E` key. This panel is part of the **Inspector Mode** functionality.

### Modes

*   **Inspector Mode:** Allows you to view details about specific elements in the model.
    *   *(Functionality is currently limited.)*
    *   Press `E` to toggle the **Properties Panel**.
    *   When you are looking at a structural element (like a wall or column segment), a pink arrow indicator will show which element is currently being "inspected".
    *   This panel will allow you to view and potentially edit ACI-specific properties like concrete cover, element type, restraint conditions, etc. for the selected element.
*   **Simulator Mode:** The primary mode for running the fire simulation.
    *   This mode contains controls like Start, Pause, and Reset Simulation.*

## Simulation Visualization

As the simulation time progresses, the color of structural elements will change based on their calculated fire resistance according to ACI 216.1M-14:

*   **Green:** The element currently meets the required structural integrity standards based on ACI calculations for the elapsed simulation time. (Safe)
*   **Yellow:** The element is exposed to fire and approaching its fire resistance limit. (Exposed / Warning)
*   **Red:** The element has failed the ACI fire resistance check for the elapsed simulation time. Its required structural integrity has been lost. (Unsafe / Yielded)
*   **Black:** The structural element has yielded.

*(Note: The simulation will trigger these color changes automatically as time passes and the fire spreads.)*

## Simulation Timer

A timer will display the elapsed simulation time, likely in `H:MM:S` format.

## Settings

Click the **Hamburger Icon** (three horizontal lines) in the top-right corner of the Header to access the Settings menu.

*(Note: Not all settings options are fully functional in the beta version.)*

## Known Issues & Limitations

*   Occasional WASD/mouse input issues depending on browser/OS.
*   Performance lag. The application may be slow, especially with complex models or during simulation.
*   The "fire source" spawned by pressing `X` is currently just a grey placeholder cube. It does not yet simulate fire spread or heat transfer.
*   The Properties panel (`E` key) is a functionality for viewing/editing element data is limited in this beta.
*   Simulation is strictly based on the prescriptive methods in ACI 216.1M-14 for concrete and masonry, primarily focusing on element-level checks (cover, thickness, dimension) based on elapsed time.
*   Advanced analytical methods (temperature profiles, strength reduction) are not implemented in the MVP.
*   No complex structural analysis (load bearing capacity, internal forces) is performed beyond checking ACI minimum requirements.
*   The simulation does not account for many real-world fire variables (ventilation, fuel load beyond placement, specific material spalling beyond ACI code assumptions).
*   An Evacuation Dynamics module will be implemented in the future.

---