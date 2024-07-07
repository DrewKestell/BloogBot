using BaseSocketServer;
using System.Net;
using System.Xml;

namespace MaNGOSDBDomain.Client
{
    public class MaNGOSCommandClient(IPAddress ipAddress) : AbstractSocketClient(8081, ipAddress)
    {
        private string soapResult;
        public string SendCommand(string command)
        {
            try
            {
                XmlDocument soapEnvelopeXml = GetXMLDoc(command);
                HttpWebRequest webRequest = CreateWebRequest($"http://127.0.0.1:7878", "ADMINISTRATOR", "ADMINISTRATOR");
                InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);

                // begin async call to web request.
                IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);

                // suspend this thread until call is complete. You might want to
                // do something usefull here like update your UI.
                asyncResult.AsyncWaitHandle.WaitOne();

                // get the response from the completed web request.
                using WebResponse webResponse = webRequest.EndGetResponse(asyncResult);
                using StreamReader rd = new(webResponse.GetResponseStream());
                soapResult = rd.ReadToEnd();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return soapResult;
        }

        private static XmlDocument GetXMLDoc(string command)
        {
            XmlDocument soapEnvelopeDocument = new();
            soapEnvelopeDocument.LoadXml(
                $"<SOAP-ENV:Envelope xmlns:SOAP-ENV=\"http://schemas.xmlsoap.org/soap/envelope/\" " +
                $"  xmlns:SOAP-ENC=\"http://schemas.xmlsoap.org/soap/encoding/\" " +
                $"  xmlns:xsi=\"http://www.w3.org/1999/XMLSchema-instance\" " +
                $"  xmlns:xsd=\"http://www.w3.org/1999/XMLSchema\" " +
                $"  xmlns:ns1=\"urn:MaNGOS\">" +
                $"  <SOAP-ENV:Body>" +
                $"      <ns1:executeCommand>" +
                $"          <command>{command}</command>" +
                $"      </ns1:executeCommand>" +
                $"  </SOAP-ENV:Body>" +
                $"</SOAP-ENV:Envelope>");
            return soapEnvelopeDocument;
        }

        private static HttpWebRequest CreateWebRequest(string url, string username, string password)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.Credentials = new NetworkCredential(username, password);
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }

        private static void InsertSoapEnvelopeIntoWebRequest(XmlDocument soapEnvelopeXml, HttpWebRequest webRequest)
        {
            using Stream stream = webRequest.GetRequestStream();
            soapEnvelopeXml.Save(stream);
        }
    }
}
