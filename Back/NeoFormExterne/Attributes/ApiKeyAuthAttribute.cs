using Microsoft.AspNetCore.Mvc;

namespace NeoForm_Externe.Attributes
{
    public class ApiKeyAuthAttribute : TypeFilterAttribute
    {
        public ApiKeyAuthAttribute() : base(typeof(Filters.ApiKeyAuthFilter)) { }
    }
}
