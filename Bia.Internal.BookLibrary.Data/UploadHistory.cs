using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Bia.Internal.BookLibrary.Data
{
    /// <summary>
    /// An ebook download history. Who has had upload the book.
    /// </summary>
    [DataContract]
    public class UploadHistory
    {
        [DataMember]
        public DateTime UploadDate { get; set; }

        public AppUser User
        {
            get => LazyLoader.Load(this, ref _user);
            set => _user = value;
        }
        private AppUser _user;

        [DataMember]
        public Guid AppUserUid { get; set; }

        public Book Book
        {
            get => LazyLoader.Load(this, ref _book);
            set => _book = value;
        }
        private Book _book;
        [DataMember]
        public int BookId { get; set; }

        private ILazyLoader LazyLoader { get; set; }

        public UploadHistory()
        {

        }

        public UploadHistory(ILazyLoader lazyLoader)
        {
            LazyLoader = lazyLoader;
        }

    }
}
