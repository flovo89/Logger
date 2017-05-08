using System.IO;
using System.Text;




namespace Logger.Core.Resources
{
    public interface IResourceConverter
    {
        bool ConvertToObject (Stream stream, Encoding encoding, string targetType, out object value);
    }
}
