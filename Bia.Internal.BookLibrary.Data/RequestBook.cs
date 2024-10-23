using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Bia.Internal.BookLibrary.Data
{
    [DataContract]
    public class RequestBook
    {
        [DataMember]
        public int BookId { get; set; }

        [DataMember]
        public Guid AppUserUid { get; set; }

        public DateTime Date { get; set; }

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

        public RequestBook()
        {

        }
        public RequestBook(ILazyLoader lazyLoader)
        {
            LazyLoader = lazyLoader;
        }
    }
}
