using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EgyptTechJobsAdmin.Data;
using EgyptTechJobsAdmin.Models.DTOs;
using EgyptTechJobsAdmin.Models.Entities;

namespace EgyptTechJobsAdmin.Controllers;

/// <summary>
/// Controller for managing blocked companies and keywords
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FiltersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public FiltersController(ApplicationDbContext context)
    {
        _context = context;
    }

    #region Blocked Companies

    /// <summary>
    /// Get all blocked companies
    /// </summary>
    [HttpGet("companies")]
    public async Task<ActionResult<List<BlockedCompanyDto>>> GetBlockedCompanies([FromQuery] bool? isActive = null)
    {
        var query = _context.BlockedCompanies.AsQueryable();
        
        if (isActive.HasValue)
            query = query.Where(c => c.IsActive == isActive.Value);

        var companies = await query
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new BlockedCompanyDto
            {
                Id = c.Id,
                CompanyName = c.CompanyName,
                Reason = c.Reason,
                CreatedAt = c.CreatedAt,
                IsActive = c.IsActive
            })
            .ToListAsync();

        return Ok(companies);
    }

    /// <summary>
    /// Add a blocked company
    /// </summary>
    [HttpPost("companies")]
    public async Task<ActionResult<BlockedCompanyDto>> AddBlockedCompany([FromBody] CreateBlockedCompanyDto dto)
    {
        // Check if already exists
        var exists = await _context.BlockedCompanies
            .AnyAsync(c => c.CompanyName.ToLower() == dto.CompanyName.ToLower());
        
        if (exists)
            return BadRequest(new { message = "Company is already blocked" });

        var company = new BlockedCompany
        {
            CompanyName = dto.CompanyName,
            Reason = dto.Reason,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.BlockedCompanies.Add(company);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBlockedCompanies), new BlockedCompanyDto
        {
            Id = company.Id,
            CompanyName = company.CompanyName,
            Reason = company.Reason,
            CreatedAt = company.CreatedAt,
            IsActive = company.IsActive
        });
    }

    /// <summary>
    /// Toggle blocked company status
    /// </summary>
    [HttpPatch("companies/{id}/toggle")]
    public async Task<ActionResult<BlockedCompanyDto>> ToggleBlockedCompany(int id)
    {
        var company = await _context.BlockedCompanies.FindAsync(id);
        if (company == null)
            return NotFound(new { message = "Blocked company not found" });

        company.IsActive = !company.IsActive;
        await _context.SaveChangesAsync();

        return Ok(new BlockedCompanyDto
        {
            Id = company.Id,
            CompanyName = company.CompanyName,
            Reason = company.Reason,
            CreatedAt = company.CreatedAt,
            IsActive = company.IsActive
        });
    }

    /// <summary>
    /// Delete a blocked company
    /// </summary>
    [HttpDelete("companies/{id}")]
    public async Task<ActionResult> DeleteBlockedCompany(int id)
    {
        var company = await _context.BlockedCompanies.FindAsync(id);
        if (company == null)
            return NotFound(new { message = "Blocked company not found" });

        _context.BlockedCompanies.Remove(company);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Get unique companies from jobs for autocomplete
    /// </summary>
    [HttpGet("companies/suggestions")]
    public async Task<ActionResult<List<string>>> GetCompanySuggestions([FromQuery] string? search = null)
    {
        var query = _context.Jobs
            .Select(j => j.Company)
            .Distinct();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(c => c.ToLower().Contains(search.ToLower()));

        var companies = await query
            .OrderBy(c => c)
            .Take(50)
            .ToListAsync();

        return Ok(companies);
    }

    #endregion

    #region Blocked Keywords

    /// <summary>
    /// Get all blocked keywords
    /// </summary>
    [HttpGet("keywords")]
    public async Task<ActionResult<List<BlockedKeywordDto>>> GetBlockedKeywords([FromQuery] bool? isActive = null)
    {
        var query = _context.BlockedKeywords.AsQueryable();
        
        if (isActive.HasValue)
            query = query.Where(k => k.IsActive == isActive.Value);

        var keywords = await query
            .OrderByDescending(k => k.CreatedAt)
            .Select(k => new BlockedKeywordDto
            {
                Id = k.Id,
                Keyword = k.Keyword,
                Reason = k.Reason,
                CreatedAt = k.CreatedAt,
                IsActive = k.IsActive
            })
            .ToListAsync();

        return Ok(keywords);
    }

    /// <summary>
    /// Add a blocked keyword
    /// </summary>
    [HttpPost("keywords")]
    public async Task<ActionResult<BlockedKeywordDto>> AddBlockedKeyword([FromBody] CreateBlockedKeywordDto dto)
    {
        // Check if already exists
        var exists = await _context.BlockedKeywords
            .AnyAsync(k => k.Keyword.ToLower() == dto.Keyword.ToLower());
        
        if (exists)
            return BadRequest(new { message = "Keyword is already blocked" });

        var keyword = new BlockedKeyword
        {
            Keyword = dto.Keyword,
            Reason = dto.Reason,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.BlockedKeywords.Add(keyword);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBlockedKeywords), new BlockedKeywordDto
        {
            Id = keyword.Id,
            Keyword = keyword.Keyword,
            Reason = keyword.Reason,
            CreatedAt = keyword.CreatedAt,
            IsActive = keyword.IsActive
        });
    }

    /// <summary>
    /// Toggle blocked keyword status
    /// </summary>
    [HttpPatch("keywords/{id}/toggle")]
    public async Task<ActionResult<BlockedKeywordDto>> ToggleBlockedKeyword(int id)
    {
        var keyword = await _context.BlockedKeywords.FindAsync(id);
        if (keyword == null)
            return NotFound(new { message = "Blocked keyword not found" });

        keyword.IsActive = !keyword.IsActive;
        await _context.SaveChangesAsync();

        return Ok(new BlockedKeywordDto
        {
            Id = keyword.Id,
            Keyword = keyword.Keyword,
            Reason = keyword.Reason,
            CreatedAt = keyword.CreatedAt,
            IsActive = keyword.IsActive
        });
    }

    /// <summary>
    /// Delete a blocked keyword
    /// </summary>
    [HttpDelete("keywords/{id}")]
    public async Task<ActionResult> DeleteBlockedKeyword(int id)
    {
        var keyword = await _context.BlockedKeywords.FindAsync(id);
        if (keyword == null)
            return NotFound(new { message = "Blocked keyword not found" });

        _context.BlockedKeywords.Remove(keyword);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Bulk add blocked keywords
    /// </summary>
    [HttpPost("keywords/bulk")]
    public async Task<ActionResult<int>> BulkAddKeywords([FromBody] List<CreateBlockedKeywordDto> dtos)
    {
        var added = 0;
        foreach (var dto in dtos)
        {
            var exists = await _context.BlockedKeywords
                .AnyAsync(k => k.Keyword.ToLower() == dto.Keyword.ToLower());
            
            if (!exists)
            {
                _context.BlockedKeywords.Add(new BlockedKeyword
                {
                    Keyword = dto.Keyword,
                    Reason = dto.Reason,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                });
                added++;
            }
        }

        await _context.SaveChangesAsync();
        return Ok(new { added });
    }

    #endregion
}
