public interface IObjectiveMechanic
{
    void Initialize(ObjectiveRuntime runtime);
    void Update();
    bool IsCompleted { get; }
}
