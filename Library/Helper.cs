using System;
using System.Collections.Generic;
using Newtonsoft.Json;
/*
Date Created: 10/20/2020
Authors:
    - Kevin Bui
    - Thomas Jaszczult
Summary:
    Contains a bunch of short cut methods
*/


/*************************/
/******** IMPORTS ********/
/*************************/

// Default
using System;
using System.IO;
using System.Text;              // Used for encoding
using System.Net.Http;          // Used to perform HTTP requests
using System.Data;              // Directive to make SQL Statements
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Net.Http.Headers;

// Nuget - Default
using Microsoft.AspNetCore.Http;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// Nuget - JFROG
//using FacilitySource.Structures.Json;

/***********************/
/******** CLASS ********/
/***********************/
namespace sudoku.solver
{
    [Serializable()]
    class RunServiceException : Exception, ISerializable {
        public RunServiceException(string message) : base(message) { }
    }

    public static class Helper
    {

        /**********************************/
        /******** PUBLIC FUNCTIONS ********/
        /**********************************/
        
        public static async Task<string> RunService(string requestType, string url, string json="", Dictionary<string, string> headers=null) 
        {
            // Set up HttpClient
            HttpClient httpClient = new HttpClient();
            HttpRequestMessage request = null;

            // Make new HttpRequest with Method = Get or Post depending on requestType
            if (requestType.ToUpper() == "GET") {
                request = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri(url),
                    Content = new StringContent(json)
                };
                
            } else if (requestType.ToUpper() == "POST") {
                request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    RequestUri = new Uri(url),
                    Content = new StringContent(json)
                };
            } else {
                throw new Exception(@"Invalid requestType passed. Please use ""GET"" or ""POST""");
            }

            // Add headers if present
            if (headers != null)
            {
                foreach (KeyValuePair<string, string> header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }
            }
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            
            // Send our request with the http client
            var response = await httpClient.SendAsync(request).ConfigureAwait(false);

            // extract the response body from response
            string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            // if (IsErrorJson(responseBody)) {
            //     throw new RunServiceException(responseBody);
            // }

