using Bia.Internal.BookLibrary.Data;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System;

namespace Bia.Internal.BookLibrary.Models
{
    public class NewOrEditBookViewModel
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Subtitle { get; set; }
        
        public string Annotation { get; set; }
        
        //public string Isbn { get; set; }
        
        //public string Link { get; set; }
        
        public string CoverImage { get; set; }
        
        public int? Year { get; set; }
        
        public int? Pages { get; set; }
        
        public int? Edition { get; set; }
        
        //public bool PaperBook { get; set; }
        
        public LanguageId Language { get; set; }
        
        public List<BookAuthor> Authors { get; set; }
        
        public List<BookCategory> Categories { get; set; }

        public int RequestAndReadBooksCount { get; set; }
        public List<RentHistory> ReadBooksByUser { get; set; }
        public List<RequestBook> RequestBooksByUser { get; set; }

        public NewOrEditBookViewModel()
        {
            this.Id = 0;
            this.Title = string.Empty;
            this.Subtitle = string.Empty;
            this.Annotation = string.Empty;
            //this.Isbn = string.Empty;
            //this.Link = string.Empty;
            this.CoverImage = string.Empty;
            this.Year = 0;
            this.Pages = 0;
            this.Edition = 0;
            //this.PaperBook = true;
            this.Language = new LanguageId();
            this.Authors = new List<BookAuthor>();
            this.Categories = new List<BookCategory>();
            this.RequestAndReadBooksCount = 0;
            this.ReadBooksByUser = new List<RentHistory>();
            this.RequestBooksByUser = new List<RequestBook>();
        }
    }
}
