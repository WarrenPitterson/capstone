namespace BookWorm.Api.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Secret { get; set; } = "BookWormSuperSecretDevelopmentKeyMustBeAtLeast32BytesLong!";
    public string Issuer { get; set; } = "BookWorm";
    public string Audience { get; set; } = "BookWormUsers";
    public int ExpiryDays { get; set; } = 7;
}

public class PricingOptions
{
    public const string SectionName = "Pricing";
    public decimal TaxRate { get; set; } = 0.122m;
    public decimal FreeDeliveryThreshold { get; set; } = 500.00m;
    public decimal StandardDeliveryCharge { get; set; } = 50.00m;
    public int EstimatedDaysToDeliver { get; set; } = 3;
}
