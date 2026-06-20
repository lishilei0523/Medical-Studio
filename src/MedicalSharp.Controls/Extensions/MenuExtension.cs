using MedicalSharp.Controls.Base;
using MedicalSharp.Controls.Interfaces;
using MedicalSharp.Controls.Viewports;
using MedicalSharp.Controls.Visual3Ds;
using MedicalSharp.Primitives.Cameras;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Models;
using System.Collections.Generic;

namespace MedicalSharp.Controls.Extensions
{
    /// <summary>
    /// 菜单扩展
    /// </summary>
    public static class MenuExtension
    {
        #region # 获取上下文菜单项列表 —— static IReadOnlyList<ContextMenuItem> GetContextMenuItems(this Visual3D...
        /// <summary>
        /// 获取上下文菜单项列表
        /// </summary>
        /// <param name="visual">3D元素</param>
        /// <param name="viewport">OpenTK视口</param>
        /// <param name="command">形状命令</param>
        /// <returns>上下文菜单项列表</returns>
        public static IReadOnlyList<ContextMenuItem> GetContextMenuItems(this Visual3D visual, OpenTKViewport viewport, ShapeCommand command)
        {
            if (visual is ViewBoxVisual3D viewBox)
            {
                return GetContextMenuItems(viewBox, viewport, command);
            }
            if (visual is TextVisual3D text)
            {
                return GetContextMenuItems(text, viewport, command);
            }
            if (visual is PointVisual3D point)
            {
                return GetContextMenuItems(point, viewport, command);
            }
            if (visual is LineSegmentVisual3D lineSegment)
            {
                return GetContextMenuItems(lineSegment, viewport, command);
            }
            if (visual is RectangleVisual3D rectangle)
            {
                return GetContextMenuItems(rectangle, viewport, command);
            }
            if (visual is CircleVisual3D circle)
            {
                return GetContextMenuItems(circle, viewport, command);
            }
            if (visual is EllipseVisual3D ellipse)
            {
                return GetContextMenuItems(ellipse, viewport, command);
            }
            if (visual is PolylineVisual3D polyline)
            {
                return GetContextMenuItems(polyline, viewport, command);
            }
            if (visual is CurveVisual3D curve)
            {
                return GetContextMenuItems(curve, viewport, command);
            }
            if (visual is BoundingBoxVisual3D box)
            {
                return GetContextMenuItems(box, viewport, command);
            }
            if (visual is BoundingSphereVisual3D sphere)
            {
                return GetContextMenuItems(sphere, viewport, command);
            }
            if (visual is CylinderVisual3D cylinder)
            {
                return GetContextMenuItems(cylinder, viewport, command);
            }
            if (visual is ConvexPolyhedronVisual3D convexPolyhedron)
            {
                return GetContextMenuItems(convexPolyhedron, viewport, command);
            }

            return [];
        }
        #endregion


        //Private

        #region # 获取ViewBox上下文菜单项列表 —— static IReadOnlyList<ContextMenuItem> GetContextMenuItems(this ViewBoxVisual3D...
        /// <summary>
        /// 获取ViewBox上下文菜单项列表
        /// </summary>
        private static IReadOnlyList<ContextMenuItem> GetContextMenuItems(this ViewBoxVisual3D viewBox, OpenTKViewport viewport, ShapeCommand shapeCommand)
        {
            List<ContextMenuItem> items = [];
            if (viewport.Camera is OrbitCamera camera)
            {
                items =
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
            }

            return items;
        }
        #endregion

        #region # 获取文本上下文菜单项列表 —— static IReadOnlyList<ContextMenuItem> GetContextMenuItems(this TextVisual3D...
        /// <summary>
        /// 获取文本上下文菜单项列表
        /// </summary>
        private static IReadOnlyList<ContextMenuItem> GetContextMenuItems(this TextVisual3D text, OpenTKViewport viewport, ShapeCommand shapeCommand)
        {
            List<ContextMenuItem> items =
            [
                new ContextMenuItem
                {
                    Header = "删除(_D)",
                    Command = () => RemoveVisual(text, viewport, shapeCommand)
                }
            ];

            return items;
        }
        #endregion

