using Microsoft.EntityFrameworkCore;
using UserManagement.Models;

namespace UserManagement.Data
{
    public class UserManagementContext: DbContext
    {
        public UserManagementContext(DbContextOptions<UserManagementContext> options): base(options)
        {

        }
        public DbSet<User> Users { get; set; } = null;
    }
}
