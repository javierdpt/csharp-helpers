using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace GS.MFH.Lease.Api.Infrastructure.DataTables
{
    /// <summary>
    /// DataTableParamModelBinderProvider class
    /// </summary>
    public class DataTableParamModelBinderProvider : IModelBinderProvider
    {
        private readonly IModelBinder _binder =
            new DataTableParamModelBinder();

        /// <summary>
        /// Binder definition
        /// </summary>
        /// <param name="context"></param>
        /// <returns>Return the IModelBinder definition</returns>
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            return context.Metadata.ModelType == typeof(DataTableRequestModel) ? _binder : null;
        }
    }
}