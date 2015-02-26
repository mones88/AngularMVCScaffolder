using AngularMVCScaffolder.TemplatesViewModels;
using AngularMVCScaffolder.UI;
using Microsoft.AspNet.Scaffolding;
using System.Collections.Generic;
using System.Linq;

namespace AngularMVCScaffolder
{
    public class CustomCodeGenerator : CodeGenerator
    {
        CustomViewModel _viewModel;

        /// <summary>
        /// Constructor for the custom code generator
        /// </summary>
        /// <param name="context">Context of the current code generation operation based on how scaffolder was invoked(such as selected project/folder) </param>
        /// <param name="information">Code generation information that is defined in the factory class.</param>
        public CustomCodeGenerator(
            CodeGenerationContext context,
            CodeGeneratorInformation information)
            : base(context, information)
        {
            _viewModel = new CustomViewModel(Context);
        }


        /// <summary>
        /// Any UI to be displayed after the scaffolder has been selected from the Add Scaffold dialog.
        /// Any validation on the input for values in the UI should be completed before returning from this method.
        /// </summary>
        /// <returns></returns>
        public override bool ShowUIAndValidate()
        {
            // Bring up the selection dialog and allow user to select a model type
            SelectModelWindow window = new SelectModelWindow(_viewModel);
            bool? showDialog = window.ShowDialog();
            return showDialog ?? false;
        }

        /// <summary>
        /// This method is executed after the ShowUIAndValidate method, and this is where the actual code generation should occur.
        /// In this example, we are generating a new file from t4 template based on the ModelType selected in our UI.
        /// </summary>
        public override void GenerateCode()
        {
            this.CheckAndCreateDirectoriesStructure();

            // Get the selected code type
            var codeType = _viewModel.SelectedModelType.CodeType;
            var angularResourceViewModel = new AngularResourceViewModel(codeType);

            if (angularResourceViewModel.Methods.Any())
            {
                // Setup the scaffolding item creation parameters to be passed into the T4 template.
                var parameters = new Dictionary<string, object>() {
                    {"ServiceName", angularResourceViewModel.ServiceName},
                    {"Methods", angularResourceViewModel.Methods.Select(ngViewModel => ngViewModel.ToNgString()).ToArray()}
                };

                // Add the custom scaffolding item from T4 template.
                this.AddFileFromTemplate(
                    project: GetWebSiteProject(),
                    outputPath: "Scripts\\app\\services\\" + angularResourceViewModel.GeneratedFileName,
                    templateName: "AngularResource",
                    templateParameters: parameters,
                    skipIfExists: false);


                var angularControllerViewModel = new AngularControllerViewModel(codeType);
                parameters = new Dictionary<string, object>() {
                    {"ControllerName", angularControllerViewModel.ControllerName},
                    {"ControllerQuotedDependencies", angularControllerViewModel.ControllerQuotedDependencies},
                    {"ControllerDependencies", angularControllerViewModel.ControllerDependencies},
                    {"SupportedHttpVerbs", angularControllerViewModel.SupportedHttpVerbs}
                };
                this.AddFileFromTemplate(
                    project: GetWebSiteProject(),
                    outputPath: "Scripts\\app\\controllers\\" + angularControllerViewModel.GeneratedFileName,
                    templateName: "AngularController",
                    templateParameters: parameters,
                    skipIfExists: false);
            }
        }

        private EnvDTE.Project GetWebSiteProject() 
        {
            var solution = this.Context.ActiveProject.DTE.Solution;
            var webApiProject = solution.Projects.Cast<EnvDTE.Project>().First(p => p.Name.EndsWith(".WebSite"));
            return webApiProject;
        }

        private const string KIND_FOLDER = "{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}";
        private static readonly string[] DirectoriesStructure = { 
            @"Scripts\app\services",
            @"Scripts\app\controllers"
        };

        private void CheckAndCreateDirectoriesStructure()
        {
            foreach (var dirPath in DirectoriesStructure)
            {
                var splittedPath = dirPath.Split('\\');
                BuildStructure(this.Context.ActiveProject.ProjectItems, splittedPath, 0);
            }
            this.Context.ActiveProject.Save();
        }

        private static void BuildStructure(EnvDTE.ProjectItems projectItems, string[] levels, int currentLevel)
        {
            if (currentLevel == levels.Length)
                return;

            var folderName = levels[currentLevel];
            var toBeCreated = true;
            EnvDTE.ProjectItem newProjectItem = null;
            foreach (EnvDTE.ProjectItem pi in projectItems)
            {
                if (pi.Name == folderName && pi.Kind == KIND_FOLDER)
                {
                    newProjectItem = pi;
                    toBeCreated = false;
                    break;
                }
            }
            if (toBeCreated)
            {
                newProjectItem = projectItems.AddFolder(folderName, KIND_FOLDER);
            }
            BuildStructure(newProjectItem.ProjectItems, levels, currentLevel + 1);
        }
    }
}
