/// <summary>
/// 腐蚀
/// </summary>
/// <param name="input">输入图像</param>
/// <param name="output">输出图像</param>
/// <param name="kernelSize">核矩阵尺寸</param>
__kernel void erode_3d(__read_only image3d_t input, __write_only image3d_t output, const int kernelSize)
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

	//取邻域最小值
	float minVal = 1e10f;
	for (int dz = -radius; dz <= radius; dz++)
	{
		for (int dy = -radius; dy <= radius; dy++)
		{
			for (int dx = -radius; dx <= radius; dx++)
			{
				float4 value = read_imagef(input, (int4)(x + dx, y + dy, z + dz, 0));
				minVal = fmin(minVal, value.x);
			}
		}
	}

	write_imagef(output, position, (float4)(minVal, 0, 0, 0));
}

/// <summary>
/// 膨胀
/// </summary>
/// <param name="input">输入图像</param>
/// <param name="output">输出图像</param>
/// <param name="kernelSize">核矩阵尺寸</param>
__kernel void dilate_3d(__read_only image3d_t input, __write_only image3d_t output, const int kernelSize)
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

	//取邻域最大值
	float maxVal = -1e10f;
	for (int dz = -radius; dz <= radius; dz++)
	{
		for (int dy = -radius; dy <= radius; dy++)
		{
			for (int dx = -radius; dx <= radius; dx++)
			{
				float4 value = read_imagef(input, (int4)(x + dx, y + dy, z + dz, 0));
				maxVal = fmax(maxVal, value.x);
			}
		}
	}

	write_imagef(output, position, (float4)(maxVal, 0, 0, 0));
}

/// <summary>
/// 图像逐体素减法
/// </summary>
/// <param name="imageA">输入图像A</param>
/// <param name="imageB">输入图像B</param>
/// <param name="output">输出图像（A-B）</param>
__kernel void subtract_3d(__read_only image3d_t imageA, __read_only image3d_t imageB, __write_only image3d_t output)
{
	int x = get_global_id(0);
	int y = get_global_id(1);
	int z = get_global_id(2);

	int width = get_image_width(imageA);
	int height = get_image_height(imageA);
	int depth = get_image_depth(imageA);
	if (x >= width || y >= height || z >= depth)
	{
		return;
	}

	int4 position = (int4)(x, y, z, 0);
	float4 valueA = read_imagef(imageA, position);
	float4 valueB = read_imagef(imageB, position);

	write_imagef(output, position, valueA - valueB);
}
