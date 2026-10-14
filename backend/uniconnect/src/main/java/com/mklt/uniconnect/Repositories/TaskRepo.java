package com.mklt.uniconnect.Repositories;

import org.springframework.data.jpa.repository.JpaRepository;

import com.mklt.uniconnect.Entities.Task;

public interface TaskRepo extends JpaRepository<Task, Long> {
    
}
