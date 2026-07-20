//using AgriVision.Application.DTOs.FarmApp;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Security.Claims;
//using AgriVision.Domain.FarmApp;
//using AgriVision.Infrastructure.Persistence;

//namespace AgriVision.API.Controllers
//{
//    [Authorize]
//    [Route("api/Farms/{FarmId}/[controller]")]
//    [ApiController]
//    public class RobotsController : ControllerBase
//    {
//        private readonly AppDbContext _context;
//        private readonly ILogger<RobotsController> _logger;

//        public RobotsController(AppDbContext context, ILogger<RobotsController> logger)
//        {
//            _context = context;
//            _logger = logger;
//        }

//        [HttpGet]
//        public async Task<ActionResult<IEnumerable<RobotDTO>>> GetRobots(int farmId)
//        {
//            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//            if (userId == null)
//            {
//                return Unauthorized();
//            }

//            var farm = await _context.farms.FindAsync(farmId);
//            if (farm == null)
//                return NotFound();
//            if (farm.OwnerId != userId)
//                return Forbid();

//            //var robots = await _context.robots                
//            //    .Where(r => r.Field.FarmId == farmId)
//            //    .Select(f => new RobotDTO
//            //    {
//            //        RobotId = f.RobotId,
//            //        Name = f.Name,
//            //        Status = f.Status,
//            //        FieldId = f.FieldId,
//            //        FarmId = f.Field.FarmId
//            //    }).ToListAsync();

//            return Ok(robots);
//        }
//        [HttpGet("{RobotId}")]
//        public async Task<ActionResult<RobotDTO>> GetRobotById(int farmId, int robotId)
//        {
//            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//            if (userId == null)
//            {
//                return Unauthorized();
//            }
//            var farm = await _context.farms.FindAsync(farmId);
//            if (farm == null)
//                return NotFound();
//            if (farm.OwnerId != userId)
//                return Forbid();
//            var robot = await _context.robots
//                .Include(r => r.Field)
//                .Where(r => r.RobotId == robotId && r.Field.FarmId == farmId)
//                .Select(r => new RobotDTO
//                {
//                    RobotId = r.RobotId,
//                    Name = r.Name,
//                    Status = r.Status,
//                    FieldId = r.FieldId,
//                    FarmId = r.Field.FarmId
//                }).FirstOrDefaultAsync();
//            if (robot == null)
//                return NotFound();
//            return Ok(robot);
//        }
//        [HttpPost]
//        public async Task<ActionResult<RobotDTO>> AddRobot(int farmId, [FromBody] CreateRobotDTO createRobotDTO)
//        {
//            if(!ModelState.IsValid)
//            {
//                return BadRequest(ModelState);
//            }
//            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//            if (userId == null)
//            {
//                return Unauthorized();
//            }
//            var farm = await _context.farms.FindAsync(farmId);
//            if (farm == null)
//                return NotFound();
//            if (farm.OwnerId != userId)
//                return Forbid();
//            var field = await _context.fields.AnyAsync(f => f.FieldId == createRobotDTO.FieldId && f.FarmId == farmId);
//            if(!field)
//                return NotFound();

//            var robot = new Robot
//            {
//                Name = createRobotDTO.Name,
//                FieldId = createRobotDTO.FieldId,
//                Status = createRobotDTO.Status,
//            };
//            _context.robots.Add(robot);
//            await _context.SaveChangesAsync();

//            var robotDTO = new RobotDTO
//            {
//                RobotId = robot.RobotId,
//                FieldId = createRobotDTO.FieldId,
//                Name = robot.Name,
//                Status = robot.Status,
//                FarmId = farmId
//            };
//            return CreatedAtAction(nameof(GetRobotById), new { farmID = farmId, robotId = robot.RobotId }, robotDTO);
//        }
//        [HttpPut("{robotId}")]
//        public async Task<IActionResult> UpdateRobot(int farmId, int robotId, [FromBody] UpdateRobotDTO updateRobotDto)
//        {
//            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//            if (userId == null)
//            {
//                return Unauthorized();
//            }

//            var farm = await _context.farms.FindAsync(farmId);
//            if(farm == null)
//            {
//                return NotFound();
//            }
//            if (farm.OwnerId != userId)
//            {
//                return Forbid();
//            }
//            var robot = await _context.robots
//                .FirstOrDefaultAsync(r => r.RobotId == robotId && r.Field.FarmId == farmId);

//            if (robot == null)
//            {
//                return NotFound();
//            }


//            var newField = await _context.fields
//                .FirstOrDefaultAsync(f => f.FieldId == updateRobotDto.FieldId);

//            if(newField == null)
//                return NotFound();
//            if(newField.FarmId != farmId)
//                return BadRequest(new {message = "cant move the robot to a field in another farm"});

//            robot.Name = updateRobotDto.Name;
//            robot.Status = updateRobotDto.Status;
//            robot.FieldId = updateRobotDto.FieldId;

//            await _context.SaveChangesAsync();
//            return NoContent();
//        }
//        [HttpDelete("{robotId}")]
//        public async Task<IActionResult> DeleteRobot(int farmId, int robotId)
//        {
//            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
//            if (userId == null)
//            {
//                return Unauthorized();
//            }

//            var farm = await _context.farms.FindAsync(farmId);
//            if (farm == null)
//            {
//                return NotFound();
//            }
//            if (farm.OwnerId != userId)
//            {
//                return Forbid();
//            }
//            var robot = await _context.robots
//                .FirstOrDefaultAsync(r => r.RobotId == robotId && r.Field.FarmId == farmId);

//            if (robot == null)
//            {
//                return NotFound();
//            }   

//            _context.robots.Remove(robot);
//            await _context.SaveChangesAsync();

//            return NoContent();
//        }
//    }
//}