﻿using System;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;

/*
 * WARNING - QUICK AND DIRTY CODE TO MASSAGE SOME OF OUR PROJECT SPECIFIC TEXT FILES.
 * 
 * THIS CLASS IS UNRELATED TO THE GENERAL ADMIN PROJECT DEVELOPMENT IN THE REST OF THIS REPOSITORY.
 * I JUST NEEDED A QUICK PLACE TO TRY STUFF OUT.
 */

namespace TblAdmin.Areas.Production.Controllers
{
    public class ConversionController : Controller
    {
        /*
        static string bookNameRaw = "Manga Touch";
        static string authorFirstNameRaw = "Jacqueline";
        static string authorLastNameRaw = "Pearce";
        static string publisherName = "Orca Currents";
        static string prefixPath = @"C:\Users\User\Documents\clients\Ronnie\Production\Books\";
        static string bookIdFromAdmin = "0000";
        static string existingChapterHeading = "^chapter [a-zA-Z0-9]{1,}";
        string chapterHeadingLookahead = @"(?=chapter [a-zA-Z0-9]{1,})";// positive lookahead to include the chapter headings
        */
        /*
        static string bookNameRaw = "Power Chord";
        static string authorFirstNameRaw = "Ted";
        static string authorLastNameRaw = "Staunton";
        static string publisherName = "Orca Currents";
        static string prefixPath = @"C:\Users\User\Documents\clients\Ronnie\Production\Books\";
        static string bookIdFromAdmin = "0000";
        static string existingChapterHeading = "^chapter [a-zA-Z0-9]{1,}";
        string chapterHeadingLookahead = @"(?=chapter [a-zA-Z0-9]{1,})";// positive lookahead to include the chapter headings
        */
        /*
        static string bookNameRaw = "Anna Karenina";
        static string authorFirstNameRaw = "Leo";
        static string authorLastNameRaw = "Tolstoy";
        static string publisherName = "Gutenberg";
        static string prefixPath = @"C:\Users\User\Documents\clients\Ronnie\Production\Books\";
        static string bookIdFromAdmin = "0000";
        static string existingChapterHeading = "^part [a-zA-Z0-9]{1,}: chapter [a-zA-Z0-9]{1,}";
        string chapterHeadingLookahead = @"(?=part [a-zA-Z0-9]{1,}: chapter [a-zA-Z0-9]{1,})";// positive lookahead to include the chapter headings
        */
        /*
        static string bookNameRaw = "Peter Pan";
        static string authorFirstNameRaw = "James M.";
        static string authorLastNameRaw = "Barrie";
        static string publisherName = "Gutenberg";
        static string prefixPath = @"C:\Users\User\Documents\clients\Ronnie\Production\Books\";
        static string bookIdFromAdmin = "0000";
        static string existingChapterHeading = "^chapter [a-zA-Z0-9:!\'?\", ]{1,}";
        string chapterHeadingLookahead = @"(?=chapter [a-zA-Z0-9:!\'?"", ]{1,})";// positive lookahead to include the chapter headings
        */

        /*
        static string bookNameRaw = "Lady Windermeres Fan";
        static string authorFirstNameRaw = "David";
        static string authorLastNameRaw = "Price";
        static string publisherName = "Gutenberg";
        static string prefixPath = @"C:\Users\User\Documents\clients\Ronnie\Production\Books\";
        static string bookIdFromAdmin = "0000";
        static string existingChapterHeading = "^chapter [a-zA-Z0-9:!\'?\", ]{1,}";
        string chapterHeadingLookahead = @"(?=chapter [a-zA-Z0-9:!\'?"", ]{1,})";// positive lookahead to include the chapter headings
        */
        /*
        static string bookNameRaw = "Major Barbara";
        static string authorFirstNameRaw = "George Bernard";
        static string authorLastNameRaw = "Shaw";
        static string publisherName = "Gutenberg";
        static string prefixPath = @"C:\Users\User\Documents\clients\Ronnie\Production\Books\";
        static string bookIdFromAdmin = "0000";
        static string existingChapterHeading = "^chapter [a-zA-Z0-9:!\'?\", ]{1,}";
        string chapterHeadingLookahead = @"(?=chapter [a-zA-Z0-9:!\'?"", ]{1,})";// positive lookahead to include the chapter headings
        */

