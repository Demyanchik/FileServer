using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Net;
using System.IO;
using WebApplication2.Controllers;
using System.Text.Json;

namespace WebApplication2.Classes
{0
    public class API_requests
    {
        static string GetResponseBody(WebResponse response)
        {
            using (Stream stream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
        public static string SendRequest(string action, string method, string query)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(HomeController.api_uri + action + "?" + query);
            request.Method = method;
            if(!method.ToUpper().Equals("GET"))
                WriteBody(request, "");

            return GetResponseBody(request.GetResponse());
        }

        public static string SendRequestBody(string action, string method, string query, string body)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(HomeController.api_uri + action + "?" + query);
            request.Method = method;
            if (!method.ToUpper().Equals("GET"))
                WriteBody(request, body);

            return GetResponseBody(request.GetResponse());
        }

        public static void WriteBody(HttpWebRequest request, HttpPostedFileBase file) 
        {
            string boundary = "----------------------------" + DateTime.Now.Ticks.ToString("x");

            request.ContentType = "multipart/form-data; boundary=" +
                            boundary;
            request.ContentLength = file.InputStream.Length;

            using (Stream rs = request.GetRequestStream()) 
            {
                var fileStream = file.InputStream;
                byte[] buffer = new byte[4096];
                int bytesRead = 0;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    rs.Write(buffer, 0, bytesRead);
                }
                fileStream.Close();
            }
        }

        static void WriteBody(HttpWebRequest request, string query)
        {
            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(query);
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = byteArray.Length;
            using (Stream dataStream = request.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }
        }
        public static UserInfo GetUserInfo(string login, string password) 
        {
            return JsonSerializer.Deserialize<UserInfo>(SendRequest("GetUserInfo", "GET", "login=" + login + "&password=" + password));
        }

        public static string SendFile(string action, string method, string query, HttpPostedFileBase file) 
        {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(HomeController.api_uri + action + "?" + query);
            request.Method = method;
            if (!method.ToUpper().Equals("GET"))
                WriteBody(request, file);

            return GetResponseBody(request.GetResponse());
        }

        public static void UploadFile(string login, HttpPostedFileBase file) 
        {
            SendFile("UploadFile", "POST", "login=" + login, file);
        }
    }
}