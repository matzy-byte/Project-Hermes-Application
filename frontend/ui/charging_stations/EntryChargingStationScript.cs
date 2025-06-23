using Godot;

namespace UI;

public partial class EntryChargingStationScript : HBoxContainer
{
    public string StationId { get; set; }

    private Label IdLabel { get; set; }
    private Label NameLabel { get; set; }
    private Label StationNameLabel { get; set; }
    private Button RemoveEntryButton { get; set; }

    public override void _Ready()
    {
        StationId = "";
        IdLabel = GetNode<Label>("IdLabel");
        NameLabel = GetNode<Label>("NameLabel");
        StationNameLabel = GetNode<Label>("StationNameLabel");
        RemoveEntryButton = GetNode<Button>("RemoveEntryButton");
        RemoveEntryButton.Pressed += OnRemoveEntryButtonPressed;
    }

    public void Initialize(int id, string name, string stationName, string stationId)
    {
        IdLabel.Text = id.ToString();
        NameLabel.Text = name;
        StationNameLabel.Text = stationName;
        StationId = stationId;
    }

    public void SetId(int id)
    {
        IdLabel.Text = id.ToString();
    }

    private void OnRemoveEntryButtonPressed()
    {
        GetParent().GetParent().GetParent().GetParent<ChargingStationsMenuScript>().RemoveEntry(this);
    }
}
