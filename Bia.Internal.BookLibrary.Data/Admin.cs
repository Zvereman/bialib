using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Bia.Internal.BookLibrary.Data
{
    public class Admin: AppUser
    {
        public Admin()
        {
        }

        public Admin(ILazyLoader lazyLoader) : base(lazyLoader)
        {
        }

        public ICollection<AdminNotify> AdminNotifies
        {
            get => base.GetLoader().Load(this, ref _adminNotifies);
            set => _adminNotifies = value;
        }
        private ICollection<AdminNotify> _adminNotifies;

    }
}
