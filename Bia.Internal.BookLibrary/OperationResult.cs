using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Bia.Internal.BookLibrary
{

    public class OperationResult
    {

        public string Status { get; set; }


        public string Details { get; set; }


        public string ExceptionData { get; set; }
        public object Data { get; set; }
        public object OtherDate { get; set; }

        public Exception Exception => GetException();

        private Exception _exception;

        public Exception GetException()
        {
            if (ExceptionData == null)
                return null;

            return _exception ?? SetException();
        }

        public OperationResult SetSuccess(string details=null, object data = null, object otherData = null)
        {
            Status = "Success";
            if (!string.IsNullOrEmpty(details))
            {
                Details = details;
            }
            Data = data;
            OtherDate = otherData;
            return this;
        }

        public OperationResult SetFail(string details)
        {
            Status = "Failed";
            Details = details;
            return this;
        }

        public OperationResult SetFail(Exception ex = null)
        {
            Status = "Failed";
            if (ex != null)
            {
                Details = ex.Message;
            }
            Data = null;
            OtherDate = null;
            return this;
        }

        private Exception SetException()
        {
            try
            {
                using (var stream = new MemoryStream(Convert.FromBase64String(ExceptionData)))
                {
                    _exception = (Exception)new BinaryFormatter().Deserialize(stream);
                    return _exception;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
