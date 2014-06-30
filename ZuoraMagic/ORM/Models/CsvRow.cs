using System;
using System.Linq;

namespace ZuoraMagic.ORM.Models
{
    public class CsvRow
    {
        private readonly string[] _row;
        private readonly string[] _headers;

        public string this[string name]
        {
            get
            {
                int index = Array.FindIndex(_headers, x => x.Equals(name));
                return _row[index];
            }
        }

        public CsvRow(string[] row, string[] headers)
        {
            _row = row;
            _headers = headers;
        }

        public bool ContainsKey(string name)
        {
            return _headers.Contains(name);
        }
    }
}