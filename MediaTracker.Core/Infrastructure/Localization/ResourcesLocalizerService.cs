using System.Collections.Concurrent;
using System.Globalization;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace MediaTracker.Core.Infrastructure.Localization
{
    public class ResourcesLocalizerFactory : IStringLocalizerFactory
    {
        static ResourcesLocalizer Localizer;

        public static void SetLocalizer(ResourcesLocalizer localizer)
        {
            if (Localizer != null)
            {
                throw new InvalidOperationException();
            }

            Localizer = localizer;
        }

        public IStringLocalizer Create(Type resourceSource)
        {
            return Localizer;
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            return Localizer;
        }
    }

    public class ResourcesLocalizer : IStringLocalizer
    {
        class CultureStrings
        {
            public DateTime Timestamp { get; set; }

            public Dictionary<string, string> Records { get; set; }
        }

        static readonly DateTime _stringsTimestamp = DateTime.MinValue;
        static readonly ConcurrentDictionary<string, CultureStrings> _strings = [];

        public ResourcesLocalizer(CultureInfo culture = null)
        {
            ResourcesLocalizerFactory.SetLocalizer(this);
        }

        readonly CultureInfo _culture;

        LocalizedString IStringLocalizer.this[string name]
        {
            get
            {
                var value = GetString(name, _culture ?? CultureInfo.CurrentUICulture);
                return new LocalizedString(name, value ?? name, resourceNotFound: value == null);
            }
        }

        LocalizedString IStringLocalizer.this[string name, params object[] arguments]
        {
            get
            {
                var value = GetString(name, _culture ?? CultureInfo.CurrentUICulture);
                return new LocalizedString(name, string.Format(value ?? name, arguments), resourceNotFound: value == null);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            return Read(_culture ?? CultureInfo.CurrentUICulture)?.Select(record => new LocalizedString(record.Key, record.Value, resourceNotFound: record.Value == null));
        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            return new ResourcesLocalizer(culture);
        }

        string GetString(string key, CultureInfo culture)
        {
            Read(culture).TryGetValue(key, out var value);
            return value;
        }

        Dictionary<string, string> Read(CultureInfo culture)
        {

            if (!_strings.TryGetValue(culture.TwoLetterISOLanguageName, out var cultureStrings) || cultureStrings.Timestamp < DateTime.UtcNow.AddMinutes(-5))
            {
                string filePath = Path.GetFullPath($"Resources/{culture.Name}.json");

                using (var fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read))
                using (var streamReader = new StreamReader(fileStream))
                {
                    var fileContent =
                        streamReader.ReadToEndAsync().Result;

                    var resources =
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(fileContent) ?? [];

                    cultureStrings = new CultureStrings
                    {
                        Timestamp = DateTime.UtcNow,
                        Records = resources
                    };
                }

                _strings.AddOrUpdate(
                    culture.TwoLetterISOLanguageName,
                    cultureStrings,
                    (key, oldValue) => cultureStrings);
            }

            return cultureStrings.Records;
        }
    }
}
