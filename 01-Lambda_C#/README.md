<!-- Table of Contents -->
- [はじめに](#はじめに)
- [開発・実行環境準備](#開発実行環境準備)
    - [VisualStudio プラグイン](#VisualStudio-プラグイン)
    - [プロジェクト作成](#プロジェクト作成)
- [開発とデプロイ](#開発とデプロイ)
    - [コード記述](#コード記述)
    - [デプロイ設定](#デプロイ設定)
        - [認証情報の準備](#認証情報の準備)
        - [デプロイ](#デプロイ)
- [テスト](#テスト)
    - [Visual Studio でテスト](#Visual-Studio-でテスト)
    - [AWS Console でテスト](#AWS-Console-でテスト)

<!-- Table of Contents -->
# はじめに

このドキュメントは AWS Lambda を C# で記述するための手順です。 <br>
コードエディットやパブリッシュには Visual Studio を使用します。

# 開発・実行環境準備
## VisualStudio プラグイン
開発者ガイドの[AWS Toolkit for Visual Studio](https://docs.aws.amazon.com/ja_jp/lambda/latest/dg/csharp-package-toolkit.html)を参考に、[Visual Studio の拡張機能](https://marketplace.visualstudio.com/items?itemName=AmazonWebServices.AWSToolkitforVisualStudio2017)をインストールします。

![](https://raw.githubusercontent.com/Kakimoty-Field/AWS-Lambda/main/01-Lambda_C%23/img/010.png)
## プロジェクト作成
Visual Studio の「ファイル」メニューから「新規作成」を選択し、「プロジェクト」を選択します。
![](https://raw.githubusercontent.com/Kakimoty-Field/AWS-Lambda/main/01-Lambda_C%23/img/100.png)

「新しいプロジェクト」ウインドウで [AWS Lambda] ツリーを選択し、 <br>
`「AWS Lambda Project(.NET Core - C#)」` を選択します。<br>
プロジェクト名と保存場所は適宜入力します。
![](https://raw.githubusercontent.com/Kakimoty-Field/AWS-Lambda/main/01-Lambda_C%23/img/110.png)

「New AWS Lambda C# Project」ウインドウでは、サンプルアプリケーションのコードを選択することができます。<br>
ここでは、最低限のコードが記述されている `[Empty Function]` を選択し、「Finish」ボタンをクリックします。 
![](https://raw.githubusercontent.com/Kakimoty-Field/AWS-Lambda/main/01-Lambda_C%23/img/120.png)

# 開発とデプロイ
## コード記述
このままでも動作するかと思いますが、念のため引数の型を変更します。 <br>
```C#
//
// 修正前(プロジェクト作成デフォルト状態)
//
    public string FunctionHandler(string input, ILambdaContext context)
    {
        // 引数で受け取った文字列を大文字にして返す
        return input?.ToUpper();
    }
```
```C#
//
// 修正後
//
    public string FunctionHandler(object input, ILambdaContext context)
    {
        // 受け取った引数をログに出力
        context.Logger.LogLine($"Arg : [{input}]");
        // ＯＫ という文字列を返す
        return "OK"; //input?.ToUpper();
    }
```

## デプロイ設定

### 認証情報の準備
Visual Studio から パブリッシュするための認証情報を AWS コンソールから取得します。

AWS コンソール右上のメニューから「ユーザ名▼」を選択し、「マイセキュリティ資格情報」を選択します
![](https://raw.githubusercontent.com/Kakimoty-Field/AWS-Lambda/main/01-Lambda_C%23/img/200.png)
[IAM Management Console] （セキュリティ認証情報）画面で、「アクセスキー（アクセスキー ＩＤとシークレットアクセスキー）」を開きます。<br>
「新しいアクセスキーの作成」ボタンをクリックしアクセスキーＩＤを発行します。
![](https://raw.githubusercontent.com/Kakimoty-Field/AWS-Lambda/main/01-Lambda_C%23/img/210.png)
[アクセスキーの作成]ダイアログが表示されたら、[アクセスキーID]と[シークレットアクセスキー]を控えておきます。
![](https://raw.githubusercontent.com/Kakimoty-Field/AWS-Lambda/main/01-Lambda_C%23/img/220.png)

### デプロイ
Visual Studio のソリューションエクスプローラで、プロジェクトを右クリックし「Publish to AWS Lambda...」を選択します。
![](https://raw.githubusercontent.com/Kakimoty-Field/AWS-Lambda/main/01-Lambda_C%23/img/300.png)

[Upload Lambda Function]ダイアログで、「Account profile to use:」のドロップダウン右にあるアイコンをクリックします。
![](https://raw.githubusercontent.com/Kakimoty-Field/AWS-Lambda/main/01-Lambda_C%23/img/310.png)

アップロードするためのユーザを追加する画面で以下の項目を入力します。

- Profile Name：任意
- Storage Location：Shared Credentials File
- Access Key ID：[認証情報の準備](#認証情報の準備)で控えたアクセスキーＩＤ
- Secret Access Key：[認証情報の準備](#認証情報の準備)で控えたシークレットアクセスキー

入力がおわったら「ＯＫ」ボタンをクリックします。
![](https://raw.githubusercontent.com/Kakimoty-Field/AWS-Lambda/main/01-Lambda_C%23/img/340.png)

再び[Upload Lambda Function]ダイアログで、「Function Name」に作成する Lambda関数の名前を入力します。
入力したら「Next」ボタンをクリックします。
![](https://raw.githubusercontent.com/Kakimoty-Field/AWS-Lambda/main/01-Lambda_C%23/img/320.png)

[Advanced Function Details]ダイアログでは、以下の項目を入力します。

- Role Name：「New role based on AWS managed policy: AWSLambdaExecute」
- Memory(MB)：「128」

入力がおわったら「Upload」ボタンをクリックします。
![](https://raw.githubusercontent.com/Kakimoty-Field/AWS-Lambda/main/01-Lambda_C%23/img/330.png)

これでデプロイは完了です。

# テスト
## Visual Studio でテスト
デプロイが完了すると、Visual Studio に、作成した Lambda 関数をテストするためのタブが表示されます。<br>
赤い枠のテキストエリアに入力された文字が、Lambda関数の引数として渡されます。<br>
「Example Requetes:」ドロップダウンでは、AWS で接続可能なサービスからの引数サンプルを選択することもできます。<br>
引数の設定が完了したら「Invoke」ボタンをクリックします。
![](https://raw.githubusercontent.com/Kakimoty-Field/AWS-Lambda/main/01-Lambda_C%23/img/400.png)

実行結果が、右側のペインに表示されます。また、タブ下部のペインには実行ログが表示されます。

## AWS Console でテスト
[AWS Console]から「Lambda」を選択すると、Visual Studio でデプロイした関数が表示されます。
![](https://raw.githubusercontent.com/Kakimoty-Field/AWS-Lambda/main/01-Lambda_C%23/img/410.png)
関数名をクリックし詳細画面を表示した後、右上の「テスト」ボタンをクリックします。
![](https://raw.githubusercontent.com/Kakimoty-Field/AWS-Lambda/main/01-Lambda_C%23/img/420.png)
[テストイベントの設定]ダイアログが表示されるので、以下の項目を入力します。

- 新しいテストイベントの作成を選択
- イベント名：保存するテストパータンの名称
- 下部テキストボックス：関数に引き渡すテストデータ

入力が終ったら「作成」ボタンをクリックします。
![](https://raw.githubusercontent.com/Kakimoty-Field/AWS-Lambda/main/01-Lambda_C%23/img/430.png)

再び「テスト」ボタンをクリックすると関数のテストが実行されます。<br>
テストの実行結果が画面に表示されるので「詳細」をクリックします。<br>
下記キャプチャの、上側の赤枠が関数の戻り値。`OK`を返していることがわかります。<br>
また、下側の赤枠には関数のログが出力されますが、テストデータとして渡した引数がログに出力されていることが確認できます。
![](https://raw.githubusercontent.com/Kakimoty-Field/AWS-Lambda/main/01-Lambda_C%23/img/440.png)

