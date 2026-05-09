using MedicalSharp.Controls.Visuals;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Maths;
using SD.Infrastructure.Avalonia.Caliburn.Base;

namespace MedicalSharp.Client.Events
{
    /// <summary>
    /// MPR平面变化事件
    /// </summary>
    public class MPRPlaneChangedEvent : CaliburnEvent
    {
        /// <summary>
        /// MPR平面
        /// </summary>
        public MPRPlane Plane { get; set; }

        /// <summary>
        /// 触发源
        /// </summary>
        public MPRPlaneChangeSource TriggerSource { get; set; }

        /// <summary>
        /// 十字线
        /// </summary>
        public CrosshairVisual3D Crosshair { get; set; }

        /// <summary>
        /// 是否由同步触发
        /// </summary>
        /// <remarks>
        /// true：其他视图同步过来触发的，不应再往外广播
        /// false：本视图用户操作触发的，应该通知其他视图
        /// </remarks>
        public bool IsSyncTriggered { get; set; }
    }
}
