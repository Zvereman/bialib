using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Bia.Internal.BookLibrary.Data
{
    /// <summary>
    /// EF many to many model.
    /// </summary>
    [DataContract]
    public class BookCategory
    {
        [DataMember]
        public int BookId { get; set; }
        [DataMember]
        public int CategoryId { get; set; }

        public Book Book
        {
            get => LazyLoader.Load(this,ref _book);
            set => _book = value;
        }
        private Book _book;

        public Category Category
        {
            get => LazyLoader.Load(this, ref _category);
            set => _category = value;
        }
        private Category _category;

        private ILazyLoader LazyLoader { get; set; }

        public BookCategory()
        {

        }

        public BookCategory(ILazyLoader lazyLoader)
        {
            LazyLoader = lazyLoader;
        }
    }
}
