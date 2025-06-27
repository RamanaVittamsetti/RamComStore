using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Exceptions
{
    public class ResourseNotFoundException : Exception
    {
        public string ResourceName { get; set; }
        public ResourseNotFoundException(string resourceName, string message):base(message)
        {
            ResourceName = resourceName;
        }
    }
}
