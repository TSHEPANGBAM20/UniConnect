package com.mklt.uniconnect.Services;

import org.springframework.stereotype.Service;
import java.time.LocalDate;
import java.util.List;

import com.mklt.uniconnect.Entities.Task;
import com.mklt.uniconnect.Repositories.TaskRepo;


@Service
public class TaskServices {

    private final TaskRepo taskRepo;

    public TaskServices(TaskRepo taskRepo) {
        this.taskRepo = taskRepo;
    }
    


//creates a new task and saves it to the database
public String createTask(String title, String description, LocalDate dueDate, String status, String priority) {
    Task task = new Task(title, description, dueDate, status, priority);
    taskRepo.save(task);
    return "Task created:" + task.getTitle();
}

//returns every task currently in the database
public List<Task> getAllTasks() {
    return taskRepo.findAll();
}

//return one task based on the id provided
public List<Task> getTaskById(Long taskId) {
    return taskRepo.findById(taskId).map(List::of).orElse(List.of());
}

//editing the details of the existing task  
public String updateTask(Long taskId, String title, String description, LocalDate dueDate, String status, String priority) {
    List<Task> taskOpt = getTaskById(taskId);

    if (taskOpt.isEmpty()) {
        return "Task not found";
    
    }

    Task task  =  taskOpt.get(0);
    task.setTitle(title);
    task.setDescription(description);
    task.setDueDate(dueDate);
    task.setStatus(status);
    task.setPriority(priority);

    taskRepo.save(task);
    return "Task updated: " + task.getTitle();
}

//deletes a task based on the id provided
public String deleteTask(Long taskId){
    if (!taskRepo.existsById(taskId)){
        return "Task not found";
    }
    taskRepo.deleteById(taskId);
    return "Task deleted";
}
}