        static string bookNameRaw = "Uncle Vanya";
        static string authorFirstNameRaw = "Anton";
        static string authorLastNameRaw = "Chekhov";
        static string publisherName = "Gutenberg";
        static string prefixPath = @"C:\Users\User\Documents\clients\Ronnie\Production\Books\";
        static string bookIdFromAdmin = "0000";
        static string existingChapterHeading = "^chapter [a-zA-Z0-9:!\'?\", ]{1,}";
        string chapterHeadingLookahead = @"(?=chapter [a-zA-Z0-9:!\'?"", ]{1,})";// positive lookahead to include the chapter headings
        
        // Remove spaces in the raw book name, eg."MangaTouch";
        static string bookName = Regex.Replace(
                bookNameRaw,
                @"\s{0,}",
                ""
            );
        static string fileToWorkOn = bookName + "_FullBook_EDITED-MANUALLY.txt";
        string authorFullName = authorFirstNameRaw + " " + authorLastNameRaw;
        static string bookFolderPath = prefixPath + publisherName + @"\" + bookName + @"\";
        string filePath = bookFolderPath + fileToWorkOn;
        string fileString;
        string blankLine = Environment.NewLine + Environment.NewLine;
        string encodedRdquo = @"\u201D";
        string decodedRdquo = HttpUtility.HtmlDecode("&rdquo;");
        string numericPageNum = @"\d{1,}";

