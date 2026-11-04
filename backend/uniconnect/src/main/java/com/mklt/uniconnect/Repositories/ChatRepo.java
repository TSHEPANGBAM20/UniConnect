package com.mklt.uniconnect.Repositories;

import org.springframework.data.jpa.repository.JpaRepository;
import org.springframework.data.jpa.repository.Query;
import org.springframework.data.repository.query.Param;
import java.util.Optional;


import com.mklt.uniconnect.Entities.Chat;
import com.mklt.uniconnect.Entities.Project;

public interface ChatRepo extends JpaRepository<Chat, Long> {
@Query("SELECT c FROM Chat c WHERE c.project.projectID = :projectId")
    Optional<Chat> findChatByProjectId(@Param("projectId") Long projectId);
    void deleteByProject(Project project);
    Chat findByProject(Project project);
}
