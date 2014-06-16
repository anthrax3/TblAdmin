﻿using System;
using System.Web;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;

namespace TblAdmin.Core.Production.Services
{
    public class Converter
    {
        string BlankLine = Environment.NewLine + Environment.NewLine;
        string NumericPageNum = @"\d{1,}";
        string FileString;
            
        public bool Convert(
            string bookNameRaw,
            string authorFirstNameRaw,
            string authorLastNameRaw,
            string bookFolderPath,
            string filePath,
            string bookIdFromAdmin,
            string chapterHeadingPattern
           )
        {
            string authorFullName = authorFirstNameRaw + " " + authorLastNameRaw;
        
            string encodedRdquo = @"\u201D";
            string decodedRdquo = HttpUtility.HtmlDecode("&rdquo;");
            
            if (!getFileAsString(filePath))
            {
                return false;
            }
            
            FileString.Trim();
            titleCaseTheChapterHeadings(chapterHeadingPattern);
            removePageHeadersAndFooters(bookNameRaw, authorFullName);
            removeBlankLinesWithinSentences();
            addMarkersBetweenParagraphs();
            removeSpecialCharacters();
            addEndOfParagraphPunctuation();

            
            

            // ---------------------------
            // Working with regular quotes
            // ---------------------------
            
            // Replace ".""." at end of paragraph (just before the paragraph marker) with just "."""
            FileString = Regex.Replace(
                FileString,
                @"\.""\.######",
                @".""######"
            );

            // Replace "?""." at end of paragraph (just before the paragraph marker) with just "."""
            FileString = Regex.Replace(
                FileString,
                @"\?""\.######",
                @"?""######"
            );

            // Replace "!""." at end of paragraph (just before the paragraph marker) with just "."""
            FileString = Regex.Replace(
                FileString,
                @"\!""\.######",
                @"!""######"
            );

            // Replace "-""." at end of paragraph (just before the paragraph marker) with just "..."""
            FileString = Regex.Replace(
                FileString,
                @"\-""\.######",
                @"...""######"
            );
            

            // -------------------
            // Working with &rdquo
            // -------------------

            // Replace ".&rdquo;." at end of paragraph (just before the paragraph marker) with just ".&rdquo;"
            FileString = Regex.Replace(
                FileString,
                @"\." + encodedRdquo + @"\.######",
                @"." + decodedRdquo + "######"
            );

            // Replace "?&rdquo;." at end of paragraph (just before the paragraph marker) with just "?&rdquo;"
            FileString = Regex.Replace(
                FileString,
                @"\?" + encodedRdquo + @"\.######",
                @"?" + decodedRdquo + @"######"
            );

            // Replace "!&rdquo;." at end of paragraph (just before the paragraph marker) with just "!&rdquo;"
            FileString = Regex.Replace(
                FileString,
                @"\!" + encodedRdquo + @"\.######",
                @"!" + decodedRdquo + @"######"
            );

            // Replace "-&rdquo;." at end of paragraph (just before the paragraph marker) with just "...&rdquo;"
            FileString = Regex.Replace(
                FileString,
                @"\-" + encodedRdquo + @"\.######",
                @"..." + decodedRdquo + "######"
            );
            
            /*
            // Replace all "&rdquo;" with itself as a test
            FileString = Regex.Replace(
                FileString,
                encodedRdquo,
                //decodedRdquo
                "$$$$$"
            );
             * */

            // Replace paragraph markers ###### with blank line
            FileString = Regex.Replace(
                 FileString,
                 "######",
                 BlankLine
             );

            // Split into files based on the Chapter headings
            int chapterNum = 0;
            string chapterHeadingLookahead = @"(?=" + chapterHeadingPattern + @")";// positive lookahead to include the chapter headings
            
            foreach (string s in Regex.Split(FileString, chapterHeadingLookahead, RegexOptions.Multiline | RegexOptions.IgnoreCase))
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
                        BlankLine,
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

            int numChapters = chapterNum - 1;
            string titlesXMLAsString = generateTitlesXMLAsString(
               bookNameRaw,
               authorLastNameRaw,
               authorFirstNameRaw,
               numChapters
            );
            string titlesXMLPath = bookFolderPath + bookIdFromAdmin + ".xml";
            System.IO.File.WriteAllText(titlesXMLPath, titlesXMLAsString);
            return true;
        }
            
