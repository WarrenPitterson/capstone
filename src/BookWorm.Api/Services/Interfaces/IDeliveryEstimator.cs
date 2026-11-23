namespace BookWorm.Api.Services.Interfaces;

public interface IDeliveryEstimator
{
    DateTime EstimateDeliveryDate(DateTime? fromDate = null);
}
