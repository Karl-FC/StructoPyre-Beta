using UnityEngine;
using TMPro;

public class SimulationManager : MonoBehaviour
{
    public enum SimulationState { Stopped, Running, Paused }
    public SimulationState currentState = SimulationState.Stopped;
    public float simulationTimeSeconds = 0f;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI stateText;

    // Optional: Cache found components for performance if reset is frequent
    // private FireIntegrityTracker[] trackers;
    // private IntegrityVisualizer[] visualizers;

    // void Start() {
    //     FindSimulationComponents();
    // }

    void Update()
    {
        if (currentState == SimulationState.Running)
        {
            simulationTimeSeconds += Time.deltaTime;
        }
        if (timerText != null)
        {
            timerText.text = FormatTime(simulationTimeSeconds);
        }
        if (stateText != null)
        {
            stateText.text = currentState.ToString();
        }
    }

    public void StartSimulation()
    {
        // Ensure components are found before starting if not cached
        // FindSimulationComponents(); 
        currentState = SimulationState.Running;
        Debug.Log("Simulation Started");
    }

    public void PauseSimulation()
    {
        currentState = SimulationState.Paused;
        Debug.Log("Simulation Paused");
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

    // Optional: Method to find and cache components
    // void FindSimulationComponents()
    // {
    //     trackers = FindObjectsOfType<FireIntegrityTracker>();
    //     visualizers = FindObjectsOfType<IntegrityVisualizer>();
    //     Debug.Log($"Found {trackers.Length} trackers and {visualizers.Length} visualizers.");
    // }

    string FormatTime(float seconds)
    {
        int h = (int)(seconds / 3600);
        int m = (int)((seconds % 3600) / 60);
        int s = (int)(seconds % 60);
        return $"{h:D2}:{m:D2}:{s:D2}";
    }
} 