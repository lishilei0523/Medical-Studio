/// <summary>
/// 3×3 Scharr算子X方向固定权重（各向同性更好）
/// </summary>
/// <remarks>对应OpenCV的Cv2.Sobel(..., 1, 0, -1)，即SCHARR</remarks>
__constant float scharr_weights_x[27] = {
	-3,  0,  3,  -10,  0, 10,  -3,  0,  3,  //z = -1
	-10, 0, 10,  -20,  0, 20, -10,  0, 10,  //z =  0
	-3,  0,  3,  -10,  0, 10,  -3,  0,  3   //z = +1
};

/// <summary>
/// 3×3 Scharr算子Y方向固定权重（各向同性更好）
/// </summary>
/// <remarks>对应OpenCV的Cv2.Sobel(..., 0, 1, -1)，即SCHARR</remarks>
__constant float scharr_weights_y[27] = {
	-3, -10, -3,  0,  0,  0,  3, 10,  3,  //z = -1
	-10,-20,-10,  0,  0,  0, 10, 20, 10,  //z =  0
	-3, -10, -3,  0,  0,  0,  3, 10,  3   //z = +1
};

/// <summary>
/// 3×3 Scharr算子Y方向固定权重（各向同性更好）
/// </summary>
/// <remarks>对应OpenCV的Cv2.Sobel(..., 0, 1, -1)，即SCHARR</remarks>
__constant float scharr_weights_z[27] = {
	-3, -10, -3,  -10, -20, -10,  -3, -10, -3,  //z = -1
	 0,   0,  0,    0,   0,   0,   0,   0,  0,  //z =  0
	 3,  10,  3,   10,  20,  10,   3,  10,  3   //z = +1
};

/// <summary>
/// Scharr边缘检测3D
/// </summary>
/// <param name="input">输入图像</param>
/// <param name="output">输出图像</param>
/// <param name="alpha">X方向权重（0.0~1.0，默认0.5）</param>
/// <param name="beta">Y方向权重（0.0~1.0，默认0.5）</param>
/// <param name="gamma">Z方向权重（0.0~1.0，默认0.5）</param>
/// <param name="offset">偏移量（加到最终结果，默认0）</param>
__kernel void scharr_3d(__read_only image3d_t input, __write_only image3d_t output, const float alpha, const float beta, const float gamma, const float offset)
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

	//边界：保留原值
	if (x < 1 || x >= width - 1 ||
		y < 1 || y >= height - 1 ||
		z < 1 || z >= depth - 1)
	{
		float4 value = read_imagef(input, position);
		write_imagef(output, position, value);

		return;
	}

	//X方向梯度（对应Cv2.Sobel(..., 1, 0, -1) SCHARR）
	float gradientX = 0.0f;
	int indexX = 0;
	for (int dz = -1; dz <= 1; dz++)
	{
		for (int dy = -1; dy <= 1; dy++)
		{
			for (int dx = -1; dx <= 1; dx++, indexX++)
			{
				gradientX += read_imagef(input, (int4)(x + dx, y + dy, z + dz, 0)).x * scharr_weights_x[indexX];
			}
		}
	}

	//Y方向梯度（对应Cv2.Sobel(..., 0, 1, -1) SCHARR）
	float gradientY = 0.0f;
	int indexY = 0;
	for (int dz = -1; dz <= 1; dz++)
	{
		for (int dy = -1; dy <= 1; dy++)
		{
			for (int dx = -1; dx <= 1; dx++, indexY++)
			{
				gradientY += read_imagef(input, (int4)(x + dx, y + dy, z + dz, 0)).x * scharr_weights_y[indexY];
			}
		}
	}

	//Z方向梯度（对应Cv2.Sobel(..., 0, 1, -1) SCHARR）
	float gradientZ = 0.0f;
	int indexZ = 0;
	for (int dz = -1; dz <= 1; dz++)
	{
		for (int dy = -1; dy <= 1; dy++)
		{
			for (int dx = -1; dx <= 1; dx++, indexZ++)
			{
				gradientZ += read_imagef(input, (int4)(x + dx, y + dy, z + dz, 0)).x * scharr_weights_z[indexZ];
			}
		}
	}

	//取绝对值（对应Cv2.ConvertScaleAbs）
	gradientX = fabs(gradientX);
	gradientY = fabs(gradientY);
	gradientZ = fabs(gradientZ);

	//加权混合（对应Cv2.AddWeighted）
	float magnitude = alpha * gradientX + beta * gradientY + gamma * gradientZ + offset;

	write_imagef(output, position, (float4)(magnitude, 0, 0, 0));
}
