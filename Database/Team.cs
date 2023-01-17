namespace OrmBenchmark.Database;

public class Team
{
    public virtual int Id { get; set; }

    public virtual string Name { get; set; }

    public virtual DateTime Foundation { get; set; }

    public virtual ICollection<Player> Players { get; set; }

    public virtual int SportId { get; set; }

    public virtual Sport Sport { get; set; }
}