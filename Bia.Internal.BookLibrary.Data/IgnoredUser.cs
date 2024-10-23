using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text;

namespace Bia.Internal.BookLibrary.Data
{
    [DataContract]
    public class IgnoredUser
    {
        [Key]
        [DataMember]
        public Guid AppUserUid { get; set; }

        public AppUser AppUser { get; set; }
    }
}
