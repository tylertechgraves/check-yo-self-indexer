using check_yo_self_indexer.Server;

namespace check_yo_self_indexer.Entities
{
    public class UrlAdjuster
    {
        public static string ReplaceHost(string input)
        {
            return string.IsNullOrWhiteSpace(input) ? input : input.Replace("{host}", Context.GetHost());
        }

        public static string SetTrailingSlash(string input, bool addSlash = true) 
        {
            if(!string.IsNullOrWhiteSpace(input))
            {
                var trimmed = input.TrimEnd('/');
                return addSlash ? trimmed + "/" : trimmed;
            }
            return input;
        }

        public static string ReplaceHostAndSetTrailingSlash(string input, bool addSlash = true) {
            return SetTrailingSlash(ReplaceHost(input), addSlash);
        }
    }
}