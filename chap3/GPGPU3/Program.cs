using ILGPU;
using ILGPU.Runtime;

using SixLabors.ImageSharp;
using PixelFormats = SixLabors.ImageSharp.PixelFormats;

void S1ArrayFirstStep()
{
    using Context context = Context.CreateDefault();
    using Accelerator accelerator = context.Devices.First(acc => acc.AcceleratorType != AcceleratorType.CPU).CreateAccelerator(context);

    void twice(Index1D index, ArrayView<int> array)
    {
        array[index] *= 2;
    }
    var gpuKernel = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<int>>(twice);

    // 配列を作成して，
    int[] ary = new int[] {1, 2, 3, 4, 5, 6};
    // ary.Length分の場所をGPUに作って，
    using var gpuArray = accelerator.Allocate1D<int>(ary.Length);
    // GPUにコピーします．
    gpuArray.CopyFromCPU(ary);

    // 値を表示します．
    foreach(var a in ary)
    {
        Console.Write("{0}, ", a);
    }
    Console.WriteLine();
    // カーネルを実行します．このカーネルはすべての要素を2倍にするものです．
    gpuKernel((int)gpuArray.Length, gpuArray.View);

    // 実行が終わるのを待ちます．
    accelerator.Synchronize();

    // 結果を表示します．
    Console.WriteLine("------");
    foreach(var a in gpuArray.GetAsArray1D()) // GetAsArray1DメソッドでCPU側に新たに場所を作ってコピーします．
    {
        Console.Write("{0}, ", a);
    }
    Console.WriteLine();
    
    // 結果を表示します． やり方2
    gpuArray.CopyToCPU(ary); // 新たな場所を作らずに，aryにコピーします．
    Console.WriteLine("------");
    foreach(var a in ary)
    {
        Console.Write("{0}, ", a);
    }
    Console.WriteLine();
}

byte[,] LoadImageGray(string path){
    using var img = Image.Load<PixelFormats.L8>(path);
    byte[,] ret = new byte[img.Width, img.Height];
    for(int i = 0; i < img.Height; ++i)
    {
        var line = img.GetPixelRowSpan(i);
        for(int j = 0; j < img.Width; ++j)
            ret[j, i] = line[j].PackedValue;
    }
    return ret;
}

void SaveImageGray(string path, byte[,] v) {
    var img = new Image<PixelFormats.L8>(v.GetLength(0), v.GetLength(1));
    for(int i = 0; i < img.Height; ++i)
    {
        var line = img.GetPixelRowSpan(i);
        for(int j = 0; j < img.Width; ++j)
            line[j].PackedValue = v[j, i];
    }
    img.SaveAsJpeg(path);
}

void S2GrayImage()
{
    using Context context = Context.CreateDefault();
    using Accelerator accelerator = context.Devices.First(acc => acc.AcceleratorType != AcceleratorType.CPU).CreateAccelerator(context);

    // ネガポジ変換
    void negpos(Index2D index, ArrayView2D<byte, Stride2D.DenseX> array)
    {
        array[index] = (byte)(255 - array[index]);
    }
    // Index1Dではなく，Index2Dでコンパイルすることで，2次元配列向けのプログラムにコンパイルします．
    var gpuKernel = accelerator.LoadAutoGroupedStreamKernel<Index2D, ArrayView2D<byte, Stride2D.DenseX>>(negpos);

    // 私が用意した関数です．輝度をbyteの2次元配列として返します．
    var ary = LoadImageGray("mct.jpg");
    // 配列をGPU上に確保します．
    using var gpuArray = accelerator.Allocate2DDenseX<byte>(new Index2D(ary.GetLength(0), ary.GetLength(1)));
    gpuArray.CopyFromCPU(ary);

    // カーネルをがその分だけ並列実行します．
    gpuKernel(new Index2D(ary.GetLength(0), ary.GetLength(1)), gpuArray.View);

    accelerator.Synchronize();

    // 私が用意した関数です．
    SaveImageGray("mct-negpos.jpg", gpuArray.GetAsArray2D());
}


RGB[,] LoadImageColor(string path){
    using var img = Image.Load<PixelFormats.Rgb24>(path);
    RGB[,] ret = new RGB[img.Width, img.Height];
    for(int i = 0; i < img.Height; ++i)
    {
        var line = img.GetPixelRowSpan(i);
        for(int j = 0; j < img.Width; ++j)
            ret[j, i] = (line[j].R, line[j].G, line[j].B);
    }
    return ret;
}

