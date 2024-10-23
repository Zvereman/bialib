using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text;

namespace Bia.Internal.BookLibrary.Data
{
    [DataContract]
    public class Category
    {
        [Key]
        [DataMember]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CategoryId { get; set; }

        [DataMember]
        public string CategoryName { get; set; }

        public ICollection<BookCategory> Books
        {
            get => LazyLoader.Load(this, ref _books);
            set => _books = value;
        }
        public ICollection<BookCategory> _books;

        private ILazyLoader LazyLoader { get; set; }

        public Category()
        {

        }
        public Category(ILazyLoader lazyLoader)
        {
            LazyLoader = lazyLoader;
        }
    }
}
