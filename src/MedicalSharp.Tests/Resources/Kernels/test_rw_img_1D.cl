__kernel void halve_1d(__read_write image1d_t image)
{
    int x = get_global_id(0);
    int width = get_image_width(image);
    if (x >= width)
    {
        return;
    }

    //读取
    float4 value = read_imagef(image, x);

    //写入
    write_imagef(image, x, value * 0.5f);
}
