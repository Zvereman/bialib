using System;

namespace Bia.Internal.BookLibrary.Models
{
    public class UserForAddToAdminList
    {
        public Guid UserUid { get; set; }
        public bool IsAdmin { get; set; }
        public UserForAddToAdminList() 
        {
            this.UserUid = Guid.Empty;
            this.IsAdmin = false;
        }
    }
}
