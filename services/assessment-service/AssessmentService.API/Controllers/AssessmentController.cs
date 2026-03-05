using AssessmentService.Application.DTOs;
using AssessmentService.Application.Interfaces;
using AssessmentService.Application.Interfaces.Clients;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Auth;
using Shared.Contracts;

namespace AssessmentService.API.Controllers;

/// <summary>
/// Controller for managing Goal Probability Assessments.
/// </summary>
[ApiController]
[Route("assessments")]
[Produces("application/json")]
[Authorize]
public class AssessmentController : ControllerBase
{
    private readonly IAssessmentService _assessmentService;
    private readonly IValidator<CreateAssessmentRequest> _createValidator;
    private readonly IValidator<UpdateAssessmentRequest> _updateValidator;
    private readonly IAuditClient _auditClient;

    public AssessmentController(
        IAssessmentService assessmentService,
        IValidator<CreateAssessmentRequest> createValidator,
        IValidator<UpdateAssessmentRequest> updateValidator,
        IAuditClient auditClient)
    {
        _assessmentService = assessmentService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _auditClient = auditClient;
    }

    /// <summary>
    /// Creates a new goal probability assessment.
    /// </summary>
    /// <param name="request">Assessment creation data.</param>
    /// <returns>The newly created assessment.</returns>
    /// <response code="201">Assessment successfully created.</response>
    /// <response code="400">Invalid input data.</response>
    [HttpPost]
    [RequirePermission("manage_probability_assessments")]
    [ProducesResponseType(typeof(AssessmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAssessmentRequest request)
    {
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var response = await _assessmentService.CreateAsync(request);
        _ = _auditClient.LogAsync(Guid.Empty, "System", "AssessmentCreated", "Assessment", response.Id, new { response.GoalId });
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    /// <summary>
    /// Retrieves a goal probability assessment by its unique identifier.
    /// </summary>
    /// <param name="id">The assessment ID.</param>
    /// <returns>The requested assessment.</returns>
    /// <response code="200">Assessment found.</response>
    /// <response code="404">Assessment not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AssessmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _assessmentService.GetByIdAsync(id);
        return Ok(response);
    }

    /// <summary>
    /// Retrieves the goal probability assessment associated with a specific goal.
    /// </summary>
    /// <param name="goalId">The goal ID.</param>
    /// <returns>The assessment for the given goal, or 404 if none exists.</returns>
    /// <response code="200">Assessment found for the specified goal.</response>
    /// <response code="404">No assessment exists for the specified goal.</response>
    [HttpGet("goal/{goalId:guid}")]
    [ProducesResponseType(typeof(AssessmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByGoalId(Guid goalId)
    {
        var response = await _assessmentService.GetByGoalIdAsync(goalId);
        return Ok(response);
    }

    /// <summary>
    /// Retrieves all goal probability assessments with pagination.
    /// </summary>
    /// <param name="pagination">Pagination parameters.</param>
    /// <returns>Paginated list of assessments.</returns>
    /// <response code="200">Successfully retrieved assessments.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PaginationResponse<AssessmentResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] PaginationRequest pagination)
    {
        var result = await _assessmentService.GetAllAsync(pagination);
        return Ok(result);
    }

    /// <summary>
    /// Updates an existing goal probability assessment.
    /// </summary>
    /// <param name="id">The assessment ID to update.</param>
    /// <param name="request">Updated assessment data.</param>
    /// <returns>The updated assessment.</returns>
    /// <response code="200">Assessment successfully updated.</response>
    /// <response code="400">Invalid input data.</response>
    /// <response code="404">Assessment not found.</response>
    [HttpPut("{id:guid}")]
    [RequirePermission("manage_probability_assessments")]
    [ProducesResponseType(typeof(AssessmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAssessmentRequest request)
    {
        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var response = await _assessmentService.UpdateAsync(id, request);
        _ = _auditClient.LogAsync(Guid.Empty, "System", "AssessmentStateChanged", "Assessment", response.Id,
            new { newState = request.State.ToString() });
        return Ok(response);
    }

    /// <summary>
    /// Deletes a goal probability assessment.
    /// </summary>
    /// <param name="id">The assessment ID to delete.</param>
    /// <response code="204">Assessment successfully deleted.</response>
    /// <response code="404">Assessment not found.</response>
    [HttpDelete("{id:guid}")]
    [RequirePermission("manage_probability_assessments")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _assessmentService.DeleteAsync(id);
        return NoContent();
    }
}
