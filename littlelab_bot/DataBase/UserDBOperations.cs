using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Microsoft.Data.SqlClient;
using static littlelab_bot.Dictionaries.DictionariesInMemory;
using static littlelab_bot.BagWithAnimations.StaticPhrase;
using littlelab_bot.MainEntities;
using Npgsql;
using Dapper;
using System.IdentityModel.Metadata;
using Telegram.Bot;

namespace littlelab_bot.DataBase
{
    /// <summary>
    /// Отдельный класс для того чтобы работать с выгрузками и вставками в таблице пользователей
    /// </summary>
    public class UserDBOperations
    {
        readonly ITelegramBotClient _botClient;

        readonly Update _update;

        public UserDBOperations(ITelegramBotClient botClient, Update update)
        {
            //Проводим валидацию при инициализации полей, чтобы исключить возможный null
            _update = update ?? throw new ArgumentNullException(paramName: nameof(update));

            _botClient = botClient ?? throw new ArgumentNullException(paramName: nameof(_botClient));
        }
        public async Task<LabUser> ReadUserFromDataBaseAsync(string tgid) 
        {
            using (var conn = new DataBaseConnection())
            {
                string sql = $"select id, fullname, phone, role, tgname, tgid from labuser where tgid = @tgid";

                var user = await conn.Conn.QueryFirstOrDefaultAsync<LabUser>(sql, new { tgid });

                return user;
            }
        }
        public async Task<LabUser> ReadUserFromDataBaseAsync(int id)
        {
            using (var conn = new DataBaseConnection())
            {
                string sql = $"select id, fullname, phone, role, tgname, tgid from labuser where id = @id";

                var user = await conn.Conn.QueryFirstOrDefaultAsync<LabUser>(sql, new { id });

                return user;
            }
        }
        public static async Task UserAddTableAsync(LabUser labUser) 
        {
            using (var conn = new DataBaseConnection())
            {
                string sql = $"insert into labuser(fullname, phone, role, tgname, tgid) " +
                        $"values('{labUser.Fullname}', '{labUser.Phone}', '{labUser.Role}', '{labUser.Tgname}'," +
                        $" '{labUser.Tgid}');";

                conn.Conn.Execute(sql);
            }
        }
        public static async Task UpdateUserInitialsAsync(LabUser labUser)
        {
            var queryArgs = new
            {
                id = labUser.Id,

                fullname = labUser.Fullname,

                tgname = labUser.Tgname,

                tgid = labUser.Tgid,
            };
            using (var conn = new DataBaseConnection())
            {
                string sql = $@"update labuser set fullname = @fullname,
             tgname = @tgname, tgid = @tgid where id = @id";

                await conn.Conn.ExecuteAsync(sql, queryArgs);
            }
        }
        public async Task UpdateUserRoleAsync(LabUser labUser) 
        {
            var queryArgs = new
            {
                id = labUser.Id,

                role = labUser.Role
            };
            using (var conn = new DataBaseConnection())
            {
                string sql = $@"update labuser set role = @role where id = @id";

                await conn.Conn.ExecuteAsync(sql, queryArgs);
            }
        }
        public static async Task<IEnumerable<LabUser>> UnloadUsersAsync(string pattern) 
        {
            using (var conn = new DataBaseConnection())
            {
                string sql = $@"select id, fullname, phone, role, tgname, tgid from labuser where role = @pattern";

                return await conn.Conn.QueryAsync<LabUser>(sql, new { pattern });
            }
        }
    }
}
