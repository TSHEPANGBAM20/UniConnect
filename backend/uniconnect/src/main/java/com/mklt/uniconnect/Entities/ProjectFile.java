package com.mklt.uniconnect.Entities;

import java.time.LocalDateTime;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.JoinColumn;
import jakarta.persistence.ManyToOne;
import jakarta.persistence.Table;

// One uploaded file that belongs to a project. A project can have many of these.
@Entity
@Table(name = "project_files")
public class ProjectFile {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @ManyToOne(optional = false)
    @JoinColumn(name = "projectid")
    private Project project;

    private String fileName;     // the cleaned original name, shown to users
    private String storedName;   // the unique name used on disk (never taken from the client)
    private String contentType;
    private long sizeBytes;

    @ManyToOne
    @JoinColumn(name = "uploaded_by")
    private Student uploadedBy;

    private LocalDateTime uploadedAt;

    public ProjectFile() {
    } // empty constructor for JPA

    public ProjectFile(Project project, String fileName, String storedName, String contentType,
                       long sizeBytes, Student uploadedBy, LocalDateTime uploadedAt) {
        this.project = project;
        this.fileName = fileName;
        this.storedName = storedName;
        this.contentType = contentType;
        this.sizeBytes = sizeBytes;
        this.uploadedBy = uploadedBy;
        this.uploadedAt = uploadedAt;
    }

    public Long getId() {
        return id;
    }

    public Project getProject() {
        return project;
    }

    public String getFileName() {
        return fileName;
    }

    public String getStoredName() {
        return storedName;
    }

    public String getContentType() {
        return contentType;
    }

    public long getSizeBytes() {
        return sizeBytes;
    }

    public Student getUploadedBy() {
        return uploadedBy;
    }

    public LocalDateTime getUploadedAt() {
        return uploadedAt;
    }
}