            return responseBody;
        }

        public static string EncodeUrl(string url, Dictionary<string, string> payload)
        {
            if (!url.EndsWith("?"))
                url += "?";
            
            List<string> pairs = new List<string>(); 
            foreach (KeyValuePair<string, string> kv in payload)
            {
                pairs.Add($"{kv.Key}={kv.Value}");
            }
            url += string.Join('&', pairs);

            return url;
        }

        /***************************************/
        /******** UNCLASSFIED FUNCTIONS ********/
        /***************************************/
        
        // Creates a Unique? Guid string 
        public static string GetGuid()
        {
            return System.Guid.NewGuid().ToString();
        }

        // Returns SHA256 hash string for a given string
        public static string GetHashString(string inputString)
        {
            using (HashAlgorithm algo = SHA256.Create())
            {
                byte[] hashBytes = algo.ComputeHash(Encoding.UTF8.GetBytes(inputString));
                
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("X2"));
                }
                return sb.ToString();
            }
        }

        /************************************/
        /******** DATETIME FUNCTIONS ********/
        /************************************/

        // Gets the current date (20200730 - Default format)
        public static string GetCurrentDate(string format="yyyyMMdd") {
            DateTime currDate = DateTime.UtcNow.Date;
            return currDate.ToString(format);
        }

        /// <summary>
        /// Returns the current UTC time in the specific format
        /// </summary>
        /// <param name="format">Datetime formatting string</param>
        /// <returns>String value for current datetime</returns>
        public static string GetCurrentDateTime(string format="yyyyMMdd-HHmmss") {
            DateTime currDateTime = DateTime.UtcNow;
            return currDateTime.ToString(format);
        }

        /// <summary>
        /// Returns the current utc datetime in the format: yyyyMMdd-HH:mm:ss.ffffff
        /// </summary>
        /// <returns>Current UTC datetime string</returns>
        public static string GetCurrentDateTimeMicroSeconds()
        {
            DateTime currDateTime = DateTime.UtcNow;
            return currDateTime.ToString("yyyyMMdd-HH:mm:ss.ffffff");
        }

        /// <summary>
        /// Converts a datetime string from the inputFormat to the outputFormat
        /// </summary>
        /// <param name="dateTime">DateTime string</param>
        /// <param name="inputFormat">Format the DateTime string is currently in</param>
        /// <param name="outputFormat">Format to convert the DateTime string to</param>
        /// <returns>Newly formatted DateTime string</returns>
        public static string ConvertDateTimeFormat(string dateTime, string inputFormat, string outputFormat) {
            try {
                DateTime parsedDate = DateTime.ParseExact(dateTime, inputFormat, CultureInfo.InvariantCulture);
                return parsedDate.ToString(outputFormat);
            } catch {
                throw new Exception($"Failed to convert string {dateTime} to new DateTime format");
            }
        } 

        /// <summary>
        /// Gets the string of the DateTime object in the specified format
        /// </summary>
        /// <param name="dateTime">DateTime object to get formatted string for</param>
        /// <param name="outputFormat">Format for the output string</param>
        /// <returns>Formatted DateTime string</returns>
        public static string ConvertDateTimeFormat(DateTime dateTime, string outputFormat) {
            return dateTime.ToString(outputFormat);
        }

        /// <summary>
        /// Converts a datetime string to the SQL datetime string
        /// </summary>
        /// <param name="dateTime">The datetime string to convert</param>
        /// <param name="format">The format the datetime string is <i>currently</i> in</param>
        /// <returns>SQL formatted datetime string</returns>
        public static string ConvertToSqlDateTime(string dateTime, string format="yyyyMMdd-HHmmss") {
            try {
                DateTime parsedDate = DateTime.ParseExact(dateTime, format, CultureInfo.InvariantCulture);
                return parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
            } catch (Exception){
                throw new Exception($"Failed to convert string {dateTime} to SQL DateTime format");
            }
        }
        
        /// <summary>
        /// Converts a datetime object to the SQL datetime string
        /// </summary>
        /// <param name="dateTime">The datetime object to convert</param>
        /// <returns>SQL formatted datetime string</returns>
        public static string ConvertToSqlDateTime(DateTime dateTime) {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
        }

        // Converts a datetime string to a commonly used internal format
        public static string ConvertToInternalDateTime(string dateTime, string format="yyyy-MM-dd HH:mm:ss") {
            try {
                DateTime parsedDate = DateTime.ParseExact(dateTime, format, CultureInfo.InvariantCulture);
                return parsedDate.ToString("yyyyMMdd-HHmmss");
            } catch (Exception){
                throw new Exception($"Failed to convert string {dateTime} to SQL DateTime format");
            }
        }
        

        /*******************************************/
        /******** CONVERT/MAPPING FUNCTIONS ********/
        /*******************************************/
    
        /* 
        Converts different data types to the specified property type
        This is literally some black hole magic.
        I had my own code for this, but it kept getting longer and longer, so I did some research
        THE-END-ALL-BE-ALL for conversion functions.

        NOTE: ONLY use if you need to.
            This is a naturally expensive function to run due to the type essentially not being defined until runtime.
        */
        public static Object ConvertValueToType(Object value, Type type){
            Type valueType = value.GetType();
            Object newValue;

            // Specifically Handling Conversions from datetime to string to preferred format
            if (value.GetType() == typeof(DateTime)){
                DateTime dt =  (DateTime)value;
                newValue = dt.ToString("yyyy-MM-dd HH:mm:ss.fff");
                return newValue;
            }
            // Returns 0 if desired type is double and the value is the DBNull type 
            else if (type == typeof(double) && value.GetType() == typeof(DBNull)) {
                return 0;
            }
            else if (type == typeof(string) && value.GetType() == typeof(DBNull)) {
                return null;
            }

            // Default uses magic conversion
            newValue = Convert.ChangeType(value, type); //????????
            return newValue;
        }

        // Converts a dictionary to a splunk log string
        public static string DictToLogString(Dictionary<string,string> logDict){
            string logString = "";
            List<string> logList = new List<string>();
            
            foreach (KeyValuePair<string, string> kv in logDict) {
                
                string edittedValue = kv.Value;
                if (edittedValue != null){
                    edittedValue = kv.Value.Replace("\"","'");
                }
                string formatted = $"{kv.Key}=\"{edittedValue}\"";
                formatted = formatted.Replace("\r", "");
                formatted = formatted.Replace("\n", "");
                formatted = formatted.Replace("\t", "");

                // We want the logs to be sorted by SessionId/LogTime
                if (kv.Key == "LogTime")
                    logList.Insert(0, formatted);
                else if (kv.Key == "SessionId")
                    logList.Insert(0, formatted);
                else
                    logList.Add(formatted);
                
            }
            logString = string.Join(" | ", logList);
            return logString;
        }

        // Map a list of dict(usually from a query) to an existing model if the properties exist
        // WARNING: Does NOT check if the keys inside the dict exists inside the model/class
        public static List<ObjType> MapDictToModel<ObjType>(List<Dictionary<string, object>> listDict, bool exactMatch=false)
        {

            List<ObjType> list = new List<ObjType>();  // Holds a list of all the object created

            Type type = typeof(ObjType);  // Gets the Type information

            ObjType tempInstance = (ObjType)Activator.CreateInstance(type);
            
            // Gets a set of properties inside the type of object you want to create
            HashSet<string> propertySet = GetPropertyNames(tempInstance);

            // Cycling through all items and mapping them to the object
            foreach(Dictionary<string, object> dict in listDict)
            {
                ObjType newInstance = MapDictToModel<ObjType>(dict, exactMatch:exactMatch, propertyNames:propertySet);
                list.Add(newInstance);
            }

            return list;
        }
        
        // Map a dictionary to model
        public static T MapDictToModel<T>(Dictionary<string, object> dict, bool exactMatch=false, HashSet<string> propertyNames = null)
        {
            // Gets the Type information
            Type type = typeof(T); 

            // Creating an instance of the object
            T tempInstance = (T)Activator.CreateInstance(type);

            // Getting the property names if needed - this was optional in the case where we wanted to created very many objects at once
            if (propertyNames == null)
            {
                propertyNames = GetPropertyNames(tempInstance);
            }

            // Cycling through each property name and setting the value if it exists in the dictionary
            foreach(string pName in propertyNames)
            {
                // Checking to see if the property exist in the class before assigning it
                if (dict.ContainsKey(pName))
                {
                    SetPropertyByName(tempInstance, pName, dict[pName], true);
                }
                // Optional Sanity Check - checks if the dictionary contains all the property names
                else if (exactMatch == true)
                {
                    throw new Exception($".{pName} does not exist in the dictionary");
                }

            }

            return tempInstance;
        }

        // Converts a dictionary to a data table
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataset"></param>
        /// <returns></returns>
        public static DataTable ConvertDictToDataTable(List<Dictionary<string, object>> dataset)
        {
            DataTable result = new DataTable();
            if (dataset.Count == 0)
                return result;

            var columnNames = dataset.SelectMany(dict=>dict.Keys).Distinct();
            result.Columns.AddRange(columnNames.Select(c=>new DataColumn(c)).ToArray());
            
            foreach (Dictionary<string,object> item in dataset)
            {
                var row = result.NewRow();
                foreach (var key in item.Keys)
                {
                    row[key] = item[key];
                }

                result.Rows.Add(row);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataset"></param>
        /// <returns></returns>
        public static List<Dictionary<string, string>> ConvertListToListOfDict(List<List<string>> dataset)
        {
            // Output dictionary
            List<Dictionary<string, string>> dataDict = new List<Dictionary<string, string>>();
            
            // Headers from first row
            List<string> headers = new List<string>();
            headers = dataset[0];

            foreach (var row in dataset.GetRange(1, dataset.Count - 1))
            {
                Dictionary<string, string> rowDict = new Dictionary<string, string>();
                
                for (int i = 0; i < row.Count; i++)
                {
                    rowDict.Add(headers[i], row[i]);
                }

                dataDict.Add(rowDict);
            }
            return dataDict;
        }

        /// <summary>
        /// Converts a list of objects to a list of dictionaries
        /// </summary>
        /// <param name="classInstances">List of objects</param>
        /// <returns>List of dictionaries containing values of class objects</returns>
        public static List<Dictionary<string, object>> ConvertObjectsToListOfDict<ObjType>(List<ObjType> classInstances)
        {
            List<Dictionary<string, object>> tempList = new List<Dictionary<string, object>>();

            foreach (Object o in classInstances)
                tempList.Add(GetPropertiesAndValues(o));
            
            return tempList;
        }

        /// <summary>
        /// Gets the union of 2 dictionaries using dict1 as the primary dictionary if duplicate keys are found
        /// If ignoreDuplicateKeys == true:
        ///     if dict1 has Key=1, Value=8 and dict2 has Key=1, Value=10, the returned dictionary will have Key=1, Value=8
        /// If ignoreDuplicateKeys == false:
        ///     if dict1.ContainsKey(1) and dict2.ContainsKey(1), an Exception will be thrown
        /// </summary>
        /// <param name="dict1">The primary dictionary to use in the merge</param>
        /// <param name="dict2">The secondary dictionary to join with the primary</param>
        /// <param name="ignoreDuplicateKeys">Whether you want to ignore duplicate keys by always using the value from dict 1 or throw errors on duplicates</param>
        /// <typeparam name="TKey">The object Type of the key in the dictionaries</typeparam>
        /// <typeparam name="TValue">The object type of the value in the dictionaries</typeparam>
        /// <returns>The union of 2 dictionaries</returns>
        public static Dictionary<TKey, TValue> GetDictUnion<TKey, TValue>(Dictionary<TKey, TValue> dict1, 
            Dictionary<TKey, TValue> dict2, 
            bool ignoreDuplicateKeys=false)
        {
            Dictionary<TKey, TValue> returnDict = new Dictionary<TKey, TValue>();

            foreach (KeyValuePair<TKey, TValue> kvp in dict1)
            {
                returnDict.Add(kvp.Key, kvp.Value);
            }

            foreach (KeyValuePair<TKey, TValue> kvp in dict2)
            {
                // If the primary dict already has this key and the function caller wanted fail on duplicates, throw an exception
                if (returnDict.ContainsKey(kvp.Key) && ignoreDuplicateKeys != true)
                {
                    // This will throw a System.ArgumentException
                    returnDict.Add(kvp.Key, kvp.Value);
                }
                // If it doesn't already exist, we have nothing to worry about
                else if (returnDict.ContainsKey(kvp.Key) == false)
                {
                    returnDict.Add(kvp.Key, kvp.Value);
                }
                // If returnDict *does* contain this key and ignoreDuplicateKeys==true, 
                // we leave it alone and don't overwrite it or anything
            }

            return returnDict;
        }

        /// <summary>
        /// Builds HTML string based on data from a List of Dictionaries
        /// </summary>
        /// <param name="dataset">Data to be used in the table. dataset[0].Keys will be used as column names</param>
        /// <param name="tagFormatting">{"tr", @"border=""1"""} replaces all &#60;tr with &#60;tr border="1"</param>
        /// <returns>HTML string</returns>
        public static string GetHtmlTableFromDict(List<Dictionary<string, object>> dataset, Dictionary<string, string> tagFormatting=null)
        {
            string htmlBody = "<table><tr>";

            List<string> columns = dataset[0].Keys.ToList();

            // Get header columns 
            foreach (string column in columns)
            {
                htmlBody += $"<th>{column}</th>";
            }
            htmlBody += "</tr>";

            foreach (Dictionary<string, object> row in dataset)
            {
                htmlBody += "<tr>";

                foreach (string column in columns)
                {
                    if (row[column] != null)
                        htmlBody += $"<td>{row[column].ToString()}</td>";
                    else
                        htmlBody += $"<td></td>";
                }
                htmlBody += "</tr>";
            }
            
            // Apply formatting to elements
            if (tagFormatting != null)
            {
                foreach (KeyValuePair<string, string> kv in tagFormatting)
                {
                    string htmlTag = kv.Key;
                    string formatting = kv.Value;
                    htmlBody = htmlBody.Replace($"<{htmlTag}", $@"<{htmlTag} {formatting}");
                }
            }
            htmlBody += "</table>";

            return htmlBody;
        }

        /***********************************/
        /******** FILE MANIPULATION ********/
        /***********************************/

        // Moves a file from src to destination
        /// <summary>
        /// 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        /// <param name="overwrite"></param>
        /// <returns></returns>
        public static bool CutPasteFile(string src, string dest, bool overwrite=false) {
            try {
                if (File.Exists(dest)) {    // Check if file already exists at destination
                    if (!overwrite) {       // make sure overwrite option is true
                        throw new Exception($"File already exists at {dest} and could not be overwritten");
                    } else {
                        File.Delete(dest);
                    }
                }

                if (File.Exists(src)) {     // Make sure the file we're moving exists
                    File.Move(src, dest);
                } else {
                    throw new FileNotFoundException($"File was not found at {src}");
                }

                if (File.Exists(src)) {     // Check and make sure the file has been deleted from src
                    throw new Exception($"Cut/Paste Failed. File was not removed from {src}");
                }
                if (!File.Exists(dest)) {   // Check for file at dest
                    throw new FileNotFoundException($"Cut/Paste Failed. File was not found at {dest}");
                }

                return true; // Return true if everything goes smoothly
            } catch (Exception e) {
                throw e;
            }
        }

        // Copies a file from src to destination
        public static bool CopyPasteFile(string src, string dest, bool overwrite=false) {
            try {
                if (File.Exists(dest)) {
                    if (!overwrite) {
                        throw new Exception($"File already exists at {dest} and cannot be overwritten"); 
                    } else {
                        File.Delete(dest);
                    }
                }

                File.Copy(src, dest);
                if (!File.Exists(dest)) {
                    throw new Exception($"Copy/Paste Failed. File was not found at {dest}");
                }
                return true;
            } catch (Exception e) {
                throw e;
            }
        }

        // Returns true if the file at the path exists, false if it doesn't
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool FileExists(string filePath) {
            return File.Exists(filePath);
        }

        // Returns the basename of the specified file
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetFileName(string filePath) {
            return Path.GetFileName(filePath);
        }

        // Reads in a .JSON File
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static Dictionary<string, object> LoadJsonFile(string filePath)
        {
            using (StreamReader r = new StreamReader(filePath))
            {
                string json = r.ReadToEnd();
                var values = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                return values;
            }
        }
        

        /****************************/
        /******** READ/WRITE ********/
        /****************************/

        // Writes a list to a csv file
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="dataset"></param>
        /// <param name="fieldTerm"></param>
        /// <param name="quoteAll"></param>
        /// <returns></returns>
        public static bool WriteCsv(string filePath, List<List<string>> dataset, string fieldTerm=",", bool quoteAll=true) {
            // Convert 2dlist to 2d array with Linq
            string[][] outputArr = dataset.Select(a => a.ToArray()).ToArray();

            using (TextWriter writer = new StreamWriter(filePath)) {
                for (int i = 0; i < outputArr.Length; i++)
                {
                    string content = "";
                    for (int k = 0; k < outputArr[i].Length; k++)
                    {
                        // enclose all of our data in quotes so commas won't mess up the output
                        content += @"""" + outputArr[i][k].ToString() + @"""";
                        if (k < outputArr[i].Length-1){
                            content += fieldTerm;
                        }
                    }
                    //trying to write data to csv
                    writer.WriteLine(content);
                }
            }

            return true;
        }

        // Reads a CSV file
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="isQuoted"></param>
        /// <param name="fieldTerm"></param>
        /// <returns></returns>
        public static List<List<string>> ReadCsv(string filePath, bool isQuoted=true, string fieldTerm=",") {
            List<List<string>> outputLst = new List<List<string>>();

            if (!FileExists(filePath)) {    // Make sure the file exists
                throw new FileNotFoundException($"Error occured while trying to read file. File {filePath} does not exist");
            }

            using (TextFieldParser csvParser = new TextFieldParser(filePath))
            {
                csvParser.SetDelimiters(new string[] { fieldTerm });    // Set field delim
                csvParser.HasFieldsEnclosedInQuotes = isQuoted;         // Account for quoted fields

                while (!csvParser.EndOfData)
                {
                    // Read current line fields, pointer moves to the next line.
                    List<string> fields = csvParser.ReadFields().ToList();
                    outputLst.Add(fields);
                }
            }
            return outputLst;
        }

        /// <summary>
        /// Serializes an object and writes the JSON to a file
        /// </summary>
        /// <param name="filePath">File path to write JSON to</param>
        /// <param name="obj">The object to serialize and export</param>
        /// <param name="formatting">Specifies formatting options for the JsonTextWriter</param>
        public static void WriteJsonFile(string filePath, object obj, Formatting formatting=Formatting.None)
        {
            using (StreamWriter file = File.CreateText(filePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, obj);
            }
        }

        /// <summary>
        /// Loads data from a JSON file into an object of the specified type and returns it
        /// </summary>
        /// <param name="filePath">Path of file to load JSON from</param>
        /// <typeparam name="T">The type of the object to deserialize to</typeparam>
        /// <returns>The deserialized object from the JSON string</returns>
        public static T LoadJsonFile<T>(string filePath)
        {
            string json = ReadFile(filePath);

            T obj = JsonConvert.DeserializeObject<T>(json);

            return obj;
        }

        /// <summary>
        /// Reads a string from a file
        /// </summary>
        /// <param name="filePath">Path of file to read</param>
        /// <returns>JSON string</returns>
        public static string ReadFile(string filePath)
        {
            using (StreamReader r = new StreamReader(filePath))
            {
                string text = r.ReadToEnd();
                return text;
            }
        }

        /************************************/
        /******** OBJECT REFLECTION/ ********/
        /************************************/

        /// <summary>
        /// Extracts the names of the properties of a class - does not currently check for types - beware
        /// </summary>
        /// <param name="classInstance"></param>
        /// <param name="lowerCase"></param>
        /// <returns></returns>
        public static HashSet<string> GetPropertyNames(Object classInstance, bool lowerCase = false)
        {
            // Variables
            HashSet<string> propertySet = new HashSet<string>();
            Type classType = classInstance.GetType();
            
            PropertyInfo[] info = classType.GetProperties();

            // Adding property names to the hashset
            foreach (PropertyInfo pi in info)
            {
                if (lowerCase == true) {
                    propertySet.Add(pi.Name.ToLower());
                } else {
                    propertySet.Add(pi.Name);
                }
            }
            
            //Return New Hashset
            return propertySet;
        }

        /// <summary>
        /// Gets a dictionary of property names and values from class properties
        /// </summary>
        /// <param name="classInstance"></param>
        /// <returns></returns>
        public static Dictionary<string, Object> GetPropertiesAndValues(Object classInstance)
        {

            Dictionary<string, Object> tempDict = new Dictionary<string, Object>();

            // Gets the object type
            Type classType = classInstance.GetType();

            // Gets the available public properties
            PropertyInfo[] info = classType.GetProperties();

            // Gets the associated values for that instance
            foreach (PropertyInfo pi in info){
                string propertyName = pi.Name;
                Object propertyValue = pi.GetValue(classInstance);
                tempDict.Add(propertyName, propertyValue);
            }

            return tempDict;
        }

        /// <summary>
        /// Returns a Dictionary of all property names and their values (string format of their values)
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="includeNull"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetJsonPropertiesAndValues(Object obj, bool includeNull=true)
        {
            Dictionary<string, string> propertyDict = new Dictionary<string, string>();

            // Turn into JSON object to get properties
            JObject json = JObject.FromObject(obj);
            foreach (JProperty property in json.Properties())
            {   
                // Ignore null values if specified
                if (includeNull == false && property.Value.ToString() == "")
                    continue;

                propertyDict.Add(property.Name, property.Value.ToString());
            }
            
            //Return Dict of properties
            return propertyDict;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="includeNull"></param>
        /// <returns></returns>
        public static Dictionary<string, object> GetBuiltInPropertiesAndValues(object obj, bool includeNull=true)
        {
            Dictionary<string, object> builtInProperties = null;
            Dictionary<string, object> allProperties = GetPropertiesAndValues(obj);
            
            // Loop through all the properties of the object
            foreach (KeyValuePair<string, object> kvp in allProperties)
            {
                // TODO: CONSIDER WHETHER NULLS SHOULD EVER BE INCLUDED
                if (kvp.Value == null && includeNull)
                {
                    builtInProperties.Add(kvp.Key, null);
                }
                else if (kvp.Value != null && !(IsArray(kvp.Value) || IsList(kvp.Value)))
                {
                    string valueTypeNamespace = kvp.Value.GetType().Namespace;

                    // If the object belongs to the System namespace, that means it is not one of our custom objects
                    // See all "built-in" types here:
                    // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/built-in-types
                    if (valueTypeNamespace == "System")
                    {
                        if (builtInProperties == null)
                            builtInProperties = new Dictionary<string, object>();

                        builtInProperties.Add(kvp.Key, kvp.Value.ToString());
                    }
                }
            }

            return builtInProperties;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static IEnumerable GetEnumerableOfObject(object obj)
        {
            return (IEnumerable)obj;
        }

        // Doesn't consider List<object> an IEnumerable. Will fix later...
        // public bool IsIEnumerable(object obj)
        // {
        //     if(obj == null) return false;

        //     return obj.GetType().IsAssignableFrom(typeof(IEnumerable<>));
        // }

        public static bool IsList(object obj)
        {
            if(obj == null) return false;

            return obj is IList &&
                obj.GetType().IsGenericType &&
                obj.GetType().GetGenericTypeDefinition().IsAssignableFrom(typeof(List<>));
        }

        public static bool IsArray(object obj)
        {
            if(obj == null) return false;

            return obj.GetType().IsArray;
        }

        public static bool IsBuiltInType(object obj)
        {
            if(obj == null) return false;
            return obj.GetType().Namespace == "System";
        }

        public static Type GetListType(List<object> list)
        {
            Type listType = null;

            for (int i = 0; i < list.Count; i++)
            {
                if (i == 0)
                {
                    listType = list[i].GetType();
                }
                else if (list[i].GetType() != listType)
                {
                    throw new Exception($"Error, List contains objects of varying types");
                }
            }
            
            return listType;
        }

        /// <summary>
        /// Maps a list of models into a dictionary using the value of propertyName as the dictionary key
        /// </summary>
        /// <param name="models">The list of models to put into a dictionary</param>
        /// <param name="propertyName">The name of the property whose value to be used as the dictionary key</param>
        /// <param name="raiseExceptionOnDuplicate">Whether or not an exception should be thrown if a duplicate is spotted</param>
        /// <typeparam name="T">The type of the value for the returned dictionary</typeparam>
        /// <returns>A dictionary mapping the propertyName value to the model instance</returns>
        public static Dictionary<string, T> MapModelsByKey<T>(List<T> models, string propertyName, bool raiseExceptionOnDuplicate=true)
        {
            Dictionary<string, T> modelDict = new Dictionary<string, T>();

            foreach (var model in models)
            {
                var key = GetPropertyValue(model, propertyName).ToString();

                if (modelDict.ContainsKey(key))
                    throw new Exception($"Error. Key {key} already exists in dataset");
                    
                modelDict.Add(key, (T)model);
            }

            return modelDict;
        }

        public static Dictionary<string, List<T>> MapModelsToListByKey<T>(List<T> models, string propertyName)
        {
            Dictionary<string, List<T>> modelDict = new Dictionary<string, List<T>>();

            foreach (T model in models)
            {
                var key = GetPropertyValue(model, propertyName).ToString();

                if (!modelDict.ContainsKey(key))
                    modelDict[key] = new List<T>();
                    
                modelDict[key].Add((T)model);
            }

            return modelDict;
        }

        // Gets the Property Value of an instantiated class/obj using a string
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static Object GetPropertyValue(object obj, string propertyName)
        {   
            // Getting the Property Value
            Type type = obj.GetType();
            PropertyInfo propertyInfo = type.GetProperty(propertyName);

            // USER should know what data type the property is!
            Object propertyValue = propertyInfo.GetValue(obj, null);

            return propertyValue;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static Object GetPropertyValueIfExists(object obj, string propertyName)
        {   
            try
            {
                // Getting the Property Value
                Type type = obj.GetType();
                PropertyInfo propertyInfo = type.GetProperty(propertyName);

                // USER should know what data type the property is!
                Object propertyValue = propertyInfo.GetValue(obj, null);

                return propertyValue;
            }
            catch (NullReferenceException e)
            {
                return null;
            }
        }

        // Gets a dictionary of the property types with the keys being the property name and the values being the Types
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ignore_empty"></param>
        /// <typeparam name="ObjType"></typeparam>
        /// <returns></returns>
        public static Dictionary<string, Type> GetPropertyTypes<ObjType>(bool ignore_empty=false){
            
            Dictionary<string, Type> typeDict = new Dictionary<string, Type>(); // Will be returned
            
            Type type = typeof(ObjType); // Getting Type information
            PropertyInfo[] propertyInfo = type.GetProperties();  // Gets array of PropertyInfo

            // cycling through each property to set in the typeDict
            foreach (PropertyInfo pi in propertyInfo){
                typeDict[pi.Name] = pi.PropertyType;
            }

            // Sanity check
            if (ignore_empty == false && typeDict.Count == 0){
                throw new Exception($"No properties exist in class of '{type.Name}'. If this is intended, set ignore_empty=true.");
            }

            return typeDict;
        }

        // Sets the property of a class programatically by using its name and providing a value
        /// <summary>
        /// 
        /// </summary>
        /// <param name="classInstance"></param>
        /// <param name="propertyName"></param>
        /// <param name="intendedValue"></param>
        /// <param name="_convert"></param>
        /// <returns></returns>
        public static int SetPropertyByName(Object classInstance, string propertyName, Object intendedValue, bool auto_convert=false){
            
            Type classType = classInstance.GetType();  //Gets the Type of the Class
            PropertyInfo propertyInfo = classType.GetProperty(propertyName); 
            
            Type propertyType = propertyInfo.PropertyType;

            // Checking to see if propererty exists
            if (propertyInfo == null)
            {
                throw new Exception($"{classType.Name}.{propertyName} is not an existing property");
            }
            
            // Auto converts to property data tpe
            if (auto_convert == true && intendedValue != null)
            {
                intendedValue = ConvertValueToType(intendedValue, propertyType);
            }

            // Sets the chose property value of the class instance
            classType.GetProperty(propertyName).SetValue(classInstance, intendedValue);  // Sets the chose property value of the class instance
            return 1;
        }

        // Gets a Dictionary of possible methods within the class
        /// <summary>
        /// 
        /// </summary>
        /// <param name="classInstance"></param>
        /// <returns></returns>
        public static Dictionary<string, int> GetClassFunctions(Object classInstance)
        {
            Dictionary<string, int> functions = new Dictionary<string, int>();
            Type objType = classInstance.GetType();

            foreach (MethodInfo method in objType.GetMethods())
            {
                method.GetParameters();
                method.GetGenericArguments().Count();
                functions.Add(method.Name, method.GetParameters().Length);
            }

            return functions;
        }

        /**************************/
        /******** ENCODING ********/
        /**************************/
        public static string ConvertBase64ToUtf8(string text)
        {   
            byte[] data = Convert.FromBase64String(text);
            string decodedString = Encoding.UTF8.GetString(data);

            return decodedString;
        }

        /**********************/
        /******** JSON ********/
        /**********************/

        // Checks if Json is Empty
        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static bool JsonIsempty(string jsonString)
        {
            if (jsonString.Contains("{}") ||   // Check for empty curly brackets
                jsonString.Contains("[]") ||       // Check for Null Array
                //jsonString.Contains("null") ||    // Check for Null Value
                jsonString.Contains("\"\""))        // Check for empty Stirng
            {
                return true;
            }  
            else {
                return false;
            }       
        }

        // Checks if Json is in a valid format
        /// <summary>
        /// 
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static bool JsonIsValid(string jsonString)
        {
            jsonString = jsonString.Trim();
            if ((jsonString.StartsWith("{") && jsonString.EndsWith("}")) || //For object
                (jsonString.StartsWith("[") && jsonString.EndsWith("]"))) //For array
            {
                try{
                    JToken obj = JToken.Parse(jsonString);
                    return true;
                }
                catch (JsonReaderException){
                    return false;
                }
                catch (Exception) {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        // returns true if input is the json string of an ErrorLog object
        // TODO: Switch to evaluate for existing error code
        // public bool IsErrorJson(string input) {
        //     try {
        //         ErrorLog logObj = JsonConvert.DeserializeObject<ErrorLog>(input);
        //         var ResponseCode = logObj.ResponseCode;

        //         // ensure that guid is neither null nor empty
        //         return (ResponseCode != 0);
        //     } catch {
        //         return false;
        //     }
        // }

        /// <summary>
        /// Serializes an objset to json
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GetJson(Object obj)
        {
            JsonSerializerSettings jsonSettings = new JsonSerializerSettings
            { 
                NullValueHandling = NullValueHandling.Ignore, 
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            
            // Converting model to json
            string jsonOutput =  JsonConvert.SerializeObject(obj, Formatting.Indented, jsonSettings);

            return jsonOutput;
        }

        /**************************/
        /****** LIST METHODS ******/
        /**************************/

        /// <summary>
        /// Splits a given list into a lists of lists of chunkSize 
        /// </summary>
        /// <param name="inputList"></param>
        /// <param name="chunkSize"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<List<T>> SplitList<T>(List<T> inputList, int chunkSize)  
        {        
            var chunkedList = new List<List<T>>(); 

            for (int i = 0; i < inputList.Count; i += chunkSize) 
            { 
                chunkedList.Add(inputList.GetRange(i, Math.Min(chunkSize, inputList.Count - i))); 
            } 

            return chunkedList; 
        } 
    
        public static List<T> Flatten2dList<T>(List<List<T>> dataset)
        {
            List<T> flat = new List<T>();

            foreach (List<T> list in dataset)
            {
                flat.AddRange(list);
            }

            return flat;
        }

        public static void PrintJson(object o, bool indent=true)
        {
            if (indent)
                Console.WriteLine(JsonConvert.SerializeObject(o, Formatting.Indented));
            else
                Console.WriteLine(JsonConvert.SerializeObject(o));
        }
    }
}