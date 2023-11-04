// GU Hackathon 2023
// Project: EbookWebScraper
// Author: Nathan Flack
// Team Name: The Llammas
// Date: 11/4/2023
using System.Text.RegularExpressions;
using HtmlAgilityPack;

string Pattern = "[(http(s)?):\\/\\/(www\\.)?a-zA-Z0-9@:%._\\+~#=]{2,256}\\.[a-z]{2,6}\\b([-a-zA-Z0-9@:%_\\+.~#?&//=]*)";

EbookWebScraper scraper = new EbookWebScraper();
bool IsValidURL = false;
string? IndexURL;
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
    scraper.GetWebData(IndexURL);
    Console.WriteLine(scraper.Title);
}

//Console.WriteLine("{0}", URLMatch.Value);
class EbookWebScraper
{
    private string result = "";
    private string title = "";
    private string author = "";
    private int chapterAmount = 0;
    private List<string> chapterURLList;

    public EbookWebScraper()
    {
        
    }

    public void GetWebData(string URL)
    {
        HtmlWeb web = new HtmlWeb();
        HtmlDocument doc = web.Load(URL);
        Title = doc.DocumentNode.SelectSingleNode("/html/body/div[3]/div/div/div/div[1]/div/div[1]/div[2]/div/h1").Attributes["value"].Value;
    }

    public string Result { get => result; set => result = value; }
    public List<string> ChapterURLList { get => chapterURLList; set => chapterURLList = value; }
    public int ChapterAmount { get => chapterAmount; set => chapterAmount = value; }
    public string Author { get => author; set => author = value; }
    public string Title { get => title; set => title = value; }
}