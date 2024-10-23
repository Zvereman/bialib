using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bia.Internal.BookLibrary.Models
{
    public class AddReviewModel
    {
        public int BookId { get; set; }
        public int Rating { get; set; }
        public string Text { get; set; }
    }
}
