using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using PortalTarefas.Web.Models;

namespace PortalTarefas.Web.Services
{
    public class TarefaDapperService
    {
        private readonly string _connectionString;

        public TarefaDapperService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        private IDbConnection CreateConnection() => new SqliteConnection(_connectionString);

        public async Task<IEnumerable<Tarefa>> ObterTodasAsync()
        {
            using var connection = CreateConnection();
            const string sql = "SELECT Id, Titulo, Descricao, Prioridade, Prazo, ConcurrencyToken FROM Tarefas";
            return await connection.QueryAsync<Tarefa>(sql);
        }
    }
}