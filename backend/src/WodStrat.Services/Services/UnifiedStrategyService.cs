using WodStrat.Services.Dtos;
using WodStrat.Services.Extensions;
using WodStrat.Services.Interfaces;

namespace WodStrat.Services.Services;

/// <summary>
/// Service implementation for calculating unified strategy results
/// by orchestrating all strategy services with shared context.
/// </summary>
public class UnifiedStrategyService : IUnifiedStrategyService
{
    private readonly IMovementContextService _movementContextService;
    private readonly IPacingService _pacingService;
    private readonly IVolumeLoadService _volumeLoadService;
    private readonly ITimeEstimateService _timeEstimateService;
    private readonly IStrategyInsightsService _strategyInsightsService;
    private readonly IAthleteService _athleteService;

    /// <summary>
    /// Initializes a new instance of the UnifiedStrategyService.
    /// </summary>
    /// <param name="movementContextService">Service for building shared movement context.</param>
    /// <param name="pacingService">Service for pacing calculations.</param>
    /// <param name="volumeLoadService">Service for volume load calculations.</param>
    /// <param name="timeEstimateService">Service for time estimate calculations.</param>
    /// <param name="strategyInsightsService">Service for strategy insights calculations.</param>
    /// <param name="athleteService">Service for current user operations.</param>
    public UnifiedStrategyService(
        IMovementContextService movementContextService,
        IPacingService pacingService,
        IVolumeLoadService volumeLoadService,
        ITimeEstimateService timeEstimateService,
        IStrategyInsightsService strategyInsightsService,
        IAthleteService athleteService)
    {
        _movementContextService = movementContextService;
        _pacingService = pacingService;
        _volumeLoadService = volumeLoadService;
        _timeEstimateService = timeEstimateService;
        _strategyInsightsService = strategyInsightsService;
        _athleteService = athleteService;
    }

    /// <inheritdoc />
    public async Task<UnifiedStrategyResultDto?> CalculateUnifiedStrategyAsync(
        int athleteId,
        int workoutId,
        CancellationToken cancellationToken = default)
    {
        // Build the shared workout context first
        var workoutContext = await _movementContextService.BuildWorkoutContextAsync(
            athleteId,
            workoutId,
            cancellationToken);

        if (workoutContext == null)
        {
            return null;
        }

        // Call existing services sequentially (they share DbContext, so no parallel calls)
        var pacingResult = await _pacingService.CalculateWorkoutPacingAsync(
            athleteId,
            workoutId,
            cancellationToken);

        var volumeResult = await _volumeLoadService.CalculateWorkoutVolumeLoadAsync(
            athleteId,
            workoutId,
            cancellationToken);

        var timeResult = await _timeEstimateService.EstimateWorkoutTimeAsync(
            athleteId,
            workoutId,
            cancellationToken);

        var insightsResult = await _strategyInsightsService.CalculateStrategyInsightsAsync(
            athleteId,
            workoutId,
            cancellationToken);

        // Map results to slim summary DTOs
        var pacingSummary = pacingResult?.ToPacingSummary() ?? new PacingAnalysisSummaryDto();
        var volumeSummary = volumeResult?.ToVolumeLoadSummary() ?? new VolumeLoadAnalysisSummaryDto();
        var timeSummary = timeResult?.ToTimeEstimateSummary() ?? new TimeEstimateAnalysisSummaryDto();
        var insightsSummary = insightsResult?.ToInsightsSummary() ?? new StrategyInsightsSummaryDto();

        return new UnifiedStrategyResultDto
        {
            Context = workoutContext,
            Pacing = pacingSummary,
            VolumeLoad = volumeSummary,
            TimeEstimate = timeSummary,
            Insights = insightsSummary
        };
    }

    /// <inheritdoc />
    public async Task<UnifiedStrategyResultDto?> CalculateCurrentUserUnifiedStrategyAsync(
        int workoutId,
        CancellationToken cancellationToken = default)
    {
        var athlete = await _athleteService.GetCurrentUserAthleteAsync(cancellationToken);
        if (athlete == null)
        {
            return null;
        }

        return await CalculateUnifiedStrategyAsync(athlete.Id, workoutId, cancellationToken);
    }
}
