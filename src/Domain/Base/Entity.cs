using UUIDNext;

namespace Domain.Base;

public class Entity
{
    public Guid Id { get; set; }

    public static Guid NewId()
    {
        return Uuid.NewSequential();
    }
}
