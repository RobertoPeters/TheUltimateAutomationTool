using Tuat.Models;
using Tuat.Interfaces;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace Tuat.Repository;

public class DataRepository(IConfiguration _configuration) : IRepository
{
    private readonly string _connectionString = _configuration.GetConnectionString("tuat")!;

    const string ClientTableName = "Client";
    const string VariableTableName = "Variable";
    const string VariableValueTableName = "VariableValue";
    const string AutomationTableName = "Automation";
    const string LibraryTableName = "Library";
    const string AIProviderTableName = "AIProvider";
    const string AIConversationTableName = "AIConversation";

    public async Task SetupAsync()
    {
        SQLitePCL.Batteries.Init();

        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText =
@$"CREATE TABLE IF NOT EXISTS {ClientTableName}(Id INTEGER PRIMARY KEY AUTOINCREMENT, Data blob);
CREATE TABLE IF NOT EXISTS {VariableTableName}(Id INTEGER PRIMARY KEY AUTOINCREMENT, Data blob);
CREATE TABLE IF NOT EXISTS {AutomationTableName}(Id INTEGER PRIMARY KEY AUTOINCREMENT, Data blob);
CREATE TABLE IF NOT EXISTS {VariableValueTableName}(Id INTEGER PRIMARY KEY AUTOINCREMENT, Data blob);
CREATE TABLE IF NOT EXISTS {LibraryTableName}(Id INTEGER PRIMARY KEY AUTOINCREMENT, Data blob);
CREATE TABLE IF NOT EXISTS {AIProviderTableName}(Id INTEGER PRIMARY KEY AUTOINCREMENT, Data blob);
CREATE TABLE IF NOT EXISTS {AIConversationTableName}(Id INTEGER PRIMARY KEY AUTOINCREMENT, Data blob);
";
        await command.ExecuteNonQueryAsync();
        await command.DisposeAsync();
    }

    public async Task<List<Client>> GetClientsAsync()
    {
        return await GetItemsAsync<Client>(ClientTableName);
    }

    public async Task<List<Variable>> GetVariablesAsync()
    {
        return await GetItemsAsync<Variable>(VariableTableName);
    }

    public async Task<List<Automation>> GetAutomationsAsync()
    {
        return await GetItemsAsync<Automation>(AutomationTableName);
    }

    public async Task<List<Library>> GetLibrariesAsync()
    {
        return await GetItemsAsync<Library>(LibraryTableName);
    }

    public async Task<List<VariableValue>> GetVariableValuesAsync()
    {
        return await GetItemsAsync<VariableValue>(VariableValueTableName);
    }

    public async Task<List<AIProvider>> GetAIProvidersAsync()
    {
        return await GetItemsAsync<AIProvider>(AIProviderTableName);
    }

    public async Task<List<AIConversation>> GetAIConversationsAsync()
    {
        return await GetItemsAsync<AIConversation>(AIConversationTableName);
    }

    public async Task AddClientAsync(Client client)
    {
        await AddItemAsync(ClientTableName, client);
    }

    public async Task AddVariableAsync(Variable variable)
    {
        await AddItemAsync(VariableTableName, variable);
    }

    public async Task AddAutomationAsync(Automation automation)
    {
        await AddItemAsync(AutomationTableName, automation);
    }

    public async Task AddLibraryAsync(Library library)
    {
        await AddItemAsync(LibraryTableName, library);
    }

    public async Task AddVariableValueAsync(VariableValue variableValue)
    {
        await AddItemAsync(VariableValueTableName, variableValue);
    }

    public async Task AddAIProviderAsync(AIProvider aIProvider)
    {
        await AddItemAsync(AIProviderTableName, aIProvider);
    }

    public async Task AddAIConversationAsync(AIConversation aIConversation)
    {
        await AddItemAsync(AIConversationTableName, aIConversation);
    }

    public async Task UpdateClientAsync(Client client)
    {
        await UpdateItemAsync(ClientTableName, client);
    }

