using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CA_SVC.Exceptions
{
    public class InvalidGUIDException : AppExceptionBase
    {
        public InvalidGUIDException(string objectTypeName, string keys)
        {
            ObjectTypeName = objectTypeName;
            Keys = keys;
        }

        public override string Message => $"Object [{ ObjectTypeName}] ({Keys}) guid is not valid.";
    }
}