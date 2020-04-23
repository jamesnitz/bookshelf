using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bookshelf.Models.ViewModels
{
    public class BookFormViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public List<SelectListItem> GenreOptions { get; set; }
        public List<int> SelectedGenreIds { get; set; }
    }
}
