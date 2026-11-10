
package com.mklt.uniconnect.Repositories;

import java.time.LocalDate;
import java.util.List;

import org.springframework.data.jpa.repository.JpaRepository;

import com.mklt.uniconnect.Entities.Task;

public interface TaskRepo extends JpaRepository<Task, Long> {

    // Find tasks by project ID.
    List<Task> findByProjectId(Long projectId);

    // Find tasks assigned to a student with a specific status and date range.
    List<Task> findByAssignedToAndStatusNotAndDueDateBetween(
        Long assignedTo,
        String status,
        LocalDate start,
        LocalDate end
    );

    // Find all tasks that have a due date.
    List<Task> findByDueDateIsNotNull();

    // Find tasks belonging to selected projects that have due dates.
    List<Task> findByProjectIdInAndDueDateIsNotNull(List<Long> projectId);

    // BACKEND:
    // Retrieve all tasks assigned to a particular student
    // that have a due date, including overdue tasks.
    List<Task> findByAssignedToAndDueDateIsNotNull(Long assignedTo);
}
