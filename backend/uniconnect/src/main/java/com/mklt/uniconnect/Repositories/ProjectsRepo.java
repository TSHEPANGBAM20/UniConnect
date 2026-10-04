package com.mklt.uniconnect.Repositories;

import java.util.List;

import org.springframework.data.jpa.repository.JpaRepository;

import com.mklt.uniconnect.Entities.Project;

public interface ProjectsRepo extends JpaRepository<Project, Long> {
boolean existsByProjectName(String projectName);
List <Long> findByProjectID(Long id);

}
