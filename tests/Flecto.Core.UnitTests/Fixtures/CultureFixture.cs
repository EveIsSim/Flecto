using System.Globalization;

namespace Flecto.Core.UnitTests.Fixtures;

public class CultureFixture
{
    public CultureFixture()
    {
        var culture = new CultureInfo("en-US");
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }
}
