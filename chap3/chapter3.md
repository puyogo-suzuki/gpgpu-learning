# 3. 配列とはじめての画像処理
この章では，GPGPUでの配列の取り扱いと簡単な画像処理を行っていきます．

## 3.1. 配列を扱う(S1ArrayFirstStep)
まずは，配列を扱ってみましょう．  

```csharp
// 配列を作成して，
int[] ary = new int[] {1, 2, 3, 4, 5, 6};
// ary.Length分の場所をGPUに作って，
using var gpuArray = accelerator.Allocate1D<int>(ary.Length);
```
GPUとCPUは，使用しているメモリが異なります．
そのため，まずはGPUのメモリに配列の場所を確保してあげる必要があります．
そのために，`accelerator.Allocate1D`メソッドによって，1次元の配列の領域を確保します．
このGPU上にある配列は`MemoryBuffer<T>`型となります．

```csharp
// GPUにコピーします．
gpuArray.CopyFromCPU(ary);
```
そして，確保した領域にCPU上にある配列の値をコピーします．

```csharp
void twice(Index1D index, ArrayView<int> array)
{
    array[index] *= 2;
}
```
カーネルの引数は`ArrayView<T>`型で受け取ります．
この型は普通の配列のように`[]`によって要素にアクセスすることができます．

```csharp
var gpuKernel = accelerator.LoadAutoGroupedStreamKernel<Index1D, ArrayView<int>>(twice);
```
コンパイルの方も，引数を`ArrayView<int>`に変えたので，ジェネリクスの型引数をそのように変更します．

```csharp
gpuKernel((int)gpuArray.Length, gpuArray.View);
```
`MemoryBuffer1D<int>.View`に渡すべき，`ArrayView<T>`型の実態を表すオブジェクト（ポインタ？）が入っています．  
今回は，要素すべてを2倍にするので，実行数は配列の要素の数だけにしましょう．

そして，実行が終わるのを`Synchronize`メソッドを用いて待ってから，
```csharp
gpuArray.GetAsArray1D()
```
`GetAsArray1D`メソッドを用いて，CPUにコピーすることができます．
このメソッドを用いると，CPUのメモリ上に新たに領域を確保してからコピーします．

```csharp
gpuArray.CopyToCPU(ary)
```
または，`CopyToCPU`メソッドを用いて，すでにある領域にコピーすることができます．
何回もCPUとGPU間で値をコピーする場合，`GetAsArray1D`を用いると，たくさんメモリを消費してしまいます．
なので，何回もコピーするときは，めんどくさがらずに`CopyToCPU`を使いましょう．

## 3.2. グレースケール画像（S2GrayImage）
まずは簡単なグレースケール画像に対する画像処理をやりましょう．

```csharp
void negpos(Index2D index, ArrayView2D<byte, Stride2D.DenseX> array)
{
    array[index] = (byte)(255 - array[index]);
}
```

これはネガポジ反転を行うカーネルです．
最初の引数が`Index1D`ではなく，`Index2D`，配列も`ArrayView1D<T>`から，`ArrayView2D<byte, Stride2D.DenseX>`になっています．
画像は2次元配列なので，GPUのコアを2次元に並べたようにして実行させます．
そのため，最初の引数は，2次元の座標を表す`Index2D`型にしています．
そして，画像は2次元配列によって表現されるため，2次元の配列を表す型になっています．

```csharp
array[0, 3] = 0;
```
2次元配列はこのようにしてアクセスすることもできます．

```csharp
var ary = LoadImageGray("mct.jpg");
```
`LoadImageGray`関数は画像ファイルを読み込み，`byte`型の配列を返す関数です．

```csharp
using var gpuArray = accelerator.Allocate2DDenseX<byte>(new Index2D(ary.GetLength(0), ary.GetLength(1)));
```
横(`X`)は`GetLength(0)`で，縦(`Y`)は`GetLength(1)`で取得できます．

```csharp
gpuKernel(new Index2D(ary.GetLength(0), ary.GetLength(1)), gpuArray.View);
```
そうして，最初の引数に2次元の値を指定することで，2次元状にGPUコアを配置したようにして実行できます．

