using MedicalSharp.Dicoms;
using MedicalSharp.Dicoms.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;

namespace MedicalSharp.Tests.TestCases
{
    /// <summary>
    /// DICOM测试
    /// </summary>
    [TestClass]
    public class DicomTests
    {
        /// <summary>
        /// 测试加载序列
        /// </summary>
        [TestMethod]
        public void TestLoadSeries()
        {
            string dicomFolder = @"F:\Files\DICOMs\CT";
            //string dicomFolder = @"F:\Files\DICOMs\HE_00003969\1.2.876877.204130127066024003.20231130161143.2484.004";

            for (int i = 0; i < 10; i++)
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();

                VolumeData volumeData = DicomLoader.LoadSeries(dicomFolder);

                stopwatch.Stop();
                Trace.WriteLine(stopwatch.Elapsed);

                Assert.IsNotNull(volumeData);
            }
        }
    }
}
