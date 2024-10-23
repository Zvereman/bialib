using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text;

namespace Bia.Internal.BookLibrary.Data
{
    /// <summary>
    /// Book author.
    /// </summary>
    [Serializable]
    [DataContract]
    public class Author
    {
        [Key]
        [DataMember(EmitDefaultValue = false)]
        public Guid AuthorUid { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }
        [DataMember]
        public string MiddleName { get; set; }

        public ICollection<BookAuthor> Books
        {
            get => LazyLoader.Load(this, ref _books);
            set => _books = value;
        }
        private ICollection<BookAuthor> _books;

        private ILazyLoader LazyLoader { get; set; }

        public Author()
        {
        }
        public Author(ILazyLoader lazyLoader)
        {
            LazyLoader = lazyLoader;
        }
    }
}
