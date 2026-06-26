using System.Text;
using Feezbow.Application.Common.Interfaces;
using UglyToad.PdfPig;

namespace Feezbow.Infrastructure.AI.Helpers;

public class PdfTextExtractor : IPdfTextExtractor
{
    public string ExtractFirstPages(byte[] pdfBytes, int maxPages = 2)
    {
        using var doc = PdfDocument.Open(pdfBytes);
        var sb = new StringBuilder();
        foreach (var page in doc.GetPages().Take(maxPages))
            foreach (var word in page.GetWords())
                sb.Append(word.Text).Append(' ');
        return sb.ToString().Trim();
    }
}
