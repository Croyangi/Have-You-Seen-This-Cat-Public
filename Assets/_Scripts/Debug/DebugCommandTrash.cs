using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
public class DebugCommandBase
{
    public string Id { get; private set; }
    public string Description { get; private set; }
    public string Format { get; private set; }

    public DebugCommandBase(string id, string description, string format)
    {
        Id = id;
        Description = description;
        Format = format;
    }
}

public class DebugCommand : DebugCommandBase
{
    private Action _command;

    public DebugCommand(string id, string description, string format, Action command) : base(id, description, format)
    {
        _command = command;
    }
    
    public void Invoke() 
    {
        _command.Invoke();
    }
}
*/