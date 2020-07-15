using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TodoApi.Services;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
namespace TodoApi.Controllers
{
    [Authorize]
    [ApiController]
    public class TodoItemsController : ControllerBase
    {
        private readonly TodoContext _context;
       
        public TodoItemsController(TodoContext context)
        {
            _context = context;
        }
        // GET: api/TodoItems
        [AllowAnonymous]
        [HttpGet]
        [Route("/login")]
        public IActionResult GetTodoItem([FromQuery] string username,[FromQuery] string password)
        {    string passwordHash=hashPassword(password);
        password=passwordHash;
            var result = _context.TodoItems.Where(x =>x.passwordhash == password.Trim() && x.Name == username.Trim()).SingleOrDefault();
            IActionResult response;
            if (result == null)
                return NotFound();
            var tokenString = generateJwtToken(result);
            response = Ok(new { token = tokenString });
            return response;
        }
        [HttpGet]
        [Route("/hello")]
        public string GetTodoItem()
        {
            return "Hello Authenticated User";
        }
        // GET: api/TodoItems/5
        /*     [HttpGet]
             [Route("")]
             public async Task<ActionResult<TodoItem>> GetTodoItem(long id)
             {
                 var todoItem = await _context.TodoItems.FindAsync(id);
                 if (todoItem == null)
                 {
                     return NotFound();
                 }
                 return todoItem;
             }
     */
        // PUT: api/TodoItems/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        // POST: api/TodoItems
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        // POST: api/TodoItems
        [AllowAnonymous]
        [HttpPost]
        [Route("/signup")]
        public async Task<ActionResult<TodoItem>> PostTodoItem(TodoItem todoItem)
        {
            todoItem.registeredon = GetTimestamp(DateTime.Now);
            var passwordHash=hashPassword(todoItem.passwordhash);
            todoItem.passwordhash=passwordHash.ToString();

            _context.TodoItems.Add(todoItem);
            await _context.SaveChangesAsync();
            //return CreatedAtAction("GetTodoItem", new { id = todoItem.Id }, todoItem);
            return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
        }
        private string generateJwtToken(TodoItem user)
        {
            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("mysecretkey007bvcfghjkugcv");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("name", user.Id.ToString()),
                    new Claim("registeredon",user.registeredon.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(5),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        // DELETE: api/TodoItems/5
        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmssffff");
        }
        private bool TodoItemExists(long id)
        {
            return _context.TodoItems.Any(e => e.Id == id);
        }

        string hashPassword(string password)
        {
           byte[] salt = new byte[128 / 8];
        
       
 
        // derive a 256-bit subkey (use HMACSHA1 with 10,000 iterations)
        string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: password,
            salt:salt,
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));
        return hashed;
        }
    }
}