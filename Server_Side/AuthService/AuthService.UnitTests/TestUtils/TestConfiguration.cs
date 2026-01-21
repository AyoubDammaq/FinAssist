using Microsoft.Extensions.Configuration;

namespace AuthService.UnitTests.TestUtils;

internal static class TestConfiguration
{
    public static IConfiguration Jwt()
        => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "0123456789ABCDEF0123456789ABCDEF", 
                ["Jwt:Issuer"] = "AuthService.Tests",
                ["Jwt:Audience"] = "AuthService.Tests",
                ["Jwt:ExpireMinutes"] = "60"
            })
            .Build();
}