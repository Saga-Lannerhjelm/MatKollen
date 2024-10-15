using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MatKollen.Extensions
{
    // Code retrieved from the lecture video: http://www2.tfe.umu.se/webbutveckling/dow/asp.net%20core%20mvc/Listor%20och%20JSON%20-%20HandsOn.mp4
    public static class SessionExtensions
    {
        public static void SetObject(this ISession session, string key, object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }
        public static T GetObject<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
        }
    }
}