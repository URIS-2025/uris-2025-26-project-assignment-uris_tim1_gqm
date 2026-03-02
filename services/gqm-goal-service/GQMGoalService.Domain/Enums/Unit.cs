namespace GQMGoalService.Domain.Enums;

public enum Unit
{
    // =========================
    // Dimensionless
    // =========================
    None = 0,
    Percentage,
    Ratio,
    Index,
    Score,
    Rating,
    Grade,
    Multiplier,
    Points,
    Count,

    // =========================
    // Time
    // =========================
    Milliseconds,
    Seconds,
    Minutes,
    Hours,
    Days,
    Weeks,
    Months,
    Quarters,
    Years,

    // =========================
    // Length
    // =========================
    Millimeters,
    Centimeters,
    Meters,
    Kilometers,
    Inches,
    Feet,
    Yards,
    Miles,

    // =========================
    // Area
    // =========================
    SquareMeters,
    SquareKilometers,
    SquareFeet,
    Hectares,
    Acres,

    // =========================
    // Volume
    // =========================
    Milliliters,
    Liters,
    CubicMeters,
    CubicFeet,
    Gallons,

    // =========================
    // Mass / Weight
    // =========================
    Milligrams,
    Grams,
    Kilograms,
    Tons,
    Pounds,
    Ounces,

    // =========================
    // Temperature
    // =========================
    Celsius,
    Fahrenheit,
    Kelvin,

    // =========================
    // Speed
    // =========================
    MetersPerSecond,
    KilometersPerHour,
    MilesPerHour,

    // =========================
    // Energy / Power
    // =========================
    Joules,
    KilowattHours,
    Watts,
    Kilowatts,

    // =========================
    // Financial
    // =========================
    Currency,
    CurrencyPerHour,
    CurrencyPerDay,
    CurrencyPerMonth,
    CurrencyPerYear,
    CostPerUnit,
    RevenuePerUnit,
    BudgetVariance,

    // =========================
    // Quality & Defects
    // =========================
    DefectCount,
    DefectsPerUnit,
    DefectsPerMillion,
    ErrorRate,
    FailureRate,
    AvailabilityPercentage,
    DowntimeHours,
    UptimeHours,

    // =========================
    // Performance
    // =========================
    ResponseTimeMilliseconds,
    ThroughputPerSecond,
    ThroughputPerMinute,
    RequestsPerSecond,
    TransactionsPerSecond,
    LatencyMilliseconds,

    // =========================
    // Productivity
    // =========================
    TasksCompleted,
    TasksPerHour,
    OutputPerEmployee,
    VelocityPoints,
    StoryPoints,
    BurndownRate,

    // =========================
    // Human / HR
    // =========================
    Employees,
    EmployeesPerManager,
    TrainingHours,
    SatisfactionScore,
    EngagementScore,
    AttritionRate,

    // =========================
    // Customer / Business
    // =========================
    Customers,
    NewCustomers,
    CustomerRetentionRate,
    ChurnRate,
    NetPromoterScore,
    ConversionRate,
    MarketSharePercentage,

    // =========================
    // Technical / Software
    // =========================
    LinesOfCode,
    CodeCoveragePercentage,
    BuildDurationMinutes,
    DeploymentFrequency,
    LeadTimeDays,
    CycleTimeDays,

    // =========================
    // Risk & Compliance
    // =========================
    RiskScore,
    RiskExposureCurrency,
    CompliancePercentage,
    AuditFindingsCount,

    // =========================
    // Environmental / Sustainability
    // =========================
    CO2EmissionsTons,
    CH4EmissionsTons,
    EnergyConsumptionKWh,
    WaterUsageLiters,

    // =========================
    // Custom / Fallback
    // =========================
    Custom,
    Other
}
