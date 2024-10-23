using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace Bia.Internal.BookLibrary.Models
{
    public class AddBookForm
    {
        public int BiaId { get; set; }
        public string Title { get; set; }
        public string Subtitle { get; set; }
        public string Annotation { get; set; }
        public IFormFile CoverImage { get; set; }
        public int? Year { get; set; }
        public int? Pages { get; set; }
        public string Language { get; set; }
        public List<Guid> Authors { get; set; }
        public List<int> Categories { get; set; }
    }
}
