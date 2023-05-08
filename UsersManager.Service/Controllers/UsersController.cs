using System.Security.Claims;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using UsersManager.Domain.Models;
using UsersManager.Domain.Services;
using UsersManager.Service.HostUtilities;
using UsersManager.Service.Models;

namespace UsersManager.Service.Controllers;

[ApiController]
[Route("user")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateUserRequestDto> _validator;

    public UsersController(IUserService userService,
        IMapper mapper,
        IValidator<CreateUserRequestDto> validator)
    {
        _userService = userService;
        _mapper = mapper;
        _validator = validator;
    }

    [HttpGet]
    public async Task<ActionResult<UserDto>> GetUser([FromQuery]string? login, CancellationToken token)
    {
        var (validatedLogin, error) = ChooseLogin(login);

        if (error is not null)
            return error;

        var user = await _userService.GetUser(validatedLogin, token);
        return user is not null
            ? Ok(_mapper.Map<UserDto>(user))
            : NotFound();
    }

    [Authorize]
    [HttpGet("all")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers([FromQuery]int? size, [FromQuery]int? pageIndex,
        CancellationToken token)
    {
        var users = await _userService.GetUsers(size, pageIndex, token);
        return Ok(users.Select(_mapper.Map<UserDto>));
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody]CreateUserRequestDto requestDto,
        CancellationToken token)
    {
        const string requestErrorTemplateString = "Bad request to creating user with errors: {Errors}";

        var validation = await _validator.ValidateAsync(requestDto, token);
        if (!validation.IsValid)
        {
            var errors = validation.ToString();
            Log.Information(requestErrorTemplateString, errors);
            return BadRequest(errors);
        }

        User user;
        try
        {
            var request = _mapper.Map<CreateUserRequest>(requestDto);
            user = await _userService.CreateUser(request, token);
        }
        catch (InvalidOperationException e)
        {
            Log.Information(requestErrorTemplateString, e.Message);
            return BadRequest(e.Message);
        }
        return Ok(_mapper.Map<UserDto>(user));
    }

    [Authorize]
    [HttpDelete]
    public async Task<ActionResult<UserDto>> DeleteUser([FromQuery]string? login,
        CancellationToken token)
    {
        var (validatedLogin, error) = ValidatePrivilegesAndChooseLogin(login);

        if (error is not null)
            return error;

        var isDeleted = await _userService.DeleteUser(validatedLogin, token);
        return isDeleted ? Ok() : NotFound();
    }

    private (string login, ActionResult? error) ChooseLogin(string? loginFromUser)
    {
        var nameClaim = HttpContext.User.FindFirstValue(ServiceAuthentication.NameClaimType);

        if (string.IsNullOrWhiteSpace(loginFromUser))
            loginFromUser = nameClaim;

        return loginFromUser is not null
            ? (loginFromUser, null)
            : ("", Unauthorized("Invalid login for operation"));
    }

    private (string login, ActionResult? error) ValidatePrivilegesAndChooseLogin(string? loginFromUser)
    {
        var isAdmin = HttpContext.User.IsInRole(ServiceAuthentication.AdminRole);
        var nameClaim = HttpContext.User.FindFirstValue(ServiceAuthentication.NameClaimType);

        if (!isAdmin && (nameClaim is null || loginFromUser != nameClaim))
            return ("", BadRequest("Can't delete other user without admin privileges"));

        if (string.IsNullOrWhiteSpace(loginFromUser))
            loginFromUser = nameClaim;

        return loginFromUser is not null
            ? (loginFromUser, null)
            : ("", Unauthorized("Invalid login for operation"));
    }
}