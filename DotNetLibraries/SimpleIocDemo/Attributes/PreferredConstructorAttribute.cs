using System;

namespace SimpleIocDemo.Attributes
{
    [AttributeUsage(AttributeTargets.Constructor)]
    public sealed class PreferredConstructorAttribute : Attribute
    {
    }
}
