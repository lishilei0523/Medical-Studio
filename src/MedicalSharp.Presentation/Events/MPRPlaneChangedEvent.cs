using MedicalSharp.Controls.Visual3Ds;
using MedicalSharp.Primitives.Enums;
using MedicalSharp.Primitives.Maths;
using SD.Infrastructure.Avalonia.Caliburn.Base;

namespace MedicalSharp.Presentation.Events
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
        /// 跳过体积视图同步
        /// </summary>
        public bool SkipVolumeSync { get; set; }
    }
}
