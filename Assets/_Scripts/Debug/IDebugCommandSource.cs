using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDebugCommandSource
{
    IEnumerable<DebugCommand> GetCommands();
}

