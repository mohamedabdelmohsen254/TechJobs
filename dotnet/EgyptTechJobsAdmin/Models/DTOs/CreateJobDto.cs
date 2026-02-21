using System.ComponentModel.DataAnnotations;

namespace EgyptTechJobsAdmin.Models.DTOs;

public class CreateJobDto
{
    [Required]
    [MaxLength(500)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Company { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Location { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(50)]
    public string? WorkType { get; set; }

    [Required]
    [MaxLength(1000)]
    public string ApplyUrl { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Source { get; set; }

    public DateTime? PostedDate { get; set; }

    [MaxLength(5000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? SalaryRange { get; set; }

    [MaxLength(500)]
    public string? Tags { get; set; }

    public bool IsVisibleToUsers { get; set; } = true;
}
