package com.mklt.uniconnect.Entities;
import com.fasterxml.jackson.annotation.JsonProperty;
import jakarta.persistence.*;
import java.time.LocalDate;

@Entity

public class Task {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    @JsonProperty("taskId")
    private Long id;

    @JsonProperty("title")
    private String title;
    private String description;
    private LocalDate dueDate;
    private String status;
    private String priority;

    private Long projectId; // Which project this task belongs to

    
    private Long assignedTo; // the id of the student this task is assigned to
    
    public Task() {
    }

    // Constructor without projectId and assignedTo(old)
    public Task(String title, String description, LocalDate dueDate, String status, String priority){
        this.title = title;
        this.description = description;
        this.dueDate = dueDate;
        this.status = status;
        this.priority = priority;
    
    }
 
    // Constructor with projectId and assignedTo
    public Task(String title, String description, LocalDate dueDate, String status, String priority, Long projectId, Long assignedTo) {
        this.title = title;
        this.description = description;
        this.dueDate = dueDate;
        this.status = status;
        this.priority = priority;
        this.projectId = projectId;
        this.assignedTo = assignedTo;
    }

    //getters and setters are used to access and modify the private variables of a class
    public Long getId() { return id;}

    public String getTitle() { return title;}
    public void setTitle(String title) { this.title = title;}

    public String getDescription() { return description;}
    public void setDescription(String description) { this.description = description;}

    public LocalDate getDueDate() { return dueDate;}
    public void setDueDate(LocalDate dueDate) { this.dueDate = dueDate;}

    public String getStatus() { return status;}
    public void setStatus(String status) { this.status = status;}

    public String getPriority() { return priority;}
    public void setPriority(String priority) { this.priority = priority;}

    public Long getProjectId() { return projectId;}
    public void setProjectId(Long projectId) { this.projectId = projectId;}

    public Long getAssignedTo() { return assignedTo;}
    public void setAssignedTo(Long assignedTo) { this.assignedTo = assignedTo;}



}
