using Bia.Internal.BookLibrary.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bia.Internal.BookLibrary.Models
{
    public class PaperBookViewModel : UserBookViewModel
    {
        public List<PaperBook> PaperBooks { get; set; }
        public List<Category> Categories { get; set; }
        public int TotalCount { get; set; }

    }
}
