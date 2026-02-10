using Fluid.Parser;
using System.IO;
using System.Threading.Tasks;

namespace Fluid.ViewEngine
{
    public interface IFluidViewRenderer
    {
        Task RenderViewAsync(TextWriter writer, string path, TemplateContext context);
        Task RenderTemplateAsync(TextWriter writer, string templateString, TemplateContext context);
        Task RenderTemplateAsync(TextWriter writer, FluidTemplate template, TemplateContext context);
        Task RenderPartialAsync(TextWriter writer, string path, TemplateContext context);
        Task RenderViewAsync(IFluidOutput output, string path, TemplateContext context);
        Task RenderPartialAsync(IFluidOutput output, string path, TemplateContext context);
    }
}
