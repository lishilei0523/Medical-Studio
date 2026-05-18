using Silk.NET.OpenCL;
using System;

namespace MedicalSharp.Inspiration.Resources
{
    /// <summary>
    /// OpenCL异常
    /// </summary>
    public class ClException : Exception
    {
        #region # 字段及构造器

        /// <summary>
        /// OpenCL错误码
        /// </summary>
        private readonly int? _errorCode;

        /// <summary>
        /// 创建OpenCL异常构造器
        /// </summary>
        /// <param name="message">异常消息</param>
        /// <param name="errorCode">错误码</param>
        public ClException(string message, int? errorCode = null)
            : base(message)
        {
            this._errorCode = errorCode;
        }

        #endregion

        #region # 属性

        #region 只读属性 - OpenCL错误码 —— int? ErrorCode
        /// <summary>
        /// 只读属性 - OpenCL错误码
        /// </summary>
        public int? ErrorCode
        {
            get => this._errorCode;
        }
        #endregion

        #endregion

        #region # 方法

        #region 检查错误码 —— static void ThrowOnError(int errorCode, string operation)
        /// <summary>
        /// 检查错误码
        /// </summary>
        /// <param name="errorCode">OpenCL错误码</param>
        /// <param name="operation">操作描述</param>
        /// <remarks>非成功则抛出异常</remarks>
        public static void ThrowOnError(int errorCode, string operation)
        {
            if (errorCode != (int)ErrorCodes.Success)
            {
                ErrorCodes errorCodeEnum = (ErrorCodes)errorCode;
                string message = $"{operation} 失败，错误码: {errorCode}，描述: {errorCodeEnum.ToString()}";
                throw new ClException(message, errorCode);
            }
        }
        #endregion 

        #endregion
    }
}
