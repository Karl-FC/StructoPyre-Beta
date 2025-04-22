//MIT License
//Copyright (c) 2023 DA LAB (https://www.youtube.com/@DA-LAB)
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.

using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using SFB;
using TMPro;
using UnityEngine.Networking;
using Dummiesman; //Load OBJ Model
using System; // Needed for Action

// Main class for opening and processing OBJ files
public class OpenFile : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public GameObject model; //Load OBJ Model

    public delegate void ModelLoadedDelegate(GameObject loadedModel);
    public event ModelLoadedDelegate OnModelLoaded;

    [Header("Model Scaling")]
    [Tooltip("Enable to manually set scale factor instead of auto-detection")]
    [SerializeField] private bool useManualScaling = false;
    private float manualScaleFactor = 1.0f; // Internal flag, not serialized
    [Tooltip("Toggle to show scale debugging information")]
    [SerializeField] private bool showScaleDebugging = true;

    [Header("Unit Conversions")]
    [SerializeField] private TMP_Dropdown unitDropdown;

    public RealMaterialMapperUI materialMapperUI; // <--- ADD THIS LINE

    // Store initial scale factors determined on click for WebGL two-step process
    private bool initialIsManualWebGL = false;
    private float initialScaleFactorWebGL = 1.0f;

#if UNITY_WEBGL && !UNITY_EDITOR
    // WebGL
    [DllImport("__Internal")]
    private static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);

    private string objContentWebGL; // Temporary storage for OBJ content

    public void OnClickOpen() {
        // Determine initial scaling based on dropdown *at the time of click*
        DetermineInitialScale(out initialIsManualWebGL, out initialScaleFactorWebGL);
        Debug.Log($"[OnClickOpen WebGL] Determined initial scale: isManual={initialIsManualWebGL}, factor={initialScaleFactorWebGL}");

        // Request OBJ file first
        UploadFile(gameObject.name, "OnFileUpload", ".obj", false);
    }

    // Called from browser after OBJ file selection
    public void OnFileUpload(string objContent) {
        objContentWebGL = objContent; // Store OBJ content
        // Now request the MTL file
        UploadFile(gameObject.name, "OnMTLFileUpload", ".mtl", false);
        Debug.Log("OBJ file received. Please select the corresponding MTL file.");
        // Optionally: Show a message to the user indicating they need to select the MTL file.
    }

    // Called from browser after MTL file selection
    public void OnMTLFileUpload(string mtlContent) {
        if (string.IsNullOrEmpty(objContentWebGL))
        {
            Debug.LogError("MTL file received, but OBJ content was missing!");
            return;
        }
        Debug.Log("MTL file received. Loading model with stored initial scale...");
        // Start coroutine, passing the scale factors determined back when OnClickOpen was called
        StartCoroutine(OutputRoutineOpenWebGL(objContentWebGL, mtlContent, initialIsManualWebGL, initialScaleFactorWebGL));
        objContentWebGL = null; // Clear temporary storage
    }

#else

    // Standalone platforms & editor
    public void OnClickOpen()
    {
        // Determine initial scaling based on dropdown *at the time of click*
        DetermineInitialScale(out bool isManual, out float scaleFactor);
        Debug.Log($"[OnClickOpen Standalone] Determined initial scale: isManual={isManual}, factor={scaleFactor}");

        string[] paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", new[] { new ExtensionFilter("3D Model Files", "obj") }, false);
        if (paths.Length > 0)
        {
            // Start coroutine, passing the determined scale factors
            StartCoroutine(OutputRoutineOpen(new System.Uri(paths[0]).AbsoluteUri, isManual, scaleFactor));
        }
    }
