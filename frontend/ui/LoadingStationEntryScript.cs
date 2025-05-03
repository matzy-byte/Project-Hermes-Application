using Godot;
using System;

public partial class LoadingStationEntryScript : HBoxContainer
{
    public string EntryID;
    public string StationID;

    public override void _Ready()
    {
        GetNode<Button>("EntryRemove").Pressed += DeleteButtonPressed;
    }

    public void SetID(string id)
    {
        EntryID = id;
        GetNode<Label>("EntryLabel").Text = EntryID;
    }

    public void SetStation(string id)
    {
        StationID = id;
    }

    private void DeleteButtonPressed()
    {
        GetParent().GetParent().GetParent<HUDScript>().RemoveLoadingStationEntry(this);
    }

}
