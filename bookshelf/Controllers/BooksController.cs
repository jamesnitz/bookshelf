﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bookshelf.Data;
using bookshelf.Models;
using bookshelf.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace bookshelf.Controllers
{
    [Authorize]
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public BooksController(ApplicationDbContext context, UserManager<ApplicationUser> usermanager)
        {
            _userManager = usermanager;
            _context = context;
        }
        // GET: Books
        public async Task<ActionResult> Index()
        {
            var user = await GetCurrentUserAsync();
            var books = await _context.Book
                .Where(b => b.ApplicationuserId == user.Id)
                .Include(b => b.BookGenres)
                    .ThenInclude(bg => bg.Genre)
                .ToListAsync();
            return View(books);
        }

        // GET: Books/Details/5
        public async Task<ActionResult> Details(int id)
        {
            return View();
        }

        // GET: Books/Create
        public async Task<ActionResult> Create()
        {
            var genreOptions = await _context.Genre
                .Select(g => new SelectListItem() { Text = g.Name, Value = g.Id.ToString() })
                .ToListAsync();

            var viewModel = new BookFormViewModel();

            viewModel.GenreOptions = genreOptions;

            return View(viewModel);
        }

        // POST: Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(BookFormViewModel bookViewModel)
        {
            try
            {
                var user = await GetCurrentUserAsync();

                var book = new Book
                {
                    Title = bookViewModel.Title,
                    Author = bookViewModel.Author,
                    ApplicationuserId = user.Id
                };
                book.BookGenres = bookViewModel.SelectedGenreIds.Select(gi =>
                
                    new BookGenre()
                    {
                        Book = book,
                        GenreId = gi
                    }).ToList();
                _context.Book.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return View();
            }
        }

        // GET: Books/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var book = await _context.Book.Include(b => b.BookGenres).FirstOrDefaultAsync(b => b.Id == id);

            var viewModel = new BookFormViewModel();

            var genreOptions = await _context
                .Genre.Select(g => new SelectListItem()
                {
                    Text = g.Name,
                    Value = g.Id.ToString()
                }).ToListAsync();

            viewModel.Id = id;
            viewModel.Title = book.Title;
            viewModel.Author = book.Author;
            viewModel.GenreOptions = genreOptions;
            viewModel.SelectedGenreIds = book.BookGenres.Select(bg => bg.GenreId).ToList();

            return View(viewModel);
        }

        // POST: Books/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, BookFormViewModel bookViewModel)
        {
            try
            {
                var bookDataModel = await _context.Book.Include(b => b.BookGenres).FirstOrDefaultAsync(b => b.Id == id);

                bookDataModel.Title = bookViewModel.Title;
                bookDataModel.Author = bookViewModel.Author;
                bookDataModel.BookGenres.Clear();
                bookDataModel.BookGenres = bookViewModel.SelectedGenreIds.Select(genreId => new BookGenre()
                {
                    Book = bookDataModel,
                    GenreId = genreId
                }).ToList();

                _context.Book.Update(bookDataModel);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: Books/Delete/5
        public async Task<ActionResult> Delete(int id)
        {

            var book = await _context.Book.Include(b => b.BookGenres).FirstOrDefaultAsync(item => item.Id == id);

            var loggedInUser = await GetCurrentUserAsync();

            if (book.ApplicationuserId != loggedInUser.Id)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Delete(int id, Book book)
        {
                try
                {
                    _context.Book.Remove(book);
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
                catch
                {
                    return View();
                }

            
        }
        private async Task<ApplicationUser> GetCurrentUserAsync() => await _userManager.GetUserAsync(HttpContext.User);
    }
}