        #region # 获取点上下文菜单项列表 —— static IReadOnlyList<ContextMenuItem> GetContextMenuItems(this PointVisual3D...
        /// <summary>
        /// 获取点上下文菜单项列表
        /// </summary>
        private static IReadOnlyList<ContextMenuItem> GetContextMenuItems(this PointVisual3D point, OpenTKViewport viewport, ShapeCommand shapeCommand)
        {
            List<ContextMenuItem> items =
            [
                new ContextMenuItem
                {
                    Header = "删除(_D)",
                    Command = () => RemoveVisual(point, viewport, shapeCommand)
                }
            ];

            return items;
        }
        #endregion

        #region # 获取线段上下文菜单项列表 —— static IReadOnlyList<ContextMenuItem> GetContextMenuItems(this LineSegmentVisual3D...
        /// <summary>
        /// 获取线段上下文菜单项列表
        /// </summary>
        private static IReadOnlyList<ContextMenuItem> GetContextMenuItems(this LineSegmentVisual3D lineSegment, OpenTKViewport viewport, ShapeCommand shapeCommand)
        {
            List<ContextMenuItem> items =
            [
                new ContextMenuItem
                {
                    Header = "删除(_D)",
                    Command = () => RemoveVisual(lineSegment, viewport, shapeCommand)
                }
            ];

            return items;
        }
        #endregion

        #region # 获取矩形上下文菜单项列表 —— static IReadOnlyList<ContextMenuItem> GetContextMenuItems(this RectangleVisual3D...
        /// <summary>
        /// 获取矩形上下文菜单项列表
        /// </summary>
        private static IReadOnlyList<ContextMenuItem> GetContextMenuItems(this RectangleVisual3D rectangle, OpenTKViewport viewport, ShapeCommand shapeCommand)
        {
            List<ContextMenuItem> items =
            [
                new ContextMenuItem
                {
                    Header = "删除(_D)",
                    Command = () => RemoveVisual(rectangle, viewport, shapeCommand)
                },
                new ContextMenuItem
                {
                    Header = "内切(_I)",
                    Command = () => ApplyCut(rectangle, viewport, shapeCommand, CutMode.Inside),
                    IsEnabled = shapeCommand.GetMarkValue != null
                },
                new ContextMenuItem
                {
                    Header = "外切(_O)",
                    Command = () => ApplyCut(rectangle, viewport, shapeCommand, CutMode.OutSide),
                    IsEnabled = shapeCommand.GetMarkValue != null
                },
                new ContextMenuItem
                {
                    Header = "统计(_S)",
                    Command = () => ApplyAnalyse2D(rectangle, viewport, shapeCommand)
                }
            ];

            return items;
        }
        #endregion

        #region # 获取圆形上下文菜单项列表 —— static IReadOnlyList<ContextMenuItem> GetContextMenuItems(this CircleVisual3D...
        /// <summary>
        /// 获取圆形上下文菜单项列表
        /// </summary>
        private static IReadOnlyList<ContextMenuItem> GetContextMenuItems(this CircleVisual3D circle, OpenTKViewport viewport, ShapeCommand shapeCommand)
        {
            List<ContextMenuItem> items =
            [
                new ContextMenuItem
                {
                    Header = "删除(_D)",
                    Command = () => RemoveVisual(circle, viewport, shapeCommand)
                },
                new ContextMenuItem
                {
                    Header = "内切(_I)",
                    Command = () => ApplyCut(circle, viewport, shapeCommand, CutMode.Inside),
                    IsEnabled = shapeCommand.GetMarkValue != null
                },
                new ContextMenuItem
                {
                    Header = "外切(_O)",
                    Command = () => ApplyCut(circle, viewport, shapeCommand, CutMode.OutSide),
                    IsEnabled = shapeCommand.GetMarkValue != null
                },
                new ContextMenuItem
                {
                    Header = "统计(_S)",
                    Command = () => ApplyAnalyse2D(circle, viewport, shapeCommand)
                }
            ];

            return items;
        }
        #endregion

