using System;

namespace PowerView.Model
{
    public class DomainConstraintException : DataException
    {
        public DomainConstraintException()
        {
        }

        public DomainConstraintException(string message)
          : base(message)
        {
        }

        public DomainConstraintException(string message, Exception inner)
          : base(message, inner)
        {
        }
    }
}
