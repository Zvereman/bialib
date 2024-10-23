using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text;

namespace Bia.Internal.BookLibrary.Data
{
    [Serializable]
    [DataContract]
    public class Book
    {
        /// <summary>
        /// International Standard Book Number.
        /// </summary>
        [DataMember]
        public string Isbn { get; set; }

        [Key]
        [DataMember(EmitDefaultValue = false)]
        public int BiaId { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Subtitle { get; set; }

        [DataMember]
        public LanguageId Language { get; set; }

        [DataMember]
        public int? Year { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int? Edition { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int? Pages { get; set; }

        [DataMember]
        public string CoverImage { get; set; }

        [DataMember]
        public string Annotation { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public int AverageRating { get; set; }

  //      public UploadHistory Upload { get; set; }

        //public ICollection<AppUser> Downloads
        //{
        //    get => LazyLoader.Load(this, ref _downloads);
        //    set => _downloads = value;
        //}
        //private ICollection<AppUser> _downloads;

        public ICollection<BookAuthor> Authors
        {
            get => LazyLoader.Load(this, ref _authors);
            set => _authors = value;
        }
        private ICollection<BookAuthor> _authors;

        public virtual ICollection<BookCategory> Cathegories
        {
            get => LazyLoader.Load(this, ref _Cathegories);
            set => _Cathegories = value;
        }
        private ICollection<BookCategory> _Cathegories;

        public ICollection<Review> Reviews
        {
            get => LazyLoader.Load(this, ref _reviews);
            set => _reviews = value;
        }
        private ICollection<Review> _reviews;

        public ICollection<RequestBook> ReqestBooks
        {
            get => LazyLoader.Load(this, ref _reqestBooks);
            set => _reqestBooks = value;
        }
        private ICollection<RequestBook> _reqestBooks;

        public ICollection<UploadHistory> UploadHistory
        {
            get => LazyLoader.Load(this, ref _uploadHistory);
            set => _uploadHistory = value;
        }

        private ICollection<UploadHistory> _uploadHistory;


        private ILazyLoader LazyLoader { get; set; }

        public Book()
        {

        }
        public Book(ILazyLoader lazyLoader)
        {
            LazyLoader = lazyLoader;
        }

        protected ILazyLoader GetLoader()
        {
            return LazyLoader;
        }
    }




}