        #region # 获取椭圆形上下文菜单项列表 —— static IReadOnlyList<ContextMenuItem> GetContextMenuItems(this EllipseVisual3D...
        /// <summary>
        /// 获取椭圆形上下文菜单项列表
        /// </summary>
        private static IReadOnlyList<ContextMenuItem> GetContextMenuItems(this EllipseVisual3D ellipse, OpenTKViewport viewport, ShapeCommand shapeCommand)
        {
            List<ContextMenuItem> items =
            [
                new ContextMenuItem
                {
                    Header = "删除(_D)",
                    Command = () => RemoveVisual(ellipse, viewport, shapeCommand)
                },
                new ContextMenuItem
                {
                    Header = "内切(_I)",
                    Command = () => ApplyCut(ellipse, viewport, shapeCommand, CutMode.Inside),
                    IsEnabled = shapeCommand.GetMarkValue != null
                },
                new ContextMenuItem
                {
                    Header = "外切(_O)",
                    Command = () => ApplyCut(ellipse, viewport, shapeCommand, CutMode.OutSide),
                    IsEnabled = shapeCommand.GetMarkValue != null
                },
                new ContextMenuItem
                {
                    Header = "统计(_S)",
                    Command = () => ApplyAnalyse2D(ellipse, viewport, shapeCommand)
                }
            ];

            return items;
        }
        #endregion

        #region # 获取折线上下文菜单项列表 —— static IReadOnlyList<ContextMenuItem> GetContextMenuItems(this PolylineVisual3D...
        /// <summary>
        /// 获取折线上下文菜单项列表
        /// </summary>
        private static IReadOnlyList<ContextMenuItem> GetContextMenuItems(this PolylineVisual3D polyline, OpenTKViewport viewport, ShapeCommand shapeCommand)
        {
            List<ContextMenuItem> items =
            [
                new ContextMenuItem
                {
                    Header = "删除(_D)",
                    Command = () => RemoveVisual(polyline, viewport, shapeCommand)
                },
                new ContextMenuItem
                {
                    Header = "内切(_I)",
                    Command = () => ApplyCut(polyline, viewport, shapeCommand, CutMode.Inside),
                    IsEnabled = polyline.Closed && shapeCommand.GetMarkValue != null
                },
                new ContextMenuItem
                {
                    Header = "外切(_O)",
                    Command = () => ApplyCut(polyline, viewport, shapeCommand, CutMode.OutSide),
                    IsEnabled = polyline.Closed && shapeCommand.GetMarkValue != null
                },
                new ContextMenuItem
                {
                    Header = "统计(_S)",
                    Command = () => ApplyAnalyse2D(polyline, viewport, shapeCommand),
                    IsEnabled = polyline.Closed
                }
            ];

            return items;
        }
        #endregion

        #region # 获取曲线上下文菜单项列表 —— static IReadOnlyList<ContextMenuItem> GetContextMenuItems(this CurveVisual3D...
        /// <summary>
        /// 获取曲线上下文菜单项列表
        /// </summary>
        private static IReadOnlyList<ContextMenuItem> GetContextMenuItems(this CurveVisual3D curve, OpenTKViewport viewport, ShapeCommand shapeCommand)
        {
            List<ContextMenuItem> items =
            [
                new ContextMenuItem
                {
                    Header = "删除(_D)",
                    Command = () => RemoveVisual(curve, viewport, shapeCommand)
                },
                new ContextMenuItem
                {
                    Header = "内切(_I)",
                    Command = () => ApplyCut(curve, viewport, shapeCommand, CutMode.Inside),
                    IsEnabled = curve.Closed && shapeCommand.GetMarkValue != null
                },
                new ContextMenuItem
                {
                    Header = "外切(_O)",
                    Command = () => ApplyCut(curve, viewport, shapeCommand, CutMode.OutSide),
                    IsEnabled = curve.Closed && shapeCommand.GetMarkValue != null
                },
                new ContextMenuItem
                {
                    Header = "统计(_S)",
                    Command = () => ApplyAnalyse2D(curve, viewport, shapeCommand),
                    IsEnabled = curve.Closed
                }
            ];

            return items;
        }
        #endregion

        #region # 获取立方体上下文菜单项列表 —— static IReadOnlyList<ContextMenuItem> GetContextMenuItems(this BoundingBoxVisual3D...
        /// <summary>
        /// 获取立方体上下文菜单项列表
        /// </summary>
        private static IReadOnlyList<ContextMenuItem> GetContextMenuItems(this BoundingBoxVisual3D box, OpenTKViewport viewport, ShapeCommand shapeCommand)
        {
            List<ContextMenuItem> items =
            [
                new ContextMenuItem
                {
                    Header = "删除(_D)",
                    Command = () => RemoveVisual(box, viewport, shapeCommand)
                },
                new ContextMenuItem
                {
                    Header = "内切(_I)",
                    Command = () => ApplyCut(box, viewport, shapeCommand, CutMode.Inside),
                    IsEnabled = shapeCommand.GetMarkValue != null
                },
                new ContextMenuItem
                {
                    Header = "外切(_O)",
                    Command = () => ApplyCut(box, viewport, shapeCommand, CutMode.OutSide),
                    IsEnabled = shapeCommand.GetMarkValue != null
                },
                new ContextMenuItem
                {
                    Header = "统计(_S)",
                    Command = () => ApplyAnalyse3D(box, viewport, shapeCommand)
                }
            ];

            return items;
        }
        #endregion

