using ContactsWeb.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace ContactsWeb.Repositories;

public class ContactRepository : IContactRepository
{
    private readonly string _connectionString;
    private readonly ILogger<ContactRepository> _logger;

    public ContactRepository(string connectionString, ILogger<ContactRepository> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    public async Task InitializeDatabaseAsync()
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            var createTableCommand = """            
                    IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Contacts]') AND type in (N'U'))
                    BEGIN
                        CREATE TABLE [dbo].[Contacts](
                            [Id] [int] IDENTITY(1,1) NOT NULL,
                            [FirstName] [nvarchar](100) NOT NULL,
                            [LastName] [nvarchar](100) NOT NULL,
                            [Email] [nvarchar](255) NOT NULL,
                            [Phone] [nvarchar](50) NULL,
                            [Website] [nvarchar](500) NULL,
                            [AddressStreet] [nvarchar](255) NULL,
                            [AddressSuite] [nvarchar](100) NULL,
                            [AddressCity] [nvarchar](100) NULL,
                            [AddressZipCode] [nvarchar](50) NULL,
                            [AddressLatitude] [decimal](9, 6) NULL,
                            [AddressLongitude] [decimal](9, 6) NULL,
                            [CompanyName] [nvarchar](255) NULL,
                            [CompanyBs] [nvarchar](500) NULL,
                            [CompanyCatchPhrase] [nvarchar](500) NULL,
                            [CreatedAt] [datetime2](7) NOT NULL
                        CONSTRAINT [PK_Contacts] PRIMARY KEY CLUSTERED ([Id] ASC)
                        )
                    END
            """;

            await using (var tableCommand = new SqlCommand(createTableCommand, connection, (SqlTransaction)transaction))
            {
                await tableCommand.ExecuteNonQueryAsync();
            }

            var createIndexCommand = """
                    IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Contacts_Email' AND object_id = OBJECT_ID('Contacts'))
                    BEGIN
                        CREATE NONCLUSTERED INDEX [IX_Contacts_Email] ON [dbo].[Contacts] ([Email])
                    END
                    """;

            await using (var indexCommand = new SqlCommand(createIndexCommand, connection, (SqlTransaction)transaction))
            {
                await indexCommand.ExecuteNonQueryAsync();
            }

            var createViewCommand = """
                    CREATE OR ALTER VIEW BizEmailContacts AS
                    SELECT Id, FirstName, LastName, Email, Phone, Website, 
                           AddressStreet, AddressSuite, AddressCity, AddressZipCode, AddressLatitude, AddressLongitude, 
                           CompanyName, CompanyBs, CompanyCatchPhrase, CreatedAt
                    FROM Contacts
                    WHERE Email LIKE '%.biz';
                """;

            await using (var viewCommand = new SqlCommand(createViewCommand, connection, (SqlTransaction)transaction))
            {
                await viewCommand.ExecuteNonQueryAsync();
            }

            await transaction.CommitAsync();

            _logger.LogInformation("Database schema initilized successufully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing database schema");
            throw;
        }
    }

    public async Task<bool> CanInsertContactAsync(string email, CancellationToken token)
    {
        try
        {
            await using var connection = new SqlConnection(connectionString: _connectionString);

            await connection.OpenAsync(token);

            const string query = """
                SELECT CASE WHEN COUNT(*) = 0 THEN 1 ELSE 0 END
                FROM Contacts
                WHERE Email = @Email
                AND CreatedAt > DATEADD(MINUTE, -1, GETUTCDATE())
                """;

            await using var command = new SqlCommand(query, connection);
            command.Parameters.Add("@Email", SqlDbType.NVarChar, 255).Value = email;

            var result = await command.ExecuteScalarAsync(token);

            if (result is null || result == DBNull.Value)
            {
                _logger.LogWarning("Anti-spam check failed for {Email}", email);
                throw new InvalidOperationException("Anti-spam check failed.");
            }
            
            bool canInsert = Convert.ToBoolean(result);
            
            _logger.LogInformation("Anti-spam check for email {Email}: Can insert = {CanInsert}", email, canInsert);
            return canInsert;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Database error while checking anti-spam for email {Email}", email);
            throw;
        }
        catch (Exception ex) 
        {
            _logger.LogError(ex, "Unexpected error while checking anti-spam for email {Email}", email);
            throw;
        }
    }

    public async Task<int> InsertContactAsync(Contact contact, CancellationToken token)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(token);

            string query = """
                INSERT INTO Contacts
                (FirstName, LastName, Email, Phone, Website,
                AddressStreet, AddressSuite, AddressCity, AddressZipCode, AddressLatitude, AddressLongitude,
                CompanyName, CompanyBs, CompanyCatchPhrase, CreatedAt)
                OUTPUT INSERTED.Id
                VALUES
                (@FirstName, @LastName, @Email, @Phone, @Website,
                @AddressStreet, @AddressSuite, @AddressCity, @AddressZipCode, @AddressLatitude, @AddressLongitude,
                @CompanyName, @CompanyBs, @CompanyCatchPhrase, @CreatedAt);
                """;

            await using var command = new SqlCommand(query, connection);

            command.Parameters.AddParam("@FirstName", SqlDbType.NVarChar, contact.FirstName, 100);
            command.Parameters.AddParam("@LastName", SqlDbType.NVarChar, contact.LastName, 100);
            command.Parameters.AddParam("@Email", SqlDbType.NVarChar, contact.Email, 255);
            command.Parameters.AddParam("@Phone", SqlDbType.NVarChar, contact.Phone, 50);
            command.Parameters.AddParam("@Website", SqlDbType.NVarChar, contact.Website, 500);
            command.Parameters.AddParam("@AddressStreet", SqlDbType.NVarChar, contact.AddressStreet, 255);
            command.Parameters.AddParam("@AddressSuite", SqlDbType.NVarChar, contact.AddressSuite, 100);
            command.Parameters.AddParam("@AddressCity", SqlDbType.NVarChar, contact.AddressCity, 100);
            command.Parameters.AddParam("@AddressZipCode", SqlDbType.NVarChar, contact.AddressZipCode, 50);
            command.Parameters.AddParam("@AddressLatitude", SqlDbType.Decimal, contact.AddressLatitude, precision: 9, scale: 6);
            command.Parameters.AddParam("@AddressLongitude", SqlDbType.Decimal, contact.AddressLongitude, precision: 9, scale: 6);
            command.Parameters.AddParam("@CompanyName", SqlDbType.NVarChar, contact.CompanyName, 255);
            command.Parameters.AddParam("@CompanyBs", SqlDbType.NVarChar, contact.CompanyBs, 500);
            command.Parameters.AddParam("@CompanyCatchPhrase", SqlDbType.NVarChar, contact.CompanyCatchPhrase, 500);
            command.Parameters.AddParam("@CreatedAt", SqlDbType.DateTime2, contact.CreatedAt);

            var result = await command.ExecuteScalarAsync(token);

            if (result is null || result == DBNull.Value)
            {
                _logger.LogWarning("Insert failed for email {Email}", contact.Email);
                throw new InvalidOperationException("Insert failed.");
            }

            var contactId = Convert.ToInt32(result);
            
            _logger.LogInformation("Contact inserted successfully with ID {ContactId} for email {Email}", contactId, contact.Email);
            return contactId;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Database error while inserting contact {Email}", contact.Email);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while inserted contact {email}", contact.Email);
            throw;
        }
    }

    public async Task<IEnumerable<Contact>> GetBizEmailContactsAsync(CancellationToken token)
    {
        try
        {
            await using var connection = new SqlConnection(_connectionString);

            await connection.OpenAsync(token);

            string query = "SELECT * FROM BizEmailContacts ORDER BY CreatedAt DESC";

            await using var command = new SqlCommand(query, connection);

            List<Contact> contacts = [];

            await using var reader = await command.ExecuteReaderAsync(token);

            while (await reader.ReadAsync(token))
            {
                contacts.Add(MapReaderToContact(reader));
            }

            _logger.LogInformation("Retrieved {Count} contacts with .biz email addresses", contacts.Count);
            return contacts;
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Database error while retrieving .biz email addresses");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while retrieving .biz email addresses");
            throw;
        }
    }

    private static Contact MapReaderToContact(SqlDataReader reader)
    {
        return new Contact
        {
            Id = reader.GetInt32("Id"),
            FirstName = reader.GetString("FirstName"),
            LastName = reader.GetString("LastName"),
            Email = reader.GetString("Email"),
            Phone = reader.IsDBNull("Phone") ? null : reader.GetString("Phone"),
            Website = reader.IsDBNull("Website") ? null : reader.GetString("Website"),
            AddressStreet = reader.IsDBNull("AddressStreet") ? null : reader.GetString("AddressStreet"),
            AddressSuite = reader.IsDBNull("AddressSuite") ? null : reader.GetString("AddressSuite"),
            AddressCity = reader.IsDBNull("AddressCity") ? null : reader.GetString("AddressCity"),
            AddressZipCode = reader.IsDBNull("AddressZipCode") ? null : reader.GetString("AddressZipCode"),
            AddressLatitude = reader.IsDBNull("AddressLatitude") ? null : reader.GetDecimal("AddressLatitude"),
            AddressLongitude = reader.IsDBNull("AddressLongitude") ? null : reader.GetDecimal("AddressLongitude"),
            CompanyName = reader.IsDBNull("CompanyName") ? null : reader.GetString("CompanyName"),
            CompanyBs = reader.IsDBNull("CompanyBs") ? null : reader.GetString("CompanyBs"),
            CompanyCatchPhrase = reader.IsDBNull("CompanyCatchPhrase") ? null : reader.GetString("CompanyCatchPhrase"),
            CreatedAt = reader.GetDateTime("CreatedAt")
        };
    }
}