```csharp
SaveImageGray("mct-negpos.jpg", gpuArray.GetAsArray2D());
```
`SaveImageGray`関数で画像ファイルを保存します．

### 3.2.1. 演習
 1. 閾値100以上であれば，255，そうでなければ0を代入する二値化処理を実装しましょう．
 2. 閾値をカーネルの引数として自由に変更できるようにしましょう．
 3. ガンマ補正処理を実装しましょう．入力輝度をX，出力輝度をYとすると，Y＝255(X/255)^(1/γ)であらわされます．γはγ補正値で任意の値です．また，x^yは`MathF.Pow(x,y)`で計算できます．

## 3.3. カラー画像（S3ColorImage）
今度はカラー画像に対する画像処理をやりましょう．

```csharp
void negpos(Index2D index, ArrayView2D<RGB, Stride2D.DenseX> array)
{
    array[index].R = (byte)(255 - array[index].R);
    array[index].G = (byte)(255 - array[index].G);
    array[index].B = (byte)(255 - array[index].B);
}
```
`RGB`型はカラー画像の1画素を表す構造体で，`RGB.cs`に定義されています．
C#で普通に構造体を定義すれば，普通にGPUでも使えます．（ただし，`StructLayout`属性を利用した構造体など，一部の構造体は使えません．）

```csharp
var ary = LoadImageColor("mct.jpg");
SaveImageColor("mct-negpos-color.jpg", gpuArray.GetAsArray2D());
```
`LoadImageColor`関数，`SaveImageColor`関数でそれぞれカラー画像に対する処理ができます．

### 3.3.1. 演習
 1. 赤チャネルだけを抜き出す処理を実装しましょう．
 2. HSVに変換して，明るくしてからRGBに戻すことで，画像を明るくする処理を実装しましょう．（参考： [dobon.net: RGBをHSV(HSB)、HSL(HLS)、HSIに変換、復元する](https://dobon.net/vb/dotnet/graphics/hsv.html) ）
 3. 市松模様を重ねる処理を実装しましょう．

## 3.4. 射影（S4Projection）
今度は，射影をやってみましょう．
射影変換はいわゆるプロジェクタでいう台形補正ですが，意味を広げて，魚眼レンズのような画像処理なども入れてしまうことにします．（良い用語が思いつかないので）
この変換は非線形となります．

```csharp
using var gpuArray = accelerator.Allocate2DDenseX<RGB>(new Index2D(ary.GetLength(0), ary.GetLength(1)));
using var outArray = accelerator.Allocate2DDenseX<RGB>(new Index2D(ary.GetLength(0), ary.GetLength(1)));
```
読み書きの配列が同じだと結果がおかしくなってしまうため，分けることにしました．
カーネルも二つの配列を受け取るようにしています．

```csharp
var diff = index - center;
var angle = MathF.Atan2(diff.Y, diff.X);
var far = MathF.Sqrt(diff.Y * diff.Y + diff.X * diff.X);
var min_center = Math.Min(center.X, center.Y);
var max_center = MathF.Sqrt(center.Y * center.Y + center.X * center.X);
// var far_out = MathF.Log(far) / MathF.Log(min_center) * min_center;
var far_out = far * far / (max_center * max_center) * max_center;
var newIndex = new Index2D((int)(far_out * MathF.Cos(angle)) + center.X, (int)(far_out * MathF.Sin(angle)) + center.Y);
array_out[index] = array[normalize(newIndex, max)];
```
このカーネルは，画像の中央を大きく見せて周りを小さく見せるものです．
最後にあるように，array_outの担当の画素に別の画素を代入することで，座標変換したように見せることもできます．
しかしながら，出力結果が汚くなっているのがわかるでしょうか．
これは，新しい座標を計算するときに丸めたことが原因です．
そのため，DirectXやVulkanといったグラフィクスAPIでは，同じようなことをするときに，座標に浮動小数点数が用いられます．

### 3.4.1. 演習
 1. 上下反転する処理を実装しましょう．
 2. 画像の左上1/4を拡大する処理を実装しましょう．
 3. 拡大するときによりきれいにする処理を実装しましょう．（キーワード：バイリニア補正）