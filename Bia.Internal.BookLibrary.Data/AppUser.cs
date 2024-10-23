using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Bia.Internal.BookLibrary.Data
{
    /// <summary>
    /// Represents an application user.
    /// </summary>
    public class AppUser
    {
        [Key]
        public Guid Uid { get; set; }
        public string LoginName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public DateTime DateJoined { get; set; }
        public AccessGroupId AccessGroup { get; set; }
        public IgnoredUser Ignored { get; set; }
        public string Discriminator { get; set; }

        public ICollection<PaperBook> BooksTaken
        {
            get => LazyLoader.Load(this, ref _booksTaken);
            set => _booksTaken = value;
        }
        private ICollection<PaperBook> _booksTaken;

        public ICollection<DownloadHistory> DownloadHistory
        {
            get => LazyLoader.Load(this, ref _downloadHistory);
            set => _downloadHistory = value;
        }

        private ICollection<DownloadHistory> _downloadHistory;

        public ICollection<UploadHistory> UploadHistory
        {
            get => LazyLoader.Load(this, ref _uploadHistory);
            set => _uploadHistory = value;
        }

        private ICollection<UploadHistory> _uploadHistory;

        public ICollection<RentHistory> RentHistory
        {
            get => LazyLoader.Load(this, ref _rentHistory);
            set => _rentHistory = value;
        }
        private ICollection<RentHistory> _rentHistory;

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


        public ICollection<UserNotify> UserNotifies
        {
            get => LazyLoader.Load(this, ref _userNotifies);
            set => _userNotifies = value;
        }
        private ICollection<UserNotify> _userNotifies;

        public Arrangement Arrangement { get; set; }

        private ILazyLoader LazyLoader { get; set; }

        public AppUser() { }

        public AppUser(ILazyLoader lazyLoader)
        {
            LazyLoader = lazyLoader;
        }

        protected ILazyLoader GetLoader()
        {
            return LazyLoader;
        }
    }
}
