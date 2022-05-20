using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace API_NG_App
{
    public static class Connection
    {
        // ***************************************************************************
        // Fill this is with your Betfair Appkey, fraid its £300 for a non delayed one
        // ***************************************************************************
        public static string m_appKey = "";

        public static JsonRpcClient m_jsonRpcClient;
        public static string m_sessionToken;

        public static string getSessionToken(string username, string password, out string errStr)
        {
            errStr = "OK";

            string template = string.Format("https://identitysso.betfair.com/api/login?username={0}&password={1}", username, password);
            Uri temp_uri = new Uri(template);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(temp_uri);

            //set required headers
            request.Method = "POST";//set the type of request
            request.Accept = "application/json";//accept format
            request.Headers.Add(string.Format("X-Application: {0}", m_appKey));

            WebResponse response = null;

            try
            {
                response = request.GetResponse();
            }
            catch (WebException WebEx)
            {
                //some additional exception handling
                return null;
            }

            Stream temp_stream = response.GetResponseStream();

            string json_result = null;//JSON is a string, do not forget about it

            using (StreamReader reader = new StreamReader(temp_stream))
            {
                json_result = reader.ReadToEnd();//read the data from the stream to line
            }

            //object that represents the type which immediately created
            var temp_type = new { token = "", product = "", status = "", error = "" };
            var temp_obj = JsonConvert.DeserializeAnonymousType(json_result, temp_type);

            if (temp_obj.error != "")
                errStr = temp_obj.error;

            //this is sesion token from Betfair api that we need
            string session_token = temp_obj.token;

            response.Close();
            return (session_token);
        }

        public static JsonRpcClient createJsonRpcClient(string sessionToken)
        {
            m_sessionToken = sessionToken;

            string Url = "https://api.betfair.com/exchange/betting";
            m_jsonRpcClient = new JsonRpcClient(Url, m_appKey, sessionToken);
            return m_jsonRpcClient;
        }

        public static bool LogOut()
        {
            Uri temp_uri = new Uri(@"https://identitysso.betfair.com/api/logout");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(temp_uri);

            request.Method = "POST";
            request.Accept = "application/json";

            request.Headers.Add(string.Format("X-Authentication: {0}", m_sessionToken));
            request.Headers.Add(string.Format("X-Application: {0}", m_appKey));

            WebResponse response = null;

            try
            {
                response = request.GetResponse();
            }

            catch (WebException WebEx)
            {//some exception handling

                return false;
            }

            Stream temp_stream = response.GetResponseStream();

            string json_result = null;

            using (StreamReader reader = new StreamReader(temp_stream, Encoding.ASCII))
            {
                json_result = reader.ReadToEnd();//string with your response data
            }

            var temp_type = new { token = "", product = "", status = "", error = "" };
            var temp_obj = JsonConvert.DeserializeAnonymousType(json_result, temp_type);

            response.Close();

            return (temp_obj.status == "SUCCESS");
        }






    }
}
