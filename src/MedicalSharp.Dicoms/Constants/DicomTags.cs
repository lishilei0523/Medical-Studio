namespace MedicalSharp.Dicoms.Constants
{
    using System.Globalization;

    /// <summary>
    /// DICOM标准常量
    /// </summary>
    public static class DicomTags
    {
        // ================ 患者模块 (Patient Module) ================

        /// <summary>
        /// 患者姓名 (PN)
        /// </summary>
        public const string PatientName = "0010|0010";

        /// <summary>
        /// 患者ID (LO)
        /// </summary>
        public const string PatientID = "0010|0020";

        /// <summary>
        /// 患者出生日期 (DA)
        /// </summary>
        public const string PatientBirthDate = "0010|0030";

        /// <summary>
        /// 患者性别 (CS)
        /// </summary>
        public const string PatientSex = "0010|0040";

        /// <summary>
        /// 患者年龄 (AS)
        /// </summary>
        public const string PatientAge = "0010|1010";

        /// <summary>
        /// 患者身高/体型 (DS)
        /// </summary>
        public const string PatientSize = "0010|1020";

        /// <summary>
        /// 患者体重 (DS)
        /// </summary>
        public const string PatientWeight = "0010|1030";

        // ================ 研究模块 (Study Module) ================

        /// <summary>
        /// 研究实例UID (UI)
        /// </summary>
        public const string StudyInstanceUID = "0020|000d";

        /// <summary>
        /// 研究日期 (DA)
        /// </summary>
        public const string StudyDate = "0008|0020";

        /// <summary>
        /// 研究时间 (TM)
        /// </summary>
        public const string StudyTime = "0008|0030";

        /// <summary>
        /// 研究描述 (LO)
        /// </summary>
        public const string StudyDescription = "0008|1030";

        /// <summary>
        /// 研究ID (SH)
        /// </summary>
        public const string StudyID = "0020|0010";

        // ================ 序列模块 (Series Module) ================

        /// <summary>
        /// 序列实例UID (UI)
        /// </summary>
        public const string SeriesInstanceUID = "0020|000e";

        /// <summary>
        /// 序列日期 (DA)
        /// </summary>
        public const string SeriesDate = "0008|0021";

        /// <summary>
        /// 序列时间 (TM)
        /// </summary>
        public const string SeriesTime = "0008|0031";

        /// <summary>
        /// 序列描述 (LO)
        /// </summary>
        public const string SeriesDescription = "0008|103e";

        /// <summary>
        /// 序列号 (IS)
        /// </summary>
        public const string SeriesNumber = "0020|0011";

        /// <summary>
        /// 成像设备模态 (CS)
        /// </summary>
        public const string Modality = "0008|0060";

        /// <summary>
        /// 检查部位 (CS)
        /// </summary>
        public const string BodyPartExamined = "0018|0015";

        // ================ 设备模块 (Equipment Module) ================

        /// <summary>
        /// 设备制造商 (LO)
        /// </summary>
        public const string Manufacturer = "0008|0070";

        /// <summary>
        /// 机构名称 (LO)
        /// </summary>
        public const string InstitutionName = "0008|0080";

        /// <summary>
        /// 设备站名 (SH)
        /// </summary>
        public const string StationName = "0008|1010";

        /// <summary>
        /// 软件版本 (LO)
        /// </summary>
        public const string SoftwareVersions = "0018|1020";

        // ================ 图像像素模块 (Image Pixel Module) ================

        /// <summary>
        /// 每个像素的样本数 (US)
        /// </summary>
        public const string SamplesPerPixel = "0028|0002";

        /// <summary>
        /// 光度解释 (CS)
        /// </summary>
        public const string PhotometricInterpretation = "0028|0004";

        /// <summary>
        /// 图像行数 (US)
        /// </summary>
        public const string Rows = "0028|0010";

        /// <summary>
        /// 图像列数 (US)
        /// </summary>
        public const string Columns = "0028|0011";

        /// <summary>
        /// 分配的位数 (US)
        /// </summary>
        public const string BitsAllocated = "0028|0100";

        /// <summary>
        /// 存储的位数 (US)
        /// </summary>
        public const string BitsStored = "0028|0101";

        /// <summary>
        /// 最高位 (US)
        /// </summary>
        public const string HighBit = "0028|0102";

        /// <summary>
        /// 像素表示 (US)
        /// </summary>
        public const string PixelRepresentation = "0028|0103";

        /// <summary>
        /// 像素数据 (OW/OB)
        /// </summary>
        public const string PixelData = "7fe0|0010";


        // ================ CT专用模块 ================

        /// <summary>
        /// 管电压 (DS)
        /// </summary>
        public const string KVP = "0018|0060";

        /// <summary>
        /// 采集号 (IS)
        /// </summary>
        public const string AcquisitionNumber = "0020|0012";

        /// <summary>
        /// 重建直径 (DS)
        /// </summary>
        public const string ReconstructionDiameter = "0018|1100";

        /// <summary>
        /// 卷积核 (SH)
        /// </summary>
        public const string ConvolutionKernel = "0018|1210";

        /// <summary>
        /// 曝光时间 (IS)
        /// </summary>
        public const string ExposureTime = "0018|1150";

        /// <summary>
        /// X射线管电流 (IS)
        /// </summary>
        public const string XRayTubeCurrent = "0018|1151";

        /// <summary>
        /// 曝光量 (IS)
        /// </summary>
        public const string Exposure = "0018|1152";

        // ================ MR专用模块 ================

        /// <summary>
        /// 磁场强度 (DS)
        /// </summary>
        public const string MagneticFieldStrength = "0018|0087";

        /// <summary>
        /// 重复时间 (DS)
        /// </summary>
        public const string RepetitionTime = "0018|0080";

        /// <summary>
        /// 回波时间 (DS)
        /// </summary>
        public const string EchoTime = "0018|0081";

        /// <summary>
        /// 反转时间 (DS)
        /// </summary>
        public const string InversionTime = "0018|0082";

        /// <summary>
        /// 翻转角 (DS)
        /// </summary>
        public const string FlipAngle = "0018|1314";

        /// <summary>
        /// 序列名称 (SH)
        /// </summary>
        public const string SequenceName = "0018|0024";

        // ================ PET专用模块 ================

        /// <summary>
        /// 放射性药物 (LO)
        /// </summary>
        public const string Radiopharmaceutical = "0018|0031";

        /// <summary>
        /// 放射性核素总剂量 (DS)
        /// </summary>
        public const string RadionuclideTotalDose = "0018|1074";

        /// <summary>
        /// 放射性核素半衰期 (DS)
        /// </summary>
        public const string RadionuclideHalfLife = "0018|1075";

        // ================ 图像几何模块 (Image Geometry) ================

        /// <summary>
        /// 像素间距 (DS[2])
        /// </summary>
        public const string PixelSpacing = "0028|0030";

        /// <summary>
        /// 层厚 (DS)
        /// </summary>
        public const string SliceThickness = "0018|0050";

        /// <summary>
        /// 图像位置（病人坐标系）(DS[3])
        /// </summary>
        public const string ImagePositionPatient = "0020|0032";

        /// <summary>
        /// 图像方向（病人坐标系）(DS[6])
        /// </summary>
        public const string ImageOrientationPatient = "0020|0037";

        /// <summary>
        /// 切片位置 (DS)
        /// </summary>
        public const string SliceLocation = "0020|1041";

        /// <summary>
        /// 切片间距 (DS)
        /// </summary>
        public const string SpacingBetweenSlices = "0018|0088";

        // ================ VOI LUT模块 (关键！你的Shader核心) ================

        /// <summary>
        /// 窗位 (DS[1-n])
        /// </summary>
        public const string WindowCenter = "0028|1050";

        /// <summary>
        /// 窗宽 (DS[1-n])
        /// </summary>
        public const string WindowWidth = "0028|1051";

        /// <summary>
        /// VOI LUT函数 (CS)
        /// </summary>
        public const string VOILUTFunction = "0028|1056";

        // ================ 模态LUT模块 (Modality LUT) ================

        /// <summary>
        /// 缩放截距 (DS)
        /// </summary>
        public const string RescaleIntercept = "0028|1052";

        /// <summary>
        /// 缩放斜率 (DS)
        /// </summary>
        public const string RescaleSlope = "0028|1053";

        /// <summary>
        /// 缩放类型 (LO)
        /// </summary>
        public const string RescaleType = "0028|1054";

        /// <summary>
        /// 模态LUT序列 (SQ)
        /// </summary>
        public const string ModalityLUTSequence = "0028|3000";

        /// <summary>
        /// LUT描述符 (US[3])
        /// </summary>
        public const string LUTDescriptor = "0028|3002";

        /// <summary>
        /// LUT说明 (LO)
        /// </summary>
        public const string LUTExplanation = "0028|3003";

        /// <summary>
        /// 模态LUT数据 (US/OW)
        /// </summary>
        public const string ModalityLUTData = "0028|3006";


        // ================ 显示LUT模块 (Presentation LUT) ================

        /// <summary>
        /// 显示LUT序列 (SQ)
        /// </summary>
        public const string PresentationLUTSequence = "2050|0010";

        /// <summary>
        /// 显示LUT形状 (CS)
        /// </summary>
        public const string PresentationLUTShape = "2050|0020";

        /// <summary>
        /// 显示LUT数据 (US/OW)
        /// </summary>
        public const string PresentationLUTData = "2050|0014";


        // ================ 叠加层模块 (Overlay) ================

        /// <summary>
        /// 叠加层行数 (US)
        /// </summary>
        public const string OverlayRows = "6000|0010";

        /// <summary>
        /// 叠加层列数 (US)
        /// </summary>
        public const string OverlayColumns = "6000|0011";

        /// <summary>
        /// 叠加层类型 (CS)
        /// </summary>
        public const string OverlayType = "6000|0040";

        /// <summary>
        /// 叠加层原点 (SS[2])
        /// </summary>
        public const string OverlayOrigin = "6000|0050";

        /// <summary>
        /// 叠加层数据 (OW/OB)
        /// </summary>
        public const string OverlayData = "6000|3000";


        // ================ 曲线/标注模块 ================

        /// <summary>
        /// 图形标注序列 (SQ)
        /// </summary>
        public const string GraphicAnnotationSequence = "0070|0001";

        /// <summary>
        /// 图形层 (CS)
        /// </summary>
        public const string GraphicLayer = "0070|0002";

        /// <summary>
        /// 文本对象序列 (SQ)
        /// </summary>
        public const string TextObjectSequence = "0070|0008";

        /// <summary>
        /// 图形对象序列 (SQ)
        /// </summary>
        public const string GraphicObjectSequence = "0070|0009";


        // ================ 辅助函数 ================

        /// <summary>
        /// 格式化Tag用于显示
        /// </summary>
        /// <param name="tag">DICOM标签字符串 (格式: "gggg|eeee")</param>
        /// <returns>格式化的标签字符串</returns>
        public static string Format(string tag)
        {
            return tag ?? string.Empty;
        }

        /// <summary>
        /// 获取组号
        /// </summary>
        /// <param name="tag">DICOM标签字符串 (格式: "gggg|eeee")</param>
        /// <returns>组号（16位无符号整数）</returns>
        public static ushort GetGroup(string tag)
        {
            if (string.IsNullOrEmpty(tag) || tag.Length < 4)
            {
                return 0;
            }

            try
            {
                string groupStr = tag.Substring(0, 4);
                return ushort.Parse(groupStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 获取元素号
        /// </summary>
        /// <param name="tag">DICOM标签字符串 (格式: "gggg|eeee")</param>
        /// <returns>元素号（16位无符号整数）</returns>
        public static ushort GetElement(string tag)
        {
            if (string.IsNullOrEmpty(tag) || tag.Length < 9)
            {
                return 0;
            }

            try
            {
                // 跳过 "gggg|" 部分 (5个字符)
                string elemStr = tag.Substring(5, 4);
                return ushort.Parse(elemStr, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// 从组号和元素号创建标签字符串
        /// </summary>
        /// <param name="group">组号</param>
        /// <param name="element">元素号</param>
        /// <returns>格式化的标签字符串 (格式: "gggg|eeee")</returns>
        public static string CreateTag(ushort group, ushort element)
        {
            return $"{group:X4}|{element:X4}";
        }

        /// <summary>
        /// 尝试解析标签字符串
        /// </summary>
        /// <param name="tag">DICOM标签字符串</param>
        /// <param name="group">解析出的组号</param>
        /// <param name="element">解析出的元素号</param>
        /// <returns>是否解析成功</returns>
        public static bool TryParse(string tag, out ushort group, out ushort element)
        {
            group = 0;
            element = 0;

            if (string.IsNullOrEmpty(tag) || tag.Length < 9 || tag[4] != '|')
            {
                return false;
            }

            try
            {
                group = ushort.Parse(tag.Substring(0, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                element = ushort.Parse(tag.Substring(5, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
