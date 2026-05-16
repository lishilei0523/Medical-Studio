__kernel void test_rgba8(read_write image2d_t image)
{
    int x = get_global_id(0);
    int y = get_global_id(1);

    int w = get_image_width(image);
    int h = get_image_height(image);
    if (x >= w || y >= h)
    {
        return;
    }

    //定位
    int2 position = (int2)(x, y);

    //读取
    float4 value = read_imagef(image, position);

    //写入
    write_imagef(image, position, value * 0.5f);
}
