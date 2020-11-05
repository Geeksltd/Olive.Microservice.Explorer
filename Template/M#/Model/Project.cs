using MSharp;

namespace App
{
    public class Project : MSharp.Project
    {
        public Project()
        {
            Name("MY.MICROSERVICE.NAME")
                .SolutionFile("MY.MICROSERVICE.NAME.sln")
                .IsMicroservice()
                .NetCore();

            Layout("Default").Default().AjaxRedirect().VirtualPath("~/Views/Layouts/Default.cshtml");
            Layout("Modal").Modal().VirtualPath("~/Views/Layouts/Modal.cshtml");

            AutoTask("Clean old temp uploads").Every(10, TimeUnit.Minute)
                .Run(@"await Olive.Context.Current.GetService<Olive.Mvc.IFileRequestService>()
                .DeleteTempFiles(olderThan: 1.Hours());");

            // Note: Often in micro-services you can have a large number of roles.
            // In the following example we're creating a large list of role/level permissions
            // that can be used in the UI M# definitions.
            foreach (var role in "Dev,QA,BA,PM,AM,Director,Designer,IT,Reception,PA,Sales".Split(','))
                foreach (var level in ",Junior,Senior,Lead,Head".Split(','))
                    Role(level + role);
        }
    }
}