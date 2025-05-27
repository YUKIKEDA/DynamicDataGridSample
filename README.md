# DynamicDataGridSample

## 概要

DynamicDataGridSampleは、WPF（.NET 8.0）向けの動的なDataGridサンプルアプリケーションです。  
カラム定義やデータモデルを柔軟に拡張でき、MVVMパターンに基づいた設計となっています。  
カスタムコントロール `DynamicDataGrid` を用いて、動的なカラム生成や選択機能、コマンドによるデータ処理が可能です。

## 主な機能

- 動的なカラム定義（モデルごとにカラムを自動生成）
- 行選択・全選択・選択解除
- 選択行の件数表示
- 選択行に対するコマンド実行
- CSVエクスポート機能
- MVVMパターン準拠
- 拡張性の高いデータモデル設計

## ディレクトリ構成

```
DynamicDataGridSample/
├── Controls/      # カスタムDataGridコントロール
├── Models/        # データモデル・カラム定義
├── Utilities/     # 補助クラス（RelayCommand等）
├── ViewModels/    # ビューモデル
├── MainWindow.xaml(.cs) # メイン画面
├── App.xaml(.cs)  # アプリケーションエントリ
```

## 主要コンポーネント

### DynamicDataGrid（Controls/DynamicDataGrid.cs）

- `ColumnsSource` プロパティでカラムを動的にバインド可能なカスタムDataGrid。
- コードビハインド不要で、ViewModelからカラム定義を制御。

### データモデル（Models/）

- `DynamicDataModel`：拡張性の高い抽象基底クラス。プロパティの動的追加・変更が可能。
- `SampleDataModel`：サンプル用データモデル。`GetColumnDefinitions()` でカラム定義を返す。
- `TableRowModel<T>`：行データ＋選択状態を管理。

### ビューモデル（ViewModels/TableViewModel.cs）

- 行データ・カラム定義・選択状態・コマンドを一元管理。
- 選択行数のカウントや、選択行への一括処理コマンドを提供。

## 動作環境

- .NET 8.0（Windowsデスクトップアプリ/WPF）
- Windows 10以降

## 使い方

1. ソリューションをVisual Studio等で開く
2. ビルド＆実行
3. メイン画面で動的なDataGridと選択機能を体験

## 使用例

### 1. カスタムデータモデルの作成

```csharp
public class CustomDataModel : DynamicDataModel
{
    public string Title
    {
        get => GetValue<string>(nameof(Title)) ?? string.Empty;
        set => SetValue(nameof(Title), value);
    }

    public int Count
    {
        get => GetValue<int>(nameof(Count));
        set => SetValue(nameof(Count), value);
    }

    public override IEnumerable<ColumnDefinition> GetColumnDefinitions()
    {
        yield return new ColumnDefinition 
        { 
            Header = "タイトル",
            PropertyPath = nameof(Title),
            PropertyType = typeof(string)
        };

        yield return new ColumnDefinition 
        { 
            Header = "カウント",
            PropertyPath = nameof(Count),
            PropertyType = typeof(int)
        };
    }
}
```

### 2. ビューモデルの初期化

```csharp
// サンプルデータの作成
var sampleData = new List<TableRowModel<CustomDataModel>>
{
    new(new CustomDataModel { Title = "項目1", Count = 10 }),
    new(new CustomDataModel { Title = "項目2", Count = 20 }),
    new(new CustomDataModel { Title = "項目3", Count = 30 })
};

// ビューモデルの初期化
var viewModel = new TableViewModel<CustomDataModel>(sampleData);

// 選択変更イベントの購読
viewModel.SelectionChanged += (sender, args) =>
{
    var selectedRow = args.Row;
    Console.WriteLine($"選択された行: {selectedRow.Data.Title}");
};

// 選択行の処理イベントの購読
viewModel.ProcessSelected += (sender, args) =>
{
    foreach (var item in args.SelectedItems)
    {
        Console.WriteLine($"処理対象: {item.Data.Title}");
    }
};
```

### 3. XAMLでの使用

```xaml
<Window x:Class="YourNamespace.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:controls="clr-namespace:DynamicDataGridSample.Controls"
        xmlns:vm="clr-namespace:YourNamespace.ViewModels"
        Title="MainWindow" Height="450" Width="800">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>

        <!-- 選択行数表示 -->
        <TextBlock Text="{Binding SelectedCount, StringFormat=選択されている項目: {0}件}"
                   Margin="5"/>

        <!-- DynamicDataGrid -->
        <controls:DynamicDataGrid
            Grid.Row="1"
            AutoGenerateColumns="False"
            CanUserAddRows="False"
            CanUserDeleteRows="False"
            CanUserResizeColumns="False"
            CanUserResizeRows="False"
            CanUserSortColumns="False"
            ColumnsSource="{Binding Columns}"
            ItemsSource="{Binding Rows}"
            SelectionMode="Single"
            SelectionUnit="Cell"/>
    </Grid>
</Window>
```

### 4. CSVエクスポート

```csharp
// ビューモデルからCSVデータを取得
string csvData = viewModel.GetCsvData();

// ファイルに保存
File.WriteAllText("export.csv", csvData);
```

## ライセンス

本プロジェクトはMITライセンスで公開されています。  
詳細は `LICENSE.txt` をご覧ください。