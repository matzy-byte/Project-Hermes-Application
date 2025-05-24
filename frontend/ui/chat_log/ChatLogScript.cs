using Godot;
using System;
using UI;

public partial class ChatLogScript : VBoxContainer
{
    public bool Unfolded { get; set; }

    private TextureButton ChatLogButton { get; set; }
    private VBoxContainer ChatLogContainer { get; set; }

    public override void _Ready()
    {
        Unfolded = false;
        ChatLogButton = GetNode<TextureButton>("%ChatLogButton");
        ChatLogButton.Pressed += OnChatLogButtonPressed;
        ChatLogContainer = GetNode<VBoxContainer>("%ChatLogContainer");
    }

    public void Clear()
    {
        foreach (Node entry in ChatLogContainer.GetChildren())
        {
            entry.QueueFree();
        }
    }

    public void OnChatLogButtonPressed()
    {
        GetParent<HUDScript>().OpenChatLog();
    }
}
