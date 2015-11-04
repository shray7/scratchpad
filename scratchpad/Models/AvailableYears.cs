using System.Collections.Generic;
using System.Linq;

namespace scratchpad.Models
{
    public class AvailableYears
    {
        public AvailableYears(List<string> keys, List<string> values)
        {
            AvailableYearMap = keys.ToDictionary(d => d, d => values[keys.IndexOf(d)]);
        }
        public Dictionary<string, string> AvailableYearMap { get; set; }
    }
}