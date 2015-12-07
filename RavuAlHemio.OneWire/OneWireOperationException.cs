using System;

namespace RavuAlHemio.OneWire
{
    public class OneWireOperationException : Exception
    {
        public string Operation { get; }
        public int OneWireErrorCode { get; }
        public string OneWireErrorMessage { get; }

        public static OneWireOperationException Create(string operation, IOneWireLink link,
            Exception innerException = null)
        {
            int errCode = link.PopErrorNumber();
            string errMsg = ErrorMessages.Get(errCode);
            string message = string.Format("{0}: [{1}] {2}", operation, errCode, errMsg);
            return new OneWireOperationException(message, operation, errCode, errMsg, innerException);
        }

        protected OneWireOperationException(string message, string operation, int oneWireErrorCode,
            string oneWireErrorMessage, Exception innerException)
            : base(message, innerException)
        {
            Operation = operation;
            OneWireErrorCode = oneWireErrorCode;
            OneWireErrorMessage = oneWireErrorMessage;
        }
    }
}
