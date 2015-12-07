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
