using System.Text.Json;
using System.Text.Json.Serialization;

namespace Emarsys
{
    public class EmarsysResponse
    {
        public int ReplyCode { get; set; }
        public string ReplyText { get; set; }

        public bool Valid => ReplyCode == 0;
    }

    public class EmarsysResponse<TModel> : EmarsysResponse
    {
        public TModel Data { get; set; }
    }
}
