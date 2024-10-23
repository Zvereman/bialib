using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Bia.Internal.BookLibrary.Data
{
    [DataContract]
    public class UserNotify
    {
        [DataMember]
        public int BookId { get; set; }
        [DataMember]
        public Guid Uid { get; set; }

        public Book Book
        {
            get => LazyLoader.Load(this, ref _book);
            set => _book = value;
        }
        private Book _book;
        public AppUser AppUser
        {
            get => LazyLoader.Load(this, ref _appUser);
            set => _appUser = value;
        }
        private AppUser _appUser;


        private ILazyLoader LazyLoader { get; set; }

        public UserNotify()
        {

        }
        public UserNotify(ILazyLoader lazyLoader)
        {
            LazyLoader = lazyLoader;
        }

    }
}
