using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Text;

namespace Bia.Internal.BookLibrary.Data
{
    /// <summary>
    /// Feedback left by user.
    /// </summary>
    public class Review
    {
        [Key]
        public Guid Uid { get; set; }
        [DataMember]
        public int BookId { get; set; }

        public string Text { get; set; }
        public int Rating { get; set; }
        [DataMember]
        public Guid RatedByUid { get; set; }

        public AppUser RatedBy
        {
            get => LazyLoader.Load(this, ref _ratedBy);
            set => _ratedBy = value;
        }
        private AppUser _ratedBy;

        public Book Book
        {
            get => LazyLoader.Load(this, ref _book);
            set => _book = value;
        }
        private Book _book;


        private ILazyLoader LazyLoader { get; set; }

        public Review()
        {

        }

        public Review(ILazyLoader lazyLoader)
        {
            LazyLoader = lazyLoader;
        }
    }
}
