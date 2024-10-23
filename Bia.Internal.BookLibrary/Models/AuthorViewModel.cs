using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bia.Internal.BookLibrary.Models
{
    public class AuthorViewModel
    {
        public Guid AuthoUid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        //  public ICollection<int> Books { get; set; } // collection id of books in company
        public string FullName
        {
            get
            {
                return $"{FirstName} {MiddleName} {LastName}";
            }
        }
    }
}
