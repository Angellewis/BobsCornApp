using BobsCornApp.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BobsCornApp.UnitTests.Infrastructure;

[TestClass]
public class CornPurchaseRepositoryTests
{
    [TestMethod]
    public void ConstructorShouldCreateRepositoryWhenDefaultConnectionStringExists()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=(localdb)\\MSSQLLocalDB;Database=Test;"
            })
            .Build();

        var repository = new CornPurchaseRepository(configuration);

        Assert.IsTrue(
            repository is CornPurchaseRepository,
            "Expected the repository to be created when the default connection string is configured.");
    }

    [TestMethod]
    public void ConstructorShouldThrowWhenDefaultConnectionStringIsMissing()
    {
        var configuration = new ConfigurationBuilder().Build();
        InvalidOperationException? exception = null;

        try
        {
            _ = new CornPurchaseRepository(configuration);
        }
        catch (InvalidOperationException caught)
        {
            exception = caught;
        }

        Assert.IsTrue(
            exception?.Message == "Connection string 'DefaultConnection' was not found.",
            "Expected the repository constructor to fail when the default connection string is missing.");
    }
}
