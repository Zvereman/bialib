using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bia.Internal.BookLibrary.Data
{
    public class S
    {
        public const string NLogConfig = "nlog.config";
        public const string ConnectionStringName = "dbConnection";

        public const string AccessLevelClaim = "AccessLevel";
        public const string Developer = "Developer";
        public const string Admin = "Admin";
        public const string Common = "Common";

        public const string GroupDeveloper = "GroupDeveloper";
        public const string GroupAdmin = "GroupAdmin";
        public const string GroupCommon = "GroupCommon";

        //Comma separator
        private const string CommSep = ",";
        public const string CommonAccess = Developer  +  CommSep + Common;
        public const string AdminAccess = Developer + CommSep + Admin;
    }
}
