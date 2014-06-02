﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;

namespace TblAdmin.Areas.Production.Controllers
{
    public class ConversionController : Controller
    {
        static string bookName = @"MangaTouch";
        static string publisherName = @"Orca Currents";
        static string prefixPath = @"C:\Users\User\Documents\clients\Ronnie\Production\Books\";
        static string publisherPartialPath = publisherName + @"\";
        static string bookPartialPath = bookName + @"\";
        static string filePath = prefixPath + publisherPartialPath + bookPartialPath + bookName + "_FullBook_EDITED-MANUALLY.txt";
        static string destPath = prefixPath + publisherPartialPath + bookPartialPath + bookName + "_FullBook_EDITED-MANUALLY-TEST.txt";
        static string fileString;
        
        // GET: Production/Conversion
        public ActionResult Process()
        {
            bool fileExists = System.IO.File.Exists(filePath);
            if (!fileExists)
            {
                ViewBag.Results = "Could not find file with pathname: " + filePath;
                return View();
            }

            // Read file into a string
            fileString = System.IO.File.ReadAllText(filePath, Encoding.GetEncoding(1252));

            fileString = fileString.Trim();

            // Standardize the chapter heading into Camel Case followed by period.
            fileString = Regex.Replace(
                fileString, 
                @"^chapter [a-z]{3,}", 
                delegate(Match match)
                {
                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                    string v = match.ToString();
                    return textInfo.ToTitleCase(v) + ".";
                },
                RegexOptions.IgnoreCase | RegexOptions.Multiline
            );

            // BEGIN - KLUDGE
            // Replace newlines between paragraphs with ######'s temporarily
            fileString = Regex.Replace(
                fileString,
                "\n\r",
                @"######"
            );
            // Replace all remaining whitespace with a single space
            fileString = Regex.Replace(
                fileString,
                @"\s{1,}",
                @" "
            );
            // Replace ######'s with blank line
            fileString = Regex.Replace(
                fileString,
                @"\s{0,}######\s{0,}",
                "\n\r\n\r\n\r"
            );
            // END KLUDGE

            // Split into files based on the Chapter headings
            string chapterPartialPath = prefixPath + publisherPartialPath + bookPartialPath + @"chapter-";
            string chapterPartialPathNumbered = "";
            string chapterPathTxt = "";
            string chapterPathHtml = "";
            string chapterHeadingPattern = @"(?=chapter [a-z]{3,})";// positive lookahead to include the chapter headings
            
            int i = 0;
            foreach (string s in Regex.Split(fileString, chapterHeadingPattern, RegexOptions.Multiline | RegexOptions.IgnoreCase))
            {
                chapterPartialPathNumbered = chapterPartialPath + i.ToString("D3");
                chapterPathTxt = chapterPartialPathNumbered + @".txt";
                chapterPathHtml = chapterPartialPathNumbered + @".html";
                
                if (i > 0)
                {
                    string s_html = s.Trim();

                    System.IO.File.WriteAllText(chapterPathTxt, s_html, Encoding.GetEncoding(1252));

                    // Add html p tags to paragraph separations
                    s_html = Regex.Replace(
                        s_html,
                        "\n\r\n\r\n\r",
                        @"</p>" + "\n\r\n\r\n\r" + @"<p>"
                    );

                    // Add opening and closing p tags for the chapter.
                    s_html = @"<p>" + s_html + @"</p>";

                    System.IO.File.WriteAllText(chapterPathHtml, s_html, Encoding.GetEncoding(1252));
                }
                i = i + 1;
                
            }
            // Split the string on Chapters

            // Output file
            System.IO.File.WriteAllText(destPath, fileString);

            return View();
        }
            
        
    }
}