        #region # 获取球体上下文菜单项列表 —— static IReadOnlyList<ContextMenuItem> GetContextMenuItems(this BoundingSphereVisual3D...
        /// <summary>
        /// 获取球体上下文菜单项列表
        /// </summary>
        private static IReadOnlyList<ContextMenuItem> GetContextMenuItems(this BoundingSphereVisual3D sphere, OpenTKViewport viewport, ShapeCommand shapeCommand)
        {
            List<ContextMenuItem> items =
            [
                new ContextMenuItem
                {
                    Header = "删除(_D)",
                    Command = () => RemoveVisual(sphere, viewport, shapeCommand)
                },
                new ContextMenuItem
                {
                    Header = "内切(_I)",
                    Command = () => ApplyCut(sphere, viewport, shapeCommand, CutMode.Inside),
                    IsEnabled = shapeCommand.GetMarkValue != null
                },
                new ContextMenuItem
                {
                    Header = "外切(_O)",
                    Command = () => ApplyCut(sphere, viewport, shapeCommand, CutMode.OutSide),
                    IsEnabled = shapeCommand.GetMarkValue != null
                },
                new ContextMenuItem
                {
                    Header = "统计(_S)",
                    Command = () => ApplyAnalyse3D(sphere, viewport, shapeCommand)
                }
            ];

            return items;
        }
        #endregion

        #region # 获取圆柱体上下文菜单项列表 —— static IReadOnlyList<ContextMenuItem> GetContextMenuItems(this CylinderVisual3D...
        /// <summary>
        /// 获取圆柱体上下文菜单项列表
        /// </summary>
        private static IReadOnlyList<ContextMenuItem> GetContextMenuItems(this CylinderVisual3D cylinder, OpenTKViewport viewport, ShapeCommand shapeCommand)
        {
            List<ContextMenuItem> items =
            [
                new ContextMenuItem
                {
                    Header = "删除(_D)",
                    Command = () => RemoveVisual(cylinder, viewport, shapeCommand)
                },
                new ContextMenuItem
                {
                    Header = "内切(_I)",
                    Command = () => ApplyCut(cylinder, viewport, shapeCommand, CutMode.Inside),
                    IsEnabled = shapeCommand.GetMarkValue != null
                },
                new ContextMenuItem
                {
                    Header = "外切(_O)",
                    Command = () => ApplyCut(cylinder, viewport, shapeCommand, CutMode.OutSide),
                    IsEnabled = shapeCommand.GetMarkValue != null
                },
                new ContextMenuItem
                {
                    Header = "统计(_S)",
                    Command = () => ApplyAnalyse3D(cylinder, viewport, shapeCommand)
                }
            ];

            return items;
        }
        #endregion

        #region # 获取凸多面体上下文菜单项列表 —— static IReadOnlyList<ContextMenuItem> GetContextMenuItems(this ConvexPolyhedronVisual3D...
        /// <summary>
        /// 获取凸多面体上下文菜单项列表
        /// </summary>
        private static IReadOnlyList<ContextMenuItem> GetContextMenuItems(this ConvexPolyhedronVisual3D convexPolyhedron, OpenTKViewport viewport, ShapeCommand shapeCommand)
        {
            List<ContextMenuItem> items =
            [
                new ContextMenuItem
                {
                    Header = "删除(_D)",
                    Command = () => RemoveVisual(convexPolyhedron, viewport, shapeCommand)
                },
                new ContextMenuItem
                {
                    Header = "内切(_I)",
                    Command = () => ApplyCut(convexPolyhedron, viewport, shapeCommand, CutMode.Inside),
                    IsEnabled = shapeCommand.GetMarkValue != null
                },
                new ContextMenuItem
                {
                    Header = "外切(_O)",
                    Command = () => ApplyCut(convexPolyhedron, viewport, shapeCommand, CutMode.OutSide),
                    IsEnabled = shapeCommand.GetMarkValue != null
                },
                new ContextMenuItem
                {
                    Header = "统计(_S)",
                    Command = () => ApplyAnalyse3D(convexPolyhedron, viewport, shapeCommand)
                }
            ];

            return items;
        }
        #endregion

