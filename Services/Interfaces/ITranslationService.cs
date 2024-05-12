using System;
using System.Collections.Generic;
using System.Text;

namespace Services.Interfaces
{
    public interface ITranslationService
    {
        public string Translate(string text, string targetLang);
    }
}
