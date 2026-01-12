using System.ComponentModel;
using Nuke.Common.Tooling;

namespace BIMPlugins.Nuke
{
    [TypeConverter(typeof(TypeConverter<Configuration>))]
    public class Configuration : Enumeration
    {
        public static Configuration Release = new Configuration { Value = nameof(Release) };
        public static Configuration R2019 = new Configuration { Value = nameof(R2019) };
        public static Configuration R2020 = new Configuration { Value = nameof(R2020) };
        public static Configuration R2021 = new Configuration { Value = nameof(R2021) };
        public static Configuration R2022 = new Configuration { Value = nameof(R2022) };
        public static Configuration R2023 = new Configuration { Value = nameof(R2023) };
        public static Configuration R2024 = new Configuration { Value = nameof(R2024) };
        public static Configuration R2025 = new Configuration { Value = nameof(R2025) };
        public static Configuration R2026 = new Configuration { Value = nameof(R2026) };

        public static implicit operator string(Configuration configuration)
        {
            return configuration.Value;
        }
    }
}