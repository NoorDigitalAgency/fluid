using Fluid.Parser;
using Fluid.Utils;
using Fluid.ViewEngine;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading.Tasks;

namespace Fluid.MvcViewEngine
{
    /// <summary>
    /// This class is registered as a singleton.
    /// </summary>
    public class FluidRendering
    {
        private readonly IFluidViewRenderer _fluidViewRenderer;

        public FluidRendering(
            IOptions<FluidMvcViewOptions> optionsAccessor,
            IFluidViewRenderer fluidViewRenderer,
            IWebHostEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            _options = optionsAccessor.Value;

            _options.TemplateOptions.FileProvider = _options.PartialsFileProvider ?? _hostingEnvironment.ContentRootFileProvider;

            _fluidViewRenderer = fluidViewRenderer;

            _options.ViewsFileProvider ??= _hostingEnvironment.ContentRootFileProvider;
        }

        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly FluidMvcViewOptions _options;

        public async Task RenderAsync(TextWriter writer, string path, ViewContext viewContext)
        {
            var context = new TemplateContext(_options.TemplateOptions);
            context.SetValue("ViewData", viewContext.ViewData);
            context.SetValue("ModelState", viewContext.ModelState);
            context.SetValue("Model", viewContext.ViewData.Model);

            if (_options.RenderingViewAsync != null)
            {
                await _options.RenderingViewAsync.Invoke(path, viewContext, context);
            }

            var bufferSize = context.Options?.OutputBufferSize ?? 16 * 1024;
            if (bufferSize <= 0)
            {
                bufferSize = 16 * 1024;
            }

            await using var output = new TextWriterFluidOutput(writer, bufferSize, leaveOpen: true, allowSynchronousIO: false);
            await _fluidViewRenderer.RenderViewAsync(output, path, context);
            await output.FlushAsync();
        }

        public async Task RenderTemplateAsync(TextWriter writer, string templateString, ViewContext viewContext)
        {
            var template = _options.Parser.Parse(templateString);

            if (template is FluidTemplate t)
            {
                await RenderTemplateAsync(writer, t, viewContext);
            }
        }

        public async Task RenderTemplateAsync(TextWriter writer, FluidTemplate template, ViewContext viewContext)
        {
            var context = new TemplateContext(_options.TemplateOptions);
            context.SetValue("ViewData", viewContext.ViewData);
            context.SetValue("ModelState", viewContext.ModelState);
            context.SetValue("Model", viewContext.ViewData.Model);

            if (_options.RenderingViewAsync != null)
            {
                await _options.RenderingTemplateAsync.Invoke(template, viewContext, context);
            }

            var bufferSize = context.Options?.OutputBufferSize ?? 16 * 1024;
            if (bufferSize <= 0)
            {
                bufferSize = 16 * 1024;
            }

            await using var output = new TextWriterFluidOutput(writer, bufferSize, leaveOpen: true);
            await _fluidViewRenderer.RenderTemplateAsync(output, template, context);
            await output.FlushAsync();
        }
    }
}
