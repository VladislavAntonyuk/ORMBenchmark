namespace OrmBenchmark.Database;

public class Player
{
    public virtual int Id { get; set; }

    public virtual string FirstName { get; set; }

    public virtual string LastName { get; set; }

    public virtual DateTime Birthday { get; set; }

    public virtual int TeamId { get; set; }

    public virtual Team Team { get; set; }
}