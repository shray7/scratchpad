using System.Collections.Generic;
using System.Linq;

namespace scratchpad.Models
{
    public class AvailableCampus
    {
        public AvailableCampus(List<string> keys, List<string> values)
        {
            CampusMap =  keys.ToDictionary(d => d, d => values[keys.IndexOf(d)]);
        }
        public Dictionary<string, string> CampusMap { get; set; }
    }
}