using Godot;
using System;

public partial class RobotScript : StaticBody3D
{
    public int RobotID;
    public TrainScript? Train;
    public StationScript? CurrentStation; 

    public void Setup(int robotID, TrainScript? train, StationScript? currentStation)
    {
        RobotID = robotID;
        Train = train;
        CurrentStation = currentStation;
    }   
}
