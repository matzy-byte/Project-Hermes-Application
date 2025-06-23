using Godot;
using shared;
using Singletons;
using System.Collections.Generic;
using System.Linq;

namespace UI;

public partial class LoadingStationsMenuScript : Panel
{
    private List<EntryLoadingStationScript> loadingStations { get; set; }
    private LineEdit NameEdit { get; set; }
    private OptionButton StationOptionButton { get; set; }
    private VBoxContainer EntryContainer { get; set; }
    private Button AddLoadingStationButton { get; set; }
    private PackedScene entryScene = ResourceLoader.Load<PackedScene>("res://ui/loading_stations/EntryLoadingStation.tscn");

    public override void _Ready()
    {
        loadingStations = [];
        NameEdit = GetNode<LineEdit>("%NameEdit");
        StationOptionButton = GetNode<OptionButton>("%StationOptionButton");
        EntryContainer = GetNode<VBoxContainer>("%EntryContainer");
        AddLoadingStationButton = GetNode<Button>("%AddLoadingStationButton");
        AddLoadingStationButton.Pressed += CreateEntry;
    }

    public void RemoveEntry(EntryLoadingStationScript entry)
    {
        loadingStations.Remove(entry);
        DisableStationExtra(entry.StationId);
        entry.QueueFree();
        int index = 0;
        foreach (EntryLoadingStationScript validEntry in loadingStations)
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

    public List<string> GetLoadingStations()
    {
        return [.. loadingStations.Select(entry => entry.StationId)];
    }

    public void Clear()
    {
        loadingStations.Clear();
        foreach (Node entry in EntryContainer.GetChildren())
        {
            entry.QueueFree();
        }
    }

    private void CreateEntry()
    {
        EntryLoadingStationScript entry = entryScene.Instantiate<EntryLoadingStationScript>();
        int id = loadingStations.Count;
        string name = NameEdit.Text == "" ? $"LoadingStation#{id}" : NameEdit.Text;
        string stationName = StationOptionButton.GetItemText(StationOptionButton.Selected);
        string stationId = GameManagerScript.Instance.Stations.Find(station => station.Data.StationName == stationName).Data.StationId;
        loadingStations.Add(entry);
        EntryContainer.AddChild(entry);
        entry.Initialize(id, name, stationName, stationId);
        NameEdit.Clear();
        StationOptionButton.Select(-1);

        EnableStationExtra(stationId);
    }

    private static void EnableStationExtra(string stationId)
    {
        GameManagerScript.Instance.Stations.Find(station => station.Data.StationId == stationId).EnableExtraLoading();
    }

    private static void DisableStationExtra(string stationId)
    {
        GameManagerScript.Instance.Stations.Find(station => station.Data.StationId == stationId).DisableExtraLoading();
    }
}
