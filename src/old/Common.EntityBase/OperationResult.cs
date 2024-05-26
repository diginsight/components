using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public class OperationResult
    {
        public OperationResult()
        {
            result = true;
            resultCode = (int)OperationResultCodes.Ok;
            resultMessage = string.Empty;
            exception = null;
        }

        public OperationResult(bool Result, int ResultCode, string ResultMessage, Exception Exception = null)
        {
            result = Result;
            resultCode = ResultCode;
            resultMessage = ResultMessage;
            exception = Exception;
        }

        public bool result;
        public int resultCode;
        public string resultMessage;
        public Exception exception;
    }

    public class OperationResult<T> : OperationResult
    {
        public OperationResult(T ResultObject) : base()
        {
            resultObject = ResultObject;
        }
        public OperationResult(bool Result, int ResultCode, string ResultMessage, T ResultObject, Exception Exception = null)
                : base(Result, ResultCode, ResultMessage, Exception)
        {
            resultObject = ResultObject;
        }

        public T resultObject;
    }

    public enum OperationResultCodes
    {
        Ok = 0,
        UnableToSaveFile = 10,

        SerializeJsonError = 100,
        ServiceTakesTimeToStart = 200,
        UnableToStartService = 201,
        UnableToStartService_InvalidParameters = 202,
        UnableToGetFile = 300,
        UnableToGetBearerToken = 301,
        RemoteCallError = 302,
        DeserializeUpdateJsonError = 303,
        UpdateFileDataNotValid = 304,
        UnableToCreateFolder = 305,
        FileDownloadCorrupted = 306,
        UnableToRetrieveLastExecutionData = 400,
        FileDoesNotExists = 401,
        Unknown = 500
    }
}
