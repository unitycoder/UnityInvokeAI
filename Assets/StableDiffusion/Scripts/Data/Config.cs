// https://json2csharp.com/
// Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);

namespace UnityLibrary
{
    public class Config
    {
        public string prompt { get; set; }
        public string iterations { get; set; }
        public string steps { get; set; }
        public string cfg_scale { get; set; }
        public string sampler_name { get; set; }
        public string width { get; set; }
        public string height { get; set; }
        public long seed { get; set; }
        public string variation_amount { get; set; }
        public string with_variations { get; set; }
        public string initimg { get; set; }
        public string strength { get; set; }
        public string fit { get; set; }
        public string gfpgan_strength { get; set; }
        public string upscale_level { get; set; }
        public string upscale_strength { get; set; }
    }

    public class Root
    {
        public string @event { get; set; }
        public string url { get; set; }
        public long seed { get; set; }
        public Config config { get; set; }
    }
}