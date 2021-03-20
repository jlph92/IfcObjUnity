public interface IDimModel
{

}

public class DimModel : CoreElement, IDimModel
{
    public CoreApplication app { get; set; }
    public DimController controller { get; protected set; }

    public DimModel(CoreApplication app, DimController controller)
    {
        this.app = app;
        this.controller = controller;
    }
}
