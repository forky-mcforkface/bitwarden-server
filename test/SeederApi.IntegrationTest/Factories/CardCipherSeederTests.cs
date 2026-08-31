using System.Text.Json;
using Bit.Core.Vault.Enums;
using Bit.Core.Vault.Models.Data;
using Bit.RustSDK;
using Bit.Seeder.Factories;
using Bit.Seeder.Models;
using Xunit;

namespace Bit.SeederApi.IntegrationTest.Factories;

public class CardCipherSeederTests
{
    [Fact]
    public void Create_OrganizationCard_EncryptsWithOrgKeyAndCarriesOwnership()
    {
        var organizationId = Guid.NewGuid();
        var orgKey = RustSdkService.GenerateOrganizationKeys().Key;

        var cipher = CardCipherSeeder.Create(new CipherSeed
        {
            Type = CipherType.Card,
            Name = "Corporate Card",
            EncryptionKey = orgKey,
            OrganizationId = organizationId,
            UserId = null,
            Card = new CardViewDto
            {
                CardholderName = "Acme Corp",
                Brand = "Visa",
                Number = "4111111111111111",
                ExpMonth = "12",
                ExpYear = "2030",
                Code = "123"
            }
        });

        Assert.Equal(CipherType.Card, cipher.Type);
        Assert.Equal(organizationId, cipher.OrganizationId);
        Assert.Null(cipher.UserId);

        var cardData = JsonSerializer.Deserialize<CipherCardData>(cipher.Data!);
        Assert.NotNull(cardData);
        Assert.Equal("Acme Corp", RustSdkService.DecryptString(cardData.CardholderName, orgKey));
        Assert.Equal("Visa", RustSdkService.DecryptString(cardData.Brand, orgKey));
        Assert.Equal("4111111111111111", RustSdkService.DecryptString(cardData.Number, orgKey));
        Assert.Equal("12", RustSdkService.DecryptString(cardData.ExpMonth, orgKey));
        Assert.Equal("2030", RustSdkService.DecryptString(cardData.ExpYear, orgKey));
        Assert.Equal("123", RustSdkService.DecryptString(cardData.Code, orgKey));
    }
}
