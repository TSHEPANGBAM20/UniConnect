package com.mklt.uniconnect.Controllers;

import java.util.List;
import java.util.Map;

import org.springframework.web.bind.annotation.DeleteMapping;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.PutMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

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
    @PostMapping("/createTask/{projectId}")
    public Task createTask(@PathVariable Long projectId, @RequestBody Task task) {
        return taskService.createTask(projectId,task);
   
    
    }

    //Get all tasks
    @GetMapping("/getTaskByProject/{projectId}")
    public List<Task> getTasksByProject(@PathVariable Long projectId) {
        return taskService.getTaskByProject(projectId);
    }

    //Get a task by id
    @GetMapping ("/getTaskById/{taskId}")
    public List<Task> getTaskById(@PathVariable Long taskId) {
        return taskService.getTaskById(taskId);
    }

    //Get all tasks with due date
    @GetMapping("/TaskCalendar/calendar")
    public List<Task> getCalendarTasks() {
        return taskService.getTasksWithDueDate();
    }

    @GetMapping("/notofications/{studentId}")
    public List<Map<String, Object>> getNotifications(@PathVariable Long studentId) {
        return taskService.getNotifications(studentId);
    }

    //Update an existing task
    @PutMapping("/updateTask/{taskId}")
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

    //changes the status of a task
    @PutMapping("/updateStatus/{taskId}/status")
    public Task updateTaskStatus(@PathVariable Long taskId, @RequestBody Map<String, String> body) {
        return taskService.updateTaskStatus(taskId, body.get("status"));
    }
    
        
    //Delete a task
    @DeleteMapping("/deleteTask/{taskId}")
    public String deleteTask(@PathVariable Long taskId) {
        return taskService.deleteTask(taskId);
    }

}
