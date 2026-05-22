using OpenTK.Graphics.OpenGL4;
using System;

namespace MedicalSharp.Engine.Resources
{
    /// <summary>
    /// OpenGL异常
    /// </summary>
    public class GlException : Exception
    {
        #region # 字段及构造器

        /// <summary>
        /// OpenCL错误码
        /// </summary>
        private readonly int? _errorCode;

        /// <summary>
        /// 创建OpenGL异常构造器
        /// </summary>
        /// <param name="message">异常消息</param>
        /// <param name="errorCode">错误码</param>
        public GlException(string message, int? errorCode = null)
            : base(message)
        {
            this._errorCode = errorCode;
        }

        #endregion

        #region # 属性

        #region 只读属性 - OpenGL错误码 —— int? ErrorCode
        /// <summary>
        /// 只读属性 - OpenGL错误码
        /// </summary>
        public int? ErrorCode
        {
            get => this._errorCode;
        }
        #endregion

        #endregion

        #region # 方法

        #region 检查错误码 —— static void ThrowOnError(string operation)
        /// <summary>
        /// 检查错误码
        /// </summary>
        /// <param name="operation">操作描述</param>
        /// <remarks>非成功则抛出异常</remarks>
        public static void ThrowOnError(string operation)
        {
            ErrorCode errorCode = GL.GetError();
            if (errorCode != OpenTK.Graphics.OpenGL4.ErrorCode.NoError)
            {
                ErrorCode errorCodeEnum = (ErrorCode)errorCode;
                string message = $"{operation} 失败，错误码: {errorCode}，描述: {errorCodeEnum.ToString()}";
                throw new GlException(message, (int)errorCode);
            }
        }
        #endregion 

        #endregion
    }
}
