using Microsoft.AspNet.Scaffolding;
using Microsoft.AspNet.Scaffolding.EntityFramework;
using System.Collections.Generic;
using System.Linq;

namespace AngularMVCScaffolder.UI
{
    /// <summary>
    /// View model for code types so that it can be displayed on the UI.
    /// </summary>
    public class CustomViewModel
    {
        /// <summary>
        /// This gets all the Model types from the active project.
        /// </summary>
        public IEnumerable<ModelType> ModelTypes { get; private set; }

        public ModelType SelectedModelType { get; set; }

        public CodeGenerationContext Context { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context">The code generation context</param>
        public CustomViewModel(CodeGenerationContext context)
        {
            Context = context;
            ICodeTypeService codeTypeService = (ICodeTypeService)Context.ServiceProvider.GetService(typeof(ICodeTypeService));

            this.ModelTypes = codeTypeService.GetAllCodeTypes(Context.ActiveProject)
                //.Where(codeType => codeType.IsValidWebProjectEntityType())
                                            .Where(codeType => codeType.IsDerivedType("System.Web.Http.ApiController"))
                                            .Select(codeType => new ModelType(codeType));
        }
    }
}
