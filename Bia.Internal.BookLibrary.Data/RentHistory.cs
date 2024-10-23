using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Bia.Internal.BookLibrary.Data
{
    /// <summary>
    /// A book rent history. Who has had took the book.
    /// </summary>
    [DataContract]
    public class RentHistory
    {
        [Key]
        [DataMember]
        public Guid Uid { get; set; }

        [DataMember]
        public DateTime TakenDate { get; set; }

        /// <summary>
        /// If user requested to extend a rent time.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public DateTime ExtendedDueDate { get; set; }

        /// <summary>
        /// Times user requested to extend a rent time.
        /// </summary>
        [DataMember]
        public int ExtendedTimesCount { get; set; }

        public DateTime? ReturnedDate { get; set; }

        public AppUser User
        {
            get => LazyLoader.Load(this, ref _user);
            set => _user = value;
        }
        private AppUser _user;

        public PaperBook TakenBook
        {
            get => LazyLoader.Load(this, ref _takenBook);
            set => _takenBook = value;
        }
        private PaperBook _takenBook;

        private ILazyLoader LazyLoader { get; set; }

        public RentHistory()
        {

        }

        public RentHistory(ILazyLoader lazyLoader)
        {
            LazyLoader = lazyLoader;
        }
    }
}
