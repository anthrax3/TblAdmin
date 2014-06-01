﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;

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
        static string chapterHeadingPattern = @"(?=chapter [a-z]{3,})"; // positive lookahead to include the chapter headings
        
        
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
            fileString = System.IO.File.ReadAllText(filePath);

            // Split into files based on the Chapter headings
            ViewBag.Results = "";
            string chapterPartialPath = prefixPath + publisherPartialPath + bookPartialPath + @"chapter-";
            string chapterPartialPathNumbered = "";
            string chapterPathTxt = "";
            string chapterPathHtml = "";
            int i = 0;
            foreach (string s in Regex.Split(fileString, chapterHeadingPattern))
            {
                chapterPartialPathNumbered = chapterPartialPath + i.ToString("D3");
                chapterPathTxt = chapterPartialPathNumbered + @".txt";
                chapterPathHtml = chapterPartialPathNumbered + @".html";

                if (i > 0)
                {
                    System.IO.File.WriteAllText(chapterPathTxt, s);
                    System.IO.File.WriteAllText(chapterPathHtml, s);
                    ViewBag.Results = ViewBag.Results + @"==============================" + s;
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