void SaveImageColor(string path, RGB[,] v) {
    var img = new Image<PixelFormats.Rgb24>(v.GetLength(0), v.GetLength(1));
    for(int i = 0; i < img.Height; ++i)
    {
        var line = img.GetPixelRowSpan(i);
        for(int j = 0; j < img.Width; ++j) {
            line[j].R = v[j, i].R;
            line[j].G = v[j, i].G;
            line[j].B = v[j, i].B;
        }
    }
    img.SaveAsJpeg(path);
}

void S3ColorImage()
{
    using Context context = Context.CreateDefault();
    using Accelerator accelerator = context.Devices.First(acc => acc.AcceleratorType != AcceleratorType.CPU).CreateAccelerator(context);

    void negpos(Index2D index, ArrayView2D<RGB, Stride2D.DenseX> array)
    {
        array[index].R = (byte)(255 - array[index].R);
        array[index].G = (byte)(255 - array[index].G);
        array[index].B = (byte)(255 - array[index].B);
    }
    var gpuKernel = accelerator.LoadAutoGroupedStreamKernel<Index2D, ArrayView2D<RGB, Stride2D.DenseX>>(negpos);

    var ary = LoadImageColor("mct.jpg");
    using var gpuArray = accelerator.Allocate2DDenseX<RGB>(new Index2D(ary.GetLength(0), ary.GetLength(1)));
    gpuArray.CopyFromCPU(ary);

    gpuKernel(new Index2D(ary.GetLength(0), ary.GetLength(1)), gpuArray.View);

    accelerator.Synchronize();

    SaveImageColor("mct-negpos-color.jpg", gpuArray.GetAsArray2D());
}

void S4Projection()
{
    using Context context = Context.CreateDefault();
    using Accelerator accelerator = context.Devices.First(acc => acc.AcceleratorType != AcceleratorType.CPU).CreateAccelerator(context);

    Index2D normalize(Index2D index, Index2D max) =>
        new Index2D(index.X < 0 ? 0 : (index.X > max.X ? max.X - 1 : index.X), index.Y < 0 ? 0 : (index.Y > max.Y ? max.Y - 1 : index.Y));

    void project(Index2D index, ArrayView2D<RGB, Stride2D.DenseX> array, ArrayView2D<RGB, Stride2D.DenseX> array_out, Index2D center, Index2D max)
    {
        var diff = index - center;
        var angle = MathF.Atan2(diff.Y, diff.X);
        var far = MathF.Sqrt(diff.Y * diff.Y + diff.X * diff.X);
        var min_center = Math.Min(center.X, center.Y);
        var max_center = MathF.Sqrt(center.Y * center.Y + center.X * center.X);
        // var far_out = MathF.Log(far) / MathF.Log(min_center) * min_center;
        var far_out = far * far / (max_center * max_center) * max_center;
        var newIndex = new Index2D((int)(far_out * MathF.Cos(angle)) + center.X, (int)(far_out * MathF.Sin(angle)) + center.Y);
        array_out[index] = array[normalize(newIndex, max)];
    }
    var gpuKernel = accelerator.LoadAutoGroupedStreamKernel<Index2D, ArrayView2D<RGB, Stride2D.DenseX>, ArrayView2D<RGB, Stride2D.DenseX>, Index2D, Index2D>(project);

    var ary = LoadImageColor("mct.jpg");
    using var gpuArray = accelerator.Allocate2DDenseX<RGB>(new Index2D(ary.GetLength(0), ary.GetLength(1)));
    using var outArray = accelerator.Allocate2DDenseX<RGB>(new Index2D(ary.GetLength(0), ary.GetLength(1)));
    gpuArray.CopyFromCPU(ary);
    gpuKernel(new Index2D(ary.GetLength(0), ary.GetLength(1)), gpuArray.View, outArray.View, new Index2D(ary.GetLength(0) / 2, ary.GetLength(1) / 2), new Index2D(ary.GetLength(0), ary.GetLength(1)));

    accelerator.Synchronize();

    SaveImageColor("mct-proj.jpg", outArray.GetAsArray2D());
}

S1ArrayFirstStep();