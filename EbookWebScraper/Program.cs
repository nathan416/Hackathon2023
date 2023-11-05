// GU Hackathon 2023
// Project: EbookWebScraper
// Author: Nathan Flack
// Team Name: The Llammas
// Date: 11/4/2023
using System.Text.RegularExpressions;
using HtmlAgilityPack;

string Pattern = @"[(http(s)?):\/\/(www\.)?a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)";


EbookWebScraper scraper = new();
bool IsValidURL = false;
string? IndexURL;
do
{
    Console.WriteLine("Please enter URL for index page");
    IndexURL = Console.ReadLine();
    //IndexURL = "https://www.royalroad.com/fiction/75025/clover-a-litrpg-apocalypse?source=recommender&recommender_version=5";
    IndexURL = "https://www.royalroad.com/fiction/61480/amelia-the-level-zero-hero-an-op-mc-isekai-litrpg";
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
    scraper.GetIndexWebData(IndexURL);
    Console.ReadKey();

    Console.WriteLine(scraper.Title);
    Console.WriteLine(scraper.Author);
    foreach (var chapter in scraper.ChapterList)
    {
        Console.WriteLine(chapter.ChapterTitle);
        Console.WriteLine(chapter.ChapterURL);
    }
    Console.WriteLine(scraper.ChapterList.Count);
    scraper.ChapterList[63].GetChapterWebData();
    Console.WriteLine(scraper.ChapterList[63].ChapterBody[0].InnerText);
}

//Console.WriteLine("{0}", URLMatch.Value);
class EbookWebScraper
{
    private readonly string BASEURL = "https://www.royalroad.com";
    public int ChapterAmount { get; set; }
    public string? Author { get; set; }
    public string? Title { get; set; }
    public List<Chapter> ChapterList { get; set; }
    public EbookWebScraper()
    {
        ChapterList = new();
    }

    public string Epub = "";

    public void GetIndexWebData(string URL)
    {
        HtmlWeb web = new();
        HtmlDocument doc = web.Load(URL);
        var node = doc.DocumentNode.SelectSingleNode("/html/body/div[3]/div/div/div/div[1]/div/div[1]/div[2]/div/h1");
        Title = node.InnerHtml;
        node = doc.DocumentNode.SelectSingleNode("/html/body/div[3]/div/div/div/div[1]/div/div[1]/div[2]/div/h4/span[2]/a");
        Author = node.InnerHtml;
        var nodeCollection = doc.DocumentNode.SelectNodes("//*[@id=\"chapters\"]/tbody/tr/td[1]/a");
        //(Chapter Name, Chapter URL)
        foreach (HtmlNode Node in nodeCollection)
        {
            var ChapterName = Node.InnerText.Trim();
            string? ChapterURL = String.Concat(BASEURL, Node.Attributes[0].Value);
            Chapter chapter = new(ChapterName, ChapterURL);
            ChapterList.Add(chapter);
        }
        ChapterAmount = ChapterList.Count;
        //Title = doc.DocumentNode
        //.SelectNodes("//body")
        //.First()
        //.OuterHtml;
    }

    public void AddChapterToEpub
}

class Chapter
{
    public Chapter(string? chapterTitle, string? chapterURL)
    {
        ChapterTitle = chapterTitle;
        ChapterURL = chapterURL;
    }

    public string? ChapterTitle { get; set; }
    public string? ChapterURL { get; set; }
    public HtmlNodeCollection? ChapterBody { get; set; }
    public HtmlNodeCollection? ChapterPreAuthorNote { get; set; }
    public HtmlNodeCollection? ChapterPostAuthorNote { get; set; }

    public void GetChapterWebData()
    {
        HtmlWeb web = new();
        HtmlDocument doc = web.Load(ChapterURL);

        HtmlNodeCollection bodyNode = doc.DocumentNode.SelectNodes("//*[@class=\"chapter-inner chapter-content\"]/p");
        ChapterBody = bodyNode[0].ChildNodes;
        

        //TODO: Not working right now
        //HtmlNodeCollection PreAuthorNote = doc.DocumentNode.SelectNodes("");
        //if (PreAuthorNote is not null)
        //{
        //    ChapterPreAuthorNote = PreAuthorNote;
        //}

        //HtmlNodeCollection PostAuthorNote = doc.DocumentNode.SelectNodes("/html/body/div[3]/div/div/div/div/div[3]/div[2]/div[6]/div[2]");
        //if (PostAuthorNote is not null)
        //{
        //    ChapterPostAuthorNote = PostAuthorNote;
        //}
    }
}