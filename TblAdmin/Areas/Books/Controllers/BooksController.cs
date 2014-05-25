﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using TblAdmin.Areas.Books.Models;
using TblAdmin.Areas.Books.ViewModels.Books;
using TblAdmin.Areas.Base.Controllers;
using TblAdmin.Areas.Base.ViewModels;
using TblAdmin.DAL;
using PagedList;

namespace TblAdmin.Areas.Books.Controllers
{
    public class BooksController : BaseController
    {
        private TblAdminContext db;

        public BooksController(TblAdminContext context)
        {
            db = context;
        }

        // GET: Books/Books
        public ActionResult Index(SearchSortPageViewModel vm)
        {

            if (vm == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            } 
            
            IQueryable<Book> books = from b in db.Books select b; //db.Books.Include(b => b.Publisher); // the "Include" messes up mocking DbSet with mock.
            if (!String.IsNullOrEmpty(vm.SearchString))
            {
                books = books.Where(s => s.Name.ToUpper().Contains(vm.SearchString.ToUpper()));
            }

            switch (vm.SortCol)
            {
                case "name":
                    if (vm.SortColOrder == "desc")
                    {
                        books = books.OrderByDescending(b => b.Name);
                    }
                    else
                    {
                        books = books.OrderBy(b => b.Name);
                    }
                    break;
                case "createdDate":
                    if (vm.SortColOrder == "desc")
                    {
                        books = books.OrderByDescending(b => b.CreatedDate);
                    }
                    else
                    {
                        books = books.OrderBy(b => b.CreatedDate);
                    }
                    break;
                case "modifiedDate":
                    if (vm.SortColOrder == "desc")
                    {
                        books = books.OrderByDescending(b => b.ModifiedDate);
                    }
                    else
                    {
                        books = books.OrderBy(b => b.ModifiedDate);
                    }
                    break;
                case "publisher":
                    if (vm.SortColOrder == "desc")
                    {
                        books = books.OrderByDescending(b => b.Publisher.Name);
                    }
                    else
                    {
                        books = books.OrderBy(b => b.Publisher.Name);
                    }
                    break;
                default:
                    books = books.OrderBy(b => b.Name);
                    break;
            }

            vm.SortColOrder = (vm.SortColOrder == "asc") ? "desc" : "asc";
            IndexViewModel Ivm = new IndexViewModel(vm, books.ToPagedList(vm.Page, vm.PageSize));

            return View(Ivm);
        }
        
        // GET: Books/Books/Details/5
        public ActionResult Details(RecordViewModel recordVm)
        {
            Book book;
            DetailsViewModel dvm;

            if (recordVm == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //book = db.Books.Find(recordVm.Id); // Testing issue: don't use find, as it does not work when mocking EF 6
            book = db.Books.FirstOrDefault(i => i.ID == recordVm.Id);
            if (book == null)
            {
                return HttpNotFound();
            }
                
            dvm = new DetailsViewModel(recordVm.SearchSortPageParams, book);
            return View(dvm);
        }

        // GET: Books/Books/Create
        public ActionResult Create()
        {
            ViewBag.PublisherID = new SelectList(db.Publishers, "ID", "Name");
            return View();
        }

        // POST: Books/Books/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,Name,CreatedDate,ModifiedDate,PublisherID")] Book book)
        {
            if (ModelState.IsValid)
            {
                db.Books.Add(book);
                db.SaveChanges();
                return RedirectToActionFor<BooksController>(c => c.Index(null));
            }

            ViewBag.PublisherID = new SelectList(db.Publishers, "ID", "Name", book.PublisherID);
            return View(book);
        }

        // GET: Books/Books/Edit/5
        public ActionResult Edit(RecordViewModel recordVm)
        {
            Book book;
            IEnumerable<SelectListItem> publishers; 
            EditViewModel evm;

            if (recordVm == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            book = db.Books.FirstOrDefault(i => i.ID == recordVm.Id);
            if (book == null)
            {
                return HttpNotFound();
            }
            
            publishers = new SelectList(db.Publishers, "ID", "Name", book.PublisherID);
            evm = new EditViewModel(recordVm.SearchSortPageParams, book, publishers);
            return View(evm);
        }

        // POST: Books/Books/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditInputModel eim)
        {
            IEnumerable<SelectListItem> publishers;
            EditViewModel evm;

            if (ModelState.IsValid)
            {
                // here is where we would do mapping.

                db.Entry(eim.Book).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToActionFor<BooksController>(c => c.Index(null), eim.SearchSortPageParams);
            }
            publishers = new SelectList(db.Publishers, "ID", "Name", eim.Book.PublisherID);
            evm = new EditViewModel(eim.SearchSortPageParams, eim.Book, publishers);
            return View(evm);
        }

        // GET: Books/Books/Delete/5
        public ActionResult Delete(RecordViewModel recordVm)
        {
            Book book;
            DeleteInputModel dim;

            if (recordVm == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            book = db.Books.FirstOrDefault(i => i.ID == recordVm.Id);
            if (book == null)
            {
                return HttpNotFound();
            }

            dim = new DeleteInputModel(recordVm.SearchSortPageParams, book);
            
            return View(dim);
        }

        // POST: Books/Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(RecordViewModel recordVm)
        {
            Book book;

            if (recordVm == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            book = db.Books.FirstOrDefault(i => i.ID == recordVm.Id);
            if (book == null)
            {
                return HttpNotFound();
            }

            db.Books.Remove(book);
            db.SaveChanges();
            return RedirectToActionFor<BooksController>(c => c.Index(null), recordVm.SearchSortPageParams);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
