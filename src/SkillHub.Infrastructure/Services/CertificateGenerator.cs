using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SkillHub.Application.Common.Interfaces;

namespace SkillHub.Infrastructure.Services;

public class CertificateGenerator : ICertificateGenerator
{
    static CertificateGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateCertificatePdf(
        string participantName,
        string participantEmail,
        string workshopCode,
        string workshopTitle,
        string instructorName,
        DateTime startDateTime,
        DateTime endDateTime,
        DateTime enrollmentDate,
        DateTime generationDate)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header()
                    .BorderBottom(2)
                    .BorderColor(Colors.Blue.Darken2)
                    .PaddingBottom(10)
                    .Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("SkillHub").FontSize(24).Bold().FontColor(Colors.Blue.Darken3);
                            col.Item().Text("Centro de Formación Profesional Presencial").FontSize(10).FontColor(Colors.Grey.Medium);
                        });
                    });

                page.Content().PaddingVertical(25).Column(col =>
                {
                    col.Spacing(15);

                    col.Item().AlignCenter().Text("Constancia de inscripción")
                        .FontSize(20).Bold().FontColor(Colors.Blue.Darken2);

                    col.Item().PaddingTop(10).Text(text =>
                    {
                        text.Span("Se certifica formalmente que ");
                        text.Span(participantName).Bold();
                        text.Span($" ({participantEmail}) cuenta con una inscripción activa en el siguiente taller impartido por SkillHub:");
                    });

                    col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(15).Background(Colors.Grey.Lighten4).Column(details =>
                    {
                        details.Spacing(8);
                        details.Item().Text(t => { t.Span("Código del taller: ").Bold(); t.Span(workshopCode); });
                        details.Item().Text(t => { t.Span("Título: ").Bold(); t.Span(workshopTitle); });
                        details.Item().Text(t => { t.Span("Instructor a cargo: ").Bold(); t.Span(instructorName); });
                        details.Item().Text(t => { t.Span("Fecha y horario: ").Bold(); t.Span($"{startDateTime:yyyy-MM-dd HH:mm} UTC - {endDateTime:yyyy-MM-dd HH:mm} UTC"); });
                        details.Item().Text(t => { t.Span("Fecha de inscripción: ").Bold(); t.Span($"{enrollmentDate:yyyy-MM-dd HH:mm} UTC"); });
                    });

                    col.Item().PaddingTop(15).Text(t =>
                    {
                        t.Span("Documento oficial emitido y validado automáticamente el ");
                        t.Span($"{generationDate:yyyy-MM-dd HH:mm} UTC").Bold();
                        t.Span(". Esta constancia es válida mientras la inscripción permanezca en estado Activa.");
                    });
                });

                page.Footer()
                    .BorderTop(1)
                    .BorderColor(Colors.Grey.Lighten2)
                    .PaddingTop(10)
                    .Row(row =>
                    {
                        row.RelativeItem().Text("SkillHub Platform © 2026").FontSize(9).FontColor(Colors.Grey.Medium);
                        row.RelativeItem().AlignRight().Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                        });
                    });
            });
        });

        return document.GeneratePdf();
    }
}