package com.mklt.uniconnect.Entities;

import jakarta.persistence.Entity;
import jakarta.persistence.GeneratedValue;
import jakarta.persistence.GenerationType;
import jakarta.persistence.Id;
import jakarta.persistence.JoinColumn;
import jakarta.persistence.OneToOne;

@Entity 
public class Chat {

    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;

    @OneToOne
    @JoinColumn(name = "projectID") 
    private Project project;


    //constructors
    public Chat() {
    }

        public Chat(Project project) {
            this.project = project;
    }

    //getters
       public Project getProject() {
            return project;
    }

       public Long getID() {
            return id;
    }


}