#endif

    private void Start()
    {
        // Set up the dropdown listener if it exists
        if (unitDropdown != null)
        {
            unitDropdown.onValueChanged.AddListener(OnUnitTypeChanged);
        }
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    private IEnumerator OutputRoutineOpenWebGL(string objContent, string mtlContent, bool isManual, float scaleFactor)
    {
        Debug.Log($"[OutputRoutineOpenWebGL] Starting with isManual={isManual}, scaleFactor={scaleFactor}");
        // For WebGL, create streams directly from content strings
        MemoryStream objStream = new MemoryStream(Encoding.UTF8.GetBytes(objContent));
        MemoryStream mtlStream = null;

        if (!string.IsNullOrEmpty(mtlContent))
        {
            mtlStream = new MemoryStream(Encoding.UTF8.GetBytes(mtlContent));
            Debug.Log("Processing OBJ and MTL content received from browser.");
        }
        else
        {
            Debug.LogWarning("Processing OBJ content received from browser. No MTL content provided or MTL file selection was cancelled.");
        }

        // --- Common Model Loading Logic (Copied & adapted from original OutputRoutineOpen) ---

        // Load the model with materials
        if (model != null)
        {
            Destroy(model);
        }

        OBJLoader loader = new OBJLoader();
        // Set the split mode to Object (or Group if Object isn't available)
        loader.SplitMode = Dummiesman.SplitMode.Object;
        Debug.Log($"OBJLoader SplitMode set to: {loader.SplitMode}");

        if (mtlStream != null)
        {
            model = loader.Load(objStream, mtlStream);
        }
        else
        {
            model = loader.Load(objStream); // Loads OBJ only
        }

        // Close streams
        objStream?.Close();
        mtlStream?.Close();

        if (model == null)
        {
            Debug.LogError("Failed to load model from content.");
            yield break; // Exit if model loading failed
        }

        // Place the model at the origin
        model.transform.position = Vector3.zero;

        // First let's apply the X-flipping but keep scale neutral
        model.transform.localScale = new Vector3(-1, 1, 1);

        // Show the material mapping UI via UIManager
        UIManager.Instance.ShowMaterialMapper();

        // Apply normalization using passed-in parameters
        NormalizeModelScale(isManual, scaleFactor);

        // Apply double-sided faces
        DoublicateFaces();

        // Store imported model in global variables
        GlobalVariables.ImportedModel = model;

        // --- START MATERIAL MAPPING ---
        if (materialMapperUI != null)
        {
            HashSet<string> uniqueMaterialNames = new HashSet<string>(); // Use HashSet for uniqueness
            if (model != null)
            {
                // Find all MeshRenderers in the loaded hierarchy
                MeshRenderer[] renderers = model.GetComponentsInChildren<MeshRenderer>(true); // Include inactive

                foreach (MeshRenderer renderer in renderers)
                {
                    // Get the materials used by this renderer
                    foreach (Material mat in renderer.sharedMaterials) // Use sharedMaterials to get original names
                    {
                        if (mat != null)
                        {
                            // Material names might have " (Instance)" appended, try to get the base name
                            string baseName = mat.name.Replace(" (Instance)", "").Trim();
                            uniqueMaterialNames.Add(baseName);
                        }
                    }
                }
            }

            List<string> materialNames = new List<string>(uniqueMaterialNames); // Convert HashSet to List

            if (materialNames.Count > 0)
            {
                Debug.Log($"Found unique materials to map: {string.Join(", ", materialNames)}");

                // Subscribe to the confirmation event (unsubscribe first to prevent duplicates)
                materialMapperUI.OnMappingsConfirmed -= HandleMaterialMappings; // Use '-=' first
                materialMapperUI.OnMappingsConfirmed += HandleMaterialMappings; // Then use '+='

                // Populate and show the UI panel
                materialMapperUI.PopulateMappings(materialNames);
                materialMapperUI.Show();

                // At this point, the coroutine effectively pauses its 'main' work
                // The rest of the logic (like OnModelLoaded) will run AFTER
                // the user interacts with the UI and HandleMaterialMappings is called.
                // So, we stop the coroutine here for now.
                yield break; // IMPORTANT: Exit coroutine here, HandleMaterialMappings will continue the flow
            }
            else
            {
                 Debug.LogWarning("Material Mapper UI assigned, but no material child objects found on the loaded model.");
                 // No mapping needed, proceed directly to final step
                 FinalizeModelLoad();
            }
        }
        else
        {
            Debug.LogWarning("Material Mapper UI is not assigned in the Inspector. Skipping mapping.");
            // No mapping UI, proceed directly to final step
            FinalizeModelLoad();
        }
        // --- END MATERIAL MAPPING ---

        // Now that mappings are applied, finalize the load process (This might be redundant if called within HandleMaterialMappings)
        // Consider if FinalizeModelLoad() needs to be called here too or only after mapping.
        // Based on the flow, it seems HandleMaterialMappings calls FinalizeModelLoad, so it might be unnecessary here.

        // Show simulation GUI after mapping confirmation (This should likely happen *after* FinalizeModelLoad completes)
        // UIManager.Instance.ShowSimulationGUI(); // Moved to FinalizeModelLoad or after OnModelLoaded event?
    }
#endif

    // This coroutine now ONLY handles Standalone/Editor paths - accepts scale factors as parameters
    private IEnumerator OutputRoutineOpen(string url, bool isManual, float scaleFactor)
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        Debug.Log($"[OutputRoutineOpen] Starting with isManual={isManual}, scaleFactor={scaleFactor}");
        // For Standalone/Editor, 'url' is the URI/Path
        string objPath = url; // Keep original name for clarity in this block
        string directoryPath = Path.GetDirectoryName(objPath);
        string fileName = Path.GetFileNameWithoutExtension(objPath);
        string mtlPath = Path.Combine(directoryPath, fileName + ".mtl");

        // Load the OBJ file
        UnityWebRequest www = UnityWebRequest.Get(objPath);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("WWW ERROR loading OBJ: " + www.error + " from path: " + objPath);
            yield break;
        }
        Debug.Log("Successfully loaded OBJ file from path: " + objPath);


        // Load the MTL file if it exists
        Uri mtlUri;
        MemoryStream mtlStream = null;
        // Try converting mtlPath to a valid URI before requesting
        if (Uri.TryCreate(mtlPath, UriKind.Absolute, out mtlUri))
        {
            UnityWebRequest mtlRequest = UnityWebRequest.Get(mtlUri);
            yield return mtlRequest.SendWebRequest();

             if (mtlRequest.result == UnityWebRequest.Result.Success)
            {
                mtlStream = new MemoryStream(Encoding.UTF8.GetBytes(mtlRequest.downloadHandler.text));
                Debug.Log("Successfully loaded MTL file from path: " + mtlPath);
            }
            else
            {
                 Debug.LogWarning("Could not load MTL file from path: " + mtlPath + ". Error: " + mtlRequest.error);
            }
            mtlRequest.Dispose(); // Dispose the request
        }
        else
        {
             Debug.LogWarning("Could not create a valid URI for the MTL file path: " + mtlPath);
        }


        // Create memory streams for both files
        MemoryStream objStream = new MemoryStream(Encoding.UTF8.GetBytes(www.downloadHandler.text));
        www.Dispose(); // Dispose the OBJ request

        // --- Common Model Loading Logic (Copied & adapted) ---

        // Load the model with materials
        if (model != null)
        {
            Destroy(model);
        }

        OBJLoader loader = new OBJLoader();
        // Set the split mode to Object (or Group if Object isn't available)
        loader.SplitMode = Dummiesman.SplitMode.Object;
        Debug.Log($"OBJLoader SplitMode set to: {loader.SplitMode}");

        if (mtlStream != null)
        {
            model = loader.Load(objStream, mtlStream);
        }
        else
        {
            model = loader.Load(objStream); // Loads OBJ only
        }

        // Close streams
        objStream?.Close();
        mtlStream?.Close();

        if (model == null)
        {
            Debug.LogError("Failed to load model from path: " + objPath);
            yield break; // Exit if model loading failed
        }

        // Place the model at the origin
        model.transform.position = Vector3.zero;

        // First let's apply the X-flipping but keep scale neutral
        model.transform.localScale = new Vector3(-1, 1, 1);

        // Show the material mapping UI via UIManager
        UIManager.Instance.ShowMaterialMapper();

        // Apply normalization using passed-in parameters
        NormalizeModelScale(isManual, scaleFactor);

        // Apply double-sided faces
        DoublicateFaces();

        // Store imported model in global variables
        GlobalVariables.ImportedModel = model;

        // --- START MATERIAL MAPPING ---
        if (materialMapperUI != null)
        {
            HashSet<string> uniqueMaterialNames = new HashSet<string>(); // Use HashSet for uniqueness
            if (model != null)
            {
                // Find all MeshRenderers in the loaded hierarchy
                MeshRenderer[] renderers = model.GetComponentsInChildren<MeshRenderer>(true); // Include inactive

                foreach (MeshRenderer renderer in renderers)
                {
                    // Get the materials used by this renderer
                    foreach (Material mat in renderer.sharedMaterials) // Use sharedMaterials to get original names
                    {
                        if (mat != null)
                        {
                            // Material names might have " (Instance)" appended, try to get the base name
                            string baseName = mat.name.Replace(" (Instance)", "").Trim();
                            uniqueMaterialNames.Add(baseName);
                        }
                    }
                }
            }

            List<string> materialNames = new List<string>(uniqueMaterialNames); // Convert HashSet to List

            if (materialNames.Count > 0)
            {
                Debug.Log($"Found unique materials to map: {string.Join(", ", materialNames)}");

                // Subscribe to the confirmation event (unsubscribe first to prevent duplicates)
                materialMapperUI.OnMappingsConfirmed -= HandleMaterialMappings; // Use '-=' first
                materialMapperUI.OnMappingsConfirmed += HandleMaterialMappings; // Then use '+='

                // Populate and show the UI panel
                materialMapperUI.PopulateMappings(materialNames);
                materialMapperUI.Show();

                // Exit coroutine, HandleMaterialMappings will continue the flow
                yield break;
            }
            else
            {
                 Debug.LogWarning("Material Mapper UI assigned, but no material child objects found on the loaded model.");
                 // No mapping needed, proceed directly to final step
                 FinalizeModelLoad();
            }
        }
        else
        {
            Debug.LogWarning("Material Mapper UI is not assigned in the Inspector. Skipping mapping.");
            // No mapping UI, proceed directly to final step
            FinalizeModelLoad();
        }
        // --- END MATERIAL MAPPING ---

        // Now that mappings are applied, finalize the load process
        // FinalizeModelLoad(); // Likely redundant, called by HandleMaterialMappings or the else block above

        // Show simulation GUI after mapping confirmation
        // UIManager.Instance.ShowSimulationGUI(); // Should be called after FinalizeModelLoad completes
