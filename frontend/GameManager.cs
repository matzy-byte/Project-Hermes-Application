using Godot;
using System;

public partial class GameManager : Node
{
    public static GameManager Instance;
    public string simulationSettings;

    public override void _Ready()
    {
        Instance = this;
    }

    public void UpdateSimulationSettings()
    {}

    public void StartSimulation()
    {}

    public void StopSimulation()
    {}

}
