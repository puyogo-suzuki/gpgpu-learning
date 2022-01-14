# 1. 実行環境の導入
.NETと便利さのためにIDEを導入します．
すでに導入しているならば，この章はスキップしてもらってもかまいません．

## 1.1. IDEの導入
Visual Studio(Windows)かVisual Studio Code(Windows, Linux, macOS)を用いて，導入する方法をここでは取り合げます．
Emacs(Windows, Linux, macOS), Rider(Windows, Linux, macOS), MonoDevelop(別名：Visual Studio for macOS)も使えますが，ここでは取り上げません．

### 1.1.1. Visual Studio
[https://docs.microsoft.com/ja-jp/visualstudio/install/install-visual-studio](https://docs.microsoft.com/ja-jp/visualstudio/install/install-visual-studio) にしたがって，導入します．  
バージョンは2022または2019，エディションはCommunity Editionで十分です． 
ワークロードは「.NETデスクトップ開発」(.NET desktop development) を選びます．  
**Visual Studioを導入したのならば，1.2節は飛ばしてください．**

### 1.1.2. Visual Studio Code

#### 1.1.2.1. Visual Studio Codeのインストール
いくつかの環境では複数の方法でインストールできます．

##### 1.1.2.1.1. Webからインストーラを入手する方法(Win/Lin/mac)
[https://code.visualstudio.com/Download](https://code.visualstudio.com/Download)からインストーラをダウンロードし，インストールします．  

##### 1.1.2.1.2. snapを用いる方法（Ubuntu）
この方法だと，日本語入力ができません．
```sh
sudo snap install code --classic
```
によって，簡単にインストールできます．
```sh
sudo snap alias code.code code
```
により，エイリアスを設定します．

##### 1.1.2.1.3. flatpack (flatpack導入済みのLinux)
この方法だと，日本語入力ができません．  
[https://flathub.org/apps/details/com.visualstudio.code](https://flathub.org/apps/details/com.visualstudio.code)からインストールできます．

##### 1.1.2.1.4. brew cask(macOS)
```sh
brew cask install visual-studio-code
```
によって，簡単にインストールできます．

##### 1.1.2.1.5. winget(winget導入済みのWindows)
```bat
winget install Microsoft.VisualStudioCode
```
によって，簡単にインストールできます．

#### 1.1.2.2. プラグインのインストール
[https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)からインストールできます．

## 1.2. .NET Coreの導入
.NET Coreは.NETのランタイムで，Windows, Linux, macOS, z/OS, BSDに対応していますが，ここでは最初の3つについて取り上げます．

### 1.2.1. Webからインストーラを入手する方法(Win/Lin/mac)
[https://dotnet.microsoft.com/download/dotnet](https://dotnet.microsoft.com/download/dotnet)からインストーラをダウンロードし，インストールします．  

### 1.2.2. snapを用いる方法（Ubuntu）
```sh
sudo snap install dotnet-sdk --classic
```
によって，簡単にインストールできます．
```sh
sudo snap alias dotnet-sdk.dotnet dotnet
```
により，エイリアスを設定します

## 1.3. プロジェクトを作ってみる
基本的にプロジェクトをこのリポジトリに用意していますが，簡単に作り方を説明します．

### 1.3.1 Visual Studio(Windows)
Visual Studioを起動させたら，「新しいプロジェクトの作成」を押します．  
そして，言語をC#でフィルタリングして，「コンソールアプリ」を選択して次へを押します．  
プロジェクト名と作成場所を選択して，次へを押し，フレームワークを「.NET 6.0」として，作成を押します．

つぎに，必要なライブラリであるILGPUを導入します．  
プロジェクトメニューの「NuGetパッケージの管理」を押して，現れた画面の「参照」タブを押して，検索バーで「ILGPU」と検索します．  
ILGPUが出ると思うので，それを選択し，右ペインから「インストール」ボタンを押します．  
「OK」，「同意する」を押して，導入します．

### 1.3.2. Visual Studio Code (Win/Lin/mac)
端末で作成したい場所にカレントディレクトリを移し，
```sh
mkdir プロジェクト名
cd プロジェクト名
dotnet new console
```
によってプロジェクトを作成します．

つぎに，必要なライブラリであるILGPUを導入します．  
```sh
dotnet nuget add ILGPU --version "1.0.0"
```
（バージョン1.0.0がリリースされれば，--version以降は不要です．）


そして，Visual Studio Codeで開き，デバッグのための準備をしましょう．
```sh
code .
```
でVisual Studio Codeを開くことができます． 
そして，「エクスプローラ」からProgram.csを開きます．  
そして，左の「実行とデバッグ」を開き，「Generate C# Assets for Build and Debug」を押します．

## 1.4. 実行とデバッグ
プログラム（Program.cs）を次のように書き換えてみましょう．
```csharp
int x = 0;
for(int i = 0; i < 2; ++i)
{
    Console.WriteLine(x);
    x++;
}
```

### 1.4.1. まずは実行してみる
Visual Studioでは，プロジェクト名が表示された再生ボタンを押します．  
Visual Studio Codeでは，左の「実行とデバッグ」から，再生ボタンを押します．  

実行結果は，  
Visual Studioでは，新しいコンソールホストが出現して表示されます．  
Visual Studio Codeでは，下のデバッグコンソールに表示されます．（黄色は気にしなくても良いです．）
```
0
1
```

と表示されれば成功です．

### 1.4.2. ブレークポイントを置く
ブレークポイントを置くとそこでプログラムの実行を止めることができます．

Visual Studio，Codeどちらも，行番号の左に表示される赤玉を押すことで，設置できます．

設置したら，実行してみましょう．  
実行して停止すると，変数の内容や，コールスタックを見ることができます．

Visual Studioでは，  
「ローカル」ウィンドウに変数の内容が表示されます．  
「呼び出し履歴」ウィンドウには実行されている場所が表示されます．
「ウォッチ 1」ウィンドウで，「項目をウォッチに追加する」に，「x」と入力してみましょう．  
変数xの内容が表示されます．また，「x+1」と入力すると，変数xに1足された内容が表示されるように，任意の式の値を表示することができます．  
一時停止が起きると，実行ボタンがあったところにいくつかのボタンが表示されます．
ステップほにゃららは，プログラムを1行ずつ実行することができます．
続行と停止はそれぞれ言葉の意味の通りです．

Visual Studio Codeでは，  
「変数」ウィンドウに変数の内容が表示されます．  
「コールスタック」ウィンドウには実行されている場所が表示されます．
「ウォッチ式」ウィンドウで，「+」ボタンを押して，「x」と入力してみましょう．  
変数xの内容が表示されます．また，「x+1」と入力すると，変数xに1足された内容が表示されるように，任意の式の値を表示することができます．  
一時停止が起きると，画面上部に小さなコマンドパレットが表示されます．
ステップほにゃららは，プログラムを1行ずつ実行することができます．
続行と停止はそれぞれ言葉の意味の通りです．


### 1.4.3. ホットリロード（旧称：エディット&コンティニュー）
一時停止中にソースコードを書き換えて，「続行」すると，その通りにプログラムを継続させることができます．

## 1.5. Gitリポジトリのプロジェクトを開く
Gitリポジトリのプロジェクトを開くには，
Visual Studioは，slnファイルをダブルクリックで開き，  
Visual Studio Codeは，chapほにゃららディレクトリをVisual Studio Codeで開きます．