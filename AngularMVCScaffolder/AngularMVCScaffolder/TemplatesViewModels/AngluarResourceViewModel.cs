using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularMVCScaffolder.TemplatesViewModels
{
    public class AngularResourceViewModel
    {
        private EnvDTE.CodeType _controllerType;
        private List<string> _dependencies;
        private IEnumerable<EnvDTE.CodeFunction> _controllerMethods;

        public string GeneratedFileName
        {
            get
            {
                return this.ServiceName + "_ngResource";
            }
        }

        public string ServiceName { get; private set; }

        public IEnumerable<INgViewModel> Methods { get; private set; }

        public AngularResourceViewModel(EnvDTE.CodeType controllerType)
        {
            this._controllerType = controllerType;
            this._controllerMethods = controllerType.Members
                .Cast<EnvDTE.CodeElement>().Where(ce => ce.Kind == EnvDTE.vsCMElement.vsCMElementFunction)
                .Cast<EnvDTE.CodeFunction>().Where(cf => cf.Access == EnvDTE.vsCMAccess.vsCMAccessPublic);
            this.ServiceName = CSharpToNgControllerHelpers.GetServiceName(_controllerType.Name);
            this.Methods = this._controllerMethods.Select(cf => new WebApiControllerMethodViewModel(cf));
            this.EstablishDependencies();
        }

        private void EstablishDependencies()
        {
            this._dependencies = new List<string>();
            //One required dependencies will surely be the service..
            this._dependencies.Add(this.ServiceName);
        }
    }
}
