using System;
using UnityEngine;
using System.Collections.Generic;

[AttributeUsage(AttributeTargets.Field)]
public class EnumArrayAttribute : PropertyAttribute {
	public readonly Type EnumType;
	public readonly float DefaultValue;
    public readonly string EnumLabel;
    public readonly int Exception;
    public readonly float ExceptionValue;

    public EnumArrayAttribute(Type enumType, float defaultValue, string name = null)
    {
        EnumType = enumType;
        DefaultValue = defaultValue;
        EnumLabel = (name == null) ? enumType.Name : name;
        Exception = -1;
        ExceptionValue = 0f;
    }
    public EnumArrayAttribute(Type enumType, float defaultValue, string name = null, int exception = 0, float exceptionValue = 0f)
    {
        EnumType = enumType;
        DefaultValue = defaultValue;
        EnumLabel = (name == null) ? enumType.Name : name;
        Exception = exception;
        ExceptionValue = exceptionValue;
    }
}