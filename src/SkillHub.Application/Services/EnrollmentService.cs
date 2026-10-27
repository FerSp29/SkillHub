using ClosedXML.Excel;
using SkillHub.Application.Common.Interfaces;
using SkillHub.Application.DTOs.Enrollments;
using SkillHub.Domain.Entities;
using SkillHub.Domain.Enums;
using SkillHub.Domain.Interfaces;

namespace SkillHub.Application.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IWorkshopRepository _workshopRepository;
    private readonly IUserService _userService;
    private readonly ICertificateGenerator _certificateGenerator;

    public EnrollmentService(
        IEnrollmentRepository enrollmentRepository,
        IWorkshopRepository workshopRepository,
        IUserService userService,
        ICertificateGenerator certificateGenerator)
    {
        _enrollmentRepository = enrollmentRepository;
        _workshopRepository = workshopRepository;
        _userService = userService;
        _certificateGenerator = certificateGenerator;
    }

    public async Task<EnrollmentResponseDto> EnrollAsync(string participantId, EnrollRequestDto request)
    {
        var workshop = await _workshopRepository.GetByIdWithEnrollmentsAsync(request.WorkshopId);
        if (workshop == null)
        {
            throw new KeyNotFoundException("Workshop not found.");
        }

        // Rule 4: Future workshops only. Cannot enroll in a workshop that has already started.
        if (workshop.StartDateTime <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Cannot enroll in a workshop that has already started.");
        }

        // Rule 1: Capacity limit. Cannot exceed maximum capacity (only active enrollments count).
        if (workshop.AvailableSpots <= 0)
        {
            throw new InvalidOperationException("No available spots in this workshop.");
        }

        // Rule 2: No duplicates. A participant cannot have two active enrollments in the same workshop.
        var hasActive = await _enrollmentRepository.HasActiveEnrollmentInWorkshopAsync(participantId, workshop.Id);
        if (hasActive)
        {
            throw new InvalidOperationException("Participant already has an active enrollment in this workshop.");
        }

        // Rule 3: No schedule overlap with another active workshop enrollment.
        var hasOverlap = await _enrollmentRepository.HasOverlappingActiveEnrollmentAsync(
            participantId, workshop.StartDateTime, workshop.EndDateTime);
        if (hasOverlap)
        {
            throw new InvalidOperationException("Participant has a schedule conflict with another active workshop enrollment.");
        }

        var enrollment = new Enrollment
        {
            ParticipantId = participantId,
            WorkshopId = workshop.Id,
            Status = EnrollmentStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        await _enrollmentRepository.AddAsync(enrollment);
        await _enrollmentRepository.SaveChangesAsync();

        return new EnrollmentResponseDto(
            enrollment.Id,
            workshop.Id,
            workshop.Code,
            workshop.Title,
            enrollment.Status.ToString(),
            enrollment.CreatedAt,
            enrollment.CancelledAt,
            enrollment.CancelledByUserId
        );
    }

    public async Task<IEnumerable<EnrollmentResponseDto>> GetMyEnrollmentsAsync(string participantId)
    {
        var enrollments = await _enrollmentRepository.GetByParticipantIdAsync(participantId);
        return enrollments.Select(e => new EnrollmentResponseDto(
            e.Id,
            e.Workshop.Id,
            e.Workshop.Code,
            e.Workshop.Title,
            e.Status.ToString(),
            e.CreatedAt,
            e.CancelledAt,
            e.CancelledByUserId
        ));
    }

    public async Task CancelEnrollmentAsync(Guid enrollmentId, string userId, bool isEmployee)
    {
        var enrollment = await _enrollmentRepository.GetByIdWithWorkshopAsync(enrollmentId);
        if (enrollment == null)
        {
            throw new KeyNotFoundException("Enrollment not found.");
        }

        if (enrollment.Status == EnrollmentStatus.Cancelled)
        {
            throw new InvalidOperationException("Enrollment is already cancelled.");
        }

        // Participants can only cancel their own enrollments
        if (!isEmployee && enrollment.ParticipantId != userId)
        {
            throw new UnauthorizedAccessException("You can only cancel your own enrollments.");
        }

        // Rule 5: The workshop must not have started yet
        if (enrollment.Workshop.StartDateTime <= DateTime.UtcNow)
        {
            throw new InvalidOperationException("Cannot cancel an enrollment for a workshop that has already started.");
        }

        // Rule 5: 24-hour prior cancellation restriction for participants
        if (!isEmployee)
        {
            var hoursDifference = (enrollment.Workshop.StartDateTime - DateTime.UtcNow).TotalHours;
            if (hoursDifference < 24)
            {
                throw new InvalidOperationException("Participants can only cancel up to 24 hours prior to the workshop start time.");
            }
        }

        enrollment.Status = EnrollmentStatus.Cancelled;
        enrollment.CancelledAt = DateTime.UtcNow;
        enrollment.CancelledByUserId = userId;

        await _enrollmentRepository.SaveChangesAsync();
    }

    public async Task<ImportExcelResultDto> ImportFromExcelAsync(Stream excelStream, string employeeUserId)
    {
        using var workbook = new XLWorkbook(excelStream);
        var worksheet = workbook.Worksheets.FirstOrDefault(w => w.Name == "Enrollments") ?? workbook.Worksheets.First();
        var rows = worksheet.RowsUsed().Skip(1);

        int totalProcessed = 0;
        int totalSuccess = 0;
        int totalFailed = 0;
        var errors = new List<ImportErrorDetail>();

        foreach (var row in rows)
        {
            totalProcessed++;
            int rowNumber = row.RowNumber();
            string participantEmail = row.Cell(1).GetString().Trim();
            string workshopCode = row.Cell(2).GetString().Trim();

            try
            {
                var participant = await _userService.GetByEmailAsync(participantEmail);
                if (participant == null)
                {
                    throw new ArgumentException($"Participant with email '{participantEmail}' does not exist.");
                }

                var workshop = await _workshopRepository.GetByCodeAsync(workshopCode);
                if (workshop == null)
                {
                    throw new ArgumentException($"Workshop with code '{workshopCode}' does not exist.");
                }

                if (workshop.StartDateTime <= DateTime.UtcNow)
                {
                    throw new InvalidOperationException("Workshop has already started.");
                }

                if (workshop.AvailableSpots <= 0)
                {
                    throw new InvalidOperationException("No available spots in the workshop.");
                }

                var hasActive = await _enrollmentRepository.HasActiveEnrollmentInWorkshopAsync(participant.Id, workshop.Id);
                if (hasActive)
                {
                    throw new InvalidOperationException("Participant already has an active enrollment in this workshop.");
                }

                var hasOverlap = await _enrollmentRepository.HasOverlappingActiveEnrollmentAsync(
                    participant.Id, workshop.StartDateTime, workshop.EndDateTime);
                if (hasOverlap)
                {
                    throw new InvalidOperationException("Schedule conflict with another active workshop enrollment.");
                }

                var enrollment = new Enrollment
                {
                    ParticipantId = participant.Id,
                    WorkshopId = workshop.Id,
                    Status = EnrollmentStatus.Active,
                    CreatedAt = DateTime.UtcNow
                };

                await _enrollmentRepository.AddAsync(enrollment);
                await _enrollmentRepository.SaveChangesAsync();
                totalSuccess++;
            }
            catch (Exception ex)
            {
                totalFailed++;
                errors.Add(new ImportErrorDetail(rowNumber, participantEmail, workshopCode, ex.Message));
            }
        }

        return new ImportExcelResultDto(totalProcessed, totalSuccess, totalFailed, errors);
    }

    public async Task<byte[]> GenerateCertificateAsync(Guid enrollmentId, string userId, bool isEmployee)
    {
        var enrollment = await _enrollmentRepository.GetByIdWithWorkshopAsync(enrollmentId);
        if (enrollment == null)
        {
            throw new KeyNotFoundException("Enrollment not found.");
        }

        // Certificates can only be generated for active enrollments
        if (enrollment.Status != EnrollmentStatus.Active)
        {
            throw new InvalidOperationException("Certificate can only be generated for active enrollments.");
        }

        // Participants can only download their own certificates
        if (!isEmployee && enrollment.ParticipantId != userId)
        {
            throw new UnauthorizedAccessException("You can only download certificates for your own enrollments.");
        }

        var participant = await _userService.GetByIdAsync(enrollment.ParticipantId);
        var instructor = await _userService.GetByIdAsync(enrollment.Workshop.InstructorId);

        return _certificateGenerator.GenerateCertificatePdf(
            participant?.FullName ?? "Participant",
            participant?.Email ?? "participant@correo.com",
            enrollment.Workshop.Code,
            enrollment.Workshop.Title,
            instructor?.FullName ?? "SkillHub Instructor",
            enrollment.Workshop.StartDateTime,
            enrollment.Workshop.EndDateTime,
            enrollment.CreatedAt,
            DateTime.UtcNow
        );
    }

    public async Task<IEnumerable<TopWorkshopReportDto>> GetTopWorkshopsReportAsync()
    {
        var top = await _enrollmentRepository.GetTopWorkshopsAsync();
        return top.Select(t => new TopWorkshopReportDto(
            t.Workshop.Id,
            t.Workshop.Code,
            t.Workshop.Title,
            t.Workshop.Capacity,
            t.ActiveCount
        ));
    }
}