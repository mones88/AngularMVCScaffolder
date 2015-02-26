using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularMVCScaffolder.TemplatesViewModels
{
    public class AngularControllerViewModel
    {
        private EnvDTE.CodeType _controllerType;
        private IEnumerable<string> _ngDependencies;

        public string GeneratedFileName
        {
            get
            {
                return this.ControllerName;
            }
        }

        public string ControllerName { get; private set; }

        public string ControllerDependencies { get; private set; }

        public string ControllerQuotedDependencies { get; private set; }

        public string[] SupportedHttpVerbs { get; private set; }

        public AngularControllerViewModel(EnvDTE.CodeType controllerType)
        {
            this._controllerType = controllerType;
            this.EstablishDependencies();
            this.ControllerName = Common.CamelCaseString(controllerType.Name);
            this.ControllerDependencies = GetDependencies(false);
            this.ControllerQuotedDependencies = GetDependencies(true);
            this.EstablishSupportedHttpVerbs();       
        }

        private void EstablishSupportedHttpVerbs()
        {
            var methods = this._controllerType.Members.Cast<EnvDTE.CodeElement>()
                .Where(ce => ce.Kind == EnvDTE.vsCMElement.vsCMElementFunction)
                .Cast<EnvDTE.CodeFunction>().Where(cf => cf.Access == EnvDTE.vsCMAccess.vsCMAccessPublic);
            var methodsName = new List<string>();
            foreach (var method in methods)
            {
                var uppercaseName = method.Name.ToUpperInvariant();
                if (!methodsName.Contains(uppercaseName))
                    methodsName.Add(uppercaseName);
            }
            this.SupportedHttpVerbs = methodsName.ToArray();
        }

        private void EstablishDependencies()
        {
            this._ngDependencies = new List<string>() { 
                //atm only the resource/service is a dep.
                CSharpToNgControllerHelpers.GetServiceName(this._controllerType.Name) 
            };
        }

        private string GetDependencies(bool quoted)
        {
            if (this._ngDependencies == null || !this._ngDependencies.Any())
                return "";

            IEnumerable<string> deps;
            if (quoted)
            {
                deps = this._ngDependencies.Select(d =>
                    String.Format("'{0}'", d));
            }
            else
            {
                deps = this._ngDependencies;
            }
            return String.Join(", ", deps);
        }
    }
}
