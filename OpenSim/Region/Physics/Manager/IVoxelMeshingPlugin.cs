using Nini.Config;

namespace OpenSim.Region.Physics.Manager
{
    public interface IVoxelMeshingPlugin
    {
        string GetName();
        IVoxelMesher GetMesher(IConfigSource config);
    }
}
