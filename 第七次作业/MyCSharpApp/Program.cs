using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

public class WebCrawler
{
    private static readonly HttpClient httpClient = new HttpClient();
    private static readonly Regex phoneRegex = new Regex(@"\b\d{3,4}[-.\s]??\d{7,8}\b");
    private static ConcurrentDictionary<string, HashSet<string>> urlPhoneMap = new ConcurrentDictionary<string, HashSet<string>>();

    public async Task CrawlAsync(string keyword)
    {
        string searchUrl = $"https://www.bing.com/search?q={Uri.EscapeDataString(keyword)}";
        var response = await httpClient.GetStringAsync(searchUrl);
        var urls = ExtractUrls(response);

        var tasks = new List<Task>();
        foreach (var url in urls)
        {
            tasks.Add(Task.Run(() => ExtractPhonesFromUrl(url)));
        }

        await Task.WhenAll(tasks);
    }

    private List<string> ExtractUrls(string html)
    {
        var urls = new List<string>();
        // 使用 HtmlAgilityPack 解析 HTML 并提取 URL
        var document = new HtmlAgilityPack.HtmlDocument();
        document.LoadHtml(html);

        foreach (var link in document.DocumentNode.SelectNodes("//a[@href]"))
        {
            var hrefValue = link.GetAttributeValue("href", string.Empty);
            if (Uri.IsWellFormedUriString(hrefValue, UriKind.Absolute))
            {
                urls.Add(hrefValue);
            }
        }

        return urls.Distinct().ToList();
    }

    private async Task ExtractPhonesFromUrl(string url)
    {
        try
        {
            var response = await httpClient.GetStringAsync(url);
            var matches = phoneRegex.Matches(response);

            foreach (Match match in matches)
            {
                urlPhoneMap.AddOrUpdate(url, new HashSet<string> { match.Value }, (key, oldValue) =>
                {
                    oldValue.Add(match.Value);
                    return oldValue;
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching {url}: {ex.Message}");
        }
    }

    public void DisplayResults()
    {
        foreach (var entry in urlPhoneMap)
        {
            Console.WriteLine($"URL: {entry.Key}");
            foreach (var phone in entry.Value)
            {
                Console.WriteLine($" - Phone: {phone}");
            }
        }
    }
}

public class Program
{
    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var crawler = new WebCrawler();
        string keyword = "高校"; // 可以替换为用户输入的关键字
        crawler.CrawlAsync(keyword).GetAwaiter().GetResult();
        crawler.DisplayResults();
    }
}