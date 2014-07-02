using System;

namespace ZuoraMagic.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ZuoraIgnoreAttribute : Attribute
    {
    }
}