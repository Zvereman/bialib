using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Bia.Internal.BookLibrary.Data
{
    [DataContract]
    public class Arrangement
    {
        public AppUser AppUser { get; set; }

        [Key]
        public Guid AppUserUid { get; set; }
        [DataMember]
        public DateTime Date { get; set; }

        private ILazyLoader LazyLoader { get; set; }
    }
}
