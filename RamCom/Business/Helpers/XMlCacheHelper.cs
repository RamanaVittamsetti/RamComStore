using Business.Extensions;
using Business.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Business.Helpers
{
    public class XMlCacheHelper : IXmlCacheHelper
    {
        private readonly IMemoryCache _cache;
        private string _cachePrefix = "@@xmCache_";
        private readonly ILogger<XMlCacheHelper> _logger;

        public XMlCacheHelper(IMemoryCache cache, ILogger<XMlCacheHelper> logger )
        {
            _cache = cache;
            _logger = logger;
        }

        public XDocument RetriveXmlValues(string cacheName, string fullPath = null)
        {
            if (cacheName.IsNotNullOrWhiteSpace())
            {
                try
                {
                    XDocument xmlDocument;
                    string cacheKey = _cachePrefix + cacheName;

                    if (_cache.TryGetValue(cacheKey, out xmlDocument))
                    {
                        return xmlDocument;
                    }
                    else if (fullPath.IsNotNullOrWhiteSpace())
                    {

                        if (!File.Exists(fullPath))
                        {
                            throw new FileNotFoundException($"File not found: {fullPath}");
                        }
                        xmlDocument = XDocument.Load(fullPath);
                        if (xmlDocument != null)
                        {
                            var fileprovider = new PhysicalFileProvider(Path.GetDirectoryName(fullPath));
                            IChangeToken changeToken = fileprovider.Watch(Path.GetFileName(fullPath));

                            var cacheEntryOptions = new MemoryCacheEntryOptions()
                                .SetPriority(CacheItemPriority.High)
                                .AddExpirationToken(changeToken);

                            _cache.Set(cacheKey, xmlDocument, cacheEntryOptions);
                            return xmlDocument;
                        }
                        else
                        {
                            throw new Exception($"Failed to load XML document from {fullPath}");
                        }
                    }
                }
               catch(Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }

            }

            return new XDocument();
        }
    }
}
