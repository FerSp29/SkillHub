namespace SkillHub.Application.Common.Interfaces;

public interface ICertificateGenerator
{
    byte[] GenerateCertificatePdf(
        string participantName,
        string participantEmail,
        string workshopCode,
        string workshopTitle,
        string instructorName,
        DateTime startDateTime,
        DateTime endDateTime,
        DateTime enrollmentDate,
        DateTime generationDate);
}