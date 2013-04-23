using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace StreamingApp
{
    public class Global : System.Web.HttpApplication
    {
        public static readonly string ERROR_ID =  "Error";
        public static readonly string COOKIE_ID = "StreamingCookie";


        public static CookieData GetCookieData(HttpRequest req)
        {
            CookieData data = new CookieData();
            HttpCookie cookie = req.Cookies[COOKIE_ID];

            if (cookie != null)
            {
                CookieData data2 = DeserializeCookie(cookie.Value);
                if (data2 != null) data = data2;
            }

            return data;
        }

        public static void SetCookieData(HttpResponse res, CookieData data)
        {
            HttpCookie cookie = new HttpCookie(COOKIE_ID);
            cookie.Value = SerializeCookie(data);
            cookie.Expires = DateTime.MaxValue;

            res.Cookies.Set(cookie);
        }

        private static CookieData DeserializeCookie(string str)
        {
            CookieData data = null;

            try
            {
                data = (CookieData)DeserializeBase64(str);
            }
            catch { }

            return data;

        }

        private static string SerializeCookie(CookieData data)
        {
            string retVal = "";

            try
            {
                retVal = SerializeBase64(data);
            }
            catch { }

            return retVal;
        }

        public static object DeserializeBase64(string s)
        {
            // We need to know the exact length of the string - Base64 can sometimes pad us by a byte or two
            int p = s.IndexOf(':');
            int length = Convert.ToInt32(s.Substring(0, p));

            // Extract data from the base 64 string!
            byte[] memorydata = Convert.FromBase64String(s.Substring(p + 1));
            MemoryStream rs = new MemoryStream(memorydata, 0, length);
            BinaryFormatter sf = new BinaryFormatter();
            object o = sf.Deserialize(rs);
            return o;
        }

        public static string SerializeBase64(object o)
        {
            // Serialize to a base 64 string
            byte[] bytes;
            long length = 0;
            MemoryStream ws = new MemoryStream();
            BinaryFormatter sf = new BinaryFormatter();
            sf.Serialize(ws, o);
            length = ws.Length;
            bytes = ws.GetBuffer();
            string encodedData = bytes.Length + ":" + Convert.ToBase64String(bytes, 0, bytes.Length, Base64FormattingOptions.None);
            return encodedData;
        }

        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup

        }

        void Application_End(object sender, EventArgs e)
        {
            //  Code that runs on application shutdown

        }

        void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            if (ex != null && ex.GetBaseException() != null)
                Application[ERROR_ID] = ex.GetBaseException();

        
        }

        void Session_Start(object sender, EventArgs e)
        {
        }

        void Session_End(object sender, EventArgs e)
        {
            // Code that runs when a session ends. 
            // Note: The Session_End event is raised only when the sessionstate mode
            // is set to InProc in the Web.config file. If session mode is set to StateServer 
            // or SQLServer, the event is not raised.

        }

    }
}
