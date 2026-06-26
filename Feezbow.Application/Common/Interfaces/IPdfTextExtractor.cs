namespace Feezbow.Application.Common.Interfaces;

public interface IPdfTextExtractor
{
    string ExtractFirstPages(byte[] pdfBytes, int maxPages = 2);
}
