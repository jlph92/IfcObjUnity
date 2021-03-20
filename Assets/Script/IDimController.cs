public interface IDimController
{
    void notify(string message, params object[] parameters);
}

public class DimController : CoreElement, IDimController
{
    public string ControllerID = System.Guid.NewGuid().ToString("D");

    public CoreApplication app { get; set; }

    public DimModel model { get; private set; }

    public DimView view { get; protected set; }

    public virtual void notify(string message, params object[] parameters) { }

    public static DimController CreateController(CoreApplication app)
    {
        return new DimController(app);
    }

    protected DimController(CoreApplication app)
    {
        this.app = app;
    }

    public virtual void Setup(DimModel model)
    {
        this.model = model;
    }
}
