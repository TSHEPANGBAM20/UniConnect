package com.mklt.uniconnect.Services;

import org.springframework.stereotype.Service;
import java.time.LocalDate;
import java.util.List;

import org.springframework.http.HttpStatus;
import org.springframework.web.server.ResponseStatusException;
import com.mklt.uniconnect.Repositories.ProjectsRepo;

import com.mklt.uniconnect.Entities.Task;
import com.mklt.uniconnect.Repositories.TaskRepo;

import java.time.temporal.ChronoUnit;
import java.util.ArrayList;
import java.util.LinkedHashMap;
import java.util.Map;


@Service
public class TaskServices {

    private final TaskRepo taskRepo;
    private final ProjectsRepo projectsRepo;

    public TaskServices(TaskRepo taskRepo, ProjectsRepo projectsRepo) {
        
        this.taskRepo = taskRepo;
        this.projectsRepo = projectsRepo;
    }
    


//creates a new task and saves it to the database
public Task createTask(Long projectId, Task task) {
   if (!projectsRepo.existsById(projectId)) {
       throw new ResponseStatusException(HttpStatus.NOT_FOUND, "Project not found");
   }
   if (task.getTitle() == null || task.getTitle().trim().isEmpty()) {
       throw new ResponseStatusException(HttpStatus.BAD_REQUEST, "Task title cannot be empty");
   }
   String status = (task.getStatus() == null ) ? "NOT_STARTED" : task.getStatus();

   Task newTask = new Task(task.getTitle().trim(), task.getDescription(),task.getDueDate(), status, task.getPriority(), projectId, task.getAssignedTo());
    return taskRepo.save(newTask);
}

//returns every task currently in the database
public List<Task> getTaskByProject(Long projectId) {
    return taskRepo.findByProjectId(projectId);

}

public List<Task> getTasksWithDueDate() {
    return taskRepo.findByDueDateIsNotNull();
}

//return one task based on the id provided
public List<Task> getTaskById(Long taskId) {
    return taskRepo.findById(taskId).map(List::of).orElse(List.of());
}

//editing the details of the existing task  
public String updateTask(Long taskId, String title, String description, LocalDate dueDate, String status, String priority) {
    List<Task> taskOpt = getTaskById(taskId);

    if (taskOpt.isEmpty()) {
        throw new ResponseStatusException(HttpStatus.NOT_FOUND, "Task not found");
    
    }

    Task task  =  taskOpt.get(0);
    task.setTitle(title);
    task.setDescription(description);
    task.setDueDate(dueDate);
    if (status != null && !status.trim().isEmpty()) {
        task.setStatus(status);
    }
    task.setPriority(priority);

    taskRepo.save(task);
    return "Task updated: " + task.getTitle();
}

//changes the status of  a task
public Task updateTaskStatus(Long taskId, String status) {
    List<Task> taskList = getTaskById(taskId);

    if (taskList.isEmpty()) {
        throw new ResponseStatusException(HttpStatus.NOT_FOUND, "Task not found");
    }

    Task task = taskList.get(0);
    task.setStatus(status);
    return taskRepo.save(task);
}

//deletes a task based on the id provided
public String deleteTask(Long taskId){
    if (!taskRepo.existsById(taskId)){
            throw new ResponseStatusException(HttpStatus.NOT_FOUND, "Task not found");
        
    }
    taskRepo.deleteById(taskId);
    return "Task deleted";
}

// Notification for tasks due in the next 5 days
public List<Map<String, Object>> getNotifications(Long studentId) {
    LocalDate today = LocalDate.now();

    List<Task> dueSoon = taskRepo.findByAssignedToAndStatusNotAndDueDateBetween(studentId, "COMPLETED", today, today.plusDays(5));

    List<Map<String, Object>> notifications = new ArrayList<>();

    for (Task task : dueSoon) {
        long daysLeft = ChronoUnit.DAYS.between(today, task.getDueDate());

        String when;
        if (daysLeft == 0) {
            when = "is due today";
        } else if (daysLeft == 1) {
            when = "is due tomorrow";
        } else {
            when = "in " + daysLeft + " days";
        }

        // Create a notification map
        Map<String, Object> notification = new LinkedHashMap<>();
        notification.put("notificationId", task.getId());
        notification.put("taskId", task.getId());
        notification.put("message", "Task\"" + task.getTitle() + "\"" + when);
        notification.put("isRead", false);

        notifications.add(notification);
    }
    return notifications;
}
}