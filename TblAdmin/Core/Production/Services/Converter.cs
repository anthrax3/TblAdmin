﻿using System;
using System.Web;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;

namespace TblAdmin.Core.Production.Services
{
    public class Converter
    {
        string BlankLine = Environment.NewLine + Environment.NewLine;
        string FileString;

        string BookNameRaw { get; set; }
        string AuthorFirstName { get; set; }
        string AuthorLastName { get; set; }

        string BookFolderPath { get; set; } // main book folder
        string bookFilesFolder = "bookfiles"; // subfolder where system puts the generated book files
        string FilePath { get; set; } // path to the manually prepared book file before it gets processed by system

        string BookNameNoSpaces { get; set; }
        public string ZipFilePath { get; set; }
        public string ZipFileName { get; set; }
           
        
        string ChapterHeadingPattern { get; set; }

        int BookIdFromAdmin { get; set; }

        
        public class ChapterHeading
        {
            public string Name { get; set; }
            public string Pattern { get; set; }
        }
        public const int CHAPTER_AND_NUMBER = 1;
        public const int CHAPTER_NUMBER_AND_NAME = 2;
        public const int PART_CHAPTER_AND_NUMBER = 3;
        public const int BOOK_CHAPTER_AND_NUMBER = 4;
        public const int VOLUME_BOOK_CHAPTER_AND_NUMBER = 5;
        public static Dictionary<int, ChapterHeading> ChapterHeadings = new Dictionary<int, ChapterHeading>()
        {
            {CHAPTER_AND_NUMBER, new ChapterHeading
                {
                Name = "Chapter One / Chapter 1",
                Pattern = "Chapter [a-zA-Z0-9- ]{1,}"
                }
            },
            {CHAPTER_NUMBER_AND_NAME,  new ChapterHeading
                {
                Name = "Chapter 1: The First Day",
                Pattern = "Chapter [a-zA-Z0-9]{1,}: [a-zA-Z0-9:!\'?\"-, ]{1,}"
                }
            },
            {PART_CHAPTER_AND_NUMBER,  new ChapterHeading
                {
                Name = "Part 1: Chapter 1",
                Pattern = "Part [a-zA-Z0-9]{1,}: Chapter [a-zA-Z0-9:!\'?\"-, ]{1,}"
                }
            },
            {BOOK_CHAPTER_AND_NUMBER,  new ChapterHeading
                {
                Name = "Book IV: Old And Young - Chapter 1",
                Pattern = "Book [a-zA-Z0-9]{1,}: [a-zA-Z0-9:!\'?\"-, ]{0,} - Chapter [a-zA-Z0-9:!\'?\"-, ]{1,}"
                }
            },
            {VOLUME_BOOK_CHAPTER_AND_NUMBER,  new ChapterHeading
                {
                Name = "Volume II: Cossette, Book IV: Old And Young, Chapter 1: M. Myriel",
                Pattern = "Volume [a-zA-Z0-9]{1,}: [a-zA-Z0-9:!\'?\"-, ]{0,}, Book [a-zA-Z0-9]{1,}: [a-zA-Z0-9:!\'?\"-, ]{0,}, Chapter [a-zA-Z0-9:!\'?\"-, ]{1,}: [a-zA-Z0-9:!\'?\"-, ]{0,}"
                }
            },
        };
         
         

        public Converter(
            string bookNameRaw,
            string authorFirstNameRaw,
            string authorLastNameRaw,
            string bookFolderPath,
            string filePath,
            int bookIdFromAdmin,
            int chapterHeadingTypeID
        )
        {
            BookNameRaw = bookNameRaw;
            AuthorFirstName = authorFirstNameRaw;
            AuthorLastName = authorLastNameRaw;
            BookFolderPath = bookFolderPath;
            FilePath = filePath;
            BookIdFromAdmin = bookIdFromAdmin;
            ChapterHeadingPattern = ChapterHeadings[chapterHeadingTypeID].Pattern;

            BookNameNoSpaces = Regex.Replace(BookNameRaw, @"\s", "");
            ZipFileName = BookNameNoSpaces + "-Files.zip";
            ZipFilePath = BookFolderPath + @"\" + ZipFileName;
        
        }
            
