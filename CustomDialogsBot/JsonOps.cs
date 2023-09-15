using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.IO;

namespace SuncorCustomDialogsBot
{
    public class JsonOps
    {
        public JObject GetJsonFromFile(string path)
        {
            JObject jason_object = JObject.Parse(File.ReadAllText(path));
            return jason_object;
        }

        public List<dynamic> search_Json_by_key(string key, string path)
        {
            var jason_object = File.ReadAllText(path);
            var dirs = JObject.Parse(jason_object)
                .Descendants()
                .Where(x => x is JObject)
                .Where(x => x[key] != null)
                .Select(x => (dynamic)x[key])
                .ToList();

            return dirs;
        }

        private List<dynamic> hash = new List<dynamic>();
        public Dictionary<string, string> search_Json_by_value(dynamic value, JObject json_object, Dictionary<string, string> res)
        {
            foreach (var item in json_object)
            {
                if (item.Value.Type == JTokenType.String || item.Value.Type == JTokenType.Integer || item.Value.Type == JTokenType.Float || item.Value.Type == JTokenType.Boolean)
                {

                    // Console.WriteLine(String.Format("Current item value is {0}. Lookup value is {1}",item.Value,(JToken)value));
                    if (item.Value.Equals((JToken)value))
                    {
                        hash.Add(item.Key);
                        var temp = String.Join(".", hash.ToArray());
                        res.Add(temp, item.Key);
                        hash.RemoveAt(hash.Count - 1);
                    }
                }
                // If value type is object call recursive function
                if (item.Value.Type == JTokenType.Object)
                {
                    hash.Append(item.Key);
                    // Console.WriteLine(String.Format("item.Value is {0}, Item type is {1}", item.Value,item.Value.Type));
                    if (item.Value.Equals((JToken)value))
                    {
                        var temp = String.Join(".", hash.ToArray());
                        res.Add(temp, item.Key);
                        hash.RemoveAt(hash.Count - 1);
                    }
                    search_Json_by_value(value, (JObject)item.Value, res);
                    hash.Remove(hash.Count - 1);
                }

                if (item.Value.Type == JTokenType.Array)
                {
                    hash.Add(item.Key);
                    foreach (var ArrayElement in item.Value.Select((value, i) => new { i, value }))
                    {
                        if (ArrayElement.value.Type == JTokenType.Object || ArrayElement.value.Type == JTokenType.Array)
                        {
                            hash.Add($"{ArrayElement.i.ToString()}");
                            // hash.ForEach(Console.WriteLine);
                            // Console.WriteLine(hash.Count);
                            search_Json_by_value(value, (JObject)ArrayElement.value, res);
                            hash.RemoveAt(hash.Count - 1);
                        }
                    }
                    hash.RemoveAt(hash.Count - 1);
                }
            }
            return res;
        }
        // convert OpenAI reply to JObject for putting value back to json data
        public JObject ConvertString2Jobject(string OpenAI_Reply)
        {
            JObject OpenAI_JSON_Reply = JObject.Parse(OpenAI_Reply);
            return OpenAI_JSON_Reply;
        }

        // Get all keys with empty value.
        // format is like
        public IList<string> GetNullOrEmptyProperties(JToken jToken)
        {
            var result = new List<string>();

            if (jToken.Type == JTokenType.Object)
            {
                foreach (var jProperty in jToken.Children<JProperty>())
                {
                    if (jProperty.Value.Type == JTokenType.Null)
                    {
                        result.Add(jProperty.Path);
                    }
                    else
                    {
                        result = result.Concat(GetNullOrEmptyProperties(jProperty.Value)).ToList();
                    }

                }
            }
            else if (jToken.Type == JTokenType.Null)
            {
                result.Add(jToken.Path);
            }
            else if (jToken.Type == JTokenType.String)
            {
                if (string.IsNullOrWhiteSpace(jToken.Value<string>()))
                {
                    result.Add(jToken.Path);
                }
            }
            else if (jToken.Type == JTokenType.Array)
            {
                result = jToken.Children().Aggregate(result, (current, child) => current.Concat(GetNullOrEmptyProperties(child)).ToList());
            }

            return result;
        }

        public JObject SetNullOrEmptyValues(JObject sourceObject, JObject fallbackValueObject, IList<string> properties)
        {
            foreach (var property in properties)
            {
                var sourceProperty = sourceObject.SelectToken(property);
                var fallbackProperty = fallbackValueObject.SelectToken(property);
                if (sourceProperty == null || fallbackProperty == null)
                {
                    continue;
                }

                sourceProperty.Replace(fallbackProperty);
            }

            return sourceObject;
        }

        // for ConvertString2Jobject(string OpenAI_Reply) to set the value

        public void SetNullOrEmptyValue(JObject sourceObject, JObject fallbackValueObject, string property)
        {

            var sourceProperty = sourceObject.SelectToken(property);
            var fallbackProperty = fallbackValueObject.SelectToken(property);
            if (sourceProperty == null || fallbackProperty == null) { }
            sourceProperty.Replace(fallbackProperty);

        }
    }


    public static class BotAction
    {
        public static string ActionName { get; set; }
        public static IList<string> MissingWords { get; set; }
        public static JObject ActionHelperData { get; set; }
        public static JObject ActionData { get; set; }
        public static List<string> SupportedActions { get; set; }
    }
}

