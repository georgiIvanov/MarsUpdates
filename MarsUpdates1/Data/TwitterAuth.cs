using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;

namespace MarsUpdates.Data
{
    static class TwitterAuth
    {
        static string oauthToken = "1358988488-a8gfzmcb1BCqzsXMqXgfD8G3ZDv0qL1oWfCwE8";
        static string oauthTokenSecret = "83mLu3rHYSQO6QuHX7EL42k9vQtJr8XvtnsGYQhY8w";
        static string oauthConsumerKey = "vVtWHzV3NzeFRmHTjUcHg";
        static string oauthConsumerSecret = "JdznNk0lrGH3FDlL6YTEqjpCjHxFgXasElbMJJv2cn0";

        static string oauthVersion = "1.0";
        static string oauthSignatureMethod = "HMAC-SHA1";


        static string GetTimeStamp()
        {
            return Convert.ToInt64((DateTime.UtcNow - 
                new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds).ToString();
        }

        static string GetNonce()
        {
            return Convert.ToBase64String(new UTF8Encoding().GetBytes(DateTime.Now.Ticks.ToString()));
        }

        public static string ResourceURL
        {
            get;
            set;
        }

        static string baseGetFormat = "oauth_consumer_key={0}&oauth_nonce={1}&oauth_signature_method={2}&oauth_timestamp={3}&oauth_token={4}&oauth_version={5}"; //&status={6}

        static string CreateGetBaseString(string timeStamp, string nonce)
        {
            //return string.Format(baseFormat, oauthConsumerKey, GetNonce(), oauthSignatureMethod,
            //    GetTimeStamp(), oauthToken, oauthVersion);
            var baseStr = string.Format(baseGetFormat, oauthConsumerKey, nonce, oauthSignatureMethod,
                timeStamp, oauthToken, oauthVersion, "");
            baseStr = string.Concat("GET&", Uri.EscapeDataString(ResourceURL), "&", Uri.EscapeDataString(baseStr));
            return baseStr;
        }

        public static string CreateGetAuthHeader(string resource_url)
        {
            string timeStamp = GetTimeStamp();
            string nonce = GetNonce();

            string compositeKey = string.Concat(Uri.EscapeDataString(oauthConsumerSecret), 
                "&", Uri.EscapeDataString(oauthTokenSecret));

            string baseString = CreateGetBaseString(timeStamp, nonce);

            string oauth_signature = Sha1Encrypt(baseString, oauthConsumerSecret);

            string headerFormat = "OAuth oauth_consumer_key=\"{3}\", oauth_nonce=\"{0}\", oauth_signature=\"{5}\", oauth_signature_method=\"{1}\", oauth_timestamp=\"{2}\", oauth_token=\"{4}\", oauth_version=\"{6}\"";
            string authHeader = string.Format(headerFormat,
                        Uri.EscapeDataString(nonce),
                        Uri.EscapeDataString(oauthSignatureMethod),
                        Uri.EscapeDataString(timeStamp),
                        Uri.EscapeDataString(oauthConsumerKey),
                        Uri.EscapeDataString(oauthToken),
                        Uri.EscapeDataString(oauth_signature),
                        Uri.EscapeDataString(oauthVersion)
                );
            return authHeader;
        }

        public static string Sha1Encrypt(string baseString, string keyString)
        {
            var crypt = MacAlgorithmProvider.OpenAlgorithm("HMAC_SHA1");
            var buffer = CryptographicBuffer.ConvertStringToBinary(baseString, BinaryStringEncoding.Utf8);
            var keyBuffer = CryptographicBuffer.ConvertStringToBinary(keyString, BinaryStringEncoding.Utf8);
            var key = crypt.CreateKey(keyBuffer);

            var sigBuffer = CryptographicEngine.Sign(key, buffer);
            string signature = CryptographicBuffer.EncodeToBase64String(sigBuffer);
            return signature;
        }

    }
}
