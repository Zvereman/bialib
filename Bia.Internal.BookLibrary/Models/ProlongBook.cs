using Newtonsoft.Json;
using System;

namespace Bia.Internal.BookLibrary.Models
{
    public class ProlongBook
    {
        [JsonProperty("BookId")]
        public Guid BookId { get; set; }
        [JsonProperty("CurentDate")]
        public DateTime CurentDate { get; set; }
        [JsonProperty("NewDate")]
        public DateTime NewDate { get; set; }

        public ProlongBook() 
        {
            this.BookId = Guid.NewGuid();
            this.CurentDate = DateTime.Now;
            this.NewDate = DateTime.Now;
        }
    }
}
