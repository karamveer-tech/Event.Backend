using System.Diagnostics.CodeAnalysis;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using System.Xml;

namespace eventManager.DataTableExtension
{
    public static class HelperFunctions
    {
        public static bool GetResolvedConnecionIpAddress(string serverNameOrUrl, out string resolvedIpAddress)
        {
            var isResolved = false;
            IPAddress resolvIp = null;
            try
            {
                if (!IPAddress.TryParse(serverNameOrUrl, out resolvIp))
                {
                    var hostEntry = Dns.GetHostEntry(serverNameOrUrl);

                    if (hostEntry != null && hostEntry.AddressList != null
                        && hostEntry.AddressList.Length > 0)
                    {
                        if (hostEntry.AddressList.Length == 1)
                        {
                            resolvIp = hostEntry.AddressList[0];
                            isResolved = true;
                        }
                        else
                        {
                            foreach (var var in hostEntry.AddressList.Where(var => var.AddressFamily == AddressFamily.InterNetwork))
                            {
                                resolvIp = var;
                                isResolved = true;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    isResolved = true;
                }
            }
            catch (Exception)
            {
                isResolved = false;
                resolvIp = null;
            }
            finally
            {
                if (resolvIp != null) resolvedIpAddress = resolvIp.ToString();
            }

            resolvedIpAddress = null;
            return isResolved;
        }

        public static string SerializeObject<T>(T source)
        {
            var serializer = new XmlSerializer(typeof(T));

            using (var sw = new StringWriter())
            using (var writer = new XmlTextWriter(sw))
            {
                serializer.Serialize(writer, source);
                return sw.ToString();
            }
        }

        public static T DeSerializeObject<T>(string xml)
        {
            using (var sr = new StringReader(xml))
            {
                var serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(sr);
            }
        }

        public static object ReturnZeroIfNull(this object value)
        {
            if (value == DBNull.Value)
                return 0;
            if (value == null)
                return 0;
            return value;
        }

        public static object ReturnEmptyIfNull(this object value)
        {
            if (value == DBNull.Value)
                return string.Empty;
            if (value == null)
                return string.Empty;
            return value;
        }

        public static object ReturnFalseIfNull(this object value)
        {
            if (value == DBNull.Value)
                return false;
            if (value == null)
                return false;
            return value;
        }

        public static object ReturnDateTimeMinIfNull(this object value)
        {
            if (value == DBNull.Value)
                return DateTime.MinValue;
            if (value == null)
                return DateTime.MinValue;
            return value;
        }

        public static object ReturnNullIfDbNull(this object value)
        {
            if (value == DBNull.Value)
                return '\0';
            if (value == null)
                return '\0';
            return value;
        }

        public static string FormatUserTelephoneNumber(this string telephoneNumber)
        {
            var result = string.Empty;

            if (!string.IsNullOrEmpty(telephoneNumber))
            {
                //result = telephoneNumber.ToLower().Trim().Trim('+').Replace("tel:", "");
                result = telephoneNumber.ToLower().Trim().Replace("tel:", "");

                if (result.Contains(";"))
                {
                    if (!result.ToLower().Contains(";ext="))
                        result = result.Split(';')[0];
                }
            }

            return result;
        }

        /// <summary>
        /// Convert DateTime to string
        /// </summary>
        /// <param name="datetTime"></param>
        /// <param name="excludeHoursAndMinutes">if true it will execlude time from datetime string. Default is false</param>
        /// <returns></returns>
        public static string ConvertDate(this DateTime datetTime, bool excludeHoursAndMinutes = false)
        {
            if (datetTime != DateTime.MinValue)
            {
                if (excludeHoursAndMinutes)
                    return datetTime.ToString("yyyy-MM-dd");
                return datetTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
            }
            return null;
        }

        [SuppressMessage("ReSharper", "PossibleLossOfFraction")]
        public static string ConvertSecondsToReadable(this int secondsParam)
        {
            var hours = Convert.ToInt32(Math.Floor((double)(secondsParam / 3600)));
            var minutes = Convert.ToInt32(Math.Floor((double)(secondsParam - (hours * 3600)) / 60));
            var seconds = secondsParam - (hours * 3600) - (minutes * 60);

            var hoursStr = hours.ToString();
            var minsStr = minutes.ToString();
            var secsStr = seconds.ToString();

            if (hours < 10)
            {
                hoursStr = "0" + hoursStr;
            }

            if (minutes < 10)
            {
                minsStr = "0" + minsStr;
            }
            if (seconds < 10)
            {
                secsStr = "0" + secsStr;
            }

            return hoursStr + ':' + minsStr + ':' + secsStr;
        }

        [SuppressMessage("ReSharper", "PossibleLossOfFraction")]
        public static string ConvertSecondsToReadable(this long secondsParam)
        {
            var hours = Convert.ToInt32(Math.Floor((double)(secondsParam / 3600)));
            var minutes = Convert.ToInt32(Math.Floor((double)(secondsParam - (hours * 3600)) / 60));
            var seconds = Convert.ToInt32(secondsParam - (hours * 3600) - (minutes * 60));

            var hoursStr = hours.ToString();
            var minsStr = minutes.ToString();
            var secsStr = seconds.ToString();

            if (hours < 10)
            {
                hoursStr = "0" + hoursStr;
            }

            if (minutes < 10)
            {
                minsStr = "0" + minsStr;
            }
            if (seconds < 10)
            {
                secsStr = "0" + secsStr;
            }

            return hoursStr + ':' + minsStr + ':' + secsStr;
        }

        public static string RemoveSpecialCharacters(string str)
        {
            // Set the timeout for regex execution
            TimeSpan timeout = TimeSpan.FromMilliseconds(100); // Adjust as needed

            try
            {
                // Use Regex.Replace with a timeout
                return Regex.Replace(str, "[^a-zA-Z0-9]", "", RegexOptions.None, timeout);
            }
            catch (RegexMatchTimeoutException)
            {
                // Handle the timeout exception
                Console.WriteLine("Regex execution exceeded the time limit.");
                return str; // You can decide what to return in case of timeout (original string, error message, etc.)
            }
        }
        public static string RemoveSpecialCharactersWithUnderScore(string str)
        {
            // Set the timeout for regex execution
            TimeSpan timeout = TimeSpan.FromMilliseconds(100); // Adjust as needed

            try
            {
                // Use Regex.Replace with a timeout
                return Regex.Replace(str, "[^a-zA-Z0-9_]", "", RegexOptions.None, timeout);
            }
            catch (RegexMatchTimeoutException)
            {
                // Handle the timeout exception
                Console.WriteLine("Regex execution exceeded the time limit.");
                return str; // You can decide what to return in case of timeout (original string, error message, etc.)
            }
        }
        public static string RemoveSpecialCharactersWithTimeout(string key)
        {
            // Set the timeout for the regex execution (e.g., 100 milliseconds)
            TimeSpan timeout = TimeSpan.FromMilliseconds(100);

            try
            {
                // Perform the regex replacement with a timeout
                return Regex.Replace(key, @"[^\w]", "", RegexOptions.None, timeout);
            }
            catch (RegexMatchTimeoutException)
            {
                // Handle the timeout exception (you can log or return the original string)
                Console.WriteLine("Regex execution exceeded the time limit.");
                return key;  // You can decide what to return in case of timeout
            }
        }
        public static bool ContainsSpecialCharacters(string input)
        {
            string pattern = "[^a-zA-Z0-9_]";  // The pattern to match
            // Set the timeout to 100 milliseconds
            TimeSpan timeout = TimeSpan.FromMilliseconds(100);

            // Regular expression to match any character that is not a letter or number
            Regex regex = new(pattern, RegexOptions.None, timeout);
            return regex.IsMatch(input);
        }
    }
}
