using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using Npgsql;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace lambda_CSharp_postgress
{
    public class Function
    {
        //
        // パラメータ指定がないときのデフォルト接続先（テスト用）
        private NpgsqlConnection getConnection(ILambdaContext context)
        {
            var host = Environment.GetEnvironmentVariable("DB_HOST") as string;
            var user = Environment.GetEnvironmentVariable("DB_USER") as string;
            var pass = Environment.GetEnvironmentVariable("DB_PASS") as string;
            var name = Environment.GetEnvironmentVariable("DB_NAME") as string;

            return getConnection(context,
                                 host ?? "aurora-postgis.cluster-csyoj4ibnfdl.ap-northeast-1.rds.amazonaws.com",
                                 user ?? "mic",
                                 pass ?? "mic123",
                                 name ?? "predora");
        }

        // 本当はいろいろなDBアクセスを可能にしてもいいけれど (ADO.net
        // PostGIS 使うので Postgres限定で
        private NpgsqlConnection getConnection(ILambdaContext context, string host, string user, string pass, string db)
        {
            context.Logger.LogLine($"Host={host};Username={user};Password={pass};Database={db};");
            return new NpgsqlConnection($"Host={host};Username={user};Password={pass};Database={db};");
        }

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public string FunctionHandler(object input, ILambdaContext context)
        {
            var retMsg = $"OK []";
            context.Logger.LogLine($"Arg : [{input}]");
            try
            {
                using (var con = getConnection(context))
                {
                    con.Open();
                    var cmd = con.CreateCommand();
                    cmd.CommandText = $"INSERT INTO test01 (name, address, updt) VALUES ('yuka', '1', '{System.DateTime.Now.ToString("yyyyMMddhhmm-ss-fffffff")}')";
                    retMsg = $"OK insertCount=[{cmd.ExecuteNonQuery()}]";
                    con.Close();
                }
            }
            catch (Exception e)
            {
                retMsg = e.Message;
            }
            return retMsg; //input?.ToUpper();
        }
    }
}
