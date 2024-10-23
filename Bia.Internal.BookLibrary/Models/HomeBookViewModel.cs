using Bia.Internal.BookLibrary.Data;
using System.Collections.Generic;

namespace Bia.Internal.BookLibrary.Models
{
    public class HomeBookViewModel
    {
        public Dictionary<string, List<PaperBook>> BooksByCategory { get; set; }
        public int RequestAndReadBooksCount { get; set; }
        public List<RentHistory> ReadBooksByUser { get; set; }
        public List<RequestBook> RequestBooksByUser { get; set; }
        public string UserRole { get; set; }
    }
}