        // GET: Production/Conversion
        public ActionResult Process()
        {
            ViewBag.Results = "Success";
            
            // Verify file exists.
            bool fileExists = System.IO.File.Exists(filePath);
            if (!fileExists)
            {
                ViewBag.Results = "Could not find file with pathname: " + filePath;
                return View();
            }

            // Read file into a string and trim it.
            fileString = System.IO.File.ReadAllText(filePath, Encoding.GetEncoding(1252));
            fileString = fileString.Trim();

            // Standardize the chapter heading into Camel Case.
            fileString = Regex.Replace(
                fileString, 
                existingChapterHeading, 
                delegate(Match match)
                {
                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                    string v = match.ToString();
                    return textInfo.ToTitleCase(v);
                },
                RegexOptions.IgnoreCase | RegexOptions.Multiline
            );

            // Remove page numbers alone on its own line (usually means its part of page header or footer)
            fileString = Regex.Replace(
                fileString,
                @"\s{0,}" + Environment.NewLine + @"\s{0,}" + numericPageNum + @"\s{0,}" + Environment.NewLine + @"\s{0,}",
                blankLine
            );

            // Remove title alone on its own line (usually means its part of page header or footer)
            fileString = Regex.Replace(
                fileString,
                @"\s{0,}" + Environment.NewLine + @"\s{0,}" + bookNameRaw + @"\s{0,}" + Environment.NewLine + @"\s{0,}",
                blankLine,
                RegexOptions.IgnoreCase
            );

            // Remove author alone on its own line (usually means its part of page header or footer)
            fileString = Regex.Replace(
                fileString,
                @"\s{0,}" + Environment.NewLine + @"\s{0,}" + authorFullName + @"\s{0,}" + Environment.NewLine + @"\s{0,}",
                blankLine,
                RegexOptions.IgnoreCase
            );

            // Before putting in the paragraph markers, replace blank lines within a sentence with a space. 
            // Assume it is within a sentence, if there is no ending punctuation before the blank line,
            // and the first letter in the word after the blank line is not capitalized.
            
            fileString = Regex.Replace(
                fileString,
                @"[a-zA-Z,;]{1,}" + @"\s{0,}" + blankLine + @"\s{0,}" + @"[a-z]{1,}", 
                delegate(Match match)
                {
                    string v = match.ToString();
                    return Regex.Replace(
                        v,
                        @"\s{0,}" + blankLine + @"\s{0,}",
                        " "
                    );
                }
            );

            // Replace blank lines (and whitespace) between paragraphs with ######'s temporarily as paragraph markers.
            fileString = Regex.Replace(
                fileString,
                @"\s{0,}" + blankLine + @"\s{0,}",
                "######"
            );

            // Replace all remaining whitespace with a space to remove any special chars and line breaks
            fileString = Regex.Replace(
                fileString,
                @"\s{1,}",
                " "
            );
            
            // -----------------------------------------
            // Working with end of sentence punctuation.
            // -----------------------------------------

            // Prefix paragraph marker by period to make all paragraphs end in period.
            fileString = Regex.Replace(
                fileString,
                "######",
                ".######"
            );

            // Replace ".." at end of paragraph (just before the paragraph marker) with just "."
            fileString = Regex.Replace(
                fileString,
                @"\.\.######",
                ".######"
            );
            // Replace "?." at end of paragraph (just before the paragraph marker) with just "?"
            fileString = Regex.Replace(
                fileString,
                @"\?\.######",
                "?######"
            );
            // Replace "!." at end of paragraph (just before the paragraph marker) with just "!"
            fileString = Regex.Replace(
                fileString,
                @"\!\.######",
                "!######"
            );

            // ---------------------------
            // Working with regular quotes
            // ---------------------------
            
            // Replace ".""." at end of paragraph (just before the paragraph marker) with just "."""
            fileString = Regex.Replace(
                fileString,
                @"\.""\.######",
                @".""######"
            );

            // Replace "?""." at end of paragraph (just before the paragraph marker) with just "."""
            fileString = Regex.Replace(
                fileString,
                @"\?""\.######",
                @"?""######"
            );

            // Replace "!""." at end of paragraph (just before the paragraph marker) with just "."""
            fileString = Regex.Replace(
                fileString,
                @"\!""\.######",
                @"!""######"
            );

            // Replace "-""." at end of paragraph (just before the paragraph marker) with just "..."""
            fileString = Regex.Replace(
                fileString,
                @"\-""\.######",
                @"...""######"
            );
            

            // -------------------
            // Working with &rdquo
            // -------------------

            // Replace ".&rdquo;." at end of paragraph (just before the paragraph marker) with just ".&rdquo;"
            fileString = Regex.Replace(
                fileString,
                @"\." + encodedRdquo + @"\.######",
                @"." + decodedRdquo + "######"
            );

            // Replace "?&rdquo;." at end of paragraph (just before the paragraph marker) with just "?&rdquo;"
            fileString = Regex.Replace(
                fileString,
                @"\?" + encodedRdquo + @"\.######",
                @"?" + decodedRdquo + @"######"
            );

            // Replace "!&rdquo;." at end of paragraph (just before the paragraph marker) with just "!&rdquo;"
            fileString = Regex.Replace(
                fileString,
                @"\!" + encodedRdquo + @"\.######",
                @"!" + decodedRdquo + @"######"
            );

            // Replace "-&rdquo;." at end of paragraph (just before the paragraph marker) with just "...&rdquo;"
            fileString = Regex.Replace(
                fileString,
                @"\-" + encodedRdquo + @"\.######",
                @"..." + decodedRdquo + "######"
            );
            
            /*
            // Replace all "&rdquo;" with itself as a test
            fileString = Regex.Replace(
                fileString,
                encodedRdquo,
                //decodedRdquo
                "$$$$$"
            );
             * */

            // Replace paragraph markers ###### with blank line
            fileString = Regex.Replace(
                 fileString,
                 "######",
                 blankLine
             );

            // Split into files based on the Chapter headings
            int chapterNum = 0;
            foreach (string s in Regex.Split(fileString, chapterHeadingLookahead, RegexOptions.Multiline | RegexOptions.IgnoreCase))
            {
                string chapterPathTxt = bookFolderPath + "chapter-" + chapterNum.ToString("D3") + ".txt";
                string chapterPathHtml = bookFolderPath + "chapter-" + chapterNum.ToString("D3") + ".html";

                if (chapterNum > 0)
                {
                    string s_html = s.Trim();
                    
                    System.IO.File.WriteAllText(chapterPathTxt, s_html);

                    s_html = HttpUtility.HtmlEncode(s_html);

                    // Add html p tags to paragraph separations
                    s_html = Regex.Replace(
                        s_html,
                        blankLine,
                        @"</p>" + Environment.NewLine + @"<p>"
                    );

                    // Add opening and closing p tags for the chapter.
                    s_html = @"<p>" + s_html + @"</p>";

                    //Add html file header.
                    s_html = 
@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
<meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" />
<title></title>
</head>
<body>
"                   + s_html;

                    //Add html file footer
                    s_html = s_html + 
@"
</body>
</html>";
                    System.IO.File.WriteAllText(chapterPathHtml, s_html);
                }
                chapterNum = chapterNum + 1;
                
            }
            
            // Create the titles.xml file
            string s_xml = "";
            string titlesXMLPath = bookFolderPath + bookIdFromAdmin + ".xml";

            s_xml =
@"<?xml version=""1.0"" encoding=""iso-8859-1""?>
<library>
	<items>
		<book> 
			<title>" + bookNameRaw + @"</title>
			<author>" + authorLastNameRaw + ", " + authorFirstNameRaw + @"</author>
			<bookFolder>" + bookName + @"</bookFolder>
			<numChapters>" + (chapterNum - 1).ToString() + @"</numChapters>
			<ra>n</ra>
		</book>
	</items>
</library>
";
            System.IO.File.WriteAllText(titlesXMLPath, s_xml);

            return View();
        }
            
        
    }
}