using Godot;
using shared;
using Singletons;
using System.Collections.Generic;
using System.Linq;

namespace UI;

public partial class ChargingStationsMenuScript : Panel
{
    private List<EntryChargingStationScript> chargingStations { get; set; }
    private LineEdit NameEdit { get; set; }
    private OptionButton StationOptionButton { get; set; }
    private VBoxContainer EntryContainer { get; set; }
    private Button AddChargingStationButton { get; set; }
    private PackedScene entryScene = ResourceLoader.Load<PackedScene>("res://ui/charging_stations/EntryChargingStation.tscn");

    public override void _Ready()
    {
        chargingStations = [];
        NameEdit = GetNode<LineEdit>("%NameEdit");
        StationOptionButton = GetNode<OptionButton>("%StationOptionButton");
        EntryContainer = GetNode<VBoxContainer>("%EntryContainer");
        AddChargingStationButton = GetNode<Button>("%AddChargingStationButton");
        AddChargingStationButton.Pressed += CreateEntry;
    }

    public void RemoveEntry(EntryChargingStationScript entry)
    {
        chargingStations.Remove(entry);
        entry.QueueFree();
        int index = 0;
        foreach (EntryChargingStationScript validEntry in chargingStations)
        {
            validEntry.SetId(index);
            index++;
        }
    }

    public void UpdateStationsOption(List<StationData> stations)
    {
        StationOptionButton.Clear();
        foreach (StationData data in stations)
        {
            StationOptionButton.AddItem(data.StationName);
        }
        StationOptionButton.Select(-1);
    }

    public List<string> GetChargingStations()
    {
        return [.. chargingStations.Select(entry => entry.StationId)];
    }

    public void Clear()
    {
        chargingStations.Clear();
        foreach (Node entry in EntryContainer.GetChildren())
        {
            entry.QueueFree();
        }
    }

    private void CreateEntry()
    {
        EntryChargingStationScript entry = entryScene.Instantiate<EntryChargingStationScript>();
        int id = chargingStations.Count;
        string name = NameEdit.Text == "" ? $"LoadingStation#{id}" : NameEdit.Text;
        string stationName = StationOptionButton.GetItemText(StationOptionButton.Selected);
        string stationId = GameManagerScript.Instance.Stations.Find(station => station.Data.StationName == stationName).Data.StationId;
        chargingStations.Add(entry);
        EntryContainer.AddChild(entry);
        entry.Initialize(id, name, stationName, stationId);
        NameEdit.Clear();
        StationOptionButton.Select(-1);
    }
}
