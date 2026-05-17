/// <summary>
/// 高斯滤波
/// </summary>
/// <param name="input">输入图像</param>
/// <param name="output">输出图像</param>
/// <param name="kernelSize">核矩阵尺寸</param>
/// <param name="sigma">标准差</param>
__kernel void gaussian_blur_3d(__read_only image3d_t input, __write_only image3d_t output, const int kernelSize, const float sigma)
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
	int radius = kernelSize / 2;

	//边界：保留原值
	if (x < radius || x >= width - radius ||
		y < radius || y >= height - radius ||
		z < radius || z >= depth - radius)
	{
		float4 value = read_imagef(input, position);
		write_imagef(output, position, value);

		return;
	}

	//动态计算权重
	float sigma2 = 2.0f * sigma * sigma;
	float weightSum = 0.0f;
	for (int dz = -radius; dz <= radius; dz++)
	{
		for (int dy = -radius; dy <= radius; dy++)
		{
			for (int dx = -radius; dx <= radius; dx++)
			{
				float dist2 = (float)(dx * dx + dy * dy + dz * dz);
				weightSum += exp(-dist2 / sigma2);
			}
		}
	}

	//卷积
	float4 sum = (float4)(0.0f);
	for (int dz = -radius; dz <= radius; dz++)
	{
		for (int dy = -radius; dy <= radius; dy++)
		{
			for (int dx = -radius; dx <= radius; dx++)
			{
				float dist2 = (float)(dx * dx + dy * dy + dz * dz);
				float weight = exp(-dist2 / sigma2) / weightSum;
				sum += read_imagef(input, (int4)(x + dx, y + dy, z + dz, 0)) * weight;
			}
		}
	}

	write_imagef(output, position, sum);
}
