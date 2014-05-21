using System.Collections.Generic;
using ZuoraMagic.Entities;
using ZuoraMagic.SoapApi.Enum;

namespace ZuoraMagic.Configuration
{
    public class CrudOperation<T> where T : ZObject
    {
        public CrudOperations OperationType { get; set; }
        public string ExternalIdField { get; set; }
        public IEnumerable<T> Items { get; set; }
    }
}