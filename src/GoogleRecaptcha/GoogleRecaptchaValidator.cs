using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Web;
using Newtonsoft.Json;

namespace PMC.Portal.Sitefinity.Infrastructure.GoogleRecaptcha
{
    internal class GoogleRecaptchaValidator
    {
        private const string VerifyUrl = "https://www.google.com/recaptcha/api/siteverify";
        private string _remoteIp;

        public string PrivateKey { get; set; }

        public string RemoteIp
        {
            get { return this._remoteIp; }
            set
            {
                IPAddress ipAddress = IPAddress.Parse(value);
                if (ipAddress == null || ipAddress.AddressFamily != AddressFamily.InterNetwork && ipAddress.AddressFamily != AddressFamily.InterNetworkV6)
                    throw new ArgumentException("Expecting an IP address, got " + (object)ipAddress);
                this._remoteIp = ipAddress.ToString();
            }
        }

        public string Response { get; set; }

        private void CheckNotNull(object obj, string name)
        {
            if (obj == null)
                throw new ArgumentNullException(name);
        }

        public GoogleRecaptchaResponse Validate()
        {
            this.CheckNotNull((object)this.PrivateKey, "PrivateKey");
            this.CheckNotNull((object)this.RemoteIp, "RemoteIp");
            if (string.IsNullOrWhiteSpace(this.Response))
                return GoogleRecaptchaResponse.CaptchaRequired;
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("https://www.google.com/recaptcha/api/siteverify");
            httpWebRequest.ProtocolVersion = HttpVersion.Version11;
            httpWebRequest.Timeout = 30000;
            httpWebRequest.Method = "POST";
            httpWebRequest.UserAgent = "reCAPTCHA/ASP.NET";
            httpWebRequest.ContentType = "application/x-www-form-urlencoded";
            byte[] bytes = Encoding.ASCII.GetBytes(string.Format("secret={0}&response={1}&remoteip={2}", (object)HttpUtility.UrlEncode(this.PrivateKey), (object)HttpUtility.UrlEncode(this.Response), (object)HttpUtility.UrlEncode(this.RemoteIp)));
            using (Stream requestStream = httpWebRequest.GetRequestStream())
                requestStream.Write(bytes, 0, bytes.Length);
            CaptchaResponse captchaResponse = new CaptchaResponse();
            try
            {
                using (WebResponse response = httpWebRequest.GetResponse())
                {
                    using (TextReader textReader = (TextReader)new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                        captchaResponse = JsonConvert.DeserializeObject<CaptchaResponse>(textReader.ReadToEnd());
                }
            }
            catch
            {
                return GoogleRecaptchaResponse.GoogleRecaptchaNotReachable;
            }
            if (captchaResponse.Success)
                return GoogleRecaptchaResponse.Valid;
            string errorCode = string.Empty;
            if (captchaResponse.ErrorCodes != null)
                errorCode = string.Join(",", captchaResponse.ErrorCodes.ToArray());
            return new GoogleRecaptchaResponse(false, errorCode);
        }
    }
}