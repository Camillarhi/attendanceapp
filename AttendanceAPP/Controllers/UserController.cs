using AttendanceAPP.DTOs;
using AttendanceAPP.IRepository;
using AttendanceAPP.Model;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AttendanceAPP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UserController> _logger;
        private readonly UserManager<UserModel> _userManager;
        private readonly SignInManager<UserModel> _signInManager;

        public UserController(IMapper mapper, IConfiguration configuration, ILogger<UserController> logger, UserManager<UserModel> userManager, SignInManager<UserModel> signInManager, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _configuration = configuration;
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
            _unitOfWork = unitOfWork;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _unitOfWork.Users.GetAll();
                var results = _mapper.Map<IList<UserDTO>>(users);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize]
        [HttpGet("search/{user?}")]
        public async Task<IActionResult> SearchUsers(string? user )
        {
            try
            {
                if (user == null)
                {
                    var users = await _unitOfWork.Users.GetAll();
                    var results = _mapper.Map<IList<UserDTO>>(users);
                    return Ok(results);
                }
                else
                {
                    var users = await _unitOfWork.Users.GetAll(x => x.FirstName.ToUpper().Contains(user.ToUpper())
                || x.LastName.ToUpper().Contains(user.ToUpper())
                || (x.FirstName + " " + x.LastName).ToUpper().Contains(user.ToUpper()));
                    var results = _mapper.Map<IList<UserDTO>>(users);
                    return Ok(results);
                }
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Something went wrong");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserCreateDTO userDTO)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var author = _mapper.Map<UserModel>(userDTO);
                    author.UserName = userDTO.Email;
                    var result = await _userManager.CreateAsync(author, userDTO.Password);
                    if (!result.Succeeded)
                    {
                        return BadRequest($"User registration failed");
                    }
                    return StatusCode(200);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Something went wrong");
                    return StatusCode(500, "Internal server error");
                }
            }
            _logger.LogError($"Invalid Post attempt");
            return BadRequest(ModelState);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthenticationResponse>> Login([FromBody] LoginDTO login)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = await _signInManager.PasswordSignInAsync(login.Email, login.Password, isPersistent: false, lockoutOnFailure: false);
                    if (result.Succeeded)
                    {
                        var user = await _userManager.FindByEmailAsync(login.Email);
                        var role = await _userManager.GetRolesAsync(user);
                        var logedIn = new AuthenticationResponse
                        {
                            Email = user.Email,
                            Id = user.Id
                        };
                        return BuildToken(logedIn);
                    }
                    else
                    {
                        return BadRequest(new { Error = "Invalid Username or Password" });
                    }
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }

            catch (Exception ex)
            {
                return BadRequest(ex);
            }
        }

        private AuthenticationResponse BuildToken(AuthenticationResponse login)
        {
            var claims = new List<Claim>()
            {
                new Claim("email", login.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["keyjwt"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiration = DateTime.UtcNow.AddDays(1);
            var token = new JwtSecurityToken(issuer: null, audience: null, claims: claims,
                expires: expiration, signingCredentials: creds);
            return new AuthenticationResponse()
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Email = login.Email,
                Id = login.Id,
                Expiration = expiration
            };

        }
    }

    
}
