using Bia.Internal.BookLibrary.Data;
using System.Collections.Generic;

namespace Bia.Internal.BookLibrary.Models
{
    public class MyRequestViewModal
    {
        public Dictionary<string, List<PaperBook>> MyRequestBooks { get; set; }
        public int CountMyRequestsBooks {  get; set; }
    }
}
