using System;
using LINQtoCSV;

namespace ZuoraMagic.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ZuoraNameAttribute : CsvColumnAttribute
    {
        public ZuoraNameAttribute(string name)
        {
            Name = name;
        }
    }
}