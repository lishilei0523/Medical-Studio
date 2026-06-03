using Avalonia.Input;
using MedicalSharp.Controls.Extensions;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Controls.Visual3Ds;
using MedicalSharp.Primitives.Cameras;
using MedicalSharp.Primitives.Models;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace MedicalSharp.Controls.Base
{
    /// <summary>
    /// 形状命令
    /// </summary>
    public abstract class ShapeCommand : ViewportCommand
    {
        #region # 字段及构造器

        /// <summary>
        /// 默认构造器
        /// </summary>
        protected ShapeCommand()
        {

        }

        #endregion

        #region # 属性

        //

        #endregion

        #region # 方法

        #region 获取上下文菜单项列表 —— override IReadOnlyList<ContextMenuItem> GetContextMenuItems(...
        /// <summary>
        /// 获取上下文菜单项列表
        /// </summary>
        /// <returns>上下文菜单项列表</returns>
        /// <remarks>右键点击松开时调用，返回null或空列表表示不弹出菜单</remarks>
        public override IReadOnlyList<ContextMenuItem> GetContextMenuItems(OpenTKViewport viewport, PointerReleasedEventArgs eventArgs)
        {
            if (viewport is IPickVisual3D pickVisual3D)
            {
                Vector2 mousePos2D = eventArgs.GetPosition(viewport).ToVector2();
                if (pickVisual3D.FindNearest(mousePos2D, out _, out _, out Visual3D visual, out _))
                {
                    if (visual is ViewBoxVisual3D && viewport.Camera is OrbitCamera camera)
                    {
                        List<ContextMenuItem> items =
                        [
                            new ContextMenuItem
                            {
                                Header = "前面(_A)",
                                Command = () =>
                                {
                                    camera.SetRotation(-90.0f, 0);
                                    viewport.RequestNextFrameRendering();
                                }
                            },
                            new ContextMenuItem
                            {
                                Header = "后面(_P)",
                                Command = () =>
                                {
                                    camera.SetRotation(90.0f, 0);
                                    viewport.RequestNextFrameRendering();
                                }
                            },
                            new ContextMenuItem
                            {
                                Header = "左面(_L)",
                                Command = () =>
                                {
                                    camera.SetRotation(180, 0);
                                    viewport.RequestNextFrameRendering();
                                }
                            },
                            new ContextMenuItem
                            {
                                Header = "右面(_R)",
                                Command = () =>
                                {
                                    camera.SetRotation(0, 0);
                                    viewport.RequestNextFrameRendering();
                                }
                            },
                            new ContextMenuItem
                            {
                                Header = "上面(_S)",
                                Command = () =>
                                {
                                    camera.SetRotation(-90, -89);
                                    viewport.RequestNextFrameRendering();
                                }
                            },
                            new ContextMenuItem
                            {
                                Header = "下面(_I)",
                                Command = () =>
                                {
                                    camera.SetRotation(-90, 89);
                                    viewport.RequestNextFrameRendering();
                                }
                            }
                        ];

                        return items;
                    }
                }
            }

            return null;
        }
        #endregion

        #endregion
    }
}
