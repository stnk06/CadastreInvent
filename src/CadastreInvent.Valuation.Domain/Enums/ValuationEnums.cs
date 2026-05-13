namespace CadastreInvent.Valuation.Domain.Enums
{
    public enum ValuationMethod { Comparative, Cost, Income, AutomatedMachineLearning }
    public enum AppealStatus { Submitted, UnderReview, CourtDispute, Resolved, Rejected }
    public enum TransactionValidity { ValidMarket, InvalidAffiliated, InvalidForeclosure, InvalidOutlier }
}