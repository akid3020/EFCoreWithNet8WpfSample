# EFCore With .NET8 Wpf Sample

## このサンプルについて

* WPF (.NET 8) で Entity Framework Core (EF Core) 
* 在庫管理ツールのイメージで、画面から商品レコードの追加・編集・削除が可能
* 商品を管理する Product テーブルと、それに紐づく Category テーブルがある

## ブランチ

* main : MVVMでの実装 (CommunityToolkit.Mvvm)
* without_mvvm : コードビハインドでの実装

## ファイル構成

```
src/ORMapperSample/
├── Models/                        # データベースのテーブル設計
│   ├── Product.cs                # 商品テーブルの設計（商品名、価格、在庫数など）
│   └── Category.cs               # カテゴリテーブルの設計（電子機器、書籍、日用品など）
├── Data/
│   ├── AppDbContext.cs           # データベース全体の設計書
│   │                            # ・どのテーブルがあるか
│   │                            # ・テーブル同士の関係はどうなっているか
│   │                            # ・初期データは何を入れるか　を定義
│   └── DesignTimeDbContextFactory.cs # マイグレーション作成時にEF Coreが内部で使用
├── Migrations/                   # データベース変更履歴（自動生成）
│   ├── *_InitialCreate.cs        # 最初にテーブルを作成した記録
│   ├── *_AddCategoryTable.cs     # カテゴリテーブルを追加した記録
│   └── AppDbContextModelSnapshot.cs # 現在のデータベース構造の完全な記録
│                                # （EF Coreが変更を検出するための基準）
├── ViewModels/MainViewModel.cs   # 画面とデータベースをつなぐロジック
├── MainWindow.xaml/.cs           # アプリの画面
├── App.xaml.cs                   # アプリ起動時の初期設定
│                                # ・appsettings.jsonからDB接続設定を読込み
│                                # ・データベースに最新マイグレーションを自動適用
│                                # ・画面とデータベースの準備を完了してからアプリ開始
├── appsettings.json              # データベース接続先の設定
└── ORMapperSample.csproj         # 使用するライブラリの設定
```

## 新規テーブル作成時の流れ

1. Modelsフォルダに新しいエンティティクラスを作成
2. DbContextクラスに新しいDbSet<T>プロパティを追加
3. 必要に応じてOnModelCreatingメソッドでエンティティの設定を追加
4. Package Manager Consoleまたはターミナルで以下のコマンドを実行：
   ```
   Add-Migration [マイグレーション名]
   ```
   または
   ```
   dotnet ef migrations add [マイグレーション名]
   ```
5. マイグレーションファイルの内容を確認
6. データベースに適用：
   ```
   Update-Database
   ```
   または
   ```
   dotnet ef database update
   ```

## テーブル構造変更時の手順

1. 該当するエンティティクラスのプロパティを変更（追加、削除、型変更など）
2. 必要に応じてDbContextのOnModelCreatingメソッドを更新
3. 新しいマイグレーションを作成：
   ```
   Add-Migration [変更内容を表すマイグレーション名]
   ```
   または
   ```
   dotnet ef migrations add [変更内容を表すマイグレーション名]
   ```
4. 生成されたマイグレーションファイルを確認し、意図した変更になっているか検証
5. データベースに適用：
   ```
   Update-Database
   ```
   または
   ```
   dotnet ef database update
   ```

## ひとつ前のマイグレーションを取り消したい場合

1. まだデータベースに適用していない場合：
   ```
   Remove-Migration
   ```
   または
   ```
   dotnet ef migrations remove
   ```

2. 既にデータベースに適用済みの場合：
   - まず、ひとつ前のマイグレーションまで戻す：
     ```
     Update-Database [ひとつ前のマイグレーション名]
     ```
     または
     ```
     dotnet ef database update [ひとつ前のマイグレーション名]
     ```
   - その後、最新のマイグレーションファイルを削除：
     ```
     Remove-Migration
     ```
     または
     ```
     dotnet ef migrations remove
     ```

## 今までのマイグレーション履歴をリセットし、現在のテーブル構造を初期マイグレーションにする場合

1. データベースのバックアップを取得（重要）
2. Migrationsフォルダ内のすべてのファイルを削除
3. データベース内の__EFMigrationsHistoryテーブルを削除またはクリア
4. 新しい初期マイグレーションを作成：
   ```
   Add-Migration InitialCreate
   ```
   または
   ```
   dotnet ef migrations add InitialCreate
   ```
5. 既存のデータベースに対してマイグレーションを適用する場合は、-IgnoreChangesオプションを使用：
   ```
   Update-Database -IgnoreChanges
   ```
   注意：このオプションが使用できない場合は、生成されたマイグレーションのUpメソッドの内容を手動で空にしてから適用