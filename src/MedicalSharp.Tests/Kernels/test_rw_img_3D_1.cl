__kernel void test_read_write(read_write image3d_t image)
{
    int x = get_global_id(0);
    int y = get_global_id(1);
    int z = get_global_id(2);

    int width = get_image_width(image);
    int height = get_image_height(image);
    int depth = get_image_depth(image);
    if (x >= width || y >= height || z >= depth)
    {
	    return;
    }

    //定位
    int4 posision = (int4)(x, y, z, 0);

    //读取
    float4 value = read_imagef(image, posision);

    //写入
    write_imagef(image, posision, value * 0.5f);
}
