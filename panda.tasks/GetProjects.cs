using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Build.Utilities;
using Microsoft.Build.Framework;
using panda.lib;

namespace panda.tasks
{
    public class GetProjects : Task
    {
        [Required]
        public string SolutionPath { get; set; }

        [Output]
        public ITaskItem[] Projects { get; set; }

        public override bool Execute()
        {
            var solution = new SolutionRepository(SolutionPath);

            var projects = solution.All();

            var results = new List<TaskItem>(projects.Count);

            foreach (var project in projects)
            {
                var taskItem = new TaskItem("Project");

                taskItem.SetMetadata("Name", project.Name);
                taskItem.SetMetadata("RelativePath", project.RelativePath);
                taskItem.SetMetadata("ProjectId", project.ProjectId.ToString());
                taskItem.SetMetadata("ProjectPath", project.FullPath);
                results.Add(taskItem);
            }

            Projects = results.ToArray();

            return true;
        }
    }
}
