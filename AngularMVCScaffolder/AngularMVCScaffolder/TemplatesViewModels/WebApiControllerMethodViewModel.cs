using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngularMVCScaffolder.TemplatesViewModels
{
    public class WebApiControllerMethodViewModel : INgViewModel
    {
        private EnvDTE.CodeFunction _codeFunction;

        public string MethodName { get; set; }

        public string HttpMethod { get; set; }

        public bool IsArray { get; set; }

        public Dictionary<string, string> Parameters { get; set; }

        public WebApiControllerMethodViewModel(EnvDTE.CodeFunction codeFunction)
        {
            this._codeFunction = codeFunction;
            this.Parameters = new Dictionary<string, string>();
            this.DetermineHttpMethod();
            this.IsArray = codeFunction.Type.TypeKind == EnvDTE.vsCMTypeRef.vsCMTypeRefArray || codeFunction.Type.AsFullName.Contains("IEnumerable");
            this.DetermineMethodName();
            this.LoadParameters();
        }

        private void DetermineHttpMethod()
        {
            var attrs = this._codeFunction.Attributes.Cast<EnvDTE.CodeAttribute>();
            if (attrs.Any(a => a.Name == "HttpGetAttribute"))
            {
                this.HttpMethod = "GET";
            }
            else if (attrs.Any(a => a.Name == "HttpPostAttribute"))
            {
                this.HttpMethod = "POST";
            }
            else if (attrs.Any(a => a.Name == "HttpPutAttribute"))
            {
                this.HttpMethod = "PUT";
            }
            else if (attrs.Any(a => a.Name == "HttpDeleteAttribute"))
            {
                this.HttpMethod = "DELETE";
            }
            else
            {
                this.HttpMethod = this._codeFunction.Name.ToUpperInvariant();
            }
        }

        private void LoadParameters()
        {
            var validParameters = this._codeFunction.Parameters.Cast<EnvDTE.CodeParameter>()
                .Where(cp => !cp.Attributes.Cast<EnvDTE.CodeAttribute>().Any(attr => attr.Name == "FromBodyAttribute"));
            foreach (var p in validParameters)
            {
                var name = Common.CamelCaseString(p.Name);
                this.Parameters.Add(name, "@" + name);
            }
        }

        private void DetermineMethodName()
        {
            switch (this.HttpMethod)
            {
                case "GET":
                    this.MethodName = (this.IsArray ? "query" : "get");
                    break;

                case "POST":
                    this.MethodName = "save";
                    break;

                case "DELETE":
                    this.MethodName = "delete";
                    break;

                case "PUT":
                    this.MethodName = "update";
                    break;

                default:
                    this.MethodName = Common.CamelCaseString(this._codeFunction.Name);
                    break;
            }
            if (this.IsArray && this._codeFunction.Parameters.Count == 0)
            {
                this.MethodName += "All";
            }
        }

        public string ToNgString()
        {
            var parameters = String.Join(", ", this.Parameters.Select(kvp => String.Format("{0} : '{1}'", kvp.Key, kvp.Value)));
            var js = String.Format("'{0}': {{ method: '{1}', params: {{ {2} }}, isArray:{3} }},", this.MethodName, this.HttpMethod, parameters, this.IsArray.ToString().ToLowerInvariant());
            return js;
        }
    }
}
