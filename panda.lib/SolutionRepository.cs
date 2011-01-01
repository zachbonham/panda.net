using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using panda.lib.Entities;

namespace panda.lib
{
    public class SolutionRepository
    {
        public string SolutionPath { get; set; }
        List<ProjectItem> _projects = new List<ProjectItem>();

        public SolutionRepository(string solutionPath)
        {
            SolutionPath = solutionPath;

            _projects = ReadSolution(SolutionPath);
        }

        /// <summary>
        /// Rip off of http://tinyurl.com/35eokra
        /// </summary>
        /// <param name="solutionPath"></param>
        /// <returns></returns>
        List<ProjectItem> ReadSolution(string solutionPath)
        {
            if (!File.Exists(solutionPath))
            {
                throw new FileNotFoundException("Solution file not found: " + solutionPath);
            }

            var directory = Path.GetDirectoryName(solutionPath);
            var projects = new List<ProjectItem>();

            var content = File.ReadAllText(solutionPath);


            MatchCollection matches = Regex.Matches(content, @"^Project\(.*", RegexOptions.Multiline);

            foreach (Match match in matches)
            {
                var tokens = match.Value.Split(new char[] { ',', '=' }, StringSplitOptions.RemoveEmptyEntries);

                var trash = new char[] { '\"', '{', '}' };
                var relativePath = tokens[2].Trim().TrimStart('\"').TrimEnd('\"');
                
                var project = new ProjectItem()
                {
                    Name = tokens[1].Trim().TrimStart('\"').TrimEnd('\"'),
                    RelativePath = relativePath,
                    ProjectId = new Guid(Regex.Match(tokens[3], "{.*}").Value),
                    FullPath = Path.Combine(directory, relativePath) 
                };

                projects.Add(project);

            }

            return projects;

        }


        public IList<ProjectItem> All()
        {
            return _projects.Select(s => s).ToList();
        }

    }

}
