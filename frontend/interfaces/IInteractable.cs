using System.Collections.Generic;
using Godot;

namespace Interface;

public interface IInteractable
{
    public Node3D Select();
    public Dictionary<string, string> GetInfo();
}