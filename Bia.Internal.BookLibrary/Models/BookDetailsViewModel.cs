using Bia.Internal.BookLibrary.Data;
using System;
using System.Collections.Generic;

namespace Bia.Internal.BookLibrary.Models
{
    public class BookDetailsViewModel
    {
        public PaperBook Book{ get; set; }
        public DateTime? ReturnDate { get; set; }
        public int RequestAndReadBooksCount { get; set; }
        public List<RentHistory> ReadBooksByUser { get; set; }
        public List<RequestBook> RequestBooksByUser { get; set; }
        public string UserRole { get; set; }
    }
}
