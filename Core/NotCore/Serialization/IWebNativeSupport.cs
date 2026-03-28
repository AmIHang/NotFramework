using Not.Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Not.Core.Serialization
{
    public interface IWebNativeSupport : IService
    {
        public string GetPublicOPath(object obj);
    }

    public class WebNativeSupport : IWebNativeSupport
    {
        public string GetPublicOPath(object obj)
        {
            return "#";
        }
    }
}
