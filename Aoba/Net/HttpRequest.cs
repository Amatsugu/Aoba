using Flurl;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static LuminousVector.Aoba.Models.MediaModel;

namespace LuminousVector.Aoba.Net
{
	public static class AobaHttpRequest
	{
		public static string Upload(this Url targetUrl, string fileUri, CookieContainer cookies = null, MediaType mediaType = MediaType.Image, string method = "POST") => targetUrl.Upload(File.OpenRead(fileUri), Path.GetFileName(fileUri), cookies, mediaType, method);

		public static string Upload(this Url targetUrl, Stream file, string fileUri, CookieContainer cookies = null, MediaType mediaType = MediaType.Image, string method = "POST")
		{
			string sWebAddress = targetUrl;
			string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
			byte[] boundarybytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
			HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(sWebAddress);
			wr.ContentType = "multipart/form-data; boundary=" + boundary;
			wr.Method = "POST";
			wr.KeepAlive = true;
			wr.Credentials = CredentialCache.DefaultCredentials;
			wr.CookieContainer = cookies;
			Stream stream = wr.GetRequestStream();

			stream.Write(boundarybytes, 0, boundarybytes.Length);
			byte[] formitembytes = Encoding.UTF8.GetBytes(fileUri);
			stream.Write(formitembytes, 0, formitembytes.Length);
			stream.Write(boundarybytes, 0, boundarybytes.Length);
			string header = $"Content-Disposition: form-data; name=\"file\"; filename=\"{Path.GetFileName(fileUri)}\"\r\nContent-Type: {mediaType}\r\n\r\n";
			byte[] headerbytes = Encoding.UTF8.GetBytes(header);
			stream.Write(headerbytes, 0, headerbytes.Length);

			file.Position = 0;
			byte[] buffer = new byte[4096];
			int bytesRead = 0;
			while ((bytesRead = file.Read(buffer, 0, buffer.Length)) != 0)
				stream.Write(buffer, 0, bytesRead);
			file.Close();

			byte[] trailer = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
			stream.Write(trailer, 0, trailer.Length);
			stream.Close();

			WebResponse response = wr.GetResponse();
			Stream responseStream = response.GetResponseStream();
			StreamReader streamReader = new StreamReader(responseStream);
			string responseData = streamReader.ReadToEnd();
			responseStream.Close();
			response.Close();
			streamReader.Close();
			return responseData;

		}
	}
}
