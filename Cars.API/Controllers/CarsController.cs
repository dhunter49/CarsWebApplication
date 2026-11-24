using Cars.Application.Cars;
using Cars.Application.Cars.DTOs;
using Cars.Domain;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Cars.API.Controllers;

[Authorize]
public class CarsController : BaseApiController
{
    private readonly ISender _sender;
    private readonly IWebHostEnvironment _environment;

    public CarsController(ISender sender, IWebHostEnvironment environment)
    {
        _sender = sender;
        _environment = environment;
    }

    [HttpGet]
    public async Task<ActionResult<List<CarDto>>> GetCars()
    {
        return await _sender.Send(new GetCarsList.Query());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CarDto>> GetCar(Guid id)
    {
        var car = await _sender.Send(new Details.Query { Id = id });
        if (car is null) return NotFound();
        return car;
    }

    [HttpPost]
    public async Task<ActionResult<Car>> CreateCar(Car car)
    {
        var result = await _sender.Send(new Create.Command { Car = car });
        
        return CreatedAtAction(
            nameof(GetCar),
            new { id = result.Id },
            result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateCar(Guid id, Car car)
    {
        var result = await _sender.Send(new Edit.Command
        {
            Id = id,
            Car = car
        });
        
        if(!result) return NotFound();
        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteCar(Guid id)
    {
        var result = await _sender.Send(
            new Delete.Command { Id = id });

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
    
    [HttpPut("{id:guid}/features")]
    public async Task<IActionResult> UpdateFeatures(Guid id, UpdateCarFeaturesDto dto)
    {
        var result = await _sender.Send(
            new UpdateCarFeatures.Command
            {
                CarId = id,
                FeatureIds = dto.FeatureIds,
            });

        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }
    
    [Authorize]
    [HttpPost("{id:guid}/photos")]
    [RequestSizeLimit(5_000_000)]
    public async Task<ActionResult<CarPhotoDto>> UploadPhoto(Guid id, IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("Nie przesłano pliku.");
        }

        var allowedExtensions = new[]
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp",
        };

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
        {
            return BadRequest("Dozwolone są tylko pliki JPG, JPEG, PNG i WebP.");
        }

        var fileName = $"{Guid.NewGuid()}{extension}";
        var webRootPath = _environment.WebRootPath ?? Path.Combine(
            Directory.GetCurrentDirectory(), "wwwroot");
        var uploadDirectory = Path.Combine(webRootPath, "uploads", "cars");
        Directory.CreateDirectory(uploadDirectory);

        var filePath = Path.Combine(uploadDirectory, fileName);
        await using (var stream = System.IO.File.Create(filePath))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var url = $"{Request.Scheme}://{Request.Host}/uploads/cars/{fileName}";

        var result = await _sender.Send(new AddCarPhoto.Command
            {
                CarId = id,
                FileName = fileName,
                Url = url,
            },
            cancellationToken);

        if (result is null)
        {
            System.IO.File.Delete(filePath);
            return NotFound();
        }

        return CreatedAtAction(nameof(GetCar), new { id }, result);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}/photos/{photoId:guid}")]
    public async Task<IActionResult> DeletePhoto(Guid id, Guid photoId, CancellationToken cancellationToken)
    {
        var fileName = await _sender.Send(
            new DeleteCarPhoto.Command
            {
                CarId = id,
                PhotoId = photoId,
            },
            cancellationToken);

        if (fileName is null)
        {
            return NotFound();
        }

        var webRootPath = _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var filePath = Path.Combine(webRootPath, "uploads", "cars", fileName);

        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }

        return NoContent();
    }
}