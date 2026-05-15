using Silk.NET.OpenCL;
using System;

namespace MedicalSharp.Inspiration.Resources
{
    /// <summary>
    /// OpenCL异常
    /// </summary>
    public class ClException : Exception
    {
        /// <summary>
        /// OpenCL错误码
        /// </summary>
        private readonly int _errorCode;

        /// <summary>
        /// 创建OpenCL异常构造器
        /// </summary>
        /// <param name="message">异常消息</param>
        /// <param name="errorCode">错误码</param>
        public ClException(string message, int errorCode = (int)ErrorCodes.Success)
            : base(message)
        {
            this._errorCode = errorCode;
        }

        /// <summary>
        /// 检查错误码，非成功则抛出异常
        /// </summary>
        /// <param name="errorCode">OpenCL错误码</param>
        /// <param name="operation">操作描述</param>
        public static void ThrowOnError(int errorCode, string operation)
        {
            if (errorCode != (int)ErrorCodes.Success) // CL_SUCCESS = 0
            {
                throw new ClException($"{operation} 失败，错误码: {errorCode}", errorCode);
            }
        }
    }
}