        #region # 删除元素 —— static void RemoveVisual(Visual3D visual, OpenTKViewport viewport...
        /// <summary>
        /// 删除元素
        /// </summary>
        /// <param name="visual">3D元素</param>
        /// <param name="viewport">OpenTK视口</param>
        /// <param name="shapeCommand">形状命令</param>
        private static void RemoveVisual(Visual3D visual, OpenTKViewport viewport, ShapeCommand shapeCommand)
        {
            shapeCommand.VisualRemoved?.Invoke(visual);

            //请求下一帧
            viewport.RequestNextFrameRendering();
        }
        #endregion

        #region # 适用切割 —— static async void ApplyCut(ICutVolume cutVolume, OpenTKViewport viewport...
        /// <summary>
        /// 适用切割
        /// </summary>
        /// <param name="cutVolume">切割体积3D元素</param>
        /// <param name="viewport">OpenTK视口</param>
        /// <param name="shapeCommand">形状命令</param>
        /// <param name="cutMode">切割模式</param>
        private static async void ApplyCut(ICutVolume cutVolume, OpenTKViewport viewport, ShapeCommand shapeCommand, CutMode cutMode)
        {
            #region # 验证

            if (shapeCommand.GetMarkValue == null)
            {
                return;
            }

            #endregion

            byte markValue = await shapeCommand.GetMarkValue.Invoke();

            #region # 验证

            if (markValue == 0)
            {
                return;
            }

            #endregion

            if (viewport is VolumeViewport volumeViewport)
            {
                cutVolume.ApplyCutVolume(volumeViewport.VolumeData, volumeViewport.VolumeRenderable.MarkTexture, cutMode, markValue);

                //请求下一帧
                viewport.RequestNextFrameRendering();

                shapeCommand.ShapeCut?.Invoke();
            }
            if (viewport is MPRViewport mprViewport)
            {
                cutVolume.ApplyCutVolume(mprViewport.VolumeData, mprViewport.VolumeRenderable.MarkTexture, cutMode, markValue);

                //请求下一帧
                viewport.RequestNextFrameRendering();

                shapeCommand.ShapeCut?.Invoke();
            }
        }
        #endregion

        #region # 适用统计(2D) —— static void ApplyAnalyse2D(IAnalyseVolume2D analyseVolume2D, OpenTKViewport viewport...
        /// <summary>
        /// 适用统计(2D)
        /// </summary>
        /// <param name="analyseVolume2D">可统计体积2D元素</param>
        /// <param name="viewport">OpenTK视口</param>
        /// <param name="shapeCommand">形状命令</param>
        private static void ApplyAnalyse2D(IAnalyseVolume2D analyseVolume2D, OpenTKViewport viewport, ShapeCommand shapeCommand)
        {
            if (viewport is MPRViewport mprViewport)
            {
                StatisticResult result = analyseVolume2D.ApplyAnalyseVolume(mprViewport, null);
                shapeCommand.ShapeAnalysed?.Invoke(result);
            }
        }
        #endregion

        #region # 适用统计(3D) —— static async void ApplyAnalyse3D(IAnalyseVolume3D analyseVolume3D, OpenTKViewport viewport...
        /// <summary>
        /// 适用统计(3D)
        /// </summary>
        /// <param name="analyseVolume3D">可统计体积3D元素</param>
        /// <param name="viewport">OpenTK视口</param>
        /// <param name="shapeCommand">形状命令</param>
        private static async void ApplyAnalyse3D(IAnalyseVolume3D analyseVolume3D, OpenTKViewport viewport, ShapeCommand shapeCommand)
        {
            if (viewport is VolumeViewport volumeViewport)
            {
                StatisticResult result = await analyseVolume3D.ApplyAnalyseVolume(volumeViewport.VolumeData, null);
                shapeCommand.ShapeAnalysed?.Invoke(result);
            }
        }
        #endregion
    }
}
