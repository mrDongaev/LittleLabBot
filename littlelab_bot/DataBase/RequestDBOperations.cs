using Dapper;
using littlelab_bot.MainEntities;
using littlelab_bot.DataIntegration;
using Newtonsoft.Json;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static littlelab_bot.Dictionaries.DictionariesInMemory;
using Telegram.Bot.Types;
using Telegram.Bot;
using static System.Net.Mime.MediaTypeNames;

namespace littlelab_bot.DataBase
{
    public class RequestDBOperations
    {
        readonly ITelegramBotClient _botClient;

        public RequestDBOperations() { }
        public RequestDBOperations(ITelegramBotClient botClient)
        {
            _botClient = botClient ?? throw new ArgumentNullException(paramName: nameof(_botClient));
        }
        public string MaxValueRequest { get; set; }
        public async Task<Request> ReadRequestFromDataBaseAsync(int id)
        {
            using (var conn = new DataBaseConnection())
            {
                string sql = $"select id, status, dtsend, materialname, marking, testsnames from request where id = @id";

                 return await conn.Conn.QueryFirstOrDefaultAsync<Request>(sql, new { id });
            }
        }
        public async Task AddInfoRequestAsync(string reqId, string text)
        {
            using (var conn = new DataBaseConnection())
            {
                string sql = $"insert into Inforequest(requestid, startinfo) " +
                        $"values({reqId}, '{text}')";

                await conn.Conn.ExecuteAsync(sql);
            }
        }
        public async Task<Request> GetInfoRequestAsync(Request request) 
        {
            var reqid = request?.Id;

            using (var conn = new DataBaseConnection())
            {
                string sql = $@"select startinfo from inforequest where requestid = @reqid";

                return request = await conn.Conn.QueryFirstOrDefaultAsync<Request>(sql, new { reqid });
            }
        }
        public async Task<Request> ReadRequestFromDataBaseAsync(string id)
        {
            var intId = int.Parse(id);

            using (var conn = new DataBaseConnection())
            {
                string sql = $"select id, status, dtsend, materialname, marking, testsnames from request where id = @intId";

                var req = await conn.Conn.QueryFirstOrDefaultAsync<Request>(sql, new { intId });

                return req;
            }
        }
        public async Task RequestAddTableAsync(LabUser labUser, Request request)
        {
            var arrayTests = await ReturnPgArrayAsync(request);

            var id = await ReturnIdRequestAsync() + 1;

            request.Id = id;

            using (var conn = new DataBaseConnection())
            {
                string sql = $"insert into request(id, status, marking ,dtsend, materialname, userid, testsnames) " +
                        $"values('{request.Id}','{request.StatusRequest}','{request.Marking}','{request.Dtsendrequest}'," +
                        $"'{request.Materialname}', '{labUser.Id}', '{arrayTests}');";

                await conn.Conn.ExecuteAsync(sql);
            }
        }
        public async Task RequestUpdateAsync(LabUser labUser, Request request) 
        {
            var arrayTests = await ReturnPgArrayAsync(request);

            var queryArgs = new
            {
                id = request.Id,

                dt = request.Dtsendrequest,

                mn = request.Materialname,

                uid = labUser.Id,

                mark = request.Marking,
            };
            using (var conn = new DataBaseConnection())
            {
                string sql = $@"update request set id = @id, status = '{request.StatusRequest}', dtsend = @dt, materialname = @mn,
             userid = @uid, marking = @mark, testsnames = '{arrayTests}' where id = @id ";

                await conn.Conn.ExecuteAsync(sql, queryArgs);
            }
        }
        public async Task UpdateStatusRequestAsync(int id,Request.ActivityRequest status) 
        {
            using (var conn = new DataBaseConnection())
            {
                string sql = $@"update request set status = '{status}' where id = {id}";

                await conn.Conn.ExecuteAsync(sql);
            }
        }
        public async Task UpdateRequestResultAsync(Sample sample) 
        {
            SampleDBOperations sampleDB = new();

            var arrayTestsVal = await sampleDB.ReturnPgArrayAsync(sample);

            var query = new
            {
                id = sample.ReqId,
            };

            using (var conn = new DataBaseConnection())
            {
                string sql = $@"update request set testsvalues = {arrayTestsVal} where id = @id";

                await conn.Conn.ExecuteAsync(sql , query);
            }
        }
        public async Task UpdateDateTimeInRequestAsync(int id, DateTime dt) 
        {
            using (var conn = new DataBaseConnection())
            {
                string sql = $@"update request set dtsend = '{dt}' where id = {id}";

                await conn.Conn.ExecuteAsync(sql);
            }
        }
        public async Task<int> ReturnIdRequestAsync() 
        {
            int r = 0;

            using (var conn = new DataBaseConnection())
            {
                NpgsqlCommand sql = new NpgsqlCommand($"select max(id) from request", conn.Conn);

                using (NpgsqlDataReader? readerMaxValue = await sql.ExecuteReaderAsync())
                {
                    while (await readerMaxValue.ReadAsync())
                    {
                        var a = readerMaxValue.GetValue(0);

                        int.TryParse(a.ToString(), out r);
                    }
                }
            }
            return r;
        }
        public async Task<string> ReturnPgArrayAsync(Request request) 
        {
            var filteredArr = request.Testsnames.Where(x => x != null);

            var arrayJs = JsonConvert.SerializeObject(filteredArr);

            return arrayJs.Replace("[", "{").Replace("]", "}");
        }
    }
}
