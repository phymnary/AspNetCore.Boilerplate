using AspNetCore.Boilerplate.Domain;
using AspNetCore.Boilerplate.Domain.Auditing;

namespace AspNetCore.Boilerplate.Tests.Auditing.PropertyChanges;

public class TestingPropertyChangeAudit : PropertyChangeAudit, IEntity
{
    public int Id { get; set; }

    public object Key()
    {
        return Id;
    }
}
