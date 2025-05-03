using System;

public class Train
{
    public int TrainID { get; set; }
    public bool Driving { get; set; }
    public bool InStation { get; set; }
    public bool DrivingForward { get; set; }
    public string CurrentStation { get; set; }
    public string NextStation { get; set; }
    public float TravelDistance { get; set; }
    public float WaitingTime { get; set; }
}