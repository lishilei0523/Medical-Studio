__kernel void test_read_write(__read_only image3d_t inputImage, __write_only image3d_t outputImage)
{
	int x = get_global_id(0);
	int y = get_global_id(1);
	int z = get_global_id(2);

	int width = get_image_width(inputImage);
	int height = get_image_height(inputImage);
	int depth = get_image_depth(inputImage);
	if (x >= width || y >= height || z >= depth)
	{
		return;
	}

	//定位
	int4 position = (int4)(x, y, z, 0);

	//取值
	float4 value = read_imagef(inputImage, position);

	//计算
	float4 result = value * 0.5f;

	//写入
	write_imagef(outputImage, position, result);
}
