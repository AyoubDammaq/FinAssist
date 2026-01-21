using AuthService.Application.Mapping;
using AutoMapper;

namespace AuthService.UnitTests.TestUtils;

internal static class MapperFactory
{
    public static IMapper Create()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<UserMapper>();
        });

        config.AssertConfigurationIsValid();

        return config.CreateMapper();
    }
}
