package com.mklt.uniconnect.Controllers;

import org.springframework.web.bind.annotation.*;
import java.util.List;
import com.mklt.uniconnect.Entities.Task;
import com.mklt.uniconnect.Services.TaskServices;

@RestController
@RequestMapping("/tasks") // base path for task endpoint
public class TaskController {

    private TaskServices taskService;

    public TaskController(TaskServices taskService) {
        this.taskService = taskService;
    }

    //Create a new task
    @PostMapping("/create")
    public String createTask(@RequestBody Task task) {
        return taskService.createTask(
            task.getTitle(),
            task.getDescription(),
            task.getDueDate(),
            task.getStatus(),
            task.getPriority()
        );
    
    }

    //Get all tasks
    @GetMapping("/all")
    public List<Task> getAllTasks() {
        return taskService.getAllTasks();
    }

    //Get a task by id
    @GetMapping ("/{taskId}")
    public List<Task> getTaskById(@PathVariable Long taskId) {
        return taskService.getTaskById(taskId);
    }

    //Update an existing task
    @PutMapping("/{taskId}")
    public String updateTask(@PathVariable Long taskId, @RequestBody Task task) {
        return taskService.updateTask(
            taskId,
            task.getTitle(),
            task.getDescription(),
            task.getDueDate(),
            task.getStatus(),
            task.getPriority()
        );
    }
    
        
    //Delete a task
    @DeleteMapping("/{taskId}")
    public String deleteTask(@PathVariable Long taskId) {
        return taskService.deleteTask(taskId);
    }

    
}
