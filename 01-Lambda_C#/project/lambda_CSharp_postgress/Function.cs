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
        private string logString(string msg)
        {
            //            var d = new System.DateTime(DateTime.Now.Ticks, DateTimeKind.Utc);
            //            d = d.AddHours(9);
            var d = DateTime.Now;
            return $"{d.ToString("yyyyMMdd-HHmm-ss.ffff")} :{msg}";
        }

        //
        // パラメータ指定がないときのデフォルト接続先（テスト用）
        private string createConnectionString(ILambdaContext context)
        {
            var host = Environment.GetEnvironmentVariable("DB_HOST") as string;
            var user = Environment.GetEnvironmentVariable("DB_USER") as string;
            var pass = Environment.GetEnvironmentVariable("DB_PASS") as string;
            var name = Environment.GetEnvironmentVariable("DB_NAME") as string;

            context.Logger.LogLine(logString($"Host={host};Username={user};Password={pass};Database={name};"));
            return $"Host={host};Username={user};Password={pass};Database={name};";
        }

        // 本当はいろいろなDBアクセスを可能にしてもいいけれど (ADO.net
        // PostGIS 使うので Postgres限定で
        private NpgsqlConnection getConnection(string connectionString)
        {
            return new NpgsqlConnection(connectionString);
        }

        private async Task<int> dbAccessNonQueryAsync(ILambdaContext context, string connection, string query)
        {
            try
            {
                //                context.Logger.LogLine(logString("Start"));
                using (var con = getConnection(connection))
                {
                    //                    context.Logger.LogLine(logString("connect"));
                    await con.OpenAsync();
                    var cmd = con.CreateCommand();
                    cmd.CommandText = query;
                    //                    context.Logger.LogLine(logString("Scalar"));
                    await cmd.ExecuteNonQueryAsync();
                    //                    context.Logger.LogLine(logString("Close"));
                    await con.CloseAsync();
                    await con.DisposeAsync();
                }
                //                context.Logger.LogLine(logString("End"));
            }
            catch (Exception e)
            {
                context.Logger.LogLine($"dbAccessAsync Error: [{e.Message }]");
            }
            return 1;
        }

        private async Task<int> dbAccessAsync(ILambdaContext context, string connection, string query)
        {
            try
            {
//                context.Logger.LogLine(logString("Start"));
                using (var con = getConnection(connection))
                {
//                    context.Logger.LogLine(logString("connect"));
                    await con.OpenAsync();
                    var cmd = con.CreateCommand();
                    cmd.CommandText = query;
//                    context.Logger.LogLine(logString("Scalar"));
                    await cmd.ExecuteScalarAsync();
//                    context.Logger.LogLine(logString("Close"));
                    await con.CloseAsync();
                    await con.DisposeAsync();
                }
//                context.Logger.LogLine(logString("End"));
            }
            catch (Exception e)
            {
                context.Logger.LogLine($"dbAccessAsync Error: [{e.Message }]");
                throw e;
            }
            return 1 ;
        }

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public string FunctionHandler(object input, ILambdaContext context)
        {
            var taskList = new List<Task<int>>();
            var retMsg = $"OK []";
            var connectionString = createConnectionString(context);
            //context.Logger.LogLine($"Arg : [{input}]");
            try
            {
                // 一回同期処理
                var task = dbAccessNonQueryAsync(context, connectionString, $"INSERT INTO test01 (name, address, updt) VALUES ('yuka', '1', '{System.DateTime.Now.ToString("yyyyMMddhhmm-ss-fffffff")}')");
                task.Wait();

                // 非同期アクセス作成
                context.Logger.LogLine(logString("Create Thread"));
                //Enumerable.Range(0, 100).ToList().ForEach(x =>
                for (var i = 0; i < 1000; i++)
                {
                    taskList.Add(dbAccessNonQueryAsync(context, connectionString, $"INSERT INTO test01 (name, address, updt) VALUES ('yuka', '1', '{System.DateTime.Now.ToString("yyyyMMddhhmm-ss-fffffff")}')"));
                }

                // 非同期を全部待つ
                context.Logger.LogLine(logString("WaitThread"));
                var newTask = Task.WhenAll(taskList.ToArray());
                newTask.Wait();

                context.Logger.LogLine(logString("...Done"));
            }
            catch (Exception e)
            {
                retMsg = e.Message;
            }
            return retMsg; //input?.ToUpper();
        }

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public string SelectHandler(object input, ILambdaContext context)
        {
            var taskList = new List<Task<int>>();
            var retMsg = $"OK []";
            var connectionString = createConnectionString(context);
            //context.Logger.LogLine($"Arg : [{input}]");
            try
            {
                // 一回同期処理
                var task = dbAccessAsync(context, connectionString, $"SELECT count(*) FROM test01");
                task.Wait();
                
                // 非同期アクセス作成
                context.Logger.LogLine(logString("Create Thread"));
                //Enumerable.Range(0, 100).ToList().ForEach(x =>
                for(var i = 0; i < 1000; i++)
                {
                    taskList.Add(dbAccessAsync(context, connectionString, $"SELECT count(*) FROM test01"));
                }
                
                // 非同期を全部待つ
                context.Logger.LogLine(logString("WaitThread"));
                var newTask = Task.WhenAll(taskList.ToArray());
                newTask.Wait();
                
                context.Logger.LogLine(logString("...Done"));
            }
            catch (Exception e)
            {
                retMsg = e.Message;
            }
            return retMsg; //input?.ToUpper();
        }
    }
}
