using Avalonia;
using Avalonia.Input;
using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Controls.Visual3Ds;
using MedicalSharp.Primitives.Builders;
using MedicalSharp.Primitives.Enums;
using OpenTK.Mathematics;
using System;
using System.Threading.Tasks;

namespace MedicalSharp.Controls.Commands
{
    /// <summary>
    /// 绘制文本3D元素命令
    /// </summary>
    public class DrawTextCommand : DrawShapeCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// 文本3D元素
        /// </summary>
        private TextVisual3D _text;

        /// <summary>
        /// 默认构造器
        /// </summary>
        public DrawTextCommand()
        {

        }

        #endregion

        #region # 属性

        #region 获取法向量委托 —— Func<Vector3D> GetNormal
        /// <summary>
        /// 获取法向量委托
        /// </summary>
        public Func<Vector3D> GetNormal { get; set; }
        #endregion

        #region 绘制开始委托 —— Func<TextVisual3D, Task<string>> DrawStart
        /// <summary>
        /// 绘制开始委托
        /// </summary>
        public Func<TextVisual3D, Task<string>> DrawStart { get; set; }
        #endregion

        #region 绘制结束委托 —— Action<TextVisual3D> DrawEnd
        /// <summary>
        /// 绘制结束委托
        /// </summary>
        public Action<TextVisual3D> DrawEnd { get; set; }
        #endregion

        #region 绘制已取消委托 —— Action<TextVisual3D> DrawCancelled
        /// <summary>
        /// 绘制已取消委托
        /// </summary>
        public Action<TextVisual3D> DrawCancelled { get; set; }
        #endregion

        #endregion

        #region # 方法

        //Public

        #region 鼠标按下事件 —— override void OnMouseDown(OpenTKViewport viewport...
        /// <summary>
        /// 鼠标按下事件
        /// </summary>
        public override void OnMouseDown(OpenTKViewport viewport, PointerPressedEventArgs eventArgs)
        {
            if (eventArgs.Properties.IsLeftButtonPressed && viewport is BasicViewport basicViewport)
            {
                Vector2 mousePos2D = eventArgs.GetPixelPosition(viewport).ToVector2();
                Vector3? mousePos3D = basicViewport.FindNearestPosition(mousePos2D);
                if (mousePos3D.HasValue)
                {
                    Vector3D position = mousePos3D.Value.ToVector3();
                    this._text = new TextVisual3D
                    {
                        Position = position,
                        Color = ColorFactory.TextColor.ToColor(),
                        Normal = this.GetNormal?.Invoke() ?? new Vector3D(0, 1, 0)
                    };
                    if (viewport is VolumeViewport)
                    {
                        this._text.RenderMode = TextRenderMode.Billboard;
                        this._text.LockYAxis = true;
                    }
                    if (viewport is MPRViewport)
                    {
                        this._text.RenderMode = TextRenderMode.Fixed;
                        this._text.LockYAxis = false;
                    }

                    this._isDrawing = true;
                    this.RequestAndConfirmText(viewport);
                }
            }

            base.OnMouseDown(viewport, eventArgs);
        }
        #endregion

        #region 失效命令 —— override void Deactivate()
        /// <summary>
        /// 失效命令
        /// </summary>
        /// <remarks>命令被停用时调用，切换命令前</remarks>
        public override void Deactivate()
        {
            base.Deactivate();

            this._isDrawing = false;
            this._text = null;
        }
        #endregion


        //Private

        #region 请求与确认文本 —— async void RequestAndConfirmText(OpenTKViewport viewport)
        /// <summary>
        /// 请求与确认文本
        /// </summary>
        private async void RequestAndConfirmText(OpenTKViewport viewport)
        {
            #region # 验证

            if (this.DrawStart == null)
            {
                return;
            }

            #endregion

            // 弹出输入框
            string inputText = await this.DrawStart.Invoke(this._text);
            if (!string.IsNullOrWhiteSpace(inputText))
            {
                this._text.Text = inputText;
                this.DrawEnd?.Invoke(this._text);
            }
            else
            {
                this.DrawCancelled?.Invoke(this._text);
            }

            this._isDrawing = false;
            this._text = null;

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #endregion
    }
}
