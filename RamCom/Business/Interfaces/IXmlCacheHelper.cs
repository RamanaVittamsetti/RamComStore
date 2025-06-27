using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Business.Interfaces
{
    public interface IXmlCacheHelper
    {
        XDocument RetriveXmlValues(string cacheName, string path = null);
    }
}
