// GU Hackathon 2023
// Project: EbookWebScraper
// Author: Nathan Flack
// Team Name: The Llammas
// Senior Undergraduate
// Date: 11/4/2023
// Inspiration: WebtoEpub Chrome Extension
// Resources:
// HtmlAgilityPack for parsing HTML and downloading chapters
// ShellProgressBar for the command line progress bar
// VIEApps Epub library for making the epub file
// Description: This is a command line application that takes a web novel from the internet and turns it into epub file on my computer.
// For now it can only work with books from the site "Royal Road" at https://www.royalroad.com
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using ShellProgressBar;

string Pattern = @"[(http(s)?):\/\/(www\.)?a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)";             //regex for URL validation

Ebook scraper = new();
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
    scraper.GetIndexWebData(IndexURL);
    scraper.GetAllChapterText();
    scraper.SaveToEpub();
}
/// <summary>
/// Class that holds all the information about a single book
/// </summary>
class Ebook
{
    private readonly string BASEURL = "https://www.royalroad.com";
    public int ChapterAmount { get; set; }
    public string? Author { get; set; }
    public string? Title { get; set; }
    public List<Chapter> ChapterList { get; set; }
    private net.vieapps.Components.Utility.Epub.Document epub = new();
    /// <summary>
    /// constructor
    /// </summary>
    public Ebook()
    {
        ChapterList = new();
    }
    public string CoverImage { get; set; }
    public net.vieapps.Components.Utility.Epub.Document Epub { get => epub; set => epub = value; }

    /// <summary>
    /// Uses the GetChapterWebData function to get the book from the site and load it into the class
    /// </summary>
    public void GetAllChapterText()
    {
        var options = new ProgressBarOptions
        {
            ProgressCharacter = '─',
            ProgressBarOnBottom = true
        };
        using (var pbar = new ProgressBar(ChapterList.Count, "Getting Chapters from the internet", options))
        {
            foreach (Chapter chapter in ChapterList)
            {
                chapter.GetChapterWebData();
                pbar.Tick();
                Thread.Sleep(200);
            }
        }
    }
    /// <summary>
    /// Formats the ebook information into epub format and saves it to a file
    /// </summary>
    public void SaveToEpub()
    {
        var Uuid = Guid.NewGuid();
        Epub.AddBookIdentifier(Uuid.ToString());
        Epub.AddLanguage("English");
        Epub.AddTitle(Title);
        Epub.AddAuthor(Author);
        string readText = File.ReadAllText("C:\\Users\\natha\\OneDrive - Gonzaga University\\Desktop\\Hackathon2023\\EbookWebScraper\\stylesheet.css");
        Epub.AddStylesheetData("style.css", readText);

        //var coverImageId = Epub.AddImageData("cover.jpg", coverImageBinaryData);
        //Epub.AddMetaItem("cover", coverImageId);

        var pageTemplate = @"<!DOCTYPE html>
	    <html xmlns=""http://www.w3.org/1999/xhtml"">
		    <head>
			    <title>{0}</title>
			    <meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8""/>
			    <link type=""text/css"" rel=""stylesheet"" href=""style.css""/>
			    <style type=""text/css"">
				    @page {
					    padding: 0;
					    margin: 0;
				    }
			    </style>
		    </head>
		    <body>
			    {1}
		    </body>
	    </html>".Trim().Replace("\t", "");

        // cover
        Epub.AddXhtmlData("page0.xhtml", pageTemplate.Replace("{0}", Title).Replace("{1}", Title));

        // chapter
        for (var index = 0; index < ChapterList.Count; index++)
        {
            var name = string.Format("page{0}.xhtml", index + 1);
            var content = ChapterList[index];
            Epub.AddXhtmlData(name, pageTemplate.Replace("{0}", content.ChapterTitle).Replace("{1}", "<h1>" + content.ChapterTitle + "</h1>"+ content.ChapterBody.InnerHtml.Replace("<br>", "").Replace("&nbsp;", "")));
            Epub.AddNavPoint(content.ChapterTitle + " - " + (index + 1).ToString(), name, index + 1);
        }

        Epub.Generate("C:\\Users\\natha\\OneDrive - Gonzaga University\\Desktop\\Hackathon2023\\EbookWebScraper\\output\\output.epub");

    }

    /// <summary>
    /// Gets information about the whole ebook such as chapter names, URLs, title, author, etc.
    /// </summary>
    /// <param name="URL">URL that points toward the index of the ebook</param>
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
    }
}

/// <summary>
/// Class for holding information about individual chapters
/// </summary>
class Chapter
{
    public Chapter(string? chapterTitle, string? chapterURL)
    {
        ChapterTitle = chapterTitle;
        ChapterURL = chapterURL;
    }

    public string? ChapterTitle { get; set; }
    public string? ChapterURL { get; set; }
    public HtmlNode? ChapterBody { get; set; }
    public HtmlNodeCollection? ChapterPreAuthorNote { get; set; }
    public HtmlNodeCollection? ChapterPostAuthorNote { get; set; }

    /// <summary>
    /// Requests the html from the chapter URL and puts the body text into the class
    /// </summary>
    public void GetChapterWebData()
    {
        HtmlWeb web = new();
        HtmlDocument doc = web.Load(ChapterURL);

        HtmlNode bodyNode = doc.DocumentNode.SelectSingleNode("//*[@class=\"chapter-inner chapter-content\"]");
        ChapterBody = bodyNode;
        

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