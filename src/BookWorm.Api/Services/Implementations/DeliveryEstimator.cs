using BookWorm.Api.Options;
using BookWorm.Api.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace BookWorm.Api.Services.Implementations;

public class DeliveryEstimator : IDeliveryEstimator
{
    private readonly PricingOptions _pricingOptions;

    public DeliveryEstimator(IOptions<PricingOptions> pricingOptions)
    {
        _pricingOptions = pricingOptions.Value;
    }

    public DateTime EstimateDeliveryDate(DateTime? fromDate = null)
    {
        var start = fromDate ?? DateTime.UtcNow;
        var date = start.Date;
        var daysAdded = 0;
        var targetDays = _pricingOptions.EstimatedDaysToDeliver > 0 ? _pricingOptions.EstimatedDaysToDeliver : 3;

        while (daysAdded < targetDays)
        {
            date = date.AddDays(1);
            if (date.DayOfWeek != DayOfWeek.Sunday)
            {
                daysAdded++;
            }
        }

        return date;
    }
}
