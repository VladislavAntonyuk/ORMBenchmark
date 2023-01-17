namespace OrmBenchmark.Database;

public class Sport
{
    public virtual int Id { get; set; }

    public virtual string Name { get; set; }

    public virtual ICollection<Team> Teams { get; set; }

    public virtual int OlympiadId { get; set; }

    public virtual Olympiad Olympiad { get; set; }
}