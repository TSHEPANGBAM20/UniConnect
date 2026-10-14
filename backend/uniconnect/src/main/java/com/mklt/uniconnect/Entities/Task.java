package com.mklt.uniconnect.Entities;
import jakarta.persistence.*;
import java.time.LocalDate;

@Entity

public class Task {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    private String title;
    private String description;
    private LocalDate dueDate;
    private String status;
    private String priority;
    
    public Task() {
    }

    public Task(String title, String description, LocalDate dueDate, String status, String priority){
        this.title = title;
        this.description = description;
        this.dueDate = dueDate;
        this.status = status;
        this.priority = priority;
    
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



}
