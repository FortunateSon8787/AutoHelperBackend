using AutoHelper.Application.Features.Auth.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AutoHelper.Api.Features.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", Register)
            .WithSummary("Register a new customer with email and password")
            .Produces<RegisterResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    // ─── Handlers ─────────────────────────────────────────────────────────────

    private static async Task<IResult> Register(
        [FromBody] RegisterCustomerCommand command,
        ISender mediator,
        CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);

        if (result.IsFailure)
            return Results.Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = result.Error
            });

        return Results.Created($"/api/customers/{result.Value}", new RegisterResponse(result.Value));
    }

    // ─── Response DTOs ────────────────────────────────────────────────────────

    private sealed record RegisterResponse(Guid CustomerId);
}
