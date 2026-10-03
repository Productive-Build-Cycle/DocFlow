using Microsoft.EntityFrameworkCore;

namespace DocFlow.Infrastructure.Persistence;

public class DocFlowDbContext : DbContext
{
    public DocFlowDbContext(DbContextOptions<DocFlowDbContext> options)
        : base(options)
    {
    }
}
