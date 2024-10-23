using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Bia.Internal.BookLibrary.Data
{
    public class Ebook : Book
    {
        public Ebook()
        {
        }

        public Ebook(ILazyLoader lazyLoader) : base(lazyLoader)
        {
        }

        public EbookFormatId EbookFormat { get; set; }
        public int? Size { get; set; }

        public string Link { get; set; }

        public ICollection<DownloadHistory> DownloadHistory
        {
            get => base.GetLoader().Load(this, ref _downloadHistory);
            set => _downloadHistory = value;
        }

        private ICollection<DownloadHistory> _downloadHistory;
    }
}