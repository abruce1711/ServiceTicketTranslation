using Google.Api.Gax.ResourceNames;
using Google.Cloud.Translate.V3;
using HtmlAgilityPack;
using Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Services.Services
{
    public class TranslationService : ITranslationService
    {
        private readonly string projectId = "language-translation";

        public string Translate(string text, string targetLang)
        {
            HtmlDocument mainDoc = new HtmlDocument();
            mainDoc.LoadHtml(text);
            string cleanText = mainDoc.DocumentNode.InnerText;
            List<string> splitText = new List<string>();
            string translatedText = string.Empty;

            // Splits text up into 5000 character chunks for translation
            if (cleanText.Length > 5000)
            {
                splitText = SplitText(cleanText, 5000).ToList();
            } else
            {
                splitText.Add(cleanText);
            }


            foreach (string chunk in splitText)
            {
                TranslationServiceClient translationServiceClient = TranslationServiceClient.Create();
                TranslateTextRequest request = new TranslateTextRequest
                {
                    Contents = { chunk },
                    TargetLanguageCode = targetLang,
                    ParentAsLocationName = new LocationName(projectId, "global"),
                };
                TranslateTextResponse response = translationServiceClient.TranslateText(request);
                // Display the translation for each input text provided

                foreach (Translation translation in response.Translations)
                {
                    translatedText += translation.TranslatedText;
                }
            }


            return translatedText;
        }

        static IEnumerable<string> SplitText(string str, int chunkSize)
        {
            return Enumerable.Range(0, str.Length / chunkSize)
                .Select(i => str.Substring(i * chunkSize, chunkSize));
        }
    }
}
