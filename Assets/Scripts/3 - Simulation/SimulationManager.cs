using UnityEngine;
using TMPro;

public class SimulationManager : MonoBehaviour
{
    public enum SimulationState { Stopped, Running, Paused }
    public SimulationState currentState = SimulationState.Stopped;
    public float simulationTimeSeconds = 0f;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI stateText;

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
        Debug.Log("Simulation Reset");
    }

    string FormatTime(float seconds)
    {
        int h = (int)(seconds / 3600);
        int m = (int)((seconds % 3600) / 60);
        int s = (int)(seconds % 60);
        return $"{h:D2}:{m:D2}:{s:D2}";
    }
} 