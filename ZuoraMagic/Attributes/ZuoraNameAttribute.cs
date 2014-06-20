using System;
using LINQtoCSV;

namespace ZuoraMagic.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class ZuoraNameAttribute : CsvColumnAttribute
    {
        /// <summary>
        ///     Manually set the name of the object
        ///     this value should be mapped from.
        /// </summary>
        public string MappingOverride { get; set; }

        public ZuoraNameAttribute()
        {
        }

        public ZuoraNameAttribute(string name)
        {
            Name = name;
        }
    }
}