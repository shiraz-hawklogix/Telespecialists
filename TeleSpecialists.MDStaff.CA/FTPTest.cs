using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace TeleSpecialists.CA
{
    public class FTPTest
    {
        public void Test()
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create("ftp://vfwprograms.condadocloud.net/");

            
            request.EnableSsl = true;
            //request.UsePassive = true;

            //byte[] toBytes = Encoding.ASCII.GetBytes("79:f3:ac:15:a4:fe:4f:c9:2d:b0:63:0d:77:a0:f8:4b:00:35:80:9a");
            //request.ClientCertificates.Add(new X509Certificate(toBytes));

            request.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

            request.Credentials = new NetworkCredential("AS-VFWMemstats", "516ASVFWMemstats1945");

            request.Proxy = null;

            ServicePointManager.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();

            Stream responseStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(responseStream);
            Console.WriteLine(reader.ReadToEnd());

            Console.WriteLine($"Directory List Complete, status {response.StatusDescription}");

            reader.Close();
            response.Close();
        }
    }
}
