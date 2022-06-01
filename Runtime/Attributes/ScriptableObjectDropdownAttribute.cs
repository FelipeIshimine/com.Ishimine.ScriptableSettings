using System;
using UnityEngine;
[AttributeUsage(AttributeTargets.Field)]
public class BucketDropdownAttribute : PropertyAttribute
{
    public readonly Type TargetType;

    public BucketDropdownAttribute(Type targetType)
    {
        TargetType = targetType;
    }
}