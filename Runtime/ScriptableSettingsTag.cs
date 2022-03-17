using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ScriptableSettingsTag : ScriptableObject
{
   [SerializeField, AssetList] private List<BaseRuntimeScriptableSingleton> elements = new List<BaseRuntimeScriptableSingleton>();
   
   
   public List<BaseRuntimeScriptableSingleton> Elements => elements;
}