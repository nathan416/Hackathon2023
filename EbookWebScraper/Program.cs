// GU Hackathon 2023
// Project: EbookWebScraper
// Author: Nathan Flack
// Team Name: The Llammas
// Date: 11/4/2023
using System.Text.RegularExpressions;

string Pattern = "[(http(s)?):\\/\\/(www\\.)?a-zA-Z0-9@:%._\\+~#=]{2,256}\\.[a-z]{2,6}\\b([-a-zA-Z0-9@:%_\\+.~#?&//=]*)";

EbookWebScraper scraper = new EbookWebScraper();
bool IsValidURL = false;
string? IndexURL = "";
do
{
    Console.WriteLine("Please enter URL for index page");
    IndexURL = Console.ReadLine();
    Match URLMatch = Regex.Match(IndexURL, Pattern);
    if (URLMatch.Success)
    {
        IsValidURL = true;
    }
    else
    {
        Console.WriteLine("ERROR: Invalid URL, Please enter valid URL");
    }
} while (!IsValidURL);
if (IsValidURL)
{
    scraper.GetHTML(IndexURL);
    Console.WriteLine(scraper.Result);
    Console.ReadKey();
    Console.WriteLine(scraper.Result);
}

//Console.WriteLine("{0}", URLMatch.Value);
class EbookWebScraper
{
    private string result = "";

    public EbookWebScraper()
    {
    }

    public string Result { get => result; set => result = value; }

    public async void GetHTML(string URL)
    {
        using (HttpClient client = new HttpClient())
        {
            using (HttpResponseMessage response = await client.GetAsync(URL))
            {
                using (HttpContent content = response.Content)
                {
                    Result = await content.ReadAsStringAsync();
                }
            }
        }
    }
}