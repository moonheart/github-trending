using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace github_trending
{
    class Program
    {
        private const string github_trending_api = "https://github-trending-api.now.sh/repositories";

        static void Main(string[] args)
        {
            var pushurl   = Environment.GetEnvironmentVariable("pushurl");
            var formatter = Environment.GetEnvironmentVariable("template");

            var httpClient = new HttpClient();
            var json       = httpClient.GetStringAsync(github_trending_api).Result;
            var repos      = JsonConvert.DeserializeObject<Repository[]>(json);

            var sb = new StringBuilder($"Github Trending{Environment.NewLine}");
            foreach (var repo in repos)
            {
                sb.AppendLine(repo.Format(formatter));
            }

            var msg = new
            {
                msgtype = "markdown",
                markdown = new
                {
                    content = sb.ToString()
                }
            };

            var msgStr = JsonConvert.SerializeObject(msg);

            Console.WriteLine(msgStr);

            var httpResponseMessage = httpClient.PostAsync(pushurl, new StringContent(
                                                     msgStr, Encoding.UTF8, "application/json"))
                                                .Result;

            var result = httpResponseMessage.Content.ReadAsStringAsync().Result;

            Console.WriteLine(result);
        }
    }

    public static class ObjectExtensions
    {
        public static string Format<T>(this T obj, string formatter)
        {
            foreach (var propertyInfo in typeof(T).GetProperties())
            {
                var val = propertyInfo.GetValue(obj)?.ToString();
                if (string.IsNullOrWhiteSpace(val)) continue;
                formatter = Regex.Replace(formatter, $@"\{{{propertyInfo.Name}\}}", val, RegexOptions.IgnoreCase);
            }

            return formatter;
        }
    }


    public class Repository
    {
        public string Author { get; set; }
        public string Name { get; set; }
        public Uri Url { get; set; }
        public string Description { get; set; }
        public string Language { get; set; }
        public string LanguageColor { get; set; }
        public long Stars { get; set; }
        public long Forks { get; set; }
        public long CurrentPeriodStars { get; set; }
        public BuiltBy[] BuiltBy { get; set; }
    }

    public class BuiltBy
    {
        public string Username { get; set; }
        public Uri Href { get; set; }
        public Uri Avatar { get; set; }
    }
}