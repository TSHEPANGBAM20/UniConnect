package com.mklt.uniconnect.Repositories;

import java.util.List;
import java.time.LocalDate;

import org.springframework.data.jpa.repository.JpaRepository;

import com.mklt.uniconnect.Entities.Task;

public interface TaskRepo extends JpaRepository<Task, Long> {

    List<Task> findByProjectId(Long projectId); // Find tasks by project ID

    List<Task> findByAssignedToAndStatusNotAndDueDateBetween(Long assignedTo, String status, LocalDate start, LocalDate end);

    List<Task> findByDueDateIsNotNull(); // Find tasks that have a due date
    
}
