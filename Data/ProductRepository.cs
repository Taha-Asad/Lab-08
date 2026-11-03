using PMS_CRUD_Razor.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace PMS_CRUD_Razor.Data
{
    public class ProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException(
                    "DefaultConnection is missing.");
        }

        public async Task<List<Product>> GetAllAsync()
        {
            var products = new List<Product>();

            const string sql = """
                SELECT Id, Title, [description], [image],
                       price, quantity, [status]
                FROM dbo.products
                ORDER BY Id DESC;
                """;

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);

            await connection.OpenAsync();

            await using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                products.Add(MapProduct(reader));
            }

            return products;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            const string sql = """
                SELECT Id, Title, [description], [image],
                       price, quantity, [status]
                FROM dbo.products
                WHERE Id = @Id;
                """;

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

            await connection.OpenAsync();

            await using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapProduct(reader);
            }

            return null;
        }

        public async Task<int> CreateAsync(Product product)
        {
            const string sql = """
                INSERT INTO dbo.products
                    (Title, [description], [image],
                     price, quantity, [status])
                OUTPUT INSERTED.Id
                VALUES
                    (@Title, @Description, @Image,
                     @Price, @Quantity, @Status);
                """;

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);

            AddProductParameters(command, product);

            await connection.OpenAsync();

            object? result = await command.ExecuteScalarAsync();

            return Convert.ToInt32(result);
        }

        public async Task<bool> UpdateAsync(Product product)
        {
            const string sql = """
                UPDATE dbo.products
                SET Title = @Title,
                    [description] = @Description,
                    [image] = @Image,
                    price = @Price,
                    quantity = @Quantity,
                    [status] = @Status
                WHERE Id = @Id;
                """;

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);

            AddProductParameters(command, product);
            command.Parameters.Add("@Id", SqlDbType.Int).Value = product.Id;

            await connection.OpenAsync();

            int rows = await command.ExecuteNonQueryAsync();

            return rows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = """
                DELETE FROM dbo.products
                WHERE Id = @Id;
                """;

            await using var connection = new SqlConnection(_connectionString);
            await using var command = new SqlCommand(sql, connection);

            command.Parameters.Add("@Id", SqlDbType.Int).Value = id;

            await connection.OpenAsync();

            int rows = await command.ExecuteNonQueryAsync();

            return rows > 0;
        }

        private static Product MapProduct(SqlDataReader reader)
        {
            int titleOrd = reader.GetOrdinal("Title");
            int descOrd = reader.GetOrdinal("description");
            int imgOrd = reader.GetOrdinal("image");
            int priceOrd = reader.GetOrdinal("price");
            int qtyOrd = reader.GetOrdinal("quantity");
            int statusOrd = reader.GetOrdinal("status");

            return new Product
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Title = reader.IsDBNull(titleOrd) ? string.Empty : reader.GetString(titleOrd),
                description = reader.IsDBNull(descOrd) ? string.Empty : reader.GetString(descOrd),
                image = reader.IsDBNull(imgOrd) ? string.Empty : reader.GetString(imgOrd),
                // Read as Decimal, then safely assign/convert based on your Product.price type:
                price = (int)(reader.IsDBNull(priceOrd) ? 0 : Convert.ToDecimal(reader.GetDecimal(priceOrd))),
                quantity = reader.IsDBNull(qtyOrd) ? 0 : reader.GetInt32(qtyOrd),
                status = reader.IsDBNull(statusOrd) ? "A" : reader.GetString(statusOrd)
            };
        }

        private static void AddProductParameters(SqlCommand command, Product product)
        {
            command.Parameters.Add("@Title", SqlDbType.VarChar, 100).Value = product.Title ?? string.Empty;

            command.Parameters.Add("@Description", SqlDbType.NVarChar, -1).Value =
                string.IsNullOrEmpty(product.description) ? DBNull.Value : product.description;

            command.Parameters.Add("@Image", SqlDbType.VarChar, 255).Value =
                string.IsNullOrEmpty(product.image) ? DBNull.Value : product.image;

            // Send as Decimal to match DECIMAL(18,2) in SQL Server
            var priceParam = command.Parameters.Add("@Price", SqlDbType.Decimal);
            priceParam.Precision = 18;
            priceParam.Scale = 2;
            priceParam.Value = product.price;

            command.Parameters.Add("@Quantity", SqlDbType.Int).Value = product.quantity;

            command.Parameters.Add("@Status", SqlDbType.VarChar, 10).Value =
                string.IsNullOrEmpty(product.status) ? "A" : product.status;
        }
    }
}