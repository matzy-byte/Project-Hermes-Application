using Godot;
using System.Collections.Generic;
using UI;

public partial class ChatLogScript : VBoxContainer
{
    public bool Unfolded { get; set; }

    private TextureButton ChatLogButton { get; set; }
    private VBoxContainer ChatLogContainer { get; set; }
    private const int maxMessages = 500;

    public override void _Ready()
    {
        Unfolded = false;
        ChatLogButton = GetNode<TextureButton>("%ChatLogButton");
        ChatLogButton.Pressed += OnChatLogButtonPressed;
        ChatLogContainer = GetNode<VBoxContainer>("%ChatLogContainer");
    }

    public void WriteLog(List<string> messages)
    {
        int totalAfterAdd = ChatLogContainer.GetChildCount() + messages.Count;
        int removeCount = Mathf.Max(0, totalAfterAdd - maxMessages);
        for (int i = 0; i < removeCount; i++)
        {
            ChatLogContainer.GetChild(i).QueueFree();
        }

        foreach (string msg in messages)
        {
            Label label = new()
            {
                Text = msg,
                AutowrapMode = TextServer.AutowrapMode.Word,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            ChatLogContainer.AddChild(label);
        }
        ChatLogContainer.GetParent<ScrollContainer>().ScrollVertical = (int)ChatLogContainer.GetParent<ScrollContainer>().GetVScrollBar().MaxValue;
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
