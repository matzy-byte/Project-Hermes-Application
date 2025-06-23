using System;
using Godot;

namespace UI;

public partial class HUDScript : Control
{
    public ConfigurationMenuScript ConfigurationMenu { get; set; }
    public LoadingStationsMenuScript LoadingStationsMenu { get; set; }
    public ChargingStationsMenuScript ChargingStationsMenu { get; set; }
    public ControlMenuScript ControlMenu { get; set; }
    public ChatLogScript ChatLog { get; set; }
    public ObjectInfoScript ObjectInfo { get; set; }
    public AnimationPlayer AnimationPlayer { get; set; }

    public override void _Ready()
    {
        ConfigurationMenu = GetNode<ConfigurationMenuScript>("ConfigurationMenu");
        LoadingStationsMenu = GetNode<LoadingStationsMenuScript>("LoadingStationsMenu");
        ChargingStationsMenu = GetNode<ChargingStationsMenuScript>("ChargingStationsMenu");
        ControlMenu = GetNode<ControlMenuScript>("ControlMenu");
        ChatLog = GetNode<ChatLogScript>("ChatLogAnchor/ChatLog");
        ObjectInfo = GetNode<ObjectInfoScript>("ObjectInfo");
        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    }

    public void StartSimulation()
    {
        ConfigurationMenu.Visible = false;
        LoadingStationsMenu.Visible = false;
        ChargingStationsMenu.Visible = false;
        ControlMenu.Visible = true;
        ChatLog.Visible = true;
        ControlMenu.UpdateSpeedSlider();
    }

    public void NewSimulation()
    {
        ConfigurationMenu.Visible = true;
        LoadingStationsMenu.Clear();
        ChargingStationsMenu.Clear();
        ControlMenu.Visible = false;
        ChatLog.Visible = false;
        ObjectInfo.Visible = false;
        AnimationPlayer.Play("RESET");
        ControlMenu.Unfolded = false;
        ChatLog.Unfolded = false;
        ChatLog.Clear();
    }

    public void OpenLoadingStationMenu()
    {
        if (ChargingStationsMenu.Visible)
        {
            ChargingStationsMenu.Visible = false;
        }
        if (LoadingStationsMenu.Visible)
        {
            LoadingStationsMenu.Visible = false;
            return;
        }
        LoadingStationsMenu.Visible = true;
    }

    public void OpenChargingStationMenu()
    {
        if (LoadingStationsMenu.Visible)
        {
            LoadingStationsMenu.Visible = false;
        }
        if (ChargingStationsMenu.Visible)
        {
            ChargingStationsMenu.Visible = false;
            return;
        }
        ChargingStationsMenu.Visible = true;
    }

    public void OpenControlMenu()
    {
        if (ControlMenu.Unfolded)
        {
            AnimationPlayer.Play("ControlFold");
            ControlMenu.Unfolded = false;
            return;
        }
        AnimationPlayer.Play("ControlUnfold");
        ControlMenu.Unfolded = true;
    }

    public void OpenChatLog()
    {
        if (ChatLog.Unfolded)
        {
            AnimationPlayer.Play("ChatFold");
            ChatLog.Unfolded = false;
            return;
        }
        AnimationPlayer.Play("ChatUnfold");
        ChatLog.Unfolded = true;
    }
}
