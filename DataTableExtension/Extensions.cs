using System.Data;
using System.Reflection;

namespace eventManager.DataTableExtension
{
    public static class Extensions
    {
        #region "Public"

        /// <summary>
        /// Converts datatable to list<T> dynamically
        /// </summary>
        /// <typeparam name="T">Class name</typeparam>
        /// <param name="dataTable">data table to convert</param>
        /// <returns>List<T></returns>
        public static List<T> ToListFromDataTable<T>(this DataTable dataTable) where T : new()
        {
            var dataList = new List<T>();

            //Define what attributes to be read from the class
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

            //Read Attribute Names and Types
            var objFieldNames = typeof(T).GetProperties(flags).Cast<PropertyInfo>().
                Select(item => new
                {
                    Name = item.Name,
                    Type = Nullable.GetUnderlyingType(item.PropertyType) ?? item.PropertyType
                }).ToList();

            //Read Datatable column names and types
            var dtlFieldNames = dataTable.Columns.Cast<DataColumn>().
                Select(item => new
                {
                    Name = item.ColumnName,
                    Type = item.DataType
                }).ToList();

            foreach (DataRow dataRow in dataTable.AsEnumerable().ToList())
            {
                var classObj = new T();

                foreach (var dtField in dtlFieldNames)
                {
                    PropertyInfo propertyInfos = classObj.GetType().GetProperty(dtField.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (dtField != null)
                    {
                        if (propertyInfos != null)
                        {

                            if (propertyInfos.PropertyType == typeof(DateTime))
                            {
                                propertyInfos.SetValue
                                (classObj, convertToDateTime(dataRow[dtField.Name.ToLower()]), null);
                            }
                            else if (propertyInfos.PropertyType == typeof(int))
                            {
                                propertyInfos.SetValue
                                (classObj, ConvertToInt(dataRow[dtField.Name.ToLower()]), null);
                            }
                            else if (propertyInfos.PropertyType == typeof(long))
                            {
                                propertyInfos.SetValue
                                (classObj, ConvertToLong(dataRow[dtField.Name.ToLower()]), null);
                            }
                            else if (propertyInfos.PropertyType == typeof(double))
                            {
                                propertyInfos.SetValue
                                (classObj, ConvertToDouble(dataRow[dtField.Name.ToLower()]), null);
                            }
                            else if (propertyInfos.PropertyType == typeof(decimal))
                            {
                                propertyInfos.SetValue
                                (classObj, ConvertToDecimal(dataRow[dtField.Name.ToLower()]), null);
                            }
                            else if (propertyInfos.PropertyType == typeof(float))
                            {
                                propertyInfos.SetValue
                                (classObj, ConvertToDouble(dataRow[dtField.Name.ToLower()]), null);
                            }
                            else if (propertyInfos.PropertyType == typeof(Boolean))
                            {
                                propertyInfos.SetValue
                                (classObj, ConvertToBoolean(dataRow[dtField.Name.ToLower()]), null);
                            }
                            else if (propertyInfos.PropertyType == typeof(byte[]))
                            {
                                if (!string.IsNullOrEmpty(dataRow[dtField.Name.ToLower()].ToString()))
                                {
                                    propertyInfos.SetValue(classObj, convertToByteArray(dataRow[dtField.Name.ToLower()]), null);
                                }
                            }
                            else if (propertyInfos.PropertyType == typeof(String))
                            {
                                if (dataRow[dtField.Name.ToLower()].GetType() == typeof(DateTime))
                                {
                                    propertyInfos.SetValue
                                    (classObj, ConvertToDateString(dataRow[dtField.Name.ToLower()]), null);
                                }
                                else
                                {
                                    propertyInfos.SetValue
                                    (classObj, ConvertToString(dataRow[dtField.Name.ToLower()]), null);
                                }
                            }
                        }
                    }
                }
                dataList.Add(classObj);
            }
            return dataList;
        }

        /// <summary>
        /// Converts datatable to <T> dynamically
        /// </summary>
        /// <typeparam name="T">Class name</typeparam>
        /// <param name="dataTable">data table to convert</param>
        /// <returns><T></returns>
        public static T ToFromDataTable<T>(this DataTable dataTable) where T : new()
        {
            var classObj = new T();

            //Define what attributes to be read from the class
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;

            //Read Attribute Names and Types
            var objFieldNames = typeof(T).GetProperties(flags).Cast<PropertyInfo>().
                Select(item => new
                {
                    Name = item.Name,
                    Type = Nullable.GetUnderlyingType(item.PropertyType) ?? item.PropertyType
                }).ToList();

            //Read Datatable column names and types
            var dtlFieldNames = dataTable.Columns.Cast<DataColumn>().
                Select(item => new
                {
                    Name = item.ColumnName,
                    Type = item.DataType
                }).ToList();

            foreach (DataRow dataRow in dataTable.AsEnumerable().ToList())
            {
                foreach (var dtField in dtlFieldNames)
                {
                    PropertyInfo propertyInfos = classObj.GetType().GetProperty(dtField.Name, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    //var field = objFieldNames.Find(x => x.Name == dtField.Name);

                    if (dtField != null)
                    {
                        if (propertyInfos != null)
                        {
                            if (propertyInfos.PropertyType == typeof(DateTime))
                            {
                                propertyInfos.SetValue(classObj, convertToDateTime(dataRow[dtField.Name]));
                            }
                            else if (propertyInfos.PropertyType == typeof(int))
                            {
                                propertyInfos.SetValue
                                (classObj, ConvertToInt(dataRow[dtField.Name]), null);
                            }
                            else if (propertyInfos.PropertyType == typeof(long))
                            {
                                propertyInfos.SetValue
                                (classObj, ConvertToLong(dataRow[dtField.Name]), null);
                            }
                            else if (propertyInfos.PropertyType == typeof(double))
                            {
                                propertyInfos.SetValue
                                (classObj, ConvertToDouble(dataRow[dtField.Name]), null);
                            }
                            else if (propertyInfos.PropertyType == typeof(decimal))
                            {
                                propertyInfos.SetValue
                                (classObj, ConvertToDecimal(dataRow[dtField.Name]), null);
                            }
                            else if (propertyInfos.PropertyType == typeof(byte[]))
                            {
                                if (!string.IsNullOrEmpty(dataRow[dtField.Name].ToString()))
                                {
                                    propertyInfos.SetValue(classObj, convertToByteArray(dataRow[dtField.Name]), null);
                                }
                            }
                            else if (propertyInfos.PropertyType == typeof(Boolean))
                            {
                                if (!string.IsNullOrEmpty(dataRow[dtField.Name].ToString()))
                                {
                                    propertyInfos.SetValue(classObj, ConvertToBoolean(dataRow[dtField.Name]), null);
                                }
                            }
                            else if (propertyInfos.PropertyType == typeof(String))
                            {
                                if (dataRow[dtField.Name].GetType() == typeof(DateTime))
                                {
                                    propertyInfos.SetValue
                                    (classObj, ConvertToDateString(dataRow[dtField.Name]), null);
                                }
                                else
                                {
                                    propertyInfos.SetValue
                                    (classObj, ConvertToString(dataRow[dtField.Name]), null);
                                }
                            }
                        }
                    }
                }
            }
            return classObj;
        }

        #endregion

        #region "Private"

        private static string ConvertToDateString(object date)
        {
            if (date == null)
                return string.Empty;

            return Convert.ToString((Convert.ToDateTime(date)));
        }

        private static string ConvertToString(object value)
        {
            return Convert.ToString(HelperFunctions.ReturnEmptyIfNull(value));
        }

        private static int ConvertToInt(object value)
        {
            return Convert.ToInt32(HelperFunctions.ReturnZeroIfNull(value));
        }

        private static long ConvertToLong(object value)
        {
            return Convert.ToInt64(HelperFunctions.ReturnZeroIfNull(value));
        }

        private static double ConvertToDouble(object value)
        {
            return Convert.ToDouble(HelperFunctions.ReturnZeroIfNull(value));
        }

        private static decimal ConvertToDecimal(object value)
        {
            return Convert.ToDecimal(HelperFunctions.ReturnZeroIfNull(value));
        }

        private static Boolean ConvertToBoolean(object value)
        {
            return Convert.ToBoolean(HelperFunctions.ReturnZeroIfNull(value));
        }

        private static DateTime convertToDateTime(object date)
        {
            return Convert.ToDateTime(HelperFunctions.ReturnDateTimeMinIfNull(date));
        }

        private static byte[] convertToByteArray(object value)
        {
            //string test = Convert.ToBase64String(value.ToString()); // "HsIAAA=="
            //byte[] result = Convert.FromBase64String(test);


            //string test = System.Text.Encoding.ASCII.GetString(value);
            //return System.Text.Encoding.ASCII.GetBytes(test);
            return (byte[])(HelperFunctions.ReturnNullIfDbNull(value));
        }

        #endregion
    }
}