    public async Task UpdateVariableAsync(Variable variable)
    {
        await UpdateItemAsync(VariableTableName, variable);
    }

    public async Task UpdateAutomationAsync(Automation automation)
    {
        await UpdateItemAsync(AutomationTableName, automation);
    }

    public async Task UpdateLibraryAsync(Library library)
    {
        await UpdateItemAsync(LibraryTableName, library);
    }

    public async Task UpdateVariableValueAsync(VariableValue variableValue)
    {
        await UpdateItemAsync(VariableValueTableName, variableValue);
    }

    public async Task UpdateAIProviderAsync(AIProvider aIProvider)
    {
        await UpdateItemAsync(AIProviderTableName, aIProvider);
    }

    public async Task UpdateAIConversationAsync(AIConversation aIConversation)
    {
        await UpdateItemAsync(AIConversationTableName, aIConversation);
    }

    public async Task DeleteClientAsync(Client client)
    {
        await DeleteItemAsync(ClientTableName, client);
    }

    public async Task DeleteVariableAsync(Variable variable)
    {
        await DeleteItemAsync(VariableTableName, variable);
    }

    public async Task DeleteAutomationAsync(Automation automation)
    {
        await DeleteItemAsync(AutomationTableName, automation);
    }

    public async Task DeleteLibraryAsync(Library library)
    {
        await DeleteItemAsync(LibraryTableName, library);
    }

    public async Task DeleteVariableValueAsync(VariableValue variableValue)
    {
        await DeleteItemAsync(VariableValueTableName, variableValue);
    }

    public async Task DeleteVariablesAsync(List<int> ids)
    {
        await DeleteItemsAsync(VariableTableName, ids);
    }

    public async Task DeleteVariableValuesAsync(List<int> ids)
    {
        await DeleteItemsAsync(VariableValueTableName, ids);
    }

    public async Task DeleteAIProviderAsync(AIProvider aIProvider)
    {
        await DeleteItemAsync(AIProviderTableName, aIProvider);
    }

    public async Task DeleteAIConversationAsync(AIConversation aIConversation)
    {
        await DeleteItemAsync(AIConversationTableName, aIConversation);
    }

    private async Task<T> AddItemAsync<T>(string tableName, T item) where T : ModelBase
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = $"insert into {tableName}(Data) values(@data); SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("@data", item.ToData());
        item.Id = (int)(long)(await command.ExecuteScalarAsync())!;
        return item;
    }

    private async Task<T> UpdateItemAsync<T>(string tableName, T item) where T : ModelBase
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = $"update {tableName} set Data=@data where Id=@id";
        command.Parameters.AddWithValue("@data", item.ToData());
        command.Parameters.AddWithValue("@id", item.Id);
        await command.ExecuteNonQueryAsync()!;
        return item;
    }

    private async Task<T> DeleteItemAsync<T>(string tableName, T item) where T : ModelBase
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = $"delete from {tableName} where Id=@id";
        command.Parameters.AddWithValue("@id", item.Id);
        var count = await command.ExecuteNonQueryAsync()!;
        item.Id = -item.Id;
        return item;
    }

    private async Task DeleteItemsAsync(string tableName, List<int> ids)
    {
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = $"delete from {tableName} where Id in ({string.Join(", ", ids)})";
        await command.ExecuteNonQueryAsync()!;
    }

    private async Task<List<T>> GetItemsAsync<T>(string tableName) where T : ModelBase, new()
    {
        var dataBuffer = new byte[10_000_000];

        List<T> result = [];
        using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = $"select Id, Data from {tableName}";

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            T? record = null;
            var id = reader.GetInt32(0);
            var dataLength = reader.GetBytes(1, 0, dataBuffer, 0, dataBuffer.Length);
            if (dataLength > 0)
            {
                record = ModelBase.FromData<T>(id, dataBuffer, (int)dataLength);
            }
            else
            {
                record = new T();
            }
            record.Id = id;
            result.Add(record);
        }
        return result;
    }

}
