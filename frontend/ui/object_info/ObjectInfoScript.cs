using Godot;
using Interface;
using System.Collections.Generic;
using System.Linq;

namespace UI;

public partial class ObjectInfoScript : PanelContainer
{
    private GridContainer InfoContainer;
    private IInteractable Interactable = null;

    public override void _Ready()
    {
        InfoContainer = GetNode<GridContainer>("%InfoContainer");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!Visible || Interactable == null)
        {
            return;
        }

        UpdateInfo();
    }


    public void ShowInfo(Dictionary<string, string> data, IInteractable interactable)
    {
        Interactable = interactable;
        Clear();
        foreach (var entry in data)
        {
            Label key = new() { Text = entry.Key, SizeFlagsHorizontal = SizeFlags.Expand };
            Label value = new() { Text = entry.Value, SizeFlagsHorizontal = SizeFlags.Expand };
            InfoContainer.AddChild(key);
            InfoContainer.AddChild(value);
        }
        Visible = true;
    }

    public void Clear()
    {
        foreach (Node child in InfoContainer.GetChildren())
        {
            child.QueueFree();
        }
    }

    public void Stop()
    {
        Visible = false;
        Clear();
        Interactable = null;
    }

    private void UpdateInfo()
    {
        Dictionary<string, string> data = Interactable.GetInfo();
        List<Label> children = [.. InfoContainer.GetChildren().OfType<Label>()];
        List<Label> everySecond = [.. children.Where((child, index) => index % 2 == 1)];
        for (int i = 0; i < everySecond.Count; i++)
        {
            everySecond[i].Text = data.Values.ElementAt(i);
        }
    }
}
