package com.mklt.uniconnect.Repositories;

import java.util.List;

import org.springframework.data.jpa.repository.JpaRepository;

import com.mklt.uniconnect.Entities.Project;
import com.mklt.uniconnect.Entities.Student;
import com.mklt.uniconnect.Helpers.ProjectMembers;

public interface ProjectMembersRepo extends JpaRepository<ProjectMembers,Long> {
List<ProjectMembers> findAllByStudent(Student student);
boolean existsByStudentAndProject(Student student,Project project);
List<ProjectMembers> findAllByProject(Project project);
ProjectMembers findByStudentAndProject(Student student, Project project);
void deleteAllByProject(Project project);



}