        public bool Convert()
        {
            if (!GetBookFileAsString())
            {
                return false;
            }
            
            FileString.Trim();
            TitleCaseTheChapterHeadings();
            RemovePageHeadersAndFooters();
            RemoveBlankLinesWithinSentences();
            AddMarkersBetweenParagraphs();
            RemoveSpecialCharacters();
            ReplaceMistakenCapitalLettersInsideWords();
            AddEndOfParagraphPunctuation();
            AllowPunctuationInsideQuotesToEndAParagraph();
            RestoreBlankLineBetweenParagraphs();
            
            int numChapters = SplitIntoChapterFiles();
            GenerateTitlesXMLFile(numChapters);

            GenerateZipOfAllFiles();
            CleanupTempFiles();
            
            return true;
        }
            
       private bool GetBookFileAsString ()
       {
            bool fileExists = File.Exists(FilePath);
            if (fileExists)
            {
                FileString = File.ReadAllText(FilePath, Encoding.GetEncoding(1252));
            }

            return fileExists;
       }

       private string GenerateTitlesXMLAsString(int numChapters)
       {

           string titlesXMLAsString =
@"<?xml version=""1.0"" encoding=""iso-8859-1""?>
<library>
	<items>
		<book> 
			<title>" + BookNameRaw + @"</title>
			<author>" + AuthorLastName + ", " + AuthorFirstName + @"</author>
			<bookFolder>" + BookNameNoSpaces + @"</bookFolder>
		    <numChapters>" + numChapters.ToString() + @"</numChapters>
            <chapterNames>standard</chapterNames>
			<ra>n</ra>
		</book>
	</items>
</library>
";
           return titlesXMLAsString;
       }

       private void TitleCaseTheChapterHeadings()
       {
           FileString = Regex.Replace(
               FileString,
               ChapterHeadingPattern,
               delegate(Match match)
               {
                   string chapterHeading = match.ToString();
                   TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                   return textInfo.ToTitleCase(chapterHeading);
               },
               RegexOptions.IgnoreCase | RegexOptions.Multiline
           );
       }

       private void ReplaceMistakenCapitalLettersInsideWords()
       {
           FileString = Regex.Replace(
               FileString,
               @"\s[a-z]\w{1,}",
               delegate(Match match)
               {
                   string word = match.ToString();
                   TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                   return textInfo.ToLower(word);
               }
           );
       }

       private void RemovePageHeadersAndFooters()
       {
           string NumericPageNum = @"\d{1,}";
        
           // Remove page numbers alone on its own line (usually means its part of page header or footer)
           FileString = Regex.Replace(
               FileString,
               @"\s{0,}" + Environment.NewLine + @"\s{0,}" + NumericPageNum + @"\s{0,}" + Environment.NewLine + @"\s{0,}",
               BlankLine
           );

           // Remove title alone on its own line (usually means its part of page header or footer)
           FileString = Regex.Replace(
               FileString,
               @"\s{0,}" + Environment.NewLine + @"\s{0,}" + BookNameRaw + @"\s{0,}" + Environment.NewLine + @"\s{0,}",
               BlankLine,
               RegexOptions.IgnoreCase
           );

           // Remove author alone on its own line (usually means its part of page header or footer)
           string authorFullName = AuthorFirstName + " " + AuthorLastName;
           FileString = Regex.Replace(
               FileString,
               @"\s{0,}" + Environment.NewLine + @"\s{0,}" + authorFullName + @"\s{0,}" + Environment.NewLine + @"\s{0,}",
               BlankLine,
               RegexOptions.IgnoreCase
           );

           // Remove author last name alone on its own line (usually means its part of page header or footer)
           FileString = Regex.Replace(
               FileString,
               @"\s{0,}" + Environment.NewLine + @"\s{0,}" + AuthorLastName + @"\s{0,}" + Environment.NewLine + @"\s{0,}",
               BlankLine,
               RegexOptions.IgnoreCase
           );
       }

       private void RemoveBlankLinesWithinSentences()
       {
           // Assume it is within a sentence, if there is no ending punctuation before the blank line,
           // and the first letter in the word after the blank line is not capitalized.

           FileString = Regex.Replace(
               FileString,
               @"[a-zA-Z,;]{1,}" + @"\s{0,}" + BlankLine + @"\s{0,}" + @"[a-z]{1,}",
               delegate(Match match)
               {
                   string s = match.ToString();
                   return Regex.Replace(
                       s,
                       @"\s{0,}" + BlankLine + @"\s{0,}",
                       " "
                   );
               }
           );
       }
       private void AddMarkersBetweenParagraphs()
       {
           // Replace blank lines (and whitespace) between paragraphs with ######'s temporarily as paragraph markers.
           FileString = Regex.Replace(
               FileString,
               @"\s{0,}" + BlankLine + @"\s{0,}",
               "######"
           );
       }

