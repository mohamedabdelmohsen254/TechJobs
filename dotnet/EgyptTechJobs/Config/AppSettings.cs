using System;
using System.Collections.Generic;

namespace EgyptTechJobs.Config
{
    /// <summary>
    /// Application configuration settings
    /// </summary>
    public class AppSettings
    {
        // Filters
        public bool EgyptOnly { get; set; } = true;
        public bool TechOnly { get; set; } = true;
        public bool IncludeRemoteEgypt { get; set; } = true;
        public bool IncludeProduct { get; set; } = true;
        public bool IncludeDesign { get; set; } = false;

        // Performance
        public int Timeout { get; set; } = 20; // seconds
        public int MaxWorkers { get; set; } = 30; // concurrent threads

        // Jooble API Configuration
        public string JoobleApiKey { get; set; } = "08cfec52-7791-487b-9c40-a3e45efe9aa3";
        public bool JoobleEnabled { get; set; } = true;
        public int JoobleDaysBack { get; set; } = 14;
        public int JoobleMaxPages { get; set; } = 5;

        // Search Keywords for Jooble
        public List<string> JoobleSearchKeywords { get; set; } = new()
        {
            // Core Software Engineering
            "software engineer", "software developer", "programmer", "coder",
            "backend developer", "backend engineer", "frontend developer", "frontend engineer",
            
            // Database & SQL Developer Roles
            "oracle developer", "sql developer", "sql programmer", "database developer",
            "data engineer", "etl developer", "bi developer", "business intelligence developer",
            
            // Web Development
            "web developer", "full-stack developer", "nodejs developer", "python developer",
            "php developer", "java developer", "c# developer", "dotnet developer",
            
            // Mobile Development
            "mobile developer", "ios developer", "android developer", "react native developer",
            
            // DevOps & Cloud
            "devops engineer", "cloud engineer", "aws developer", "azure developer",
            "kubernetes developer", "docker engineer", "infrastructure engineer",
            
            // QA & Testing
            "qa engineer", "quality assurance", "automation tester", "test engineer",
            
            // Systems & Network
            "systems engineer", "network engineer", "system administrator", "database administrator",
            
            // Security
            "security engineer", "cybersecurity specialist", "application security engineer"
        };
    }
}
