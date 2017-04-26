using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MDOrganizer
{
    class Test
    {
        public static void Execute()
        {
            string url = "http://graph.microsoft.io/en-us/docs/platform/rest";
            Uri uri = new Uri(url);
            string path = uri.GetComponents(UriComponents.Path, UriFormat.UriEscaped);
            string[] tokens = path.Split('/');
            string culture = tokens[0];
        }
    }
}