       private void RemoveSpecialCharacters()
       {
           // Replace all remaining whitespace with a space to remove any special chars and line breaks
           FileString = Regex.Replace(
               FileString,
               @"\s{1,}",
               " "
           );
       }

       private void AddEndOfParagraphPunctuation()
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

       private void AllowPunctuationInsideQuotesToEndAParagraph()
       {
           string encodedRdquo = @"\u201D";
           string decodedRdquo = HttpUtility.HtmlDecode("&rdquo;");
             
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
       }
       private void RestoreBlankLineBetweenParagraphs()
       {
           // Replace temporary paragraph markers ###### with blank line
           FileString = Regex.Replace(
                FileString,
                "######",
                BlankLine
            );
       }
       private void GenerateTitlesXMLFile(int numChapters)
       {
           string titlesXMLAsString = GenerateTitlesXMLAsString(numChapters);
           string titlesXMLPath = BookFolderPath + @"/" + bookFilesFolder + @"/" + BookIdFromAdmin.ToString("D4") + ".xml";

           File.WriteAllText(titlesXMLPath, titlesXMLAsString);
       }

       private int SplitIntoChapterFiles()
       {
           // Split into files based on the Chapter headings
           int chapterNum = 0;
           string chapterHeadingLookahead = @"(?=" + ChapterHeadingPattern + @")";// positive lookahead to include the chapter headings

           string bookFilesFolderPath = Path.Combine(BookFolderPath, bookFilesFolder);
           Directory.CreateDirectory(bookFilesFolderPath);

           foreach (string s in Regex.Split(FileString, chapterHeadingLookahead, RegexOptions.Multiline | RegexOptions.IgnoreCase))
           {
               if (chapterNum > 0)
               {
                   string s_trimmed = s.Trim();
                   GenerateChapterTextFile(s_trimmed, chapterNum, bookFilesFolderPath);
                   GenerateChapterHtmlFile(s_trimmed, chapterNum, bookFilesFolderPath);
               }
               chapterNum = chapterNum + 1;
           }
           return chapterNum-1;  // return number of chapters.
       }

       private void GenerateChapterTextFile(string s_trimmed, int chapterNum, string bookFilesFolderPath)
       {
           string chapterTextFileName = "chapter-" + chapterNum.ToString("D3") + ".txt";
           string chapterPathTxt = Path.Combine(bookFilesFolderPath, chapterTextFileName);
           File.WriteAllText(chapterPathTxt, s_trimmed);
       }

       private void GenerateChapterHtmlFile(string s_trimmed, int chapterNum, string bookFilesFolderPath)
       {
           s_trimmed = HttpUtility.HtmlEncode(s_trimmed);

           // Add html p tags to paragraph separations
           s_trimmed = Regex.Replace(
               s_trimmed,
               BlankLine,
               @"</p>" + Environment.NewLine + @"<p>"
           );

           // Add opening and closing p tags for the chapter.
           s_trimmed = @"<p>" + s_trimmed + @"</p>";

           //Add html file header.
           s_trimmed =
@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
<meta http-equiv=""Content-Type"" content=""text/html; charset=utf-8"" />
<title></title>
</head>
<body>
" + s_trimmed;

           //Add html file footer
           s_trimmed = s_trimmed +
@"
</body>
</html>";
          
           string chapterHtmlFileName = "chapter-" + chapterNum.ToString("D3") + ".html";
           string chapterPathHtml = Path.Combine(bookFilesFolderPath, chapterHtmlFileName);
           File.WriteAllText(chapterPathHtml, s_trimmed);
       }

       public void GenerateZipOfAllFiles()
       {
           if (File.Exists(ZipFilePath))
           {
               File.Delete(ZipFilePath);
           }
           
           string bookFilesPath = BookFolderPath + @"\"+ bookFilesFolder;

           ZipFile.CreateFromDirectory(bookFilesPath, ZipFilePath);
       }

       public void CleanupTempFiles()
       {
           //Remove uploaded file
           if (System.IO.File.Exists(FilePath))
           {
               System.IO.File.Delete(FilePath);
           }

           //Remove all temporary generated chapter and title xml files
           string bookFilesSubFolderPath = Path.Combine(BookFolderPath, bookFilesFolder);
           string[] filePaths = Directory.GetFiles(bookFilesSubFolderPath);
           foreach (string filePath in filePaths)
               System.IO.File.Delete(filePath);
       }

    }

    
}