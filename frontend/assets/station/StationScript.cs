using Godot;
using System;

public partial class StationScript : StaticBody3D
{
    public string StationID { get; set; }
    public string StationName { get; set; }
    public double Long { get; set; }
    public double Lat { get; set; }

    public void Setup(string stationID, string stationName, double longitudinal, double latidudinal)
    {
        StationID = stationID;
        StationName = stationName;
        GetNode<Label>("%Label").Text = StationName;
        Long = longitudinal;
        Lat = latidudinal;
    }

    public override void _MouseEnter()
    {
        GetNode<Sprite3D>("Sprite3D").Visible = true;
    }

    public override void _MouseExit()
    {
        {
        GetNode<Sprite3D>("Sprite3D").Visible = false;
    }
    }


}
