using CitizenFX.Core;
using ELS.configuration;

namespace ELS.Light
{
    interface ILight
    {
        ELSVehicle ElsVehicle { get; set; }
        Scene Scene { get; set; }
        SpotLight SpotLight { get; set; }
        Vcfroot Vcfroot { get; set; }
    }
}
