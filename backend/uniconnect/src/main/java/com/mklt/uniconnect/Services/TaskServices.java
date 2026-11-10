
package com.mklt.uniconnect.Services;

import java.time.LocalDate;
import java.util.ArrayList;
import java.util.Comparator;
import java.util.LinkedHashMap;
import java.util.List;
import java.util.Map;
import java.util.stream.Collectors;

import org.springframework.http.HttpStatus;
import org.springframework.stereotype.Service;
import org.springframework.web.server.ResponseStatusException;

import com.mklt.uniconnect.Entities.Student;
import com.mklt.uniconnect.Entities.Task;
import com.mklt.uniconnect.Helpers.ProjectMembers;
import com.mklt.uniconnect.Repositories.ProjectMembersRepo;
import com.mklt.uniconnect.Repositories.ProjectsRepo;
import com.mklt.uniconnect.Repositories.StudentRepo;
import com.mklt.uniconnect.Repositories.TaskRepo;

@Service
public class TaskServices {

    private final TaskRepo taskRepo;
    private final ProjectsRepo projectsRepo;
    private final ProjectMembersRepo projectMembersRepo;
    private final StudentRepo studentRepo;

    public TaskServices(
            TaskRepo taskRepo,
            ProjectsRepo projectsRepo,
            ProjectMembersRepo projectMembersRepo,
            StudentRepo studentRepo) {

        this.taskRepo = taskRepo;
        this.projectsRepo = projectsRepo;
        this.projectMembersRepo = projectMembersRepo;
        this.studentRepo = studentRepo;
    }

    // Creates a new task and saves it to the database.
    public Task createTask(Long projectId, Task task) {

        if (!projectsRepo.existsById(projectId)) {
            throw new ResponseStatusException(
                HttpStatus.NOT_FOUND, "Project not found");
        }

        if (task.getTitle() == null || task.getTitle().trim().isEmpty()) {
            throw new ResponseStatusException(
                HttpStatus.BAD_REQUEST, "Task title cannot be empty");
        }

        String status = (task.getStatus() == null)
                ? "NOT_STARTED"
                : task.getStatus();

        Task newTask = new Task(
            task.getTitle().trim(),
            task.getDescription(),
            task.getDueDate(),
            status,
            task.getPriority(),
            projectId,
            task.getAssignedTo()
        );

        return taskRepo.save(newTask);
    }

    // Returns tasks belonging to a project.
    public List<Task> getTaskByProject(Long projectId) {
        return taskRepo.findByProjectId(projectId);
    }

    // Returns calendar tasks for projects belonging to a student.
    public List<Task> getTasksWithDueDate(Long studentId) {

        Student student = studentRepo.findById(studentId)
            .orElseThrow(() -> new ResponseStatusException(
                HttpStatus.NOT_FOUND, "Student not found"));

        List<ProjectMembers> memberships =
            projectMembersRepo.findAllByStudent(student);

        List<Long> projectIds = memberships.stream()
            .map(membership -> membership.getProject().getID())
            .collect(Collectors.toList());

        if (projectIds.isEmpty()) {
            return new ArrayList<>();
        }

        return taskRepo.findByProjectIdInAndDueDateIsNotNull(projectIds);
    }

    // Returns one task based on its ID.
    public List<Task> getTaskById(Long taskId) {
        return taskRepo.findById(taskId)
            .map(List::of)
            .orElse(List.of());
    }

    // Updates the details of an existing task.
    public String updateTask(
            Long taskId,
            String title,
            String description,
            LocalDate dueDate,
            String status,
            String priority) {

        List<Task> taskOpt = getTaskById(taskId);

        if (taskOpt.isEmpty()) {
            throw new ResponseStatusException(
                HttpStatus.NOT_FOUND, "Task not found");
        }

        Task task = taskOpt.get(0);

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

    // Changes the status of a task.
    public Task updateTaskStatus(Long taskId, String status) {

        List<Task> taskList = getTaskById(taskId);

        if (taskList.isEmpty()) {
            throw new ResponseStatusException(
                HttpStatus.NOT_FOUND, "Task not found");
        }

        Task task = taskList.get(0);
        task.setStatus(status);

        return taskRepo.save(task);
    }

    // Deletes a task based on its ID.
    public String deleteTask(Long taskId) {

        if (!taskRepo.existsById(taskId)) {
            throw new ResponseStatusException(
                HttpStatus.NOT_FOUND, "Task not found");
        }

        taskRepo.deleteById(taskId);

        return "Task deleted";
    }

    // Generates deadline notifications for a student.
    public List<Map<String, Object>> getNotifications(Long studentId) {

        // Confirm that the student exists.
        studentRepo.findById(studentId)
            .orElseThrow(() -> new ResponseStatusException(
                HttpStatus.NOT_FOUND, "Student not found"));

        LocalDate today = LocalDate.now();
        LocalDate tomorrow = today.plusDays(1);

        // BACKEND:
        // Retrieve all tasks assigned to this student that have deadlines.
        // This includes overdue tasks.
        List<Task> assignedTasks =
            taskRepo.findByAssignedToAndDueDateIsNotNull(studentId);

        List<Map<String, Object>> notifications = new ArrayList<>();

        // Display notifications in deadline order.
        assignedTasks.stream()
            .sorted(Comparator.comparing(Task::getDueDate))
            .forEach(task -> {

                // BACKEND:
                // Completed tasks use the status "DONE".
                // Completed tasks must not generate notifications.
                if ("DONE".equalsIgnoreCase(
                        task.getStatus() == null
                            ? ""
                            : task.getStatus().trim())) {
                    return;
                }

                LocalDate dueDate = task.getDueDate();

                String message;
                String notificationType;

                // Overdue tasks.
                if (dueDate.isBefore(today)) {
                    message = "Task \"" + task.getTitle()
                        + "\" is overdue.";
                    notificationType = "OVERDUE";
                }

                // Tasks due today.
                else if (dueDate.isEqual(today)) {
                    message = "Task \"" + task.getTitle()
                        + "\" is due today.";
                    notificationType = "DUE_TODAY";
                }

                // Tasks due tomorrow.
                else if (dueDate.isEqual(tomorrow)) {
                    message = "Task \"" + task.getTitle()
                        + "\" is due tomorrow.";
                    notificationType = "DUE_TOMORROW";
                }

                // No notification for later deadlines.
                else {
                    return;
                }

                Map<String, Object> notification =
                    new LinkedHashMap<>();

                notification.put("notificationId", task.getId());
                notification.put("taskId", task.getId());
                notification.put("message", message);
                notification.put("dueDate", dueDate);
                notification.put("isRead", false);
                notification.put("notificationType", notificationType);

                notifications.add(notification);
            });

        return notifications;
    }
}
