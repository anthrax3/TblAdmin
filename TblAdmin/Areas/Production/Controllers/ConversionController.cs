﻿using System;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Text;
using System.Collections.Generic;
using TblAdmin.Core.Production.Services;
using TblAdmin.Areas.Production.ViewModels.Conversion;
using TblAdmin.Areas.Base.Controllers;
using System.IO;

/*
 * THIS CLASS IS UNRELATED TO THE GENERAL ADMIN PROJECT DEVELOPMENT IN THE REST OF THIS REPOSITORY.
 * I JUST NEEDED A QUICK PLACE TO TRY STUFF OUT.
 */

namespace TblAdmin.Areas.Production.Controllers
{
    public class ConversionController : BaseController
    {
        
        public ActionResult Convert()
        {
            // upload the file
            ConvertInputModel cim = new ConvertInputModel();
            return View(cim);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Convert(ConvertInputModel cim)
        {
            string uploadedFilePath = "";
            string tempBookFolderPath = "";
            string fileName = "";

            if (ModelState.IsValid)
            {
                if (cim.BookFile.ContentLength > 0) {
                    fileName = Path.GetFileName(cim.BookFile.FileName);
                    uploadedFilePath = Path.Combine(Server.MapPath("~/Uploads"), fileName);
                    cim.BookFile.SaveAs(uploadedFilePath);
                }

                tempBookFolderPath = Server.MapPath("~/Temp");
                string chapterHeadingPattern = Converter.ChapterHeadings[cim.ChapterHeadingTypeID].Pattern;
                Converter myConverter = new Converter(
                    cim.BookNameRaw,
                    cim.AuthorFirstNameRaw,
                    cim.AuthorLastNameRaw,
                    //cim.BookFolderPath,
                    tempBookFolderPath,
                    uploadedFilePath,
                    cim.BookIdFromAdmin,
                    chapterHeadingPattern
                );
                Boolean result = myConverter.Convert();

                if (result)
                {
                    ViewBag.Results = "Success ! Your files are being sent to you now. ";

                    string bookNameNoSpaces = Regex.Replace(cim.BookNameRaw, @"\s", "");
                    string zipFilePath = tempBookFolderPath + @"\" + bookNameNoSpaces + "-Files.zip";
                    string servedFileName = bookNameNoSpaces + "-Files.zip";
                    serveZipFile(zipFilePath, servedFileName);
                }
                else
                {
                    ViewBag.Results = "Could not find file with pathname: " + cim.FilePath;
                }
            }
            // remove the uploaded file the temporary generated book files and the zip file.
            return View(cim);
        }
    }
}