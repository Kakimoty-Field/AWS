<!-- code_chunk_output -->

- [はじめに](#はじめに)
- [今回の環境](#今回の環境)
- [環境構築編(仮想ハード)](#環境構築編仮想ハード)
  - [VPCとセキュリティグループ作成](#vpcとセキュリティグループ作成)
    - [VPCの設定とタグ](#vpcの設定とタグ)
    - [セキュリティグループの作成](#セキュリティグループの作成)
      - [作業EC2用セキュリティグループ作成](#作業ec2用セキュリティグループ作成)
      - [RDB用セキュリティグループ作成](#rdb用セキュリティグループ作成)
  - [EC2作成](#ec2作成)
    - [ステップ1:Amazon マシンイメージ (AMI)](#ステップ1amazon-マシンイメージ-ami)
    - [ステップ2:インスタンスタイプの選択](#ステップ2インスタンスタイプの選択)
    - [ステップ3:インスタンスの詳細の設定](#ステップ3インスタンスの詳細の設定)
    - [ステップ4:ストレージの追加](#ステップ4ストレージの追加)
    - [ステップ5:タグの追加](#ステップ5タグの追加)
    - [ステップ6:セキュリティグループの設定](#ステップ6セキュリティグループの設定)
    - [ステップ7:インスタンス作成の確認](#ステップ7インスタンス作成の確認)
      - [既存のキーペアを選択するか、新しいキーペアを作成します。](#既存のキーペアを選択するか-新しいキーペアを作成します)
  - [RDB作成](#rdb作成)
    - [Aurora Serverlessの作成](#aurora-serverlessの作成)
      - [データベースの作成とエンジンのオプション](#データベースの作成とエンジンのオプション)
      - [設定とキャパシティの設定](#設定とキャパシティの設定)
      - [接続](#接続)
        - [Virtual Private Cloud(VPC)](#virtual-private-cloudvpc)
        - [追加の接続設定](#追加の接続設定)
          - [サブネットグループ](#サブネットグループ)
          - [VPC セキュリティグループ](#vpc-セキュリティグループ)
          - [Data API](#data-api)
- [環境構築編(ソフトウェア)](#環境構築編ソフトウェア)
  - [EC2側セットアップ](#ec2側セットアップ)
      - [クライアントＰＣからターミナルでSSH](#クライアントpcからターミナルでssh)
      - [EC2 Instance Connect でSSH](#ec2-instance-connect-でssh)
    - [AWS コマンドリージョン設定](#aws-コマンドリージョン設定)
      - [アクセスキーの取得](#アクセスキーの取得)
    - [psql インストール](#psql-インストール)
  - [DB側セットアップ](#db側セットアップ)
    - [コンソールログイン](#コンソールログイン)
    - [ユーザ作成](#ユーザ作成)
    - [権限追加](#権限追加)
    - [ユーザ切り替え](#ユーザ切り替え)
    - [データベース作成](#データベース作成)
    - [データベース切り替え](#データベース切り替え)
    - [テーブル作成](#テーブル作成)
    - [動作確認](#動作確認)
      - [データ挿入](#データ挿入)
      - [データ取得](#データ取得)
- [DataAPI アクセス確認](#dataapi-アクセス確認)
  - [シークレット作成](#シークレット作成)
    - [シークレットの設定１](#シークレットの設定1)
    - [シークレットの設定２](#シークレットの設定2)
    - [シークレットの設定３](#シークレットの設定3)
    - [シークレットの設定４](#シークレットの設定4)
    - [シークレットを控える](#シークレットを控える)
  - [RDB ARN 取得](#rdb-arn-取得)
- [動作確認](#動作確認-1)
- [最後に](#最後に)

<!-- /code_chunk_output -->


# はじめに
このドキュメントは AWS RDS で作成した Serverless RDB に <br>
Data API を使用してアクセスするための環境構築と確認手順です。 <br>
この手順書では `PostgreSQL` 互換を使って環境構築します。

# 今回の環境
VPC(Virtual Private Cloud)の中に、検証対象となる RDB と作業用のEC2 を用意します。クライアントPCからはEC2を経由してRDSにアクセスするようにします。

![](https://github.com/Kakimoty-Field/AWS/raw/main/50-RDB/img/001.png)
# 環境構築編(仮想ハード)
## VPCとセキュリティグループ作成
VPC コンソールでVPC を作成します。また EC2 用と RDB 用のセキュリティグループも作成します。
### VPCの設定とタグ
![](https://github.com/Kakimoty-Field/AWS/raw/main/50-RDB/img/002.png)
- 名前タグ
  - 「任意の文字列」 
- IPv4 CIDR ブロック
  - 「任意の範囲 (例：172.31.0.0/16)」 
- IPv6 CIDR ブロック
  - 「IPv6 CIDR ブロックなし」
- テナンシー
  - 「デフォルト」
- タグ
  - 設定は任意

### セキュリティグループの作成
VPCコンソールメニューの「セキュリティグループ」からセキュリティグループを作成します。
#### 作業EC2用セキュリティグループ作成
![](https://github.com/Kakimoty-Field/AWS/raw/main/50-RDB/img/003.png)
- 基本的な詳細
  - セキュリティグループ名
    - 「EC2 用とわかる文字列」
  - 説明
    - 「任意の文字列」
  - VPC
    - 「前項で作成したＶＰＣ」
- インバウンドルール
  - タイプ
    - 「SSH」
  - ソース
    - 「カスタム (0.0.0.0)」
- アウトバウンドルール
  - 変更なし

#### RDB用セキュリティグループ作成
![](https://github.com/Kakimoty-Field/AWS/raw/main/50-RDB/img/004.png)
- 基本的な詳細
  - セキュリティグループ名
    - 「RDB 用とわかる文字列」
  - 説明
    - 「任意の文字列」
  - VPC
    - 「前項で作成したＶＰＣ」
- インバウンドルール
  - タイプ
    - 「PostgresSQL」
  - ソース
    - 「カスタム (EC2用のセキュリティグループを選択)」
- アウトバウンドルール
  - 変更なし  
## EC2作成
RDBを操作する作業用のEC2を、EC2 コンソールで作成します。
### ステップ1:Amazon マシンイメージ (AMI)
`Amazon Linux 2 AMI (HVM), SSD Volume Type` を選択し「次のステップ」ボタンをクリックします。
![](https://github.com/Kakimoty-Field/AWS/raw/main/50-RDB/img/900.png)
### ステップ2:インスタンスタイプの選択
無料利用枠の対象である `t2.micro`  を選択し「次のステップ」ボタンをクリックします。
![](https://github.com/Kakimoty-Field/AWS/raw/main/50-RDB/img/910.png)
### ステップ3:インスタンスの詳細の設定
デフォルトのまま「次のステップ」ボタンをクリックします。
### ステップ4:ストレージの追加
デフォルトのまま「次のステップ」ボタンをクリックします。
### ステップ5:タグの追加
デフォルトのまま「次のステップ」ボタンをクリックします。
### ステップ6:セキュリティグループの設定
下記を設定し、「次のステップ」ボタンをクリックします。
- セキュリティグループの割り当て
  - 「既存のセキュリティグループを選択する」
- セキュリティグループＩＤ
  - 「[作業EC2用セキュリティグループ作成](#作業EC2用セキュリティグループ作成)で作成したセキュリティグループ」
![](https://github.com/Kakimoty-Field/AWS/raw/main/50-RDB/img/920.png)
### ステップ7:インスタンス作成の確認
設定内容を確認したうえで、「起動」ボタンをクリックします。<br>
#### 既存のキーペアを選択するか、新しいキーペアを作成します。
`新しいキーペアの作成`を選択し、キーペア名に任意の文字列を入力します。「キーペアのダウンロード」ボタンが有効になったらクリックし、`*.pem` ファイルをダウンロードしておきます。

![](https://github.com/Kakimoty-Field/AWS/raw/main/50-RDB/img/930.png)
  
## RDB作成
### Aurora Serverlessの作成
[Amazon Aurora ユーザガイド](https://docs.aws.amazon.com/ja_jp/AmazonRDS/latest/AuroraUserGuide/CHAP_AuroraOverview.html)の、[Amazon Aurora Serverless を使用する](https://docs.aws.amazon.com/ja_jp/AmazonRDS/latest/AuroraUserGuide/aurora-serverless.html)を参考に `Amazon Aurora Serverless DBクラスター` を、RDSコンソールで作成します。

#### データベースの作成とエンジンのオプション
![](https://github.com/Kakimoty-Field/AWS/raw/main/50-RDB/img/010.png)
- データベースの作成方法
  - 「標準作成」 
- エンジンのオプション   
  - エンジンのタイプ
    - 「Amazon Aurora」
  - エディション
    - 「PostgreSQL との互換性を持つ Amazon Aurora」
  - キャパシティータイプ
    - `「サーバーレス」` 
  - バージョン
    - 「Aurora PostgreSQL (Compatible with PostgreSQL 10.12)」 



#### 設定とキャパシティの設定
![](https://github.com/Kakimoty-Field/AWS/raw/main/50-RDB/img/020.png)
- DB クラスター識別子
  - 「任意の文字列」
- 認証情報の設定
  - マスターユーザー名
    - 「任意の文字列」
  - マスターパスワード
    -  「自動生成」 or 「任意の文字列」
- キャパシティの設定
  - 最小 Aurora キャパシティーユニット
    - 「2 (4GB RAM)」
  - 最大 Aurora キャパシティーユニット
    - 「4 (4GB RAM)」



#### 接続
![](https://github.com/Kakimoty-Field/AWS/raw/main/50-RDB/img/040.png)
##### Virtual Private Cloud(VPC)
「[VPCとセキュリティグループ作成](#VPCとセキュリティグループ作成)で作成したＶＰＣ」
##### 追加の接続設定
###### サブネットグループ
「デフォルト」(そのまま)
###### VPC セキュリティグループ
「既存の選択」を選択し<br>
「[RDB用セキュリティグループ作成](#RDB用セキュリティグループ作成)で作成したセキュリティグループ」を選択後、`default` を削除します。(図を参照)
###### Data API
必ず`チェック`を入れる

すべての設定が完了したら「データベースの作成」ボタンをクリックします。

# 環境構築編(ソフトウェア)
## EC2側セットアップ
クライアントＰＣから直接ターミナルでSSH、もしくはブラウザベースのSSHコンソール(EC2 Instance Connect)を紹介します。
#### クライアントＰＣからターミナルでSSH
[既存のキーペアを選択するか、新しいキーペアを作成します。](#既存のキーペアを選択するか、新しいキーペアを作成します。)で保存したキーペアを使用してssh接続します。<br>
![](https://github.com/Kakimoty-Field/AWS/raw/main/50-RDB/img/120.png)
#### EC2 Instance Connect でSSH
AWS EC2 コンソールから[EC2作成](#EC2作成)で作成したインスタンスを選択し、「接続」ボタンをクリックして[インスタンスに接続]から`EC2 Instance Conect`でSSH接続します。
下図はからSSH接続する参考資料です。


![](https://github.com/Kakimoty-Field/AWS/raw/main/50-RDB/img/100.png)
![](https://github.com/Kakimoty-Field/AWS/raw/main/50-RDB/img/110.png)

### AWS コマンドリージョン設定
`CLI AWS` のリージョンを設定しておきます。
#### アクセスキーの取得
![](https://github.com/Kakimoty-Field/AWS/raw/main/50-RDB/img/500.png)

`Identity and Access Management(IAM)`コンソールで`アクセスキー`を作成して控えます。AWSコンソール画面右上のユーザ名をクリックして表示されるメニューから「マイセキュリティ資格情報」をクリックします。
`AWS IAM 認証情報` 内の「アクセスキーの作成」ボタンをクリックします。
![](https://github.com/Kakimoty-Field/AWS/raw/main/50-RDB/img/510.png)

表示された `アクセスキーID`と`シークレットアクセスキー`控えておきます。
![](https://github.com/Kakimoty-Field/AWS/raw/main/50-RDB/img/520.png)

接続しているＳＳＨターミナルで、以下のコマンドを入力します。
```
aws configure
```
４つの入力項目が表示されるので適切に入力します。
```
AWS Access Key ID [None]: 「アクセスキーＩＤ」
AWS Secret Access Key [None]: 「シークレットアクセスキー」
Default region name [None]: AWSコンソール右上のリージョンメニューをクリックして表示される「現在のリージョン」
Default output format [None]: json
```
![](https://github.com/Kakimoty-Field/AWS/raw/main/50-RDB/img/530.png)

### psql インストール
`yum`コマンドを使って、`PostgreSQL client programs`をインストールします。

```
sudo yum install postgresql
```
**Is this ok [y/d/N]** では、`y` を入力します。
```
Is this ok [y/d/N]: y
```
正常にインストールされたかどうかを確認します。

```
psql --version
```
バージョン情報が表示されれば成功です。
## DB側セットアップ
[psql インストール](#psql-インストール)でインストールしたコマンドを使用して、PostgreSQL にユーザ／データベース／テーブルを作成します。<br>
ログインするために[]()で作成したAuroraServerless のホスト名を確認します。<br>
RDS コンソールからインスタンスを選択し、[エンドポイント]をコピーします。
![](https://github.com/Kakimoty-Field/AWS/raw/main/50-RDB/img/200.png)
### コンソールログイン
エンドポイントが確認できたら、コンソールからログインします。
```
psql -h [エンドポイント] -U [マスターユーザ名]
```
この後にパスワードを聞かれるので、適切に入力します。
### ユーザ作成
PostgreSQL にログインした状態で作業用のユーザを作成します。
```
CREATE USER [ユーザ名] WITH password '[パスワード]';
```
### 権限追加
新しく追加したユーザに権限を追加します。
```
alter role [新しく作ったユーザ] CREATEDB;
GRANT RDS_SUPERUSER to [新しく作ったユーザ];
```
### ユーザ切り替え
新しく作ったユーザにログインしなおします。
```
\q - [新しく作ったユーザ];
```
### データベース作成
作業用の新しいデータベースを作成します。
```
CREATE DATABASE [新しいDB名];
```

### データベース切り替え
新しく作成したデータベースに切り替えます。
```
\q [新しく作ったDB];
```

### テーブル作成
作業用に新しいテーブルを作成します。
```
CREATE TABLE test01
(
  id bigserial,      -- 自動発番ＩＤ
  name varchar(10),  -- 名前
  addr varchar(10),  -- 住所？
  updt varchar(25)   -- 更新日
);
```
### 動作確認
正しくテーブルが作成できたかＳＱＬで確認します。
#### データ挿入
**SQL**
```
INSERT INTO test01 (name, addr, updt) VALUES ('yuka', 'nakano', '202011071425');
```
#### データ取得
**SQL**
```
SELECT * FROM test01;
```
**結果**
```
 id | name |  addr  |     updt
----+------+--------+--------------
  1 | yuka | nakano | 202011071425
(1 row)
```
# DataAPI アクセス確認
今回は `CLI AWS コマンド`を利用して DataAPI の動作確認をします。
## シークレット作成
[AWS Secrets Manager] で、`RDSデータベースの認証情報`のシークレットを作成します。<br>
[AWS Secrets Manager]コンソールで「新しいシークレットを保存する」ボタンをクリックします。
![](https://github.com/Kakimoty-Field/AWS/raw/main/50-RDB/img/300.png)
### シークレットの設定１
![](https://github.com/Kakimoty-Field/AWS/raw/main/50-RDB/img/310.png)
- シークレットの種類を選択
  - [RDSデータベースの認証情報]
- ユーザー名
  - [ユーザ作成](#ユーザ作成)で作成したＤＢユーザ名
- パスワード
  - [ユーザ作成](#ユーザ作成)で入力したパスワード
- このシークレットがアクセスするＲＤＳデータベースを選択してください
  - [DB側セットアップ](#DB側セットアップ)で作成したＤＢインスタンスを選択

「次」ボタンをクリックします。
### シークレットの設定２
![](https://github.com/Kakimoty-Field/AWS/raw/main/50-RDB/img/320.png)
- シークレットの名前
  - 「任意の文字列」
  
「次」ボタンをクリックします。
### シークレットの設定３
デフォルトのまま「次」ボタンをクリックします。
### シークレットの設定４
確認画面で内容を確認したうえで「保存」ボタンをクリックします。<br>
アプリケーションでシークレットを取得するサンプルコードが記載されているので、プログラミングする場合にはコピーしておくと便利です。
![](https://github.com/Kakimoty-Field/AWS/raw/main/50-RDB/img/330.png)
### シークレットを控える
作成されたシークレット一覧が表示されるので、今回作成したシークレットの名前をクリックして詳細を表示します。
![](https://github.com/Kakimoty-Field/AWS/raw/main/50-RDB/img/340.png)
`シークレットのARN`を控えておきます。

## RDB ARN 取得
[Amazon RDS]コンソールで [Aurora Serverlessの作成](#Aurora-Serverlessの作成) で作成したRDBを選択します。<br>
画面中部、「設定」タブをクリックし、このデータベースの `ARN` を控えます。
![](https://github.com/Kakimoty-Field/AWS/raw/main/50-RDB/img/350.png)

# 動作確認
SSH コンソールで下記のコマンドを入力します。SQL が発行され、結果が JSON 形式で表示されます。

**コマンド**
```
aws rds-data execute-statement \
   --resource-arn "[RDB ARN]" \
   --secret-arn "[AWS Secrets Manager で作成したシークレットのARN]" \
   --sql "SELECT * FROM test01"\
   --database "[作成したデータベース名]"
```
**結果**
```
{
    "records": [
        [
            {
                "longValue": 1
            },
            {
                "stringValue": "yuka"
            },
            {
                "stringValue": "nakano"
            },
            {
                "stringValue": "202011071425"
            }
        ]
    ],
    "numberOfRecordsUpdated": 0
}
```

ここまでで、DataAPI によるSQL実行の確認ができました。あとは好きなプログラミング言語でコーディングしていくことになります。

# 最後に
CLI で作業できるようになりたい。