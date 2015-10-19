namespace Powerup.Model
{
    public interface IDatabaseObject
    {
        Database Database { get; }
        string Owner { get;  }
        string Name { get; }
        string QualifiedName { get; }
    }

    public interface IScriptable
    {
        string Script();
    }

    public interface IScriptableDatabaseObject :
        IDatabaseObject, IScriptable
    {
    }
}