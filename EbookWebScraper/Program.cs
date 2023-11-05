// GU Hackathon 2023
// Project: EbookWebScraper
// Author: Nathan Flack
// Team Name: The Llammas
// Date: 11/4/2023
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Aspose.Words;
using Aspose.Words.Saving;
using System.Text;
using net.vieapps.Components.Utility.Epub;
using ShellProgressBar;

string Pattern = @"[(http(s)?):\/\/(www\.)?a-zA-Z0-9@:%._\+~#=]{2,256}\.[a-z]{2,6}\b([-a-zA-Z0-9@:%_\+.~#?&//=]*)";


Ebook scraper = new();
bool IsValidURL = false;
string? IndexURL;
do
{
    Console.WriteLine("Please enter URL for index page");
    IndexURL = Console.ReadLine();
    IndexURL = "https://www.royalroad.com/fiction/75025/clover-a-litrpg-apocalypse?source=recommender&recommender_version=5";
    //IndexURL = "https://www.royalroad.com/fiction/61480/amelia-the-level-zero-hero-an-op-mc-isekai-litrpg";
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
    //Console.ReadKey();

    //Console.WriteLine(scraper.Title);
    //Console.WriteLine(scraper.Author);
    //foreach (var chapter in scraper.ChapterList)
    //{
    //    Console.WriteLine(chapter.ChapterTitle);
    //    Console.WriteLine(chapter.ChapterURL);
    //}
    //Console.WriteLine(scraper.ChapterList.Count);
    //scraper.ChapterList[63].GetChapterWebData();
    //Console.WriteLine(scraper.ChapterList[63].ChapterBody[0].InnerText);
    scraper.GetIndexWebData(IndexURL);
    scraper.GetAllChapterText();
    scraper.SaveToEpub();

    //Console.WriteLine("{0}", URLMatch.Value);
}
class Ebook
{
    private readonly string BASEURL = "https://www.royalroad.com";
    public int ChapterAmount { get; set; }
    public string? Author { get; set; }
    public string? Title { get; set; }
    public List<Chapter> ChapterList { get; set; }
    private net.vieapps.Components.Utility.Epub.Document Epub = new();
 
    public Ebook()
    {
        ChapterList = new();
    }
    public string CoverImage { get; set; }
    public net.vieapps.Components.Utility.Epub.Document Epub1 { get => Epub; set => Epub = value; }

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
    public void SaveToEpub()
    {
        //HtmlSaveOptions saveOptions = new HtmlSaveOptions();
        //saveOptions.SaveFormat = SaveFormat.Epub;
        //saveOptions.Encoding = Encoding.UTF8;
        //// create a blank document
        //Document doc = new Document();
        //// the DocumentBuilder class provides members to easily add content to a document
        //DocumentBuilder builder = new DocumentBuilder(doc);
        //// write a new paragraph in the document
        //foreach (Chapter chapter in ChapterList)
        //{
        //    //builder.Writeln(chapter.ChapterTitle);
        //    builder.InsertHtml(string.Format("<h1>{0}</h1>", chapter.ChapterTitle));
        //    foreach (HtmlNode paragraph in chapter.ChapterBody)
        //    {
        //        builder.Writeln(paragraph.InnerText);
        //        builder.Writeln();
        //        builder.Writeln();
        //    }
        //}
        //saveOptions.DocumentSplitCriteria = DocumentSplitCriteria.HeadingParagraph;

        //// Specify that we want to export document properties.
        //saveOptions.ExportDocumentProperties = true;

        //doc.Save("C:\\Users\\natha\\OneDrive - Gonzaga University\\Desktop\\Hackathon2023\\EbookWebScraper\\output\\" + "output.epub", saveOptions);
        var Uuid = Guid.NewGuid();
        Epub1.AddBookIdentifier(Uuid.ToString());
        Epub1.AddLanguage("English");
        Epub1.AddTitle(Title);
        Epub1.AddAuthor(Author);
        string readText = File.ReadAllText("C:\\Users\\natha\\OneDrive - Gonzaga University\\Desktop\\Hackathon2023\\EbookWebScraper\\stylesheet.css");
        Epub1.AddStylesheetData("style.css", readText);

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
        Epub1.AddXhtmlData("page0.xhtml", pageTemplate.Replace("{0}", Title).Replace("{1}", "Hello World"));

        // chapter
        for (var index = 0; index < ChapterList.Count; index++)
        {
            var name = string.Format("page{0}.xhtml", index + 1);
            var content = ChapterList[index];

            

            Epub1.AddXhtmlData(name, pageTemplate.Replace("{0}", content.ChapterTitle).Replace("{1}", content.ChapterBody.InnerHtml));
            Epub1.AddNavPoint(content.ChapterTitle + " - " + (index + 1).ToString(), name, index + 1);
        }

        Epub1.Generate("C:\\Users\\natha\\OneDrive - Gonzaga University\\Desktop\\Hackathon2023\\EbookWebScraper\\output\\output.epub");

    }

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
    public HtmlNode? ChapterBody { get; set; }
    public HtmlNodeCollection? ChapterPreAuthorNote { get; set; }
    public HtmlNodeCollection? ChapterPostAuthorNote { get; set; }

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