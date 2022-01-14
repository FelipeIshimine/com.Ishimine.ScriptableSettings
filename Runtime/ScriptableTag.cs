using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ScriptableTag : ScriptableObject
{
   [SerializeField, AssetList] private List<BaseRuntimeScriptableSingleton> elements = new List<BaseRuntimeScriptableSingleton>();
   
   
   public List<BaseRuntimeScriptableSingleton> Elements => elements;
}