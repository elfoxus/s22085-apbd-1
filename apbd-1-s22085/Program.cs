using System.Text.RegularExpressions;

namespace EmailFinderApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            ThrowIfNoArgs(args);
            var url = args[0];
            ThrowIfArgumentIsNotValidUri(url);
            
            var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);
            ThrowIfNotValidStatusCode(response);
            await PrintEmailsOrThrow(response);
        }

        private static async Task PrintEmailsOrThrow(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            ThrowIfContentIsNull(content);
            Regex regex =
                new Regex(
                    @"(([\w-]+\.)+[\w-]+|([a-zA-Z]{1}|[\w-]{2,}))@"
                    + @"((([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\."
                    + @"([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])\.([0-1]?[0-9]{1,2}|25[0-5]|2[0-4][0-9])){1}|"
                    + @"([a-zA-Z]+[\w-]+\.)+[a-zA-Z]{2,4})",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase);

            var emails = regex.Matches(content)
                .OfType<Match>()
                .Select(m => m.Groups[0].Value)
                .Distinct()
                .ToList();

            if (emails.Count == 0)
            {
                throw new Exception("Nie znaleziono adresów email");
            }
            
            foreach (var email in emails)
            {
                Console.WriteLine(email);
            }
        }

        private static void ThrowIfContentIsNull(string content)
        {
            if (content is null)
            {
                throw new Exception("Nie znaleziono adresów email");
            }
        }

        private static void ThrowIfNotValidStatusCode(HttpResponseMessage response)
        {
            if (getStatusCodeAsInt(response) is < 200 or > 209)
            {
                throw new Exception("Błąd pobierania ze strony");
            }
        }

        private static void ThrowIfArgumentIsNotValidUri(string url)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
            {
                throw new ArgumentException("");
            }
        }

        private static void ThrowIfNoArgs(string[] args)
        {
            if (args.Length < 1)
            {
                throw new ArgumentNullException(nameof(args));
            }
        }

        private static int getStatusCodeAsInt(HttpResponseMessage response)
        {
            return ((int)response.StatusCode);
        }
    }
    
}

