using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Bia.Internal.BookLibrary.Data
{
    [DataContract]
    public class PaperBook : Book
    {
        public PaperBook()
        {
        }
        public PaperBook(ILazyLoader lazyLoader) : base(lazyLoader)
        {
        }
        /// <summary>
        /// The rent due time if the book is taken by user.
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public DateTime? TakenDue { get; set; }

        public Guid? TakenByUserUid { get; set; }

        public AppUser TakenByUser
        {
            get => base.GetLoader().Load(this, ref _takenByUser);
            set => _takenByUser = value;
        }

        private AppUser _takenByUser;

        public ICollection<RentHistory> RentHistory
        {
            get => base.GetLoader().Load(this, ref _rentHistory);
            set => _rentHistory = value;
        }

        private ICollection<RentHistory> _rentHistory;
    }
}
