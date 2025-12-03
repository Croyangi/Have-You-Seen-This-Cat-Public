using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class DebugCommand
{
    public string Id { get; }
    public string Description { get; }
    public string Format { get; }
    public Action<string[]> Callback { get; }

    public DebugCommand(string id, string description, string format, Action<string[]> callback)
    {
        Id = id;
        Description = description;
        Format = format;
        Callback = callback;
    }

    public void Execute(string[] args) => Callback?.Invoke(args);
}