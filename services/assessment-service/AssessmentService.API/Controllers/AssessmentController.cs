using AssessmentService.Application.DTOs;
using AssessmentService.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace AssessmentService.API.Controllers;

/// <summary>
/// Controller for managing Goal Probability Assessments.
/// </summary>
[ApiController]
[Route("assessments")]
[Produces("application/json")]
public class AssessmentController : ControllerBase
{
    private readonly IAssessmentService _assessmentService;
    private readonly IValidator<CreateAssessmentRequest> _createValidator;
    private readonly IValidator<UpdateAssessmentRequest> _updateValidator;

    public AssessmentController(
        IAssessmentService assessmentService,
        IValidator<CreateAssessmentRequest> createValidator,
        IValidator<UpdateAssessmentRequest> updateValidator)
    {
        _assessmentService = assessmentService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// Creates a new goal probability assessment.
    /// </summary>
    /// <param name="request">Assessment creation data.</param>
    /// <returns>The newly created assessment.</returns>
    /// <response code="201">Assessment successfully created.</response>
    /// <response code="400">Invalid input data.</response>
    [HttpPost]
    [ProducesResponseType(typeof(AssessmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateAssessmentRequest request)
    {
        var validationResult = await _createValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var response = await _assessmentService.CreateAsync(request);
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

        if (response is null)
            return NotFound(new { Message = $"No assessment found for goal '{goalId}'." });

        return Ok(response);
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
    [ProducesResponseType(typeof(AssessmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAssessmentRequest request)
    {
        var validationResult = await _updateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors.Select(e => new { e.PropertyName, e.ErrorMessage }));

        var response = await _assessmentService.UpdateAsync(id, request);
        return Ok(response);
    }

    /// <summary>
    /// Deletes a goal probability assessment.
    /// </summary>
    /// <param name="id">The assessment ID to delete.</param>
    /// <response code="204">Assessment successfully deleted.</response>
    /// <response code="404">Assessment not found.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _assessmentService.DeleteAsync(id);
        return NoContent();
    }
}
