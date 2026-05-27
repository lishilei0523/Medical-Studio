/// <summary>
/// 前景 = 100
/// </summary>
/// <remarks>归一化到SNORM范围[-1, 1]：除以32767</remarks>
__constant float foreground_value = 100.0f / 32767.0f;

/// <summary>
/// 背景 = -1024（空气）
/// </summary>
/// <remarks>归一化到SNORM范围[-1, 1]：除以32767</remarks>
__constant float background_value = -1024.0f / 32767.0f;

/// <summary>
/// 3D自适应阈值分割
/// </summary>
/// <param name="input">输入图像（原始HU值）</param>
/// <param name="localMean">局部均值图像（由MeanBlur3D生成）</param>
/// <param name="output">输出二值图像（前景=1，背景=0）</param>
/// <param name="offset">偏移量（体素HU值 > (局部均值 - 偏移量)时判定为前景）</param>
/// <remarks>
/// 每个体素的阈值 = 局部均值 - 偏移量
/// 体素值 > 阈值 → 前景（写入前景值）
/// 体素值 ≤ 阈值 → 背景（写入背景值）
/// </remarks>
__kernel void adaptive_threshold_3d(
	__read_only image3d_t input,
	__read_only image3d_t localMean,
	__write_only image3d_t output,
	const float offset)
{
	int x = get_global_id(0);
	int y = get_global_id(1);
	int z = get_global_id(2);

	int width = get_image_width(input);
	int height = get_image_height(input);
	int depth = get_image_depth(input);
	if (x >= width || y >= height || z >= depth)
	{
		return;
	}

	int4 position = (int4)(x, y, z, 0);

	//读取原始值和局部均值
	float value = read_imagef(input, position).x;
	float mean = read_imagef(localMean, position).x;

	//自适应阈值 = 局部均值 - 偏移量
	float threshold = mean - offset;

	//比较并写入结果
	if (value > threshold)
	{
		write_imagef(output, position, (float4)(foreground_value, 0, 0, 0));
	}
	else
	{
		write_imagef(output, position, (float4)(background_value, 0, 0, 0));
	}
}
