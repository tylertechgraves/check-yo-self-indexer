using System.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace check_yo_self_indexer.Server.Filters;


public class ApiError
{
    public string Message { get; set; }
    public bool IsError { get; set; }
    public string Detail { get; set; }
    public ValidationErrorCollection Errors { get; set; }

    public ApiError(string message)
    {
        Message = message;
        IsError = true;
    }

    public ApiError(ModelStateDictionary modelState)
    {
        IsError = true;
        if (modelState != null && modelState.Any(m => m.Value.Errors.Count > 0))
        {
            Message = "Please correct the specified errors and try again.";
            //errors = modelState.SelectMany(m => m.Value.Errors).ToDictionary(m => m.Key, m=> m.ErrorMessage);
            //errors = modelState.SelectMany(m => m.Value.Errors.Select( me => new KeyValuePair<string,string>( m.Key,me.ErrorMessage) ));
            //errors = modelState.SelectMany(m => m.Value.Errors.Select(me => new ModelError { FieldName = m.Key, ErrorMessage = me.ErrorMessage }));
        }
    }
}