#else
        // Handle cases where this coroutine might be called inappropriately on WebGL
        Debug.LogError("OutputRoutineOpen(string url) called on unsupported platform (WebGL). Use OutputRoutineOpenWebGL instead.");
        yield break; // Stop execution if on the wrong platform
#endif
    }

    private void NormalizeModelScale(bool isManual, float factorToApply)
    {
        Debug.Log($"[NormalizeModelScale] Entered with isManual={isManual}, factorToApply={factorToApply}");
        
        // Get the model's bounds
        Bounds bounds = GetBound(model);
        
        if (showScaleDebugging)
        {
            Debug.Log($"Original model size: {bounds.size}, Center: {bounds.center}");
        }
        
        float scaleFactor = 1.0f;
        
        // Use the passed-in parameters to determine scaling
        if (isManual) 
        {
            // Use the factor passed as a parameter
            scaleFactor = factorToApply;
            if (showScaleDebugging)
            {
                Debug.Log($"Using manual scale factor (from parameters): {scaleFactor}");
            }
        }
        else
        {
            // Auto-detect based on model size
            float maxDimension = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
            
            // Intelligent scaling based on common SketchUp export scales
            if (maxDimension > 100.0f)
            {
                // Likely inches to meters (1 inch = 0.0254m)
                scaleFactor = 0.0254f;
                Debug.Log("Model appears to be in inches, scaling to meters");
            }
            else if (maxDimension > 10.0f && maxDimension <= 100.0f)
            {
                // Likely centimeters to meters
                scaleFactor = 0.01f;
                Debug.Log("Model appears to be in centimeters, scaling to meters");
            }
            else if (maxDimension > 0.01f && maxDimension <= 0.1f)
            {
                // Likely very small - could be SketchUp in feet but exported small
                scaleFactor = 10.0f; 
                Debug.Log("Model appears to be very small, scaling up by 10x");
            }
            else if (maxDimension <= 0.01f)
            {
                // Extremely small, needs significant scaling
                scaleFactor = 100.0f;
                Debug.Log("Model appears to be extremely small, scaling up by 100x");
            }
            else
            {
                // Size seems reasonable for meters already
                Debug.Log("Model appears to already be in meters, no scaling needed");
            }
        }
        
        // Apply the scale factor while preserving the X-flip
        Vector3 currentScale = model.transform.localScale;
        model.transform.localScale = new Vector3(
            currentScale.x * scaleFactor,
            currentScale.y * scaleFactor,
            currentScale.z * scaleFactor
        );
        
        // Log the adjusted size for reference
        if (showScaleDebugging)
        {
            Bounds newBounds = GetBound(model);
            Debug.Log($"Adjusted model size: {newBounds.size}, Scale: {model.transform.localScale}");
        }
    }

    private Bounds GetBound(GameObject gameObj)
    {
        Bounds bound = new Bounds(gameObj.transform.position, Vector3.zero);
        var rList = gameObj.GetComponentsInChildren(typeof(Renderer));
        foreach (Renderer r in rList)
        {
            bound.Encapsulate(r.bounds);
        }
        return bound;
    }

    public void FitOnScreen()
    {
        // Only calculate the model bounds but don't move the camera
        Bounds bound = GetBound(model);
        Debug.Log($"Model loaded with bounds: Size: {bound.size}, Center: {bound.center}");
        
        // Optionally, you can store this information for debugging
        GlobalVariables.ModelSize = bound.size;
    }

    // Doublicate the size of mesh components, in which the second half of the tringles winding order and normals are reverse of the first half to enable displaying front and back faces
    //https://answers.unity.com/questions/280741/how-make-visible-the-back-face-of-a-mesh.html
    public void DoublicateFaces()
    {
        for (int i = 0; i < model.GetComponentsInChildren<Renderer>().Length; i++) //Loop through the model children
        {
            // Get oringal mesh components: vertices, normals triangles and texture coordinates 
            Mesh mesh = model.GetComponentsInChildren<MeshFilter>()[i].mesh;
            Vector3[] vertices = mesh.vertices;
            int numOfVertices = vertices.Length;
            Vector3[] normals = mesh.normals;
            int[] triangles = mesh.triangles;
            int numOfTriangles = triangles.Length;
            Vector2[] textureCoordinates = mesh.uv;
            if (textureCoordinates.Length < numOfTriangles) //Check if mesh doesn't have texture coordinates 
            {
                textureCoordinates = new Vector2[numOfVertices * 2];
            }

            // Create a new mesh component, double the size of the original 
            Vector3[] newVertices = new Vector3[numOfVertices * 2];
            Vector3[] newNormals = new Vector3[numOfVertices * 2];
            int[] newTriangle = new int[numOfTriangles * 2];
            Vector2[] newTextureCoordinates = new Vector2[numOfVertices * 2];

            for (int j = 0; j < numOfVertices; j++)
            {
                newVertices[j] = newVertices[j + numOfVertices] = vertices[j]; //Copy original vertices to make the second half of the mew vertices array
                newTextureCoordinates[j] = newTextureCoordinates[j + numOfVertices] = textureCoordinates[j]; //Copy original texture coordinates to make the second half of the mew texture coordinates array  
                newNormals[j] = normals[j]; //First half of the new normals array is a copy original normals
                newNormals[j + numOfVertices] = -normals[j]; //Second half of the new normals array reverse the original normals
            }

            for (int x = 0; x < numOfTriangles; x += 3)
            {
                // copy the original triangle for the first half of array
                newTriangle[x] = triangles[x];
                newTriangle[x + 1] = triangles[x + 1];
                newTriangle[x + 2] = triangles[x + 2];
                // Reversed triangles for the second half of array
                int j = x + numOfTriangles;
                newTriangle[j] = triangles[x] + numOfVertices;
                newTriangle[j + 2] = triangles[x + 1] + numOfVertices;
                newTriangle[j + 1] = triangles[x + 2] + numOfVertices;
            }
            mesh.vertices = newVertices;
            mesh.uv = newTextureCoordinates;
            mesh.normals = newNormals;
            mesh.triangles = newTriangle;
        }
    }

    public void SetManualScaling(bool isManual)
    {
        useManualScaling = isManual;
    }

    public void SetManualScaleValue(float value)
    {
        manualScaleFactor = value;
        
        // If we already have a model loaded, apply the new scale immediately
        if (model != null && useManualScaling)
        {
            // Reset to base flipped scale first
            model.transform.localScale = new Vector3(-1, 1, 1);
            // Then apply the new scale factor
            NormalizeModelScale(useManualScaling, manualScaleFactor);
        }
    }

    private void OnUnitTypeChanged(int index)
    {
        // If a model is loaded, apply the new scale immediately
        if (model != null)
        {
            Debug.Log($"[OnUnitTypeChanged] Processing index: {index}. Applying scale to loaded model.");
            
            // Determine scale factors based on the NEW index
            bool isManualScale = (index != 0);
            float newScaleFactor;
            switch(index)
            {
                case 1: newScaleFactor = 0.001f; break;
                case 2: newScaleFactor = 1.0f; break;
                case 3: newScaleFactor = 0.3048f; break;
                case 4: newScaleFactor = 0.0254f; break;
                default: isManualScale = false; newScaleFactor = 1.0f; break; // Autodetect or unknown
            }

            // Store these potentially for future use IF the listener issue gets fixed
            // but NormalizeModelScale will use the passed parameters for this call.
            this.useManualScaling = isManualScale; 
            this.manualScaleFactor = newScaleFactor;

            // Reset scale before applying new one
            model.transform.localScale = new Vector3(-1, 1, 1);
            
            // Apply scaling using factors determined here
            Debug.Log($"[OnUnitTypeChanged] Calling NormalizeModelScale with isManual={isManualScale}, newScaleFactor={newScaleFactor}");
            NormalizeModelScale(isManualScale, newScaleFactor);
        }
        else
        {
            // If model not loaded, just update the internal state for the *next* load (via Start)
            // This part might be redundant now but doesn't hurt
            this.useManualScaling = (index != 0);
             switch(index)
            {
                case 1: this.manualScaleFactor = 0.0001f; break; //mm
                case 2: this.manualScaleFactor = 0.1f; break; //m
                case 3: this.manualScaleFactor = 0.03048f; break; //ft
                case 4: this.manualScaleFactor = 0.00254f; break; //in
                default: this.manualScaleFactor = 1.0f; break; // Autodetect or unknown
            }
             Debug.LogWarning($"[OnUnitTypeChanged] Called but model is null. Updated internal state: useManual={useManualScaling}, factor={manualScaleFactor}");
        }
    }

    private void HandleMaterialMappings(Dictionary<string, AggregateType> materialMappings)
    {
        Debug.Log("Material mappings confirmed by UI!");

        if (model != null)
        {
            // Iterate through the child OBJECTS (Columns, Slabs, etc.)
            foreach (Transform child in model.transform)
            {
                MeshRenderer renderer = child.GetComponent<MeshRenderer>();
                if (renderer == null)
                {
                    Debug.LogWarning($"Child object '{child.name}' has no MeshRenderer, skipping material property assignment.");
                    continue; // Skip if no renderer
                }

                AggregateType mappedAggregateType = null;
                string originalMaterialName = "Unknown";

                // Find the first material on this renderer that has a mapping
                foreach (Material mat in renderer.sharedMaterials)
                {
                    if (mat != null)
                    {
                        string baseName = mat.name.Replace(" (Instance)", "").Trim();
                        if (materialMappings.TryGetValue(baseName, out AggregateType foundMapping))
                        {
                            mappedAggregateType = foundMapping;
                            originalMaterialName = baseName; // Store the name of the material that triggered the mapping
                            break; // Found a mapping for this object, use it and stop checking materials
                        }
                    }
                }

                // If a mapping was found for any material on this object
                if (mappedAggregateType != null)
                {
                    // Add or get the MaterialProperties component on the CHILD OBJECT
                    MaterialProperties materialProps = child.gameObject.GetComponent<MaterialProperties>();
                    if (materialProps == null) // Add if it doesn't exist
                    {
                        materialProps = child.gameObject.AddComponent<MaterialProperties>();
                    }
                    materialProps.realMaterial = mappedAggregateType; // Assign the mapped ScriptableObject

                    // Determine the input unit system (existing logic)
                    UnitSystem selectedUnitSystem = UnitSystem.Imperial; // Default
                    if (unitDropdown != null)
                    {
                        if (unitDropdown.value == 1 || unitDropdown.value == 2) selectedUnitSystem = UnitSystem.Metric;
                        else if (unitDropdown.value == 3 || unitDropdown.value == 4) selectedUnitSystem = UnitSystem.Imperial;
                    }
                    materialProps.inputUnitSystem = selectedUnitSystem;

                    // Set default ACI values (existing logic - BUT elementType should be defaulted)
                    float inchesToMeters = 0.0254f;
                    // *** Default elementType - User will override via Inspector later ***
                    materialProps.elementType = AciElementType.Other; // Default to 'Other' or 'Slab'
                    materialProps.restraint = AciRestraint.Unrestrained;
                    materialProps.prestress = AciPrestress.Nonprestressed;
                    materialProps.actualCover_u = 1.5f * inchesToMeters; // Default
                    materialProps.actualEquivalentThickness_te = 6.0f * inchesToMeters; // Default

                    Debug.Log($"Applied mapping based on material '{originalMaterialName}' to object '{child.name}'. Mapped to '{mappedAggregateType.realmaterialName}'. ElementType defaulted to {materialProps.elementType}.");
                }
                else
                {
                    Debug.LogWarning($"No material mapping found for any materials on object: '{child.name}'. It will not have MaterialProperties.");
                    // Optionally destroy existing MaterialProperties if re-mapping
                    MaterialProperties existingProps = child.GetComponent<MaterialProperties>();
                    if (existingProps != null) Destroy(existingProps);
                }
            }
        }
        else
        {
             Debug.LogError("HandleMaterialMappings called, but the loaded model reference is null!");
        }

        // Now that mappings are applied, finalize the load process
        FinalizeModelLoad();
    }

    private void FinalizeModelLoad()
    {
        Debug.Log("Finalizing model load process...");

        // Calculate achieved fire rating for all relevant children
        if (model != null)
        {
            foreach (Transform child in model.transform)
            {
                MaterialProperties props = child.GetComponent<MaterialProperties>();
                if (props != null)
                {
                    // Add Mesh Collider if missing for raycasting
                    if (child.GetComponent<MeshCollider>() == null)
                    {
                        child.gameObject.AddComponent<MeshCollider>();
                        Debug.Log($"Added MeshCollider to {child.name}");
                    }

                    // Set the layer for raycasting
                    string layerName = "InspectableModel"; // Make sure this matches your actual layer name
                    int layerValue = LayerMask.NameToLayer(layerName);
                    child.gameObject.layer = layerValue;
                    // ADD DEBUG LOG HERE
                    if (layerValue == -1)
                        Debug.LogWarning($"Layer '{layerName}' does not exist! Could not set layer for {child.name}.");
                    else
                        Debug.Log($"Set layer for {child.name} to {layerValue} ({layerName})");

                    props.achievedFireResistanceRating = AciRatingCalculator.CalculateRating(props);
                    // Debug.Log($"Calculated rating for '{child.name}': {props.achievedFireResistanceRating} hours"); // Original log, maybe comment out if too spammy
                }
            }
        }
        else
        {
            Debug.LogWarning("Cannot calculate ratings because model is null during FinalizeModelLoad.");
        }

        // Any other final setup steps for the model could go here

        // Invoke the main event to notify other scripts the model is fully loaded and mapped
        if (OnModelLoaded != null)
        {
            if (model != null)
            {
                 OnModelLoaded(model);
                 Debug.Log($"OnModelLoaded event invoked for model: {model.name}");
            }
            else
            {
                 Debug.LogError("Attempted to invoke OnModelLoaded, but model is null.");
            }
        }
    }

    private void OnDestroy()
    {
        // Clean up event subscription to prevent memory leaks
        if (materialMapperUI != null)
        {
            materialMapperUI.OnMappingsConfirmed -= HandleMaterialMappings;
        }
    }

    // Helper function to get scaling factors based on current dropdown value
    private void DetermineInitialScale(out bool isManual, out float scaleFactor)
    {
        int currentIndex = (unitDropdown != null) ? unitDropdown.value : 0; // Default to Autodetect if no dropdown
        isManual = (currentIndex != 0);
        
        switch(currentIndex)
        {
            // case 0: // Autodetect - scaleFactor remains 1.0f, isManual is false
            //     scaleFactor = 1.0f; 
            //     break; 
            case 1: // Metric-millimeters
                scaleFactor = 0.001f;
                break;
            case 2: // Metric-meters
                scaleFactor = 1.0f;
                break;
            case 3: // Imperial-feet
                scaleFactor = 0.3048f;
                break;
            case 4: // Imperial-inches
                scaleFactor = 0.0254f;
                break;
            default: // Includes Autodetect (index 0)
                scaleFactor = 1.0f; // Default factor for auto-detect case
                break;
        }
    }
}