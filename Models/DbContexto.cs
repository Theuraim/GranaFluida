using Microsoft.EntityFrameworkCore;

namespace GranaFluida.Models
{
    public class DbContexto : DbContext
    {
        public DbContexto(DbContextOptions<DbContexto> options) : base(options) { }

        public DbSet<Usuarios> Usuario { get; set; }

        public DbSet<Movimentacoes> Movimentacao { get; set; }

        public DbSet<Metas> Meta { get; set; }
    }
}
