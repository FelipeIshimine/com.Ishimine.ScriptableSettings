using System.Collections.Generic;
using UnityEngine;

public abstract class ScriptableSettings<T> : BaseScriptableSettings where T : ScriptableSettings<T>
{
}