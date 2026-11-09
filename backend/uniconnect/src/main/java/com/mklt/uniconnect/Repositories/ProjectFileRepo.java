package com.mklt.uniconnect.Repositories;

import java.util.List;

import org.springframework.data.jpa.repository.JpaRepository;

import com.mklt.uniconnect.Entities.Project;
import com.mklt.uniconnect.Entities.ProjectFile;

public interface ProjectFileRepo extends JpaRepository<ProjectFile, Long> {

    // newest first, so the first item is the "latest" file
    List<ProjectFile> findAllByProjectOrderByUploadedAtDesc(Project project);
}
