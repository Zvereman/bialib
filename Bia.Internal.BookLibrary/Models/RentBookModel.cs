using System;

namespace Bia.Internal.BookLibrary.Models
{
    public class RentBookModel
    {
        public int Id { get; set; }
        public Guid User { get; set; }
        public DateTime Date { get; set; }

        public RentBookModel() 
        {
            this.Id = 0;
            this.User = Guid.NewGuid();
            this.Date = DateTime.Now;
        } 
    }
}
