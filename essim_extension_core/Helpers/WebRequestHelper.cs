using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Text;

namespace essim_extension_core.Helpers
{
    public static class WebRequestHelper
    {
        internal static string ExecuteWebRequest(string url, string method, string authorizationToken, string postData, out bool success, out HttpStatusCode? statusCode, TimeSpan? requestTimeout = null, Dictionary<string, string> headers = null)
        {
            string response = null;
            try
            {
                //Create a request using a URI
                WebRequest request = WebRequest.Create(url);
                //Set the Method property of the request to GET
                request.Method = method;
                request.Timeout = Convert.ToInt32(requestTimeout?.TotalMilliseconds ?? 30_000);

                if (!string.IsNullOrEmpty(authorizationToken))
                {
                    request.Headers.Add("x-api-key", authorizationToken);
                    request.AuthenticationLevel = AuthenticationLevel.MutualAuthRequested;
                }

                if (headers != null)
                {
                    foreach (KeyValuePair<string, string> header in headers)
                        request.Headers.Add(header.Key, header.Value);
                }

                Stream dataStream;
                if (!string.IsNullOrEmpty(postData))
                {
                    //convert postData to a byte array
                    byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                    //Set the ContentType property of the WebRequest
                    request.ContentType = "application/json";
                    //Set the ContentLength property of the WebRequest
                    request.ContentLength = byteArray.Length;
                    //Get the request stream
                    dataStream = request.GetRequestStream();
                    //Write the data to the request stream
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    //Close the Stream object
                    dataStream.Close();
                }

                //Get the response
                WebResponse webResponse = request.GetResponse();

                //Get the stream containing content returned by the server
                dataStream = webResponse.GetResponseStream();
                if (dataStream != null)
                {
                    using StreamReader reader = new StreamReader(dataStream);
                    response = reader.ReadToEnd();
                }

                success = true;
                statusCode = ((HttpWebResponse) webResponse).StatusCode;
            }
            catch (WebException e)
            {
                Stream dataStream = e.Response?.GetResponseStream();
                if (dataStream != null)
                {
                    using StreamReader reader = new StreamReader(dataStream);
                    response = reader.ReadToEnd();
                }

                success = false;
                statusCode = ((HttpWebResponse) e.Response)?.StatusCode ?? HttpStatusCode.InternalServerError;
            }
            catch (Exception)
            {
                success = false;
                statusCode = null;
            }

            return response;
        }
    }
}
