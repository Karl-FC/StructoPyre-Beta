using UnityEngine;
using TMPro;

public class SimulationManager : MonoBehaviour
{
    public enum SimulationState { Stopped, Running, Paused }
    public SimulationState currentState = SimulationState.Stopped;
    
    [Header("Simulation Timing")]
    public float simulationTimeSeconds = 0f;
    [Tooltip("Controls how quickly the simulation progresses. 1 = real-time, 60 = 1 minute per second, 3600 = 1 hour per second.")]
    [Range(1f, 3600f)]
    public float simulationTimeScale = 60f; // Default to 60x speed (1 simulated minute per real second)

    // Custom delta time for simulation - other scripts should access this instead of Time.deltaTime
    // This will be: real Time.deltaTime * simulationTimeScale when running, 0 when paused
    [HideInInspector] public float simulationDeltaTime = 0f;

    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI stateText;
    [Tooltip("Optional text to display the current simulation speed multiplier")]
    public TextMeshProUGUI speedText;

    // Singleton pattern for easy access
    public static SimulationManager Instance { get; private set; }

    void Awake()
    {
        // Simple singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        // Calculate our custom simulation delta time
        if (currentState == SimulationState.Running)
        {
            // Scale the real deltaTime by our simulation scale factor
            simulationDeltaTime = Time.deltaTime * simulationTimeScale;
            
            // Increment the simulation time using our scaled delta
            simulationTimeSeconds += simulationDeltaTime;
        }
        else
        {
            // When not running, the simulation delta time is zero
            simulationDeltaTime = 0f;
        }
        
        // UI updates
        if (timerText != null)
        {
            timerText.text = FormatTime(simulationTimeSeconds);
        }
        if (stateText != null)
        {
            stateText.text = currentState.ToString();
        }
        if (speedText != null && currentState == SimulationState.Running)
        {
            speedText.text = $"{simulationTimeScale}x";
        }
        else if (speedText != null)
        {
            speedText.text = "";
        }
    }

    public void StartSimulation()
    {
        if (currentState != SimulationState.Running)
        {
            currentState = SimulationState.Running;
            Debug.Log($"Simulation Started (Speed: {simulationTimeScale}x)");
        }
    }

    public void PauseSimulation()
    {
        if (currentState == SimulationState.Running)
        {
            currentState = SimulationState.Paused;
            Debug.Log("Simulation Paused");
        }
    }

    public void ResetSimulation()
    {
        currentState = SimulationState.Stopped;
        simulationTimeSeconds = 0f;
        Debug.Log("Simulation Resetting...");

        // Find all trackers and reset their state
        FireIntegrityTracker[] trackers = FindObjectsOfType<FireIntegrityTracker>();
        foreach (FireIntegrityTracker tracker in trackers)
        {
            tracker.ResetState();
        }
        
        // Find all visualizers and reset their appearance
        IntegrityVisualizer[] visualizers = FindObjectsOfType<IntegrityVisualizer>();
        foreach (IntegrityVisualizer visualizer in visualizers)
        {
            visualizer.ResetVisuals();
        }

        Debug.Log($"Simulation Reset Complete. Reset {trackers.Length} trackers and {visualizers.Length} visualizers.");
    }

    // Change simulation speed during runtime
    public void SetTimeScale(float newScale)
    {
        simulationTimeScale = newScale;
        Debug.Log($"Simulation Time Scale set to: {simulationTimeScale}x");
    }

    // Increase simulation speed (could be connected to a UI button)
    public void IncreaseSpeed()
    {
        float newScale = simulationTimeScale * 2f; // Double the speed
        // Cap at a reasonable maximum
        newScale = Mathf.Min(newScale, 3600f);
        SetTimeScale(newScale);
    }

    // Decrease simulation speed (could be connected to a UI button)
    public void DecreaseSpeed()
    {
        float newScale = simulationTimeScale / 2f; // Half the speed
        // Ensure it doesn't go below 1.0
        newScale = Mathf.Max(newScale, 1f);
        SetTimeScale(newScale);
    }

    // Format time as either HH:MM:SS or D:HH:MM:SS depending on duration
    string FormatTime(float seconds)
    {
        int totalSeconds = (int)seconds;
        
        // Check if we need to display days (time > 24 hours)
        if (totalSeconds >= 86400) // 86400 = 24 * 60 * 60 (seconds in a day)
        {
            int d = totalSeconds / 86400;
            int remainingSeconds = totalSeconds % 86400;
            
            int h = remainingSeconds / 3600;
            remainingSeconds %= 3600;
            
            int m = remainingSeconds / 60;
            int s = remainingSeconds % 60;
            
            return $"{d}:{h:D2}:{m:D2}:{s:D2}"; // Format: D:HH:MM:SS
        }
        else
        {
            // Regular HH:MM:SS format for times under 24 hours
            int h = totalSeconds / 3600;
            int remainingSeconds = totalSeconds % 3600;
            
            int m = remainingSeconds / 60;
            int s = remainingSeconds % 60;
            
            return $"{h:D2}:{m:D2}:{s:D2}"; // Format: HH:MM:SS
        }
    }
} 