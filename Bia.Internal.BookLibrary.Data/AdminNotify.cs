using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Bia.Internal.BookLibrary.Data.Enum;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Bia.Internal.BookLibrary.Data
{
    [Serializable]
    [DataContract]
    public class AdminNotify
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public AdminNotifyTypes  NotifyId { get; set; }
        [DataMember]
        public Guid Uid { get; set; }

        public Admin Admin
        {
            get => LazyLoader.Load(this, ref _admin);
            set => _admin = value;
        }
        private Admin _admin;
        
        private ILazyLoader LazyLoader { get; set; }

        public AdminNotify()
        {

        }
        public AdminNotify(ILazyLoader lazyLoader)
        {
            LazyLoader = lazyLoader;
        }
    }
}
