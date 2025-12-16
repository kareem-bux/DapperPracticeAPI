using DapperDemo.Data;
using DapperDemo.Models;
using Dapper;
using System.Data;
namespace DapperDemo.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly DapperContext _context;

        public CustomerRepository(DapperContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            var query = "SELECT * FROM Customers";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Customer>(query); // This requires the Dapper namespace
        }
        public async Task<IEnumerable<Customer>> GetActiveAsync(bool isActive)
        {
            var query = "SELECT * FROM Customers WHERE IsActive = @IsActive";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Customer>(query, new { IsActive = isActive });
        }

        //proc code
        public async Task<Customer> GetCustomerByIdAsync(int id)
        {
            using var connection = _context.CreateConnection();
            var customer = await connection.QueryFirstOrDefaultAsync<Customer>(
                "sp_GetCustomerById",
                new { Id = id },
                commandType: CommandType.StoredProcedure
            );
            return customer;
        }

        public async Task DeleteCustomerAsync(int id)
        {
            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(
                "sp_DeleteCustomer",
                new { Id = id },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            using var connection = _context.CreateConnection();
            var parameters = new
            {
                customer.Id,
                customer.Name,
                customer.Email,
                customer.IsActive
            };

            await connection.ExecuteAsync(
                "sp_UpdateCustomer",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<int> AddCustomerAsync(Customer customer)
        {
            using var connection = _context.CreateConnection();
            var parameters = new
            {
                customer.Name,
                customer.Email,
                customer.IsActive
            };
            // ExecuteScalar returns the OUTPUT INSERTED.Id
            int newId = await connection.ExecuteScalarAsync<int>(
                "sp_InsertCustomer",
                parameters,
                commandType: CommandType.StoredProcedure
            );
            return newId;
        }

    }
}
