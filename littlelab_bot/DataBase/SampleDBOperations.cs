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
using System.Runtime.Remoting.Messaging;
using Telegram.Bot.Types;

namespace littlelab_bot.DataBase
{
    public class SampleDBOperations
    {
        public async Task CreateEmptySampleInDb(int reqId) 
        {
            var sampleId = await ReturnIdSampleAsync() + 1;

            using (var conn = new DataBaseConnection())
            {
                string sql = $"insert into sample(id, reqid) " +
                        $"values({sampleId}, {reqId})";

                await conn.Conn.ExecuteAsync(sql);
            }
        }
        public async Task UpdateSampleIndValuesInDbAsync(Sample sample) 
        {
            var arrValues = await ReturnPgArrayAsync(sample);

            var queryArgs = new
            {
                id = sample.Id,
            };
            using (var conn = new DataBaseConnection())
            {
                string sql = $@"update sample set valuesind = {arrValues} where id = @id";

                await conn.Conn.ExecuteAsync(sql, queryArgs);
            }
        }
        public async Task<string> ReturnPgArrayAsync(Sample sample)
        {
             var filteredArr = sample.ValuesInd.Where(x => x != null);

             var arrayJs = JsonConvert.SerializeObject(filteredArr);

             return arrayJs.Replace("[", "'{").Replace("]", "}'");
        }
        public async Task<int> ReturnIdSampleAsync()
        {
            int r = 0;

            using (var conn = new DataBaseConnection())
            {
                NpgsqlCommand sql = new NpgsqlCommand($"select max(id) from sample", conn.Conn);

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
        public async Task<string> ReturnMaxCreateTime(int sampleid) 
        {
            using (var conn = new DataBaseConnection())
            {
                NpgsqlCommand sql = new NpgsqlCommand($"select max(timecreate) from sample", conn.Conn);

                using (NpgsqlDataReader? readerMaxValue = await sql.ExecuteReaderAsync())
                {
                    while (await readerMaxValue.ReadAsync())
                    {
                        var a = readerMaxValue.GetValue(0);

                        return a.ToString();
                    }
                }
            }
            return null;
        }
        public async Task<string> ReturnLastSampleCode(int reqid, string time) 
        {
            using (var conn = new DataBaseConnection())
            {
                NpgsqlCommand sql = new NpgsqlCommand($"select code from sample where reqid = {reqid} and timecreate = '{time}'", conn.Conn);

                using (NpgsqlDataReader? readerMaxValue = await sql.ExecuteReaderAsync())
                {
                    while (await readerMaxValue.ReadAsync())
                    {
                        var a = readerMaxValue.GetValue(0);

                        return a.ToString();
                    }
                }
            }
            return null;
        }
        public async Task UpdateDateTimeInSample(int id, DateTime dt)
        {
            using (var conn = new DataBaseConnection())
            {
                string sql = $@"update sample set timecreate = '{dt}' where id = {id}";

                await conn.Conn.ExecuteAsync(sql);
            }
        }
        public async Task SampleAddInDBAsync(Request request, Sample sample, string arrTests)
        {
            var queryArgs = new
            {
                id = sample.Id,

                uid = sample.UserId,

                reqid = request.Id,

                code = sample.Code,

                namesind = arrTests,
        };
            using (var conn = new DataBaseConnection())
            {
                string sql = $@"insert into sample(id, userid, reqid, code, namesind) values(@id, @uid, @reqid, @code, '{arrTests}')";

                await conn.Conn.ExecuteAsync(sql, queryArgs);
            }
        }
    }
}
