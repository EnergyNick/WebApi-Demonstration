using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using UsersManager.Domain.Models;
using UsersManager.Domain.Services;
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
    public async Task<ActionResult<UserDto>> GetUser([FromQuery]string login, CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(login))
            return BadRequest(OnEmptyIdErrorString);

        var user = await _userService.GetUser(login, token);
        if (user is null)
            return NotFound();
        return Ok(_mapper.Map<UserDto>(user));
    }

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
        var validation = await _validator.ValidateAsync(requestDto, token);
        if (!validation.IsValid)
        {
            var errors = validation.ToString();
            Log.Information(RequestErrorTemplateString, errors);
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
            Log.Information(RequestErrorTemplateString, e.Message);
            return BadRequest(e.Message);
        }
        return Ok(_mapper.Map<UserDto>(user));
    }

    [HttpDelete]
    public async Task<ActionResult<UserDto>> DeleteUser([FromQuery]string login,
        CancellationToken token)
    {
        if (string.IsNullOrWhiteSpace(login))
            return BadRequest(OnEmptyIdErrorString);

        var isDeleted = await _userService.DeleteUser(login, token);
        return isDeleted ? Ok() : NotFound();
    }

    private const string OnEmptyIdErrorString = "User Id must be not empty";
    private const string RequestErrorTemplateString = "Bad request to creating user with errors: {Errors}";
}