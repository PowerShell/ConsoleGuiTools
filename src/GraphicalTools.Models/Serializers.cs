// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Newtonsoft.Json;
using System;
using System.Text;
using System.Collections.Generic;
//TODO: swich to JSON.NET

namespace OutGridView.Models
{
    public class Serializers
    {
        private readonly static JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };
        public static string ObjectToJson<T>(T obj)
        {
            var jsonString = JsonConvert.SerializeObject(obj, jsonSerializerSettings);

            return ToBase64String(jsonString);
        }

        public static T ObjectFromJson<T>(string base64Json)
        {
            var jsonString = FromBase64String(base64Json);

            return JsonConvert.DeserializeObject<T>(jsonString, jsonSerializerSettings);
        }


        private static string FromBase64String(string base64string)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64string));
        }

        private static string ToBase64String(string str)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
        }
    }
}
