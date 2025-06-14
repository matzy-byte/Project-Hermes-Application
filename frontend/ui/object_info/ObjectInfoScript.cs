using Godot;
using System;
using System.Collections.Generic;

public partial class ObjectInfoScript : PanelContainer
{
    private GridContainer InfoContainer;

    public override void _Ready()
    {
        InfoContainer = GetNode<GridContainer>("%InfoContainer");
    }

    public void ShowInfo(Dictionary<string, string> data)
    {
        Clear();
        foreach (var entry in data)
        {
            Label key = new() { Text = entry.Key };
            Label value = new() { Text = entry.Value };
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
}
