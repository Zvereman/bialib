using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Bia.Internal.BookLibrary.Data
{
    /// <summary>
    /// EF many to many model.
    /// </summary>
    [DataContract]
    public class BookAuthor
    {
        [DataMember]
        public int BookId { get; set; }
        [DataMember]
        public Guid AuthorUid { get; set; }
        public Book Book
        {
            get => LazyLoader.Load(this, ref _book);
            set => _book = value;
        }
        private Book _book;
        public Author Author
        {
            get => LazyLoader.Load(this, ref _author);
            set => _author = value;
        }
        private Author _author;

        private ILazyLoader LazyLoader { get; set; }

        public BookAuthor()
        {

        }
        public BookAuthor(ILazyLoader lazyLoader)
        {
            LazyLoader = lazyLoader;
        }
    }
}
