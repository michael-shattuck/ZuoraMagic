using System;
using LINQtoCSV;

namespace ZuoraMagic.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class ZuoraMappingAdditionAttribute : CsvColumnAttribute
    {
        /// <summary>
        ///     Manually set the name of the object
        ///     this value should be mapped from.
        /// </summary>
        public string MappingName { get; set; }

        public ZuoraMappingAdditionAttribute()
        {
        }

        public ZuoraMappingAdditionAttribute(string name)
        {
            MappingName = name;
        }
    }
}