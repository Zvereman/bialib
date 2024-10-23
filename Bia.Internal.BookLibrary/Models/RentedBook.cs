using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bia.Internal.BookLibrary.Models
{
    public class RentedBook
    {
        public int BiaId { get; set; }
        public string Title { get;set;}
        public string User { get; set; }
        public DateTime StartRenting { get; set; }
        public DateTime EndRenting { get; set; }

        public RentStatus Status
        {
            get
            {
                return (EndRenting<=DateTime.Today)? RentStatus.Early: RentStatus.Late;
            }
        }
    }

    public enum RentStatus
    {
        Early,
        Late
    }
}
