using System;
using RavuAlHemio.OneWire.Driver;

namespace RavuAlHemio.OneWire
{
    public class OneWireOperationException : Exception
    {
        public string Operation { get; }
        public int OneWireErrorCode { get; }
        public string OneWireErrorMessage { get; }

        public static OneWireOperationException Create(string operation, IOneWireErrors oneWireErrors,
            Exception innerException = null)
        {
            int errCode = oneWireErrors.PopErrorNumber();
            return Create(operation, errCode, innerException);
        }

        public static OneWireOperationException Create(string operation, ErrorCode errCode, Exception innerException = null)
        {
            int errCodeInt = (int)errCode;
            return Create(operation, errCodeInt, innerException);
        }

        public static OneWireOperationException Create(string operation, int errCode, Exception innerException = null)
        {
            string errMsg = ErrorMessages.Get(errCode);
            string message = $"{operation}: [{errCode}] {errMsg}";
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
