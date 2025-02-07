using Microsoft.AspNetCore.Mvc;
using TaskApi.DTOs;
using TaskApi.Services;
using Microsoft.AspNetCore.Authorization;

namespace TaskApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly TaskService _taskService;

        public TasksController(TaskService taskService)
        {
            _taskService = taskService;
        }

        // GET: api/tasks
        [HttpGet]
        [EnableRateLimiting("get-tasks")]
        public async Task<ActionResult<IEnumerable<TaskDTO>>> GetTasks()
        {
            var username = User.Identity?.Name ?? "Unknown";
            Console.WriteLine($"Authenticated request from: {username}");

            var tasks = await _taskService.GetAllTasksAsync();
            Console.WriteLine($"Returning {tasks.Count} tasks");

            return Ok(tasks);
        }

        // GET: api/tasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskDTO>> GetTask(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null) return NotFound();
            return Ok(task);
        }

        // POST: api/tasks
        [HttpPost]
        [EnableRateLimiting("create-task")]
        public async Task<ActionResult<TaskDTO>> CreateTask(TaskDTO taskDto)
        {
            var createdTask = await _taskService.CreateTaskAsync(taskDto);
            return CreatedAtAction(nameof(GetTask), new { id = createdTask.Id }, createdTask);
        }

        // PUT: api/tasks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(int id, TaskDTO taskDto)
        {
            var updated = await _taskService.UpdateTaskAsync(id, taskDto);
            if (!updated) return NotFound();
            return NoContent();
        }

        // DELETE: api/tasks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var deleted = await _taskService.DeleteTaskAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
