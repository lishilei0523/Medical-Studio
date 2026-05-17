using MedicalSharp.Intelligence.Base;
using MedicalSharp.Intelligence.Inputs;
using MedicalSharp.Intelligence.Outputs;
using MedicalSharp.Primitives.Managers;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MedicalSharp.Intelligence.Models
{
    /// <summary>
    /// TotalSegmentor分割模型
    /// </summary>
    public class TotalSegmentor : OnnxModel<TotalSegmentorInput, TotalSegmentorOutput>
    {
        #region # 字段及构造器

        /// <summary>
        /// 创建TotalSegmentor分割模型构造器
        /// </summary>
        /// <param name="modelBytes">模型字节数组</param>
        /// <param name="sessionOptions">会话选项</param>
        public TotalSegmentor(byte[] modelBytes, SessionOptions sessionOptions)
            : base(modelBytes, sessionOptions)
        {

        }

        #endregion

        #region # 属性

        //

        #endregion

        #region # 方法

        #region 处理推理输入 —— override List<NamedOnnxValue> ProcessInput(TotalSegmentorInput input)
        /// <summary>
        /// 处理推理输入
        /// </summary>
        /// <param name="input">推理输入</param>
        /// <returns>ONNX键值列表</returns>
        protected override unsafe List<NamedOnnxValue> ProcessInput(TotalSegmentorInput input)
        {
            int depth = input.Shape[0];
            int height = input.Shape[1];
            int width = input.Shape[2];

            //创建张量 [batch, channel, D, H, W]
            int[] dimensions = [1, 1, depth, height, width];
            int totalVoxels = depth * height * width;
            NativeMemoryManager<float> memoryManager = new NativeMemoryManager<float>(input.Data, totalVoxels);
            DenseTensor<float> tensor = new DenseTensor<float>(memoryManager.Memory, dimensions);

            NamedOnnxValue namedOnnxValue = NamedOnnxValue.CreateFromTensor("input", tensor);

            return [namedOnnxValue];
        }
        #endregion

        #region 处理推理结果 —— override TotalSegmentorOutput ProcessInference(IReadOnlyList<NamedOnnxValue>...
        /// <summary>
        /// 处理推理结果
        /// </summary>
        /// <param name="inference">推理结果</param>
        /// <param name="minConfidence">最小置信度</param>
        /// <returns>推理输出</returns>
        protected override unsafe TotalSegmentorOutput ProcessInference(IReadOnlyList<NamedOnnxValue> inference, float minConfidence)
        {
            //获取输出张量
            Tensor<float> tensor = inference[0].AsTensor<float>();
            DenseTensor<float> outputTensor = tensor as DenseTensor<float>;

            #region # 验证

            if (outputTensor == null)
            {
                throw new InvalidOperationException($"输出张量类型为 {tensor.GetType()}，需要DenseTensor<float>");
            }

            #endregion

            //解析张量形状
            int[] outputShape = outputTensor.Dimensions.ToArray();
            int classesCount = outputShape[1];
            int depth = outputShape[2];
            int height = outputShape[3];
            int width = outputShape[4];
            int totalVoxels = depth * height * width;

            //分配结果内存[width, height, depth]
            byte* resultPtr = (byte*)NativeMemory.Alloc((UIntPtr)totalVoxels);

            //将输出布局[D, H, W]转换为[W, H, D]
            using MemoryHandle memoryHandle = outputTensor.Buffer.Pin();
            float* memoryPointer = (float*)memoryHandle.Pointer;
            for (int d = 0; d < depth; d++)
            {
                for (int h = 0; h < height; h++)
                {
                    for (int w = 0; w < width; w++)
                    {
                        int modelIndex = d * height * width + h * width + w;   //[D, H, W]
                        int outputIndex = w * height * depth + h * depth + d;  //[W, H, D]

                        float maxProbability = minConfidence;
                        byte maxClass = 0;
                        for (int classIndex = 1; classIndex < classesCount; classIndex++)
                        {
                            float probability = memoryPointer[classIndex * totalVoxels + modelIndex];
                            if (probability > maxProbability)
                            {
                                maxProbability = probability;
                                maxClass = (byte)classIndex;
                            }
                        }

                        resultPtr[outputIndex] = maxClass;
                    }
                }
            }

            //返回结果
            TotalSegmentorOutput output = new TotalSegmentorOutput
            {
                Data = (IntPtr)resultPtr,
                Shape = [width, height, depth]
            };

            return output;
        }
        #endregion

        #endregion
    }
}
