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

public class OpenFile : MonoBehaviour
{
    public TextMeshProUGUI textMeshPro;
    public GameObject model; //Load OBJ Model

    public delegate void ModelLoadedDelegate(GameObject loadedModel);
    public event ModelLoadedDelegate OnModelLoaded;

    [Header("Model Scaling")]
    [Tooltip("Enable to manually set scale factor instead of auto-detection")]
    [SerializeField] private bool useManualScaling = false;
    [Tooltip("Scale factor applied when manual scaling is enabled")]
    [SerializeField] private float manualScaleFactor = 1.0f;
    [Tooltip("Toggle to show scale debugging information")]
    [SerializeField] private bool showScaleDebugging = true;

    [Header("Unit Conversions")]
    [SerializeField] private TMP_Dropdown unitDropdown;

#if UNITY_WEBGL && !UNITY_EDITOR
    // WebGL
    [DllImport("__Internal")]
    private static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);

    public void OnClickOpen() {
        UploadFile(gameObject.name, "OnFileUpload", ".obj,.mtl", false);
    }

    // Called from browser
    public void OnFileUpload(string url) {
        StartCoroutine(OutputRoutineOpen(url));
    }
#else

    // Standalone platforms & editor    s
    public void OnClickOpen()
    {
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", new[] { new ExtensionFilter("3D Model Files", "obj", "mtl") }, false);
        if (paths.Length > 0)
        {
            StartCoroutine(OutputRoutineOpen(new System.Uri(paths[0]).AbsoluteUri));
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

    private IEnumerator OutputRoutineOpen(string url)
    {
        // Get the directory path of the OBJ file
        string objPath = url;
        string directoryPath = Path.GetDirectoryName(objPath);
        string fileName = Path.GetFileNameWithoutExtension(objPath);
        string mtlPath = Path.Combine(directoryPath, fileName + ".mtl");

        // Load the OBJ file
        UnityWebRequest www = UnityWebRequest.Get(objPath);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("WWW ERROR: " + www.error);
            yield break;
        }

        // Load the MTL file if it exists
        UnityWebRequest mtlRequest = UnityWebRequest.Get(mtlPath);
        yield return mtlRequest.SendWebRequest();
        
        // Create memory streams for both files
        MemoryStream objStream = new MemoryStream(Encoding.UTF8.GetBytes(www.downloadHandler.text));
        MemoryStream mtlStream = null;
        
        if (mtlRequest.result == UnityWebRequest.Result.Success)
        {
            mtlStream = new MemoryStream(Encoding.UTF8.GetBytes(mtlRequest.downloadHandler.text));
            Debug.Log("Successfully loaded MTL file");
        }
        else
        {
            Debug.Log("No MTL file found or error loading MTL file");
        }

        // Load the model with materials
        if (model != null)
        {
            Destroy(model);
        }

        OBJLoader loader = new OBJLoader();
        if (mtlStream != null)
        {
            model = loader.Load(objStream, mtlStream);
        }
        else
        {
            model = loader.Load(objStream);
        }
        
        // Place the model at the origin
        model.transform.position = Vector3.zero;
        
        // First let's apply the X-flipping but keep scale neutral
        model.transform.localScale = new Vector3(-1, 1, 1);
        
        // Apply normalization to ensure proper scaling
        NormalizeModelScale();
        
        // Apply double-sided faces
        DoublicateFaces();
        
        // Store imported model in global variables
        GlobalVariables.ImportedModel = model;

        // After model is fully prepared, invoke the event
        if (OnModelLoaded != null)
        {
            OnModelLoaded(model);
        }
    }

    private void NormalizeModelScale()
    {
        // Get the model's bounds
        Bounds bounds = GetBound(model);
        
        if (showScaleDebugging)
        {
            Debug.Log($"Original model size: {bounds.size}, Center: {bounds.center}");
        }
        
        float scaleFactor = 1.0f;
        
        if (useManualScaling)
        {
            // Use the manual scale factor if enabled
            scaleFactor = manualScaleFactor;
            if (showScaleDebugging)
            {
                Debug.Log($"Using manual scale factor: {scaleFactor}");
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
            NormalizeModelScale();
        }
    }

    private void OnUnitTypeChanged(int index)
    {
        // If a model is loaded, apply the new scale immediately
        if (model != null)
        {
            // Reset to base flipped scale first
            model.transform.localScale = new Vector3(-1, 1, 1);
            
            // Apply the selected unit conversion
            useManualScaling = (index != 0); // If not "autodetect", use manual scaling
            
            switch(index)
            {
                case 0: // Autodetect
                    useManualScaling = false;
                    break;
                case 1: // Metric-millimeters
                    manualScaleFactor = 0.001f; // 1mm = 0.001m in Unity
                    break;
                case 2: // Metric-meters
                    manualScaleFactor = 1.0f;   // 1m = 1m in Unity
                    break;
                case 3: // Imperial-feet
                    manualScaleFactor = 0.3048f; // 1ft = 0.3048m
                    break;
                case 4: // Imperial-inches
                    manualScaleFactor = 0.0254f; // 1in = 0.0254m
                    break;
            }
            
            // Re-apply scaling
            NormalizeModelScale();
        }
    }
}