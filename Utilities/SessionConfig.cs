using eventManager.Model;
using Microsoft.VisualBasic;
using System.Globalization;
using System.Xml;
using System.Xml.Serialization;

namespace eventManager.Utilities
{
    public class SessionConfig
    {

        private readonly IHttpContextAccessor _httpContextAccessor;
        private string AuthCookieName = "_event_sessionId";
        public SessionConfig(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public void SetAuthCookie(users cookieObj)
        {

            string strTicket = SerializeObject(cookieObj);
            string strEncryptedTicket = Symmetric3DESEncryptionUtility.Encrypt(strTicket, true);
            if (_httpContextAccessor.HttpContext != null)
            {
                _httpContextAccessor.HttpContext.Response.Cookies.Append(AuthCookieName.ToString(), strEncryptedTicket,
                new CookieOptions
                {
                    Path = "/",
                    HttpOnly = true,
                    IsEssential = true, //<- there
                    Expires = DateTime.Now.AddMinutes(60),
                    Secure = true
                });
            }
        }

        public users GetAuthCookie()
        {
            users _cookieAttributeObj = new users();
            if (_httpContextAccessor.HttpContext != null)
            {
                var cookie = _httpContextAccessor.HttpContext.Request.Cookies[AuthCookieName];
                if (cookie != null)
                {
                    string strEncryptedTicket = Symmetric3DESEncryptionUtility.Decrypt(cookie.ToString(), true);
                    _cookieAttributeObj = (users)DeSerializeObject(strEncryptedTicket, typeof(users));

                }
            }
            return _cookieAttributeObj;
        }
        public string SerializeObject(object myObject)
        {
            var stream = new MemoryStream();
            var xmldoc = new XmlDocument();
            var serializer = new XmlSerializer(myObject.GetType());
            using (stream)
            {
                serializer.Serialize(stream, myObject);
                stream.Seek(0, SeekOrigin.Begin);
                xmldoc.Load(stream);
            }

            return xmldoc.InnerXml;
        }
        public static object DeSerializeObject(object myObject, Type objectType)
        {
            // Convert the object to a string and check if it's null or empty
            var myObjectString = Convert.ToString(myObject);
            if (string.IsNullOrEmpty(myObjectString)) { return myObject; }

            var xmlSerial = new XmlSerializer(objectType);
            var xmlStream = new StringReader(myObjectString);
            var deserializedResult = xmlSerial.Deserialize(xmlStream);
            return deserializedResult ?? new object(); // Return a default object if deserialization returns null
        }
        public static string GetHostUrl(IHttpContextAccessor _httpContextAccessor)
        {
            var scheme = _httpContextAccessor.HttpContext.Request.Scheme;
            var forwardedHost = _httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-Host"].FirstOrDefault();
            var actualHost = !string.IsNullOrEmpty(forwardedHost)
                             ? forwardedHost
                             : _httpContextAccessor.HttpContext.Request.Host.Value;
            return $"{scheme}://{actualHost}";
        }
        public static string getRedirectUrl(string returnUrl, users user)
        {
            if (!String.IsNullOrEmpty(returnUrl))
            {
                return returnUrl;
            }
            else
            {
                string currentCulture = CultureInfo.CurrentCulture.Name;
                if (string.IsNullOrEmpty(currentCulture))
                    currentCulture = "en";

                if (user.role_name == "admin")
                {
                    return $"/{currentCulture}/dashboard";
                }
                else
                {
                    return $"/{currentCulture}/index";
                }
            }
        }
    }
}
