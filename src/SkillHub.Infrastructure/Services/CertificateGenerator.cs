using System.Security.Cryptography;
using System.Text;
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
        var rawSignature = $"{participantEmail}|{workshopCode}|{enrollmentDate:O}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawSignature));
        var verificationCode = Convert.ToHexString(hashBytes)[..12];
        var folioNumber = $"SH-{enrollmentDate:yyyy}-{verificationCode[..6]}";

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontColor("#1E293B"));

                page.Content()
                    .Border(1.5f)
                    .BorderColor("#0F172A")
                    .Padding(24)
                    .Column(main =>
                    {
                        main.Spacing(14);

                        // 1. HEADER
                        main.Item().BorderBottom(1.5f).BorderColor("#E2E8F0").PaddingBottom(12).Row(row =>
                        {
                            row.RelativeItem().Row(brand =>
                            {
                                brand.ConstantItem(38).Height(38).Background("#2563EB").AlignCenter().AlignMiddle()
                                    .Text("SH").FontSize(15).Bold().FontColor(Colors.White);

                                brand.RelativeItem().PaddingLeft(10).Column(col =>
                                {
                                    col.Item().Text("SkillHub").FontSize(19).ExtraBold().FontColor("#0F172A");
                                    col.Item().Text("Centro de Formación Profesional Presencial").FontSize(9.5f).FontColor("#475569");
                                });
                            });

                            row.ConstantItem(155).AlignRight().Column(folio =>
                            {
                                folio.Spacing(2);
                                folio.Item().AlignRight().Text($"Folio: {folioNumber}").FontSize(8.5f).Bold().FontColor("#0F172A");
                                folio.Item().AlignRight().Background("#DCFCE7").PaddingHorizontal(6).PaddingVertical(2)
                                    .Text("● INSCRIPCIÓN ACTIVA").FontSize(7.5f).Bold().FontColor("#15803D");
                            });
                        });

                        // 2. TITLE & INTRO
                        main.Item().PaddingTop(4).Column(intro =>
                        {
                            intro.Spacing(6);
                            intro.Item().AlignCenter().Text("Constancia de inscripción")
                                .FontSize(20).ExtraBold().FontColor("#0F172A");

                            intro.Item().PaddingTop(4).Text(text =>
                            {
                                text.Span("Se certifica oficialmente que ");
                                text.Span(participantName).Bold().FontColor("#0F172A");
                                text.Span($" ({participantEmail}) cuenta con un cupo confirmado y activo en el siguiente taller presencial impartido por ");
                                text.Span("SkillHub").Bold();
                                text.Span(":");
                            });
                        });

                        // 3. WORKSHOP DETAILS CARD (Compact 2x2 Grid)
                        main.Item().Border(1).BorderColor("#CBD5E1").Background("#F8FAFC").Column(card =>
                        {
                            card.Item().Background("#0F172A").PaddingHorizontal(14).PaddingVertical(10).Row(wRow =>
                            {
                                wRow.RelativeItem().Text(workshopTitle).FontSize(13).Bold().FontColor(Colors.White);
                                wRow.ConstantItem(85).AlignRight().AlignMiddle().Text(workshopCode).FontSize(10).ExtraBold().FontColor("#60A5FA");
                            });

                            card.Item().Padding(14).Column(grid =>
                            {
                                grid.Spacing(10);

                                grid.Item().Row(r1 =>
                                {
                                    r1.RelativeItem().Column(c =>
                                    {
                                        c.Item().Text("Instructor a cargo").FontSize(8).Bold().FontColor("#64748B");
                                        c.Item().Text(instructorName).FontSize(10.5f).Bold().FontColor("#0F172A");
                                    });

                                    r1.RelativeItem().Column(c =>
                                    {
                                        c.Item().Text("Fecha y horario del taller").FontSize(8).Bold().FontColor("#64748B");
                                        c.Item().Text($"{startDateTime:yyyy-MM-dd HH:mm} UTC - {endDateTime:yyyy-MM-dd HH:mm} UTC").FontSize(9.5f).Bold().FontColor("#0F172A");
                                    });
                                });

                                grid.Item().LineHorizontal(1).LineColor("#E2E8F0");

                                grid.Item().Row(r2 =>
                                {
                                    r2.RelativeItem().Column(c =>
                                    {
                                        c.Item().Text("Fecha de inscripción").FontSize(8).Bold().FontColor("#64748B");
                                        c.Item().Text($"{enrollmentDate:yyyy-MM-dd HH:mm} UTC").FontSize(9.5f).FontColor("#0F172A");
                                    });

                                    r2.RelativeItem().Column(c =>
                                    {
                                        c.Item().Text("Fecha de emisión de constancia").FontSize(8).Bold().FontColor("#64748B");
                                        c.Item().Text($"{generationDate:yyyy-MM-dd HH:mm} UTC").FontSize(9.5f).FontColor("#0F172A");
                                    });
                                });
                            });
                        });

                        // 4. COMPACT SIGNATURES & FOOTER
                        main.Item().PaddingTop(16).Row(sigRow =>
                        {
                            sigRow.RelativeItem().Column(sig1 =>
                            {
                                sig1.Item().PaddingHorizontal(16).LineHorizontal(1).LineColor("#94A3B8");
                                sig1.Item().PaddingTop(4).AlignCenter().Text("Coordinación Académica SkillHub").FontSize(9).Bold().FontColor("#0F172A");
                            });

                            sigRow.ConstantItem(30);

                            sigRow.RelativeItem().Column(sig2 =>
                            {
                                sig2.Item().PaddingHorizontal(16).LineHorizontal(1).LineColor("#94A3B8");
                                sig2.Item().PaddingTop(4).AlignCenter().Text(instructorName).FontSize(9).Bold().FontColor("#0F172A");
                            });
                        });

                        main.Item().PaddingTop(8).BorderTop(1).BorderColor("#E2E8F0").PaddingTop(6).Row(f =>
                        {
                            f.RelativeItem().Text($"Verificación: {verificationCode} • Válida mientras la inscripción permanezca activa.").FontSize(8).FontColor("#64748B");
                            f.ConstantItem(60).AlignRight().Text(x =>
                            {
                                x.Span("Pág. ").FontSize(8).FontColor("#64748B");
                                x.CurrentPageNumber().FontSize(8).FontColor("#64748B");
                            });
                        });
                    });
            });
        });

        return document.GeneratePdf();
    }
}