namespace OrmBenchmark.Database;

public class Olympiad
{
    public virtual int Id { get; set; }

    public virtual string City { get; set; }

    public virtual DateTime Start { get; set; }

    public virtual DateTime End { get; set; }

    public virtual ICollection<Sport> Sports { get; set; }
}