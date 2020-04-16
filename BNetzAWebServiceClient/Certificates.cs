using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BNetzAWebServiceClient
{
    class Certificates
    {
        public static bool ValidateRemoteCertificate(object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors policyErrors)
        {
            return true;
        }
    }
}