       public bool getFileAsString (string filePath)
       {
            bool fileExists = System.IO.File.Exists(filePath);
            if (fileExists)
            {
                FileString = System.IO.File.ReadAllText(filePath, Encoding.GetEncoding(1252));
            }

            return fileExists;
       }

       public string generateTitlesXMLAsString(
           string bookNameRaw,
           string authorLastNameRaw,
           string authorFirstNameRaw,
           int numChapters
           )
       {
           string titlesXMLAsString = "";
           string bookNameNoSpaces = Regex.Replace(bookNameRaw, @"\s{0,}", "");

           titlesXMLAsString =
@"<?xml version=""1.0"" encoding=""iso-8859-1""?>
<library>
	<items>
		<book> 
			<title>" + bookNameRaw + @"</title>
			<author>" + authorLastNameRaw + ", " + authorFirstNameRaw + @"</author>
			<bookFolder>" + bookNameNoSpaces + @"</bookFolder>
			<numChapters>" + numChapters.ToString() + @"</numChapters>
			<ra>n</ra>
		</book>
	</items>
</library>
";
           return titlesXMLAsString;
       }

       public void titleCaseTheChapterHeadings(string chapterHeadingPattern)
       {
           FileString = Regex.Replace(
               FileString,
               chapterHeadingPattern,
               delegate(Match match)
               {
                   TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                   string v = match.ToString();
                   return textInfo.ToTitleCase(v);
               },
               RegexOptions.IgnoreCase | RegexOptions.Multiline
           );
       }

       public void removePageHeadersAndFooters(string bookNameRaw, string authorFullName)
       {
           // Remove page numbers alone on its own line (usually means its part of page header or footer)
           FileString = Regex.Replace(
               FileString,
               @"\s{0,}" + Environment.NewLine + @"\s{0,}" + NumericPageNum + @"\s{0,}" + Environment.NewLine + @"\s{0,}",
               BlankLine
           );

           // Remove title alone on its own line (usually means its part of page header or footer)
           FileString = Regex.Replace(
               FileString,
               @"\s{0,}" + Environment.NewLine + @"\s{0,}" + bookNameRaw + @"\s{0,}" + Environment.NewLine + @"\s{0,}",
               BlankLine,
               RegexOptions.IgnoreCase
           );

           // Remove author alone on its own line (usually means its part of page header or footer)
           FileString = Regex.Replace(
               FileString,
               @"\s{0,}" + Environment.NewLine + @"\s{0,}" + authorFullName + @"\s{0,}" + Environment.NewLine + @"\s{0,}",
               BlankLine,
               RegexOptions.IgnoreCase
           );
       }

       public void removeBlankLinesWithinSentences()
       {
           // Assume it is within a sentence, if there is no ending punctuation before the blank line,
           // and the first letter in the word after the blank line is not capitalized.

           FileString = Regex.Replace(
               FileString,
               @"[a-zA-Z,;]{1,}" + @"\s{0,}" + BlankLine + @"\s{0,}" + @"[a-z]{1,}",
               delegate(Match match)
               {
                   string v = match.ToString();
                   return Regex.Replace(
                       v,
                       @"\s{0,}" + BlankLine + @"\s{0,}",
                       " "
                   );
               }
           );
       }
       public void addMarkersBetweenParagraphs()
       {
           // Replace blank lines (and whitespace) between paragraphs with ######'s temporarily as paragraph markers.
           FileString = Regex.Replace(
               FileString,
               @"\s{0,}" + BlankLine + @"\s{0,}",
               "######"
           );
       }

       public void removeSpecialCharacters()
       {
           // Replace all remaining whitespace with a space to remove any special chars and line breaks
           FileString = Regex.Replace(
               FileString,
               @"\s{1,}",
               " "
           );
       }

       public void addEndOfParagraphPunctuation()
       {
           // Prefix paragraph marker by period to make all paragraphs end in period.
           FileString = Regex.Replace(
               FileString,
               "######",
               ".######"
           );

           // Replace ".." at end of paragraph (just before the paragraph marker) with just "."
           FileString = Regex.Replace(
               FileString,
               @"\.\.######",
               ".######"
           );
           // Replace "?." at end of paragraph (just before the paragraph marker) with just "?"
           FileString = Regex.Replace(
               FileString,
               @"\?\.######",
               "?######"
           );
           // Replace "!." at end of paragraph (just before the paragraph marker) with just "!"
           FileString = Regex.Replace(
               FileString,
               @"\!\.######",
               "!######"
           );
       }
 
    }

    
}