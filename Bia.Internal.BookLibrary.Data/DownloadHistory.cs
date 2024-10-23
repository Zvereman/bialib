using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Bia.Internal.BookLibrary.Data
{
    /// <summary>
    /// An ebook download history. Who has had downloaded the book.
    /// </summary>
    public class DownloadHistory
    {
        [Key]
        public Guid Uid { get; set; }
        public DateTime DownloadDate { get; set; }

        public AppUser User
        {
            get => LazyLoader.Load(this, ref _user);
            set => _user = value;
        }
        private AppUser _user;

        public Ebook DownloadedBook
        {
            get => LazyLoader.Load(this, ref _downloadedBook);
            set => _downloadedBook = value;
        }
        private Ebook _downloadedBook;

        private ILazyLoader LazyLoader { get; set; }

        public DownloadHistory()
        {

        }

        public DownloadHistory(ILazyLoader lazyLoader)
        {
            LazyLoader = lazyLoader;
        }

    }
}
