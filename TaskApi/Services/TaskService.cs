using Microsoft.EntityFrameworkCore;
using TaskApi.Data;
using TaskApi.DTOs;
using TaskApi.Models;

namespace TaskApi.Services
{
    public class TaskService
    {
        private readonly AppDbContext _context;

        public TaskService(AppDbContext context)
        {
            _context = context;
        }

        // Get all tasks
        public async Task<List<TaskDTO>> GetAllTasksAsync()
        {
            return await _context.Tasks
                .Select(task => new TaskDTO
                {
                    Title = task.Title,
                    Description = task.Description,
                    Status = task.Status,
                    DueDate = task.DueDate
                })
                .ToListAsync();
        }

        // Get task by ID
        public async Task<TaskDTO?> GetTaskByIdAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return null;

            return new TaskDTO
            {
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                DueDate = task.DueDate
            };
        }

        // Create a new task
        public async Task<TaskModel> CreateTaskAsync(TaskDTO taskDto)
        {
            var task = new TaskModel
            {
                Title = taskDto.Title,
                Description = taskDto.Description,
                Status = taskDto.Status,
                DueDate = taskDto.DueDate
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        // Update an existing task
        public async Task<bool> UpdateTaskAsync(int id, TaskDTO taskDto)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return false;

            task.Title = taskDto.Title;
            task.Description = taskDto.Description;
            task.Status = taskDto.Status;
            task.DueDate = taskDto.DueDate;

            await _context.SaveChangesAsync();
            return true;
        }

        // Delete a task
        public async Task<bool> DeleteTaskAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task == null) return false;

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
