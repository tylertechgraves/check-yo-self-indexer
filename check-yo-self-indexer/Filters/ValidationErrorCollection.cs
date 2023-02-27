using System.Collections.Generic;
using System.Text;

namespace check_yo_self_indexer.Server.Filters;

/// <summary>
/// A collection of ValidationError objects that is used to collect
/// errors that occur duing calls to the Validate method.
/// </summary>
public class ValidationErrorCollection : List<ValidationError>
{
    /// <summary>
    /// Adds a new error to the collection
    /// <seealso>Class ValidationErrorCollection</seealso>
    /// </summary>
    /// <param name="message">
    /// Message of the error
    /// </param>
    /// <param name="fieldName">
    /// optional field name that it applies to (used for Databinding errors on 
    /// controls)
    /// </param>
    /// <param name="id">
    /// An optional ID you assign the error
    /// </param>
    /// <returns>Void</returns>
    public void Add(string message, string fieldName = "", string id = "")
    {
        var error = new ValidationError()
        {
            Message = message,
            ControlID = fieldName,
            ID = id
        };
        Add(error);
    }

    /// <summary>
    /// Like Add but allows specifying of a format  
    /// </summary>
    /// <param name="message"></param>
    /// <param name="fieldName"></param>
    /// <param name="id"></param>
    /// <param name="arguments"></param>
    public void AddFormat(string message, string fieldName, string id, params object[] arguments)
    {
        Add(string.Format(message, arguments), fieldName, id);
    }

    /// <summary>
    /// Removes the item specified in the index from the Error collection
    /// </summary>
    /// <param name="index"></param>
    public void Remove(int index)
    {
        if (index > Count - 1 || index < 0)
            RemoveAt(index);
    }

    /// <summary>
    /// Adds a validation error if the condition is true. Otherwise no item is 
    /// added.
    /// <seealso>Class ValidationErrorCollection</seealso>
    /// </summary>
    /// <param name="condition">
    /// If true this error is added. Otherwise not.
    /// </param>
    /// <param name="message">
    /// The message for this error
    /// </param>
    /// <param name="fieldName">
    /// Name of the UI field (optional) that this error relates to. Used optionally
    ///  by the databinding classes.
    /// </param>
    /// <param name="id">
    /// An optional Error ID.
    /// </param>
    /// <returns>value of condition</returns>
    public bool Assert(bool condition, string message, string fieldName = null, string id = null)
    {
        if (condition)
            Add(message, fieldName, id);

        return condition;
    }

    /// <summary>
    /// Asserts a business rule - if condition is true it's added otherwise not.
    /// <seealso>Class ValidationErrorCollection</seealso>
    /// </summary>
    /// <param name="condition">
    /// If this condition evaluates to true the Validation Error is added
    /// </param>
    /// <param name="error">
    /// Validation Error Object
    /// </param>
    /// <returns>value of condition</returns>
    public bool Assert(bool condition, ValidationError error)
    {
        if (condition)
            Add(error);

        return condition;
    }


    /// <summary>
    /// Returns a string representation of the errors in this collection.
    /// The string is separated by CR LF after each line.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        if (Count < 1)
            return "";

        var sb = new StringBuilder(128);

        foreach (var error in this)
        {
            sb.AppendLine(error.Message);
        }

        return sb.ToString();
    }

    /// <summary>
    /// Returns an HTML representation of the errors in this collection.
    /// The string is returned as an HTML unordered list.
    /// </summary>
    /// <returns></returns>
    public string ToHtml()
    {
        if (Count < 1)
            return "";

        var sb = new StringBuilder(256);
        sb.Append("<ul>\r\n");

        foreach (var error in this)
        {
            sb.Append("<li>");
            if (error.ControlID != null && error.ControlID != "")
                sb.AppendFormat("<a href='#' onclick=\"_errorLinkClick('{0}');return false;\" " +
                              "style='text-decoration:none'>{1}</a>",
                              error.ControlID.Replace(".", "_"), error.Message);
            else
                sb.Append(error.Message);

            sb.AppendLine("</li>");
        }

        sb.Append("</ul>\r\n");
        var script =
        @"    <script>
                function _errorLinkClick(id) {
                    var $t = $('#' + id).focus().addClass('errorhighlight');            
                    setTimeout(function() {
                        $t.removeClass('errorhighlight');
                    }, 5000);
                }
            </script>";
        sb.AppendLine(script);

        return sb.ToString();
    